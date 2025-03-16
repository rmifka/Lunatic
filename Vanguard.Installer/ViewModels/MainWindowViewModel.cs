using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Vanguard.Bootstrapper;
using Vanguard.Installer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string gamePath;

    [ObservableProperty]
    private string log = "> Ready.";

    [ObservableProperty]
    private string status = "Unknown";

    [RelayCommand]
    private async Task Browse()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Game Folder"
        };
        var result = await dialog.ShowAsync(new Window()); // Note: You'll inject the window properly in real scenario
        if (!string.IsNullOrWhiteSpace(result))
        {
            GamePath = result;
            AppendLog($"Selected game path: {result}");
            CheckIfVanguardInstalled(result);
        }
    }

    [RelayCommand]
    public void InstallCommand()
    {
        var path = GamePath;
        try
        {
            var dataFolders = LocateDataFolder(path);
            if (dataFolders.Length == 0)
            {
                Status = "Not a Unity game folder";
                return;
            }

            var managedPath = Path.Combine(dataFolders[0], "Managed");
            var assemblyPath = Path.Combine(managedPath, "UnityEngine.CoreModule.dll");
            var backupPath = Path.Combine(managedPath, "UnityEngine.CoreModule.original.dll");
            var bootstrapperPath = Path.Combine(managedPath, "Vanguard.Bootstrapper.dll");
            var loaderPath = Path.Combine(managedPath, "Vanguard.Loader.dll");
            var publicPath = Path.Combine(managedPath, "Vanguard.Public.dll");

            BackupOriginalAssembly(assemblyPath, backupPath);
            CopyBootstrapper(bootstrapperPath);
            CopyLoader(loaderPath);
            CopyPublic(publicPath);

            InjectBootstrapper(managedPath, assemblyPath);

            CopyExternalLibraries();

            Status = "Vanguard successfully installed!";
        }
        catch (Exception ex)
        {
            Status = $"Error during installation: {ex.Message}";
            AppendLog($" Error > {ex.Message}");
        }
    }

    private void CopyExternalLibraries()
    {
        var externalLibraries = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExternalLibraries");

        if (!Directory.Exists(externalLibraries))
        {
            AppendLog("External libraries not found.");
            return;
        }

        var libraryDirectory = Path.Combine(GamePath, "Libraries");
        if (!Directory.Exists(libraryDirectory))
        {
            Directory.CreateDirectory(libraryDirectory);
        }

        var files = Directory.GetFiles(externalLibraries, "*.dll");
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(libraryDirectory, fileName);
            File.Copy(file, destFile, true);
            AppendLog($"Copied {fileName} to Libraries folder.");
        }
    }

    private string[] LocateDataFolder(string path)
    {
        return Directory.GetDirectories(path, "*_Data", SearchOption.TopDirectoryOnly);
    }

    private void BackupOriginalAssembly(string assemblyPath, string backupPath)
    {
        if (!File.Exists(backupPath))
        {
            File.Copy(assemblyPath, backupPath);
            Status = "Backup created.";
        }
        else
        {
            Status = "Backup already exists.";
        }
    }

    private void CopyBootstrapper(string bootstrapperPath)
    {
        var sourceBootstrapper = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Vanguard.Bootstrapper.dll");
        File.Copy(sourceBootstrapper, bootstrapperPath, true);
        Status = "Bootstrapper copied.";
    }

    private void CopyLoader(string bootstrapperPath)
    {
        var sourceBootstrapper = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Vanguard.Loader.dll");
        File.Copy(sourceBootstrapper, bootstrapperPath, true);
        Status = "Bootstrapper copied.";
    }

    private void CopyPublic(string bootstrapperPath)
    {
        var sourceBootstrapper = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Vanguard.Public.dll");
        File.Copy(sourceBootstrapper, bootstrapperPath, true);
        Status = "Bootstrapper copied.";
    }

    public void InstallMods(string[] dllFiles)
    {
        var modsFolder = Path.Combine(GamePath, "Mods");
        if (!Directory.Exists(modsFolder))
        {
            Directory.CreateDirectory(modsFolder);
        }

        AppendLog($"Copying mods to Mods folder... {modsFolder}");

        foreach (var file in dllFiles)
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(modsFolder, fileName);
            AppendLog($"Copying {fileName} to Mods folder... + {destFile}");

            File.Copy(file, destFile, true);
            AppendLog($"Copied {fileName} to Mods folder.");
        }
    }

    private void InjectBootstrapper(string managedPath, string assemblyPath)
    {
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managedPath);

        var readerParams = new ReaderParameters { AssemblyResolver = resolver, ReadWrite = true };
        using var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, readerParams);

        var module = assemblyDefinition.MainModule;

        var monoBehaviour = module.Types.FirstOrDefault(t => t.Name == "MonoBehaviour");
        if (monoBehaviour == null)
        {
            throw new Exception("MonoBehaviour type not found in UnityEngine.CoreModule.dll");
        }

        var awakeMethod = monoBehaviour.Methods.FirstOrDefault(m => m.Name == "Awake");

        if (awakeMethod != null)
        {
            monoBehaviour.Methods.Remove(awakeMethod);
        }

        if (awakeMethod == null)
        {
            awakeMethod = new MethodDefinition("Awake",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                module.ImportReference(typeof(void)));
            monoBehaviour.Methods.Add(awakeMethod);

            var il = awakeMethod.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ret));
        }

        var initMethod = module.ImportReference(typeof(Bootstrapper)
            .GetMethod(nameof(Bootstrapper.Init)));

        var processor = awakeMethod.Body.GetILProcessor();
        var firstInstruction = awakeMethod.Body.Instructions.First();

        processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Call, initMethod));

        assemblyDefinition.Write(assemblyPath + ".new");
        var backupPath = Path.Combine(managedPath, "UnityEngine.CoreModule.original.dll");
        assemblyDefinition.Dispose();

        if (File.Exists(backupPath))
        {
            AppendLog("Deleting original assembly...");
            File.Delete(assemblyPath);
            AppendLog("Renaming new assembly...");
            File.Move(assemblyPath + ".new", assemblyPath);
        }

        AppendLog("Injection completed.");
    }


    [RelayCommand]
    private void Uninstall()
    {
        if (!Directory.Exists(GamePath))
        {
            AppendLog("Invalid game path. Please select a valid folder.");
            return;
        }

        AppendLog("Starting uninstallation...");
        try
        {
            var dataFolders = LocateDataFolder(GamePath);
            if (dataFolders.Length == 0)
            {
                AppendLog("Not a Unity game folder");
                return;
            }

            var managedPath = Path.Combine(dataFolders[0], "Managed");
            var assemblyPath = Path.Combine(managedPath, "UnityEngine.CoreModule.dll");
            var backupPath = Path.Combine(managedPath, "UnityEngine.CoreModule.original.dll");
            var bootstrapperPath = Path.Combine(managedPath, "Vanguard.Bootstrapper.dll");
            var loaderPath = Path.Combine(managedPath, "Vanguard.Loader.dll");
            var publicPath = Path.Combine(managedPath, "Vanguard.Public.dll");

            if (File.Exists(backupPath))
            {
                File.Delete(assemblyPath);
                File.Move(backupPath, assemblyPath);
                File.Delete(bootstrapperPath);
                File.Delete(loaderPath);
                File.Delete(publicPath);
                AppendLog("Uninstallation completed.");
                Status = "Uninstalled";
            }
            else
            {
                AppendLog("Backup not found. Nothing to uninstall.");
            }
        }
        catch (Exception ex)
        {
            AppendLog($"Error during uninstallation: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenGitHub()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/rmifka/Vanguard",
            UseShellExecute = true
        });
    }

    [RelayCommand]
    private void About()
    {
        AppendLog("Vanguard Mod Loader Installer. Made with ❤️ and SukiUI.");
    }

    public void CloseCommand()
    {
        Desktop.TryShutdown();
    }

    private void CheckIfVanguardInstalled(string path)
    {
        var dataFolders = Directory.GetDirectories(path, "*_Data", SearchOption.TopDirectoryOnly);
        if (dataFolders.Length == 0)
        {
            Status = "Not a Unity game folder";
            return;
        }

        var managedPath = Path.Combine(dataFolders[0], "Managed");
        var loaderPath = Path.Combine(managedPath, "Vanguard.Loader.dll");
        var publicPath = Path.Combine(managedPath, "Vanguard.Public.dll");


        Status = File.Exists(loaderPath) && File.Exists(publicPath) ? "Installed" : "Not Installed";
    }

    private void AppendLog(string message)
    {
        Log += $"\n> {DateTime.Now:HH:mm:ss} {message}";
    }
}