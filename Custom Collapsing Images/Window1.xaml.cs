using System;
using System.IO;
using System.Windows;
using Stimulsoft.Report;
using System.Data;

namespace CustomCollapsingImages
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private string path = string.Empty;

        public Window1()
        {
            StiOptions.Wpf.CurrentTheme = StiOptions.Wpf.Themes.Office2013Theme;
            StiOptions.Wpf.Viewer.CollapsingImages.UseCustomCollapsingImages = true;
            StiOptions.Wpf.Viewer.CollapsingImages.CollapsedImagePath = "pack://application:,,,/CustomCollapsingImages;component/Images/Collapsed.png";
            StiOptions.Wpf.Viewer.CollapsingImages.ExpandedImagePath = "pack://application:,,,/CustomCollapsingImages;component/Images/Expanded.png";

            InitializeComponent();
            StiReport report = new StiReport();            

            DataSet ds = new DataSet();
           

            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Stimulsoft\\Stimulsoft Reports");
            bool is64Bit = IntPtr.Size == 8;
            if (is64Bit) key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Stimulsoft\\Stimulsoft Reports");
            path = (string)key.GetValue("Bin");

            string dataSetPath = path + "\\Data\\";
            if (File.Exists(dataSetPath + "Demo.xsd")) ds.ReadXmlSchema(dataSetPath + "Demo.xsd");
            else MessageBox.Show("File \"Demo.xsd\" not found");

            if (File.Exists(dataSetPath + "Demo.xsd")) ds.ReadXml(dataSetPath + "Demo.xml");
            else MessageBox.Show("File \"Demo.xml\" not found");

            ds.DataSetName = "Demo";

            report.Load(path + "Reports\\" + "DrillDownGroupWithCollapsing.mrt");
            report.RegData(ds);

            report.Compile();
            report.Render(false);
            viewer.Report = report;
        }

        private void viewer_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}