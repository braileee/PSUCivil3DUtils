using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Civil3DDataExport.Utils
{
    public static class DialogUtils
    {
        public static string GetFilePath(string initialDirectory = "c:\\", string dialogFilter = "dwg files (*.dwg)|*.dwg|All files (*.*)|*.*")
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = initialDirectory;
            openFileDialog1.Filter = dialogFilter;
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;
            string selectedFileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                selectedFileName = openFileDialog1.FileName;
            }
            return selectedFileName;
        }

        public static string SaveFileToFolder(Environment.SpecialFolder initDirectory, string defaultExtension)
        {
            //EXAMPLE
            //OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //openFileDialog1.InitialDirectory = "c:\\";
            //openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            //openFileDialog1.FilterIndex = 2;
            //openFileDialog1.RestoreDirectory = true;
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
