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
using System.Windows.Shapes;
using System.Data;
using System.IO;
using IOPath = System.IO.Path;

namespace InvoiceCreator
{
    /// <summary>
    /// Interaction logic for PrintWindow.xaml
    /// </summary>
    public partial class PrintWindow : Window
    {

        #region Fields

        private DataSet dataSetInvoice;
        private string ReportsPath = IOPath.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Reports\\");

        #endregion

        #region Methods

        public PrintWindow()
        {
            Stimulsoft.Report.Wpf.StiThemesHelper.LoadTheme(this);
            InitializeComponent();
        }

        public PrintWindow(DataSet dataSetInvoice)
        {
            Stimulsoft.Report.Wpf.StiThemesHelper.LoadTheme(this);
            this.dataSetInvoice = dataSetInvoice;
            InitializeComponent();
        }

        private void FillTreeView()
        {
            treeViewReports.Items.Clear();
            string[] strFiles = Directory.GetFiles(ReportsPath);
            foreach (string myFile in strFiles)
            {
                if (IOPath.GetExtension(myFile).ToUpperInvariant() == ".MRT")
                {
                    treeViewReports.Items.Add(IOPath.GetFileNameWithoutExtension(myFile));
                };
            }
            if (!treeViewReports.Items.IsEmpty)
            {
                TreeViewItem firstNode = treeViewReports.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                if (firstNode != null)
                {
                    firstNode.IsSelected = true;
                    firstNode.Focus();
                }
            }
        }

        private void CheckButtons()
        {
            if (!treeViewReports.Items.IsEmpty)
            {
                buttonPrint.IsEnabled = true;
                buttonDesign.IsEnabled = true;
            }
            else
            {
                buttonPrint.IsEnabled = false;
                buttonDesign.IsEnabled = false;
            }
        }

        #endregion

        #region Handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillTreeView();
            if (!treeViewReports.Items.IsEmpty)
            {
                TreeViewItem firstNode = treeViewReports.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                if (firstNode != null)
                {
                    firstNode.IsSelected = true;
                    firstNode.Focus();
                }
            };
            CheckButtons();
        }

        private void buttonPrint_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewReports.SelectedItem != null)
            {
                using (Stimulsoft.Report.StiReport report = new Stimulsoft.Report.StiReport())
                {
                    report.Load(ReportsPath + treeViewReports.SelectedItem.ToString() + ".mrt");
                    report.Dictionary.Databases.Clear();
                    report.RegData("Demo", "Demo", dataSetInvoice);
                    report.Compile();
                    report.ShowWithWpf(true);
                }
            }
            else
            {
                Stimulsoft.Report.Wpf.StiMessageBox.Show("Select invoice first to proceed!");
            }
        }

        private void buttonDesign_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewReports.SelectedItem != null)
            {
                using (Stimulsoft.Report.StiReport report = new Stimulsoft.Report.StiReport())
                {
                    report.Load(ReportsPath + treeViewReports.SelectedItem.ToString() + ".mrt");
                    report.Dictionary.Databases.Clear();
                    report.RegData("Demo", "Demo", dataSetInvoice);
                    report.Compile();
                    report.DesignWithWpf();
                }
            }
            else
            {
                Stimulsoft.Report.Wpf.StiMessageBox.Show("Select invoice first to proceed!");
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void treeViewReports_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            IInputElement element = treeViewReports.InputHitTest(e.GetPosition(treeViewReports));
            while (!((element is TreeView) || element == null))
            {
                if (element is TreeViewItem)
                    break;

                if (element is FrameworkElement)
                {
                    FrameworkElement fe = (FrameworkElement)element;
                    element = (IInputElement)(fe.Parent ?? fe.TemplatedParent);
                }
                else
                    break;
            }
            if (element is TreeViewItem)
            {
                element.Focus();
                e.Handled = true;
            }
        }

        #endregion

    }
}
