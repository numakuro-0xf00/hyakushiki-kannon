using System.Windows;

namespace hyakushiki_kannon
{
    /// <summary>
    /// Small control panel for the resident GridMouse app: shows the key bindings and offers a
    /// way to quit. Closing it shuts the application down (and disposes the hotkey/overlay).
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnExitClick(object sender, RoutedEventArgs e) => Close();
    }
}
