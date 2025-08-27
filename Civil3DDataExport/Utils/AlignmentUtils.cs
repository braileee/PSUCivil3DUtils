using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using System.Collections.Generic;
using Civil = Autodesk.Civil.DatabaseServices;

namespace Civil3DDataExport.Utils
{
    public static class AlignmentUtils
    {

        public static List<Civil.Alignment> GetAlignments(OpenMode openMode, Transaction transaction)
        {

            List<Civil.Alignment> oAlignmentList = new List<Civil.Alignment>();
            ObjectIdCollection oAlignmentIdCollection = App.ActiveDocumentCivil.GetAlignmentIds();

            foreach (ObjectId oAlignmentId in oAlignmentIdCollection)
            {
                Civil.Alignment oAlignment = transaction.GetObject(oAlignmentId, openMode, false, true) as Civil.Alignment;
                oAlignmentList.Add(oAlignment);
            }

            return oAlignmentList;
        }

        public static List<Civil.Profile> GetProfilesOfAlignment(Civil.Alignment oAlignment, Transaction ts, OpenMode openMode)
        {
            var oProfileIds = oAlignment.GetProfileIds();
            
            if (oProfileIds == null || oProfileIds.Count == 0)
            {
                return new List<Civil.Profile>();
            }

            var oProfiles = new List<Civil.Profile>();

            foreach (ObjectId oProfileId in oProfileIds)
            {
                Civil.Profile oProfile = ts.GetObject(oProfileId, openMode, false, true) as Civil.Profile;
                oProfiles.Add(oProfile);
            }
            return oProfiles;
        }

    }
}
