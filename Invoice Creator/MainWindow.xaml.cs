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
using System.Data;
using System.IO;
using IOPath = System.IO.Path;

namespace InvoiceCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        #region Fields

        private DataSet dataSetInvoice = new DataSet("Invoice");
        private string InvoicesPath = IOPath.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Invoices\\");
        private string SchmemaPath = IOPath.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..\\..\\EmptyTemplate.xsd");

        #endregion

        #region Methods

        public MainWindow()
        {
            Stimulsoft.Report.StiOptions.Wpf.CurrentTheme = Stimulsoft.Report.StiOptions.Wpf.Themes.Office2013Theme;
            Stimulsoft.Report.Wpf.StiThemesHelper.LoadTheme(this);
            InitializeComponent();
        }

        private void FillTreeView()
        {
            treeViewInvoices.Items.Clear();
            string[] strFiles = Directory.GetFiles(InvoicesPath);
            foreach (string myFile in strFiles)
            {
                if (IOPath.GetExtension(myFile).ToUpperInvariant() == ".XML")
                {
                    treeViewInvoices.Items.Add(IOPath.GetFileNameWithoutExtension(myFile));
                };
            }
            if (!treeViewInvoices.Items.IsEmpty)
            {
                TreeViewItem firstNode = treeViewInvoices.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                if (firstNode != null)
                {
                    firstNode.IsSelected = true;
                    firstNode.Focus();
                }
            }
        }

        private void CheckButtons()
        {
            if (!treeViewInvoices.Items.IsEmpty)
            {
                buttonDelete.IsEnabled = true;
                buttonEdit.IsEnabled = true;
                buttonPrint.IsEnabled = true;
            }
            else
            {
                buttonDelete.IsEnabled = false;
                buttonEdit.IsEnabled = false;
                buttonPrint.IsEnabled = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillTreeView();
            CheckButtons();
        }

        #endregion

        #region Handlers

        private void buttonNew_Click(object sender, RoutedEventArgs e)
        {
            string defaultXML = InvoiceCreator.Resources.EmptyTemplate.ToString();
            string invoiceFileName = InvoicesPath + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xml";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(invoiceFileName))
            {
                file.WriteLine(defaultXML);
                file.Close();
                dataSetInvoice.Clear();
                dataSetInvoice.ReadXmlSchema(SchmemaPath);

                EditWindow editWindow = new EditWindow(invoiceFileName, dataSetInvoice);
                    editWindow.Title = "New Invoice";
                    if (editWindow.ShowDialog() == true)
                    {
                        File.Move(invoiceFileName, InvoicesPath + "Invoice #" + editWindow.invoiceNumber.ToString() + " from " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".xml");
                    }
                    else
                    {
                        File.Delete(invoiceFileName);
                    }
            }
            FillTreeView();
            CheckButtons();
        }

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewInvoices.SelectedItem != null)
            {
                dataSetInvoice.Clear();
                dataSetInvoice.ReadXmlSchema(SchmemaPath);
                EditWindow editWindow = new EditWindow(InvoicesPath + treeViewInvoices.SelectedItem.ToString() + ".xml", dataSetInvoice);
                editWindow.Title = "Edit Invoice";
                editWindow.ShowDialog();
            }
            else
            {
                Stimulsoft.Report.Wpf.StiMessageBox.Show("Select invoice first to proceed!");
            }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (Stimulsoft.Report.Wpf.StiMessageBox.Show("Do you want to delete invoice?", "Confirm deletion", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string filePath = treeViewInvoices.SelectedItem.ToString();
                if (filePath.Length > 0)
                {
                    File.Delete(InvoicesPath + filePath + ".xml");
                }
                FillTreeView();
                CheckButtons();
            }
        }

        private void buttonPrint_Click(object sender, RoutedEventArgs e)
        {
            if (treeViewInvoices.SelectedItem != null)
            {
                dataSetInvoice.Clear();
                dataSetInvoice.ReadXmlSchema(SchmemaPath);
                dataSetInvoice.ReadXml(InvoicesPath + treeViewInvoices.SelectedItem.ToString() + ".xml");
                PrintWindow printWindow = new PrintWindow(dataSetInvoice);
                printWindow.ShowDialog();
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

        private void treeViewInvoices_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            IInputElement element = treeViewInvoices.InputHitTest(e.GetPosition(treeViewInvoices));
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
