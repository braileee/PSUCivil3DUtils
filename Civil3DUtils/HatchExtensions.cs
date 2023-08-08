using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DUtils
{
    public static class HatchExtensions
    {
        public static double GetArea(this Hatch pHatch)
        {
            double area = 0;
            try
            {
                area = pHatch.Area;
            }
            catch
            {
                int nLoop = pHatch.NumberOfLoops;
                int loopType;
                for (int i = 0; i < nLoop; i++)
                {
                    double looparea = 0;
                    loopType = (int)pHatch.LoopTypeAt(i);
                    if ((loopType & (int)HatchLoopTypes.Polyline) > 0)
                    {
                        HatchLoop hatchLoop = pHatch.GetLoopAt(i);
                        BulgeVertexCollection bulgeVertex = hatchLoop.Polyline;
                        using (Polyline pPoly = new Polyline(bulgeVertex.Count))
                        {
                            for (int j = 0; j < bulgeVertex.Count; j++)
                            {
                                pPoly.AddVertexAt(j, bulgeVertex[j].Vertex, bulgeVertex[j].Bulge, 0, 0);
                            }
                            pPoly.Closed = (loopType & (int)HatchLoopTypes.NotClosed) == 0;
                            looparea = pPoly.Area;
                            if ((loopType & (int)HatchLoopTypes.External) > 0)
                                area += Math.Abs(looparea);
                            else
                                area -= Math.Abs(looparea);
                        }
                    }
                    else
                    {
                        HatchLoop hatchLoop = pHatch.GetLoopAt(i);
                        Curve2d[] cur2ds = new Curve2d[hatchLoop.Curves.Count];
                        hatchLoop.Curves.CopyTo(cur2ds, 0);
                        using (CompositeCurve2d compCurve = new CompositeCurve2d(cur2ds))
                        {
                            Interval interval = compCurve.GetInterval();
                            double dMin = interval.GetBounds()[0], dMax = interval.GetBounds()[1];
                            if (Math.Abs(dMax - dMin) > 1e-6)
                            {
                                try
                                {
                                    looparea = compCurve.GetArea(dMin, dMax);
                                    if ((loopType & (int)HatchLoopTypes.External) > 0)
                                        area += Math.Abs(looparea);
                                    else
                                        area -= Math.Abs(looparea);
                                }
                                catch
                                {
                                    Point2d[] pts = compCurve.GetSamplePoints((int)1e+6);
                                    int np = pts.Length;
                                    for (int j = 0; j < np; j++)
                                    {
                                        looparea += 0.5 * pts[j].X * (pts[(j + 1) % np].Y - pts[(j + np - 1) % np].Y);
                                    }
                                    if ((loopType & (int)HatchLoopTypes.External) > 0)
                                        area += Math.Abs(looparea);
                                    else
                                        area -= Math.Abs(looparea);
                                }
                            }
                        }
                    }
                }
            }
            return Math.Abs(area);
        }
    }
}
