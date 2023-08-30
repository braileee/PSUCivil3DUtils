using AutoCADUtils;
using AutoCADUtils.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Civil3DToolbox
{
    public class Main
    {
        [CommandMethod("PSV", "ReverseTextElevation", CommandFlags.Modal)]
        public static void ReverseTextElevation()
        {
            List<DBText> texts = SelectionUtils.GetElements<DBText>("Select text to reverse elevation");

            if (texts.Count == 0)
            {
                AutocadDocumentService.Editor.WriteMessage($"No text was selected");
                return;
            }

            AutocadDocumentService.Editor.WriteMessage($"Text entities selected: {texts.Count}");

            using (AutocadDocumentService.LockActiveDocument())
            {
                using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                {
                    foreach (DBText text in texts)
                    {
                        DBText textOpened = transaction.GetObject(text.Id, OpenMode.ForWrite, false, true) as DBText;
                        textOpened.Position = new Point3d(textOpened.Position.X, textOpened.Position.Y, -textOpened.Position.Z);
                    }

                    transaction.Commit();
                }
            }
        }

        [CommandMethod("PSV", "BindXrefsForDwgsInFolder", CommandFlags.Modal)]
        public static void BindXrefsForDwgsInFolder()
        {
            string directory = FolderUtils.GetFolderPathExtendedWindow(Environment.SpecialFolder.Desktop);

            if (!Directory.Exists(directory))
            {
                MessageBox.Show("No such directory");
                return;
            }

            string[] dwgFilePaths = Directory.GetFiles(directory, "*.dwg", SearchOption.AllDirectories);

            foreach (string dwgFilePath in dwgFilePaths)
            {
                Document document = AutocadDocumentService.DocumentManager.Open(dwgFilePath);
                AutocadDocumentService.DocumentManager.MdiActiveDocument = document;

                using (AutocadDocumentService.LockActiveDocument())
                {
                    using (Transaction transaction = AutocadDocumentService.TransactionManager.StartTransaction())
                    {
                        ObjectIdCollection xrefCollection = new ObjectIdCollection();

                        using (XrefGraph xrefGraph = AutocadDocumentService.Database.GetHostDwgXrefGraph(false))
                        {
                            int numOfNodes = xrefGraph.NumNodes;

                            for (int nodeIndex = 0; nodeIndex < xrefGraph.NumNodes; nodeIndex++)
                            {
                                XrefGraphNode xNode = xrefGraph.GetXrefNode(nodeIndex);

                                if (!xNode.Database.Filename.Equals(AutocadDocumentService.Database.Filename))
                                {
                                    if (xNode.XrefStatus == XrefStatus.Resolved)
                                    {
                                        xrefCollection.Add(xNode.BlockTableRecordId);
                                    }
                                }
                            }
                        }

                        if (xrefCollection.Count != 0)
                        {
                            AutocadDocumentService.Database.BindXrefs(xrefCollection, true);
                        }

                        transaction.Commit();
                    }
                }
            }
        }
    }
}
