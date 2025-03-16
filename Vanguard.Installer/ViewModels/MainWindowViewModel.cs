using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Vanguard.Installer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string gamePath;

    [ObservableProperty]
    private string status = "Unknown";

    [ObservableProperty]
    private string log = "> Ready.";

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

            BackupOriginalAssembly(assemblyPath, backupPath);
            CopyBootstrapper(bootstrapperPath);

            InjectBootstrapper(managedPath, assemblyPath);

            Status = "Vanguard successfully installed!";
        }
        catch (Exception ex)
        {
            Status = $"Error during installation: {ex.Message}";
            AppendLog($" Error > {ex.Message}");
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
        File.Copy(sourceBootstrapper, bootstrapperPath, overwrite: true);
        Status = "Bootstrapper copied.";
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
        if (awakeMethod == null)
        {
            awakeMethod = new MethodDefinition("Awake",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                module.ImportReference(typeof(void)));
            monoBehaviour.Methods.Add(awakeMethod);

            var il = awakeMethod.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ret));
        }

        var initMethod = module.ImportReference(typeof(Vanguard.Bootstrapper.Bootstrapper)
            .GetMethod(nameof(Vanguard.Bootstrapper.Bootstrapper.Init)));

        var processor = awakeMethod.Body.GetILProcessor();
        var firstInstruction = awakeMethod.Body.Instructions.First();

        processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Call, initMethod));

        assemblyDefinition.Write(assemblyPath + ".new");
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
            var loaderTarget = Path.Combine(GamePath, "Vanguard.Loader.dll");

            if (File.Exists(loaderTarget))
            {
                File.Delete(loaderTarget);
                AppendLog("Vanguard.Loader.dll removed.");
            }
            else
            {
                AppendLog("Vanguard is not installed in selected path.");
            }

            CheckIfVanguardInstalled(GamePath);
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


        Status = File.Exists(loaderPath) ? "Installed" : "Not Installed";
    }

    private void AppendLog(string message)
    {
        Log += $"\n> {DateTime.Now:HH:mm:ss} {message}";
    }
}