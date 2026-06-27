using System.Windows;
using hyakushiki_kannon.GridUi;

namespace hyakushiki_kannon
{
    /// <summary>
    /// Application entry point. Starts the resident <see cref="GridModeController"/> (global
    /// hotkey + grid overlay) on launch and tears it down on exit. The visible
    /// <see cref="MainWindow"/> serves only as a small control panel / quit affordance.
    /// </summary>
    public partial class App : Application
    {
        private GridModeController? _controller;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                _controller = new GridModeController();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"GridMouse failed to start:\n\n{ex.Message}",
                    "GridMouse", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _controller?.Dispose();
            _controller = null;
            base.OnExit(e);
        }
    }
}
