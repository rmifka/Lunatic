using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;

namespace Lunatic.Installer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if ((e.Data.GetFiles() ?? Array.Empty<IStorageItem>()).Any())
        {
            var files = e.Data.GetFiles()!.Select(file => file.Path).ToArray();
            var dllFiles = files.Where(file => file.AbsolutePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)).ToArray();

            var viewModel = (MainWindowViewModel)DataContext!;

            viewModel.InstallMods(dllFiles.Select(x => x.AbsolutePath).ToArray());
        }
    }
}