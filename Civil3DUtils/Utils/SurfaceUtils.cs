using AutoCADUtils;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DUtils.Utils
{
    public static class SurfaceUtils
    {
        public static List<SurfaceStyle> GetSurfaceStyles(OpenMode openMode)
        {
            SurfaceStyleCollection surfaceStyleIds = CivilDocumentUtils.CivilDocument.Styles.SurfaceStyles;

            List<SurfaceStyle> surfaceStyles = new List<SurfaceStyle>();

            using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
            {
                foreach (ObjectId surfaceStyleId in surfaceStyleIds)
                {
                    SurfaceStyle surfaceStyle = transaction.GetObject(surfaceStyleId, openMode, false, true) as SurfaceStyle;
                    surfaceStyles.Add(surfaceStyle);
                }

                transaction.Commit();
            }

            return surfaceStyles;
        }
    }
}
