using AutoCADUtils;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
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
            SurfaceStyleCollection surfaceStyleIds = CivilDocumentService.CivilDocument.Styles.SurfaceStyles;

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

        public static TinSurface PromptTinSurface(OpenMode openMode, string promptMessage = "Select a surface")
        {
            TinSurface tinSurface = null;

            using (AutocadDocumentService.LockActiveDocument())
            {
                using (Transaction ts = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    var opt = new PromptEntityOptions($"{Environment.NewLine}{promptMessage}{Environment.NewLine}");
                    opt.Message = promptMessage;
                    var objectId = AutocadDocumentService.Editor.GetEntity(opt).ObjectId;

                    if (objectId.IsNull)
                    {
                        return null;
                    }

                    tinSurface = ts.GetObject(objectId, openMode, false, true) as TinSurface;
                    ts.Commit();
                }
            }

            return tinSurface;
        }
    }
}
