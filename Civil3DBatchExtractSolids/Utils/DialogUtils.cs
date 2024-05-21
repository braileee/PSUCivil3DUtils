using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Civil3DBatchExtractSolids.Utils
{
    public static class DialogUtils
    {
        public static string GetFilePath(string initialDirectory = "c:\\", string dialogFilter = "dwg files (*.dwg)|*.dwg|All files (*.*)|*.*")
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = initialDirectory,
                Filter = dialogFilter,
                FilterIndex = 0,
                RestoreDirectory = true
            };
            string selectedFileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                selectedFileName = openFileDialog1.FileName;
            }
            return selectedFileName;
        }

        public static string SaveFileToFolder(Environment.SpecialFolder initDirectory, string defaultExtension)
        {
            var saveDialog = new SaveFileDialog
            {
                OverwritePrompt = true,
                InitialDirectory = Environment.GetFolderPath(initDirectory),
                Filter = "All files (*.*)|*.*",
                //folderDialog.DefaultExt = ".txt";
                DefaultExt = defaultExtension
            };

            string selectedFolderPath = string.Empty;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFolderPath = saveDialog.FileName;
            }
            return selectedFolderPath;
        }
    }
}
