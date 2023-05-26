using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Exporting_a_Report_from_Code
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // How to Activate
            //Stimulsoft.Base.StiLicense.Key = "6vJhGtLLLz2GNviWmUTrhSqnO...";
            //Stimulsoft.Base.StiLicense.LoadFromFile("license.key");
            //Stimulsoft.Base.StiLicense.LoadFromStream(stream);

            InitializeComponent();
        }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            var report = new StiReport();
            report.Load(@"Reports\TwoSimpleLists.mrt");
            report.Render();

            var stream = new MemoryStream();
            switch (((Label)ComboBoxFormat.SelectedItem).Content)
            {
                case "PDF":
                    report.ExportDocument(StiExportFormat.Pdf, stream);
                    break;

                case "Word":
                    report.ExportDocument(StiExportFormat.Word2007, stream);
                    break;

                case "Excel":
                    report.ExportDocument(StiExportFormat.Excel2007, stream);
                    break;

                case "Text":
                    report.ExportDocument(StiExportFormat.Text, stream);
                    break;

                case "Image":
                    report.ExportDocument(StiExportFormat.ImagePng, stream);
                    break;
            }

            // Save to Local Storage
            /*using (var fileStream = File.Create(@"d:\TwoSimpleLists.pdf"))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }*/

            MessageBox.Show("The export action is complete.", "Export Report");
        }
    }
}
