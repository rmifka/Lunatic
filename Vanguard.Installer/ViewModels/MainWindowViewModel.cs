using System.Collections.ObjectModel;
using Vanguard.Installer.Models;

namespace Vanguard.Installer.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<MenuItem> MenuItems { get; } = new ObservableCollection<MenuItem>
        {
            
        };
    }
}