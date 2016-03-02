using System;
using System.Collections.Generic;
using System.Linq;
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
using Stimulsoft.Report;
using Stimulsoft.Report.Wpf;

namespace CustomComponent.Wpf
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            StiOptions.Wpf.CurrentTheme = StiOptions.Wpf.Themes.Office2013Theme;

            InitializeComponent();
            AddCustomComponent();

        }

        private static void AddCustomComponent()
        {
            StiConfig.Load();

            StiOptions.Engine.ReferencedAssemblies
                 = new string[]{
							"System.Dll",
                            "System.Drawing.Dll",
							"System.Windows.Forms.Dll",
							"System.Data.Dll",
							"System.Xml.Dll",
							"Stimulsoft.Base.Dll",
							"Stimulsoft.Report.Dll",

							#region Add reference to your assembly
							"CustomComponent.Wpf.exe"
							#endregion
						};

            StiConfig.Services.Add(new MyCustomComponent());
            StiConfig.Save();


        }

        private void buttonRunDesigner_Click(object sender, RoutedEventArgs e)
        {
            StiReport report = new StiReport();
            report.DesignWithWpf();
        }
    }
}
