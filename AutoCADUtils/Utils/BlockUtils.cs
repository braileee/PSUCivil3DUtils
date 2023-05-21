using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Linq;

namespace AutoCADUtils.Utils
{
    public static class BlockUtils
    {
        public static void TryLoadBlocksFromAnotherFile(string filePath, params string[] blockNames)
        {
            DocumentCollection documentCollection = Application.DocumentManager;
            Document currentDocument = documentCollection.MdiActiveDocument;

            Editor ed = currentDocument.Editor;
            Database destDb = documentCollection.MdiActiveDocument.Database;
            Database sourceDb = new Database(false, true);

            try
            {
                // Read the DWG into a side database
                sourceDb.ReadDwgFile(filePath, System.IO.FileShare.Read, true, "");

                // Create a variable to store the list of block identifiers
                ObjectIdCollection blockIds = new ObjectIdCollection();

                Autodesk.AutoCAD.DatabaseServices.TransactionManager transactionManager = sourceDb.TransactionManager;

                using (Transaction myT = transactionManager.StartTransaction())
                {
                    // Open the block table
                    BlockTable bt = (BlockTable)transactionManager.GetObject(sourceDb.BlockTableId, OpenMode.ForRead, false);

                    // Check each block in the block table
                    foreach (ObjectId btrId in bt)
                    {
                        BlockTableRecord blockTableRecord = (BlockTableRecord)transactionManager.GetObject(btrId, OpenMode.ForRead, false);

                        // Only add named & non-layout blocks to the copy list
                        if (!blockTableRecord.IsLayout)
                        {
                            if (blockNames.Contains(blockTableRecord.Name))
                            {
                                blockIds.Add(btrId);
                            }
                        }

                        blockTableRecord.Dispose();
                    }
                }
                // Copy blocks from source to destination database

                using (DocumentLock documentLock = currentDocument.LockDocument())
                {
                    IdMapping mapping = new IdMapping();
                    sourceDb.WblockCloneObjects(blockIds, destDb.BlockTableId, mapping, DuplicateRecordCloning.Replace, false);
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                ed.WriteMessage("\nError during copy: " + ex.Message);
            }
            sourceDb.Dispose();
        }
    }
}
