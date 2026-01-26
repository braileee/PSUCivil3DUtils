using AutoCADUtils.Utils;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using Civil3DCorridorReport.Models.Json;
using Civil3DUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DCorridorReport.Models
{
    public class CorridorData
    {
        public CorridorData(Corridor corridor, Settings settings)
        {
            Corridor = corridor;
            Settings = settings;
        }

        public Corridor Corridor { get; }
        public Settings Settings { get; }


        public List<CorridorDataItem> Get()
        {
            Database db = DocumentUtils.Document.Database;

            List<CorridorDataItem> corridorDataItems = new List<CorridorDataItem>();

            using (var tr = db.TransactionManager.StartTransaction())
            {
                foreach (Baseline bl in Corridor.Baselines)
                {
                    Alignment alignment = bl.GetAlignment(OpenMode.ForRead);

                    if (alignment.GetProfileIds().Count == 0)
                    {
                        continue;
                    }

                    Profile profile = tr.GetObject(alignment.GetProfileIds()[0], OpenMode.ForRead, false, true) as Profile;

                    foreach (BaselineRegion region in bl.BaselineRegions)
                    {
                        string slopeNoValue = "n/a";
                        string slopeLeftRatio = slopeNoValue;
                        string slopeRightRatio = slopeNoValue;
                        double? slopeLeftPercent = null;
                        double? slopeRightPercent = null;
                        AppliedAssembly firstAppliedAssembly = region.AppliedAssemblies[region.AppliedAssemblies.Count / 2];

                        ObjectId asmId = firstAppliedAssembly.AssemblyId;
                        Assembly asm = (Assembly)tr.GetObject(asmId, OpenMode.ForRead);
                        string name = asm.Name;

                        double regionStartStation = Math.Round(region.StartStation, 2);
                        double regionEndStation = Math.Round(region.EndStation, 2);


                        if (firstAppliedAssembly == null)
                        {
                            continue;
                        }

                        AppliedSubassemblyCollection appliedSubassemblies = firstAppliedAssembly.GetAppliedSubassemblies();

                        double totalWidth = 0;

                        foreach (AppliedSubassembly appliedSubassembly in appliedSubassemblies)
                        {
                            double? width = GetLinkLength(appliedSubassembly, Settings.Structure.WidthData.CalcualationWidthLinkCodes);

                            if (width.HasValue)
                            {
                                totalWidth += width.Value;
                            }

                            if (IsNoneSide(appliedSubassembly))
                            {
                                slopeLeftPercent = TryGetSlopePercentFromLinkToken(appliedSubassembly, Settings.Structure.SlopeData.SlopeLeft);
                                slopeRightPercent = TryGetSlopePercentFromLinkToken(appliedSubassembly, Settings.Structure.SlopeData.SlopeRight);

                                slopeLeftRatio = slopeLeftPercent.HasValue ? FormatSlopeAsRatio(slopeLeftPercent.Value) : slopeNoValue;
                                slopeRightRatio = slopeRightPercent.HasValue ? FormatSlopeAsRatio(slopeRightPercent.Value) : slopeNoValue;
                            }
                            else if (IsLeftSide(appliedSubassembly))
                            {
                                slopeLeftPercent = TryGetSlopePercentFromLinkToken(appliedSubassembly, Settings.Structure.SlopeData.SlopeCommon);
                                slopeLeftRatio = slopeLeftPercent.HasValue ? FormatSlopeAsRatio(slopeLeftPercent.Value) : slopeNoValue;
                            }
                            else
                            {
                                slopeRightPercent = TryGetSlopePercentFromLinkToken(appliedSubassembly, Settings.Structure.SlopeData.SlopeCommon);
                                slopeRightRatio = slopeRightPercent.HasValue ? FormatSlopeAsRatio(slopeRightPercent.Value) : slopeNoValue;
                            }
                        }

                        List<ProfilePoint> profilePoints = new List<ProfilePoint>();
                        profilePoints = profile.PVIs.Select(item => new ProfilePoint { Station = item.RawStation }).ToList();

                        profilePoints = profilePoints.DistinctBy(item => item.Station).OrderBy(item => item.Station).ToList();

                        List<ProfilePoint> rangedProfilePoints = profilePoints.Where(item => item.Station >= regionStartStation && item.Station <= regionEndStation).ToList();
                        rangedProfilePoints.Add(new ProfilePoint { Station = regionStartStation });
                        rangedProfilePoints.Add(new ProfilePoint { Station = regionEndStation });

                        rangedProfilePoints = rangedProfilePoints.DistinctBy(item => item.Station).OrderBy(item => item.Station).ToList();

                        Dictionary<double, ProfilePoint> addedStation = new Dictionary<double, ProfilePoint>();

                        foreach (ProfilePoint profilePoint in rangedProfilePoints)
                        {
                            ProfilePoint? prevProfilePoint = rangedProfilePoints.ElementAtOrDefault(rangedProfilePoints.IndexOf(profilePoint) - 1);

                            double startStation = regionStartStation;

                            if (prevProfilePoint != null)
                            {
                                startStation = Math.Round(prevProfilePoint.Station, 2);
                            }

                            double endStation = Math.Round(profilePoint.Station, 2);

                            if (startStation == endStation)
                            {
                                continue;
                            }

                            double elevation = profile.ElevationAt((startStation + endStation) / 2);

                            CorridorDataItem corridorDataItem = new CorridorDataItem
                            {
                                AssemblyName = asm.Name,
                                StartStation = startStation,
                                EndStation = endStation,
                                SlopeFrontPercent = slopeLeftPercent,
                                SlopeBackPercent = slopeRightPercent,
                                TopElevation = elevation,
                                Width = totalWidth,
                            };

                            corridorDataItems.Add(corridorDataItem);
                        }
                    }
                }

                tr.Commit();
            }

            return corridorDataItems;

        }

        double? GetPointElevation(AppliedAssembly aa, string code)
        {
            foreach (var sub in aa.GetAppliedSubassemblies())
            {
                foreach (var pt in sub.Points)
                {
                    if (pt.CorridorCodes.Contains(code))
                        return pt.XYZ.Z;
                }
            }
            return null;
        }

        double? GetLinkLength(AppliedSubassembly aa, List<string> codes)
        {
            double totalLength = 0;

            foreach (var link in aa.Links)
            {
                if (codes.Any(code => link.CorridorCodes.Contains(code)))
                {
                    var points = link.CalculatedPoints;
                    if(points.Count < 2)
                    {
                        continue;
                    }

                    var point1 = points[0];
                    var point2 = points[1];

                    double length = point1.XYZ.DistanceTo(point2.XYZ);
                    totalLength += length;
                }
            }
            return totalLength;
        }

        private bool IsNoneSide(AppliedSubassembly appliedSubassembly)
        {
            var parameter = appliedSubassembly.Parameters.FirstOrDefault(item => item.KeyName == "Side");

            return parameter == null;
        }

        private bool IsLeftSide(AppliedSubassembly appliedSubassembly)
        {
            var parameter = appliedSubassembly.Parameters.FirstOrDefault(item => item.KeyName == "Side");


            if (parameter == null)
            {
                return false;
            }

            if (parameter.ValueAsObject.ToString() == "0")
            {
                return true;
            }

            return false;
        }

        public static string FormatSlopeAsRatio(double dzOverDx)
        {
            // percent slope means rise/run = percent/100
            double run = Math.Abs(100.0 / dzOverDx);   // convert to 1 : X form

            return $"1 : {run:F2}";
        }


        private static string Fmt(Dictionary<string, double> dict, string key)
     => dict.TryGetValue(key, out var v) ? v.ToString("F4") : "n/a";

        private static Dictionary<string, double> CollectDoubleParamsFromAppliedSubassemblies(
            AppliedAssembly aa,
            params string[] paramKeys)
        {
            var found = new Dictionary<string, double>(StringComparer.Ordinal);

            var subs = aa.GetAppliedSubassemblies();
            foreach (AppliedSubassembly sub in subs)
            {
                foreach (var key in paramKeys)
                {
                    if (found.ContainsKey(key)) continue;

                    // Contains(...) + GetParameter<T>(...)
                    if (!sub.Contains(key)) continue;

                    try
                    {
                        var p = sub.GetParameter<double>(key);
                        found[key] = p.Value;
                    }
                    catch
                    {
                        // Parameter exists but not double (or type mismatch). Ignore here.
                    }
                }

                if (found.Count == paramKeys.Length) break;
            }

            return found;
        }


        private static double? TryGetSlopePercentFromLinkToken(AppliedSubassembly aa, string token)
        {
            // AppliedAssembly.Links are CalculatedLink objects in most corridor contexts.
            // We’ll avoid hard-typing CalculatedLink members that vary by version and instead
            // probe common properties defensively.
            foreach (var linkObj in aa.Links)
            {
                if (!linkObj.CorridorCodes.Contains(token))
                {
                    continue;
                }

                if (linkObj.CalculatedPoints.Count < 2)
                {
                    continue;
                }

                Point3d p1 = linkObj.CalculatedPoints[0].XYZ;
                Point3d p2 = linkObj.CalculatedPoints[1].XYZ;

                // Compute slope from its endpoints if we can get them

                var dx = p2.X - p1.X;
                var dy = p2.Y - p1.Y;
                var dz = p2.Z - p1.Z;

                var dxy = Math.Sqrt(dx * dx + dy * dy);
                if (dxy <= 1e-9) return null;

                // percent grade
                return (dz / dxy) * 100.0;
            }

            return null;
        }

        private static bool LinkHasToken(object link, string token)
        {
            // Try: Name, Code, Codes (string or IEnumerable<string>)
            bool MatchString(object v)
                => v is string s && s.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;

            var t = link.GetType();

            var nameProp = t.GetProperty("Name");
            if (nameProp != null && MatchString(nameProp.GetValue(link))) return true;

            var codeProp = t.GetProperty("Code");
            if (codeProp != null && MatchString(codeProp.GetValue(link))) return true;

            var codesProp = t.GetProperty("Codes");
            if (codesProp != null)
            {
                var codesVal = codesProp.GetValue(link);
                if (codesVal is IEnumerable<string> codes && codes.Any(c => c.Equals(token, StringComparison.OrdinalIgnoreCase)))
                    return true;

                if (MatchString(codesVal)) return true;
            }

            return false;
        }

        private static bool TryGetLinkEndPoints(object link, out Point3d p1, out Point3d p2)
        {
            p1 = default;
            p2 = default;

            var t = link.GetType();

            // Common patterns seen in corridor calculated objects:
            // - StartPoint / EndPoint (Point3d)
            // - StartPointLocation / EndPointLocation (Point3d)
            // - Point1 / Point2 (Point3d)
            // - StartPointIndex / EndPointIndex referencing aa.Points (not handled here)
            Point3d? TryGetPoint(string propName)
            {
                var pi = t.GetProperty(propName);
                if (pi == null) return null;

                var val = pi.GetValue(link);
                if (val is Point3d p) return p;

                return null;
            }

            var a = TryGetPoint("StartPoint") ?? TryGetPoint("StartPointLocation") ?? TryGetPoint("Point1");
            var b = TryGetPoint("EndPoint") ?? TryGetPoint("EndPointLocation") ?? TryGetPoint("Point2");

            if (a.HasValue && b.HasValue)
            {
                p1 = a.Value;
                p2 = b.Value;
                return true;
            }

            return false;
        }
    }
}
