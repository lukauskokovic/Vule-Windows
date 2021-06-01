using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Vule_Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] Args;
        void OnStartup(object sender, StartupEventArgs e) => Args = e.Args.Length == 0 ? new string[0] : e.Args;
    }
}
