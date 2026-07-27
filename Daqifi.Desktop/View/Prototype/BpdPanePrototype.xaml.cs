using Daqifi.Desktop.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace Daqifi.Desktop.View.Prototype;

public partial class BpdPanePrototype : UserControl
{
    public BpdPanePrototype()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // Mirror the Channels pane: tab-switches trigger Unloaded (disposing the
        // VM) then Loaded when the tab returns. Recreate the VM so a returning
        // tab re-subscribes to device status frames.
        if (DataContext is not BpdPaneViewModel)
        {
            DataContext = new BpdPaneViewModel();
        }
    }

    private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
            DataContext = null;
        }
    }
}
