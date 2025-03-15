using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vanguard.Installer.ViewModels;

public class ViewModelBase : ObservableObject
{
    protected IClassicDesktopStyleApplicationLifetime Desktop => Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime ??
                                                                 throw new Exception("Desktop is not available");

    protected Window MainWindow => Desktop.MainWindow ?? throw new Exception("MainWindow is not available");
}