using System.Diagnostics;
using System.Runtime.CompilerServices;
using WixSharp;
using WixToolset.Dtf.WindowsInstaller;
using File = WixSharp.File;
using System.Reflection;
using Asm = System.Reflection.Assembly;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[assembly: InternalsVisibleTo(assemblyName: "PSUCivil3DUtils.Setup.aot")] // assembly name + '.aot suffix

public class Program
{
    public static string ApplicationPluginsFolder { get; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Autodesk\\ApplicationPlugins\\";
    public static string CurrentAppFolderPath
    {
        get
        {
            return Path.Combine(ApplicationPluginsFolder, BundleName);
        }
    }

    public static string Content2018FolderPath
    {
        get
        {
            return Path.Combine(ApplicationPluginsFolder, BundleName, "Contents", "2018");
        }
    }

    public static string Content2025FolderPath
    {
        get
        {
            return Path.Combine(ApplicationPluginsFolder, BundleName, "Contents", "2025");
        }
    }

    public static string StartupAppDllPath
    {
        get
        {
            return Path.Combine(Content2018FolderPath, MainDllName);
        }
    }

    public static string MainDllName
    {
        get
        {
            return "HASSAutoCadPlugin.App.dll";
        }
    }

    public static string CuixFileName
    {
        get
        {
            return "HASSCADribbon.cuix";
        }
    }

    public static string CuixFolderPath
    {
        get
        {
            return Path.Combine(CurrentAppFolderPath, "UI");
        }
    }

    public static string CuixFilePath
    {
        get
        {
            return Path.Combine(CuixFolderPath, CuixFileName);
        }
    }

    public static string PackageContentsFileName
    {
        get
        {
            return "PackageContents.xml";
        }
    }

    /// <summary>Set Autocad bundle folder name.</summary>
    public static string BundleName { get; } = "PSU.Civil3D.Utils.bundle";

    /// <summary>Set product guid.</summary>
    public static Guid ProductGuid { get; } = new Guid("1B55B619-E05A-4AAC-858C-92854C30E26B");

    /// <summary>Set product name.</summary>
    public static string ProductName { get; } = "PSU Civil 3D Utils for AutoCAD 2019-2024";

    /// <summary>Set installer title.</summary>
    public static string InstallerTitle { get; } = "PSU Civil 3D Utils for AutoCAD 2019-2024 Installer";

    /// <summary>Set installer output name template.</summary>
    public static string InstallerName { get; } = "PSU-Civil3D-Utils-AutoCAD-2019-2024-Win-Installer";

    /// <summary>Produces MSI with digital signature.</summary>
    private static void Main()
    {
        string filepath = BuildMsi();
    }

    /// <summary>Produces msi.</summary>
    /// <returns>Path to MSI file</returns>
    private static string BuildMsi()
    {
        Dir[] dirs = GetInstallInvetory();

        Project project = new Project()
        {
            Name = ProductName,
            Dirs = dirs,
            Version = GetVersion(),
            GUID = ProductGuid,
            ValidateBackgroundImage = false,
            SourceBaseDir = CurrentAppFolderPath,
            OutDir = "Build"
        };

        project.OutFileName = $"{InstallerName}-{project.Version}";

        project.MajorUpgradeStrategy = new MajorUpgradeStrategy
        {
            UpgradeVersions = VersionRange.ThisAndOlder,
            PreventDowngradingVersions = VersionRange.NewerThanThis,
            NewerProductInstalledErrorMessage = "Newer version already installed",
        };

        return Compiler.BuildMsi(project);
    }

    /// <summary>Gets install directories with files to be installed.</summary>
    /// <returns>Array of install directories</returns>
    private static Dir[] GetInstallInvetory()
    {
        List<Dir> directories = new List<Dir>();

        List<File> bundleFiles = new List<File>
            {
                new File(PackageContentsFileName)
            };

        List<File> cuixFiles = new List<File>
            {
                new File(Path.Combine("UI", CuixFileName))
            };

        // 2018 content
        const string versionFolderName2018 = "2018";

        List<File> contentFiles2018 = new List<File>
            {
                new File(Path.Combine("Contents", versionFolderName2018, MainDllName)),
                new File(Path.Combine("Contents", versionFolderName2018,"Newtonsoft.Json.dll")),
            };

        Dir bundleDirectory = new Dir(CurrentAppFolderPath, bundleFiles.ToArray());
        Dir cuixDirectory = new Dir(CuixFolderPath, cuixFiles.ToArray());
        Dir contentsDirectory2018 = new Dir(Content2018FolderPath, contentFiles2018.ToArray());

        // 2025 content
        const string versionFolderName2025 = "2025";

        List<File> contentFiles2025 = new List<File>
            {
                new File(Path.Combine("Contents", versionFolderName2025, MainDllName)),
                new File(Path.Combine("Contents", versionFolderName2025,"Newtonsoft.Json.dll")),
            };

        Dir contentsDirectory2025 = new Dir(Content2025FolderPath, contentFiles2025.ToArray());

        directories.Add(cuixDirectory);
        directories.Add(bundleDirectory);
        directories.Add(contentsDirectory2018);
        directories.Add(contentsDirectory2025);

        return directories.ToArray();
    }

    /// <summary>During generating installer it will retrieve version from assembly.</summary>
    /// <returns>Version</returns>
    private static Version GetVersion()
    {
        Asm assembly = Asm.LoadFrom(StartupAppDllPath);

        return assembly.GetName().Version;
    }

    /// <summary>Gets install directory path.</summary>
    /// <param name="session">Session object</param>
    /// <returns>Path to install directory</returns>
    private static string GetInstallDirectory(Session session = null)
    {
        string installDirectory = "NotFound";

        if (System.IO.Directory.Exists(ApplicationPluginsFolder))
        {
            installDirectory = Path.Combine(ApplicationPluginsFolder, BundleName);
        }
        else
        {
            LogMessage(session, "AutoCAD installation not found");
        }

        return installDirectory;
    }

    /// <summary>Prevents install if AutoCAD not present in os.</summary>
    /// <param name="e">Args</param>
    private static void OnStart(SetupEventArgs e)
    {
        string location = GetInstallDirectory();

        if (location == "NotFound")
        {
            e.Result = ActionResult.Failure;

            if (!e.IsUISupressed)
            {
                MessageBox.Show(e.Session.GetMainWindow(), "Setup had to be aborted because AutoCAD 3d installation was not found.");
            }
        }

        List<Process> processes = Process.GetProcesses().ToList();

        if (processes.Any(p => p.ProcessName == "acad"))
        {
            e.Result = ActionResult.Failure;

            if (!e.IsUISupressed)
            {
                MessageBox.Show(e.Session.GetMainWindow(), "Setup had to be aborted because AutoCAD is running.");
            }
        }
    }

    /// <summary>Logs messages during OnStart event.</summary>
    /// <param name="session">Install session</param>
    /// <param name="message">Message</param>
    private static void LogMessage(Session session, string message)
    {
        if (session != null)
        {
            session.Log(message);
        }
    }
}