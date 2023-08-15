using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoCADUtils.Utils
{
    public class FolderUtils
    {
        public static string GetFolderPath(Environment.SpecialFolder initDirectory)
        {
            var folderDialog = new FolderBrowserDialog();
            folderDialog.RootFolder = initDirectory;
            string selectedFolderPath = string.Empty;
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFolderPath = folderDialog.SelectedPath;
            }
            return selectedFolderPath;
        }

        public static string GetFolderPathExtendedWindow(Environment.SpecialFolder initDirectory)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.OverwritePrompt = true;
            saveDialog.InitialDirectory = Environment.GetFolderPath(initDirectory);
            saveDialog.Filter = "All files (*.*)|*.*";
            saveDialog.FileName = "Selected a folder";

            string selectedFolderPath = string.Empty;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFolderPath = Path.GetDirectoryName(saveDialog.FileName);
            }
            return selectedFolderPath;
        }

        public static string GetFolderPathExtendedWindow(string path)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.OverwritePrompt = true;
            saveDialog.InitialDirectory = path;
            saveDialog.Filter = "All files (*.*)|*.*";
            saveDialog.FileName = "Selected a folder";

            string selectedFolderPath = string.Empty;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFolderPath = Path.GetDirectoryName(saveDialog.FileName);
            }
            return selectedFolderPath;
        }

        public static bool CreateDirectoryIfNotExists(string path)
        {

            bool exists = Directory.Exists(path);

            if (!exists)
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
