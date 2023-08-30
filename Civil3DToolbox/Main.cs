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
using System.Xml.Linq;
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
            try
            {
                Document document = null;

                string directory = FolderUtils.GetFolderPathExtendedWindow(Environment.SpecialFolder.Desktop);

                if (!Directory.Exists(directory))
                {
                    MessageBox.Show("No such directory");
                    return;
                }

                string[] dwgFilePaths = Directory.GetFiles(directory, "*.dwg", SearchOption.AllDirectories);

                foreach (string dwgFilePath in dwgFilePaths)
                {

                    document = AutocadDocumentService.DocumentManager.Open(dwgFilePath, forReadOnly: false);
                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument = document;

                    using (document.LockDocument())
                    {
                        using (Transaction transaction = document.TransactionManager.StartTransaction())
                        {
                            ObjectIdCollection xrefIdCollection = new ObjectIdCollection();

                            using (XrefGraph xrefGraph = document.Database.GetHostDwgXrefGraph(false))
                            {
                                int numOfNodes = xrefGraph.NumNodes;

                                for (int nodeIndex = 0; nodeIndex < xrefGraph.NumNodes; nodeIndex++)
                                {
                                    XrefGraphNode xNode = xrefGraph.GetXrefNode(nodeIndex);

                                    if (!xNode.Database.Filename.Equals(document.Database.Filename))
                                    {
                                        if (xNode.XrefStatus == XrefStatus.Resolved)
                                        {
                                            xrefIdCollection.Add(xNode.BlockTableRecordId);
                                        }
                                    }
                                }
                            }

                            if (xrefIdCollection.Count != 0)
                            {
                                document.Database.BindXrefs(xrefIdCollection, true);

                                foreach (ObjectId xrefId in xrefIdCollection)
                                {
                                    document.Database.DetachXref(xrefId);
                                }
                            }

                            dynamic acadDoc = document.GetAcadDocument();
                            acadDoc.Save();
                            transaction.Commit();
                        }

                    }
                }
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Error");
            }
        }
    }
}
