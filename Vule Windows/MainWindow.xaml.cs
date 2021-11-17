using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Vule_Windows
{ 
    public partial class MainWindow : Window
    {
        private bool InExitMenu = false;
        private System.Windows.Forms.NotifyIcon Notification;
        private readonly string StartUpRegistry = "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        private readonly string RegistryName = "Vule Windows";
        private string RegistryText = "";
        public MainWindow()
        {
            InitializeComponent();
            
            RegistryText = String.Format("\"{0}\" -silent", System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Icon = LoopThread.ToImageSource( Properties.Resources.favicon );
            Notification = new System.Windows.Forms.NotifyIcon
            {
                Visible = true,
                Text = "Vule Window Mover App",
                Icon = Properties.Resources.favicon
            };
            Notification.MouseClick += (s, args) => 
            {
                if (args.Button == System.Windows.Forms.MouseButtons.Left && Visibility == Visibility.Hidden)
                    Visibility = Visibility.Visible;

                else if (args.Button == System.Windows.Forms.MouseButtons.Right)
                    CloseApplication();
            };
            LoopThread.StartThread();
            exitBorder.Visibility = Visibility.Hidden;
            Closing += (s, args) => 
            {
                if (!InExitMenu)
                {
                    InExitMenu = true;
                    args.Cancel = true;
                    exitBorder.Visibility = Visibility.Visible;
                }
                else
                    CloseApplication();
            };
            RunAtBootCheckBox.IsChecked = DoesApplicationRunOnStartup();
            if (App.Args[0] == "-silent") HideApplication();
        }

        private void CloseApplication()
        {
            LoopThread.StopThread();
            Notification.Visible = false;
            Notification.Dispose();
            Environment.Exit(0);
        }
        private void HideApplication()
        {
            Hide();
            exitBorder.Visibility = Visibility.Hidden;
            InExitMenu = false;
            if (!Notification.Visible) Notification.Visible = true;
        }
        private void NoClicked(object sender, RoutedEventArgs e) => CloseApplication();
        private void YesClicked(object sender, RoutedEventArgs e) => HideApplication();
        private void StartupBoxValueChanged(object sender, RoutedEventArgs e) => Registry.SetValue(StartUpRegistry, RegistryName, RunAtBootCheckBox.IsChecked.Value ? RegistryText : "");
        private bool DoesApplicationRunOnStartup()
        {
            object data = Registry.GetValue(StartUpRegistry, RegistryName, null); // Getting the registry value
            if(data != null)
            {
                try {
                    string text = data as string;
                    if (text != "")
                    {
                        if (text != RegistryText)
                            Registry.SetValue(StartUpRegistry, RegistryName, RegistryText);

                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        private void ExitEllipseClick(object sender, MouseButtonEventArgs e) => Close();
        private void MinimizeEllipseClick(object sender, MouseButtonEventArgs e) => WindowState = WindowState.Minimized;
        private void HeaderMouseDown(object sender, MouseButtonEventArgs e) => DragMove();
    }
}
