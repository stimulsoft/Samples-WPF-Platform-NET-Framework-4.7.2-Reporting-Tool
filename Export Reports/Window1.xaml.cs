using System;
using System.Data;
using System.Collections.Generic;
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

namespace Export
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private string path = string.Empty;

        DataSet dataSet1 = new DataSet();

        public Window1()
        {
            StiOptions.Wpf.CurrentTheme = StiOptions.Wpf.Themes.Office2013Theme;
            Stimulsoft.Report.Wpf.StiThemesHelper.LoadTheme(this);
            InitializeComponent();

            lbReports.SelectedIndex = 0;

            dataSet1.ReadXmlSchema("..\\..\\Data\\Demo.xsd");
            dataSet1.ReadXml("..\\..\\Data\\Demo.xml");
        }

        private void buttonPreview_Click(object sender, RoutedEventArgs e)
        {
            StiReport report = new StiReport();
            report.RegData(dataSet1);

            report.Load("..\\" + ((ListBoxItem)lbReports.SelectedItem).Content as string + ".mrt");
            report.RenderWithWpf();
            report.ShowWithWpf(true);
        }

        private void buttonExport_Click(object sender, RoutedEventArgs e)
        {
            StiReport report = new StiReport();
            report.RegData(dataSet1);

            report.Load("..\\" + ((ListBoxItem)lbReports.SelectedItem).Content as string + ".mrt");
            report.RenderWithWpf(false);

            string file = ((ListBoxItem)lbReports.SelectedItem).Content as string + ".";

            if (rbPdf.IsChecked.GetValueOrDefault())
            {
                file += "pdf";
                report.ExportDocument(StiExportFormat.Pdf, file);
                System.Diagnostics.Process.Start(file);
            }
            else if (rbHtml.IsChecked.GetValueOrDefault())
            {
                file += "html";
                report.ExportDocument(StiExportFormat.HtmlTable, file);
                System.Diagnostics.Process.Start(file);
            }
            else if (rbXls.IsChecked.GetValueOrDefault())
            {
                file += "xls";
                report.ExportDocument(StiExportFormat.Excel, file);
                System.Diagnostics.Process.Start(file);
            }
            else if (rbTxt.IsChecked.GetValueOrDefault())
            {
                file += "txt";
                report.ExportDocument(StiExportFormat.Text, file);
                System.Diagnostics.Process.Start(file);
            }
            else if (rbRtf.IsChecked.GetValueOrDefault())
            {
                file += "rtf";
                report.ExportDocument(StiExportFormat.RtfTable, file);
                System.Diagnostics.Process.Start(file);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
