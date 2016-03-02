using System;
using System.IO;
using System.Collections.Generic;
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
using Stimulsoft.Base.Drawing;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;

namespace RuntimeBuildReport
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private string path = string.Empty;

        private System.Data.DataSet dataSet1 = new System.Data.DataSet();

        public Window1()
        {
            StiOptions.Wpf.CurrentTheme = StiOptions.Wpf.Themes.Office2013Theme;
            Stimulsoft.Report.Wpf.StiThemesHelper.LoadTheme(this);
            InitializeComponent();

            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Stimulsoft\\Stimulsoft Reports");
            bool is64Bit = IntPtr.Size == 8;
            if (is64Bit) key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Stimulsoft\\Stimulsoft Reports");
            path = (string)key.GetValue("Bin") + "\\Data\\";

            if (File.Exists(path + "Demo.xsd")) dataSet1.ReadXmlSchema(path + "Demo.xsd");
            else MessageBox.Show("File \"Demo.xsd\" not found");

            if (File.Exists(path + "Demo.xsd")) dataSet1.ReadXml(path + "Demo.xml");
            else MessageBox.Show("File \"Demo.xml\" not found");

            dataSet1.DataSetName = "Demo";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StiReport report = new StiReport();

            //Add data to datastore
            report.RegData(dataSet1);

            //Fill dictionary
            report.Dictionary.Synchronize();

            StiPage page = report.Pages[0];

            //Create HeaderBand
            StiHeaderBand headerBand = new StiHeaderBand();
            headerBand.Height = 0.5;
            headerBand.Name = "HeaderBand";
            page.Components.Add(headerBand);

            //Create text on header
            StiText headerText = new StiText(new RectangleD(0, 0, 5, 0.5));
            headerText.Text = "CompanyName";
            headerText.HorAlignment = StiTextHorAlignment.Center;
            headerText.Name = "HeaderText";
            headerText.Brush = new StiSolidBrush(System.Drawing.Color.LightGreen);
            headerBand.Components.Add(headerText);

            //Create Databand
            StiDataBand dataBand = new StiDataBand();
            dataBand.DataSourceName = "Customers";
            dataBand.Height = 0.5;
            dataBand.Name = "DataBand";
            page.Components.Add(dataBand);

            //Create text
            StiText dataText = new StiText(new RectangleD(0, 0, 5, 0.5));
            dataText.Text = "{Line}.{Customers.CompanyName}";
            dataText.Name = "DataText";
            dataBand.Components.Add(dataText);

            //Create FooterBand
            StiFooterBand footerBand = new StiFooterBand();
            footerBand.Height = 0.5;
            footerBand.Name = "FooterBand";
            page.Components.Add(footerBand);

            //Create text on footer
            StiText footerText = new StiText(new RectangleD(0, 0, 5, 0.5));
            footerText.Text = "Count - {Count()}";
            footerText.HorAlignment = StiTextHorAlignment.Right;
            footerText.Name = "FooterText";
            footerText.Brush = new StiSolidBrush(System.Drawing.Color.LightGreen);
            footerBand.Components.Add(footerText);

            report.ShowWithWpf();
        }
    }
}
