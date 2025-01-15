using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Snake_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve values from textboxes
            string customVar1 = CustomVar1TextBox.Text;

            // Set environment variables
            Environment.SetEnvironmentVariable("CustomVar1", customVar1);

            // Get the current directory of the launcher
            string launcherDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Define the path to the main WPF application\Snake\bin\Debug\net8.0-windows
            string mainAppPath = System.IO.Path.Combine(launcherDirectory, "..", "..", "..", "..", "Snake", "bin", "Debug", "net8.0-windows", "Snake.exe");
            mainAppPath = System.IO.Path.GetFullPath(mainAppPath);

            // Launch the main WPF application
            ProcessStartInfo startInfo = new ProcessStartInfo(mainAppPath)
            {
                UseShellExecute = false, // Ensures it inherits environment variables
                Arguments = $"--var1 {customVar1}"
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch the application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}