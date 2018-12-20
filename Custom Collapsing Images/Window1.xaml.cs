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
        public Window1()
        {
            // How to Activate
            //Stimulsoft.Base.StiLicense.Key = "6vJhGtLLLz2GNviWmUTrhSqnO...";
            //Stimulsoft.Base.StiLicense.LoadFromFile("license.key");
            //Stimulsoft.Base.StiLicense.LoadFromStream(stream);

            StiOptions.Wpf.CurrentTheme = StiOptions.Wpf.Themes.Office2013Theme;
            StiOptions.Wpf.Viewer.CollapsingImages.UseCustomCollapsingImages = true;
            StiOptions.Wpf.Viewer.CollapsingImages.CollapsedImagePath = "pack://application:,,,/CustomCollapsingImages;component/Images/Collapsed.png";
            StiOptions.Wpf.Viewer.CollapsingImages.ExpandedImagePath = "pack://application:,,,/CustomCollapsingImages;component/Images/Expanded.png";

            InitializeComponent();
            StiReport report = new StiReport();            

            DataSet ds = new DataSet();
            ds.ReadXmlSchema("..\\..\\Data\\Demo.xsd");
            ds.ReadXml("..\\..\\Data\\Demo.xml");
            ds.DataSetName = "Demo";

            report.Load("..\\DrillDownGroupWithCollapsing.mrt");
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