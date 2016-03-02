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
using System.Xml;

namespace InvoiceCreator
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {

        #region Fields

        private string pathXML;
        private DataSet dataSetInvoice;

        public int invoiceNumber
        {
            get
            {
                int invNum = -1;
                int.TryParse(textBoxNumber.Text, out invNum);
                return invNum;
            }
        }

        #endregion

        #region Methods

        public EditWindow()
        {
            Stimulsoft.Report.Wpf.StiThemesHelper.LoadTheme(this);
            InitializeComponent();
        }

        public EditWindow(string pathXML, DataSet dataSetInvoice)
        {
            Stimulsoft.Report.Wpf.StiThemesHelper.LoadTheme(this);
            this.pathXML = pathXML;
            this.dataSetInvoice = dataSetInvoice;
            InitializeComponent();
        }

        #endregion

        #region Handlers

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataSetInvoice.ReadXml(pathXML);

            dataGridProducts.ItemsSource = dataSetInvoice.Tables["Products"].DefaultView;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(pathXML);

            DateTime tmpDate = new DateTime();
            datePickerInvoiceDate.SelectedDate = DateTime.TryParse(xmlDoc.SelectSingleNode("//Demo/Invoice/InvoiceDate").InnerText, out tmpDate) ? tmpDate : DateTime.Now;
            textBoxNumber.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/InvoiceNumber").InnerText;
            textBoxCustomer.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/CustomerID").InnerText;
            textBoxBillToName.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/BillTo_Name").InnerText;
            textBoxBillToAddress.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/BillTo_Address").InnerText;
            textBoxBillToAddress2.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/BillTo_Address2").InnerText;
            textBoxBillToCity.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/BillTo_City").InnerText;
            textBoxBillToState.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/BillTo_State").InnerText;
            textBoxBillToZipCode.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/BillTo_ZipCode").InnerText;
            textBoxShipToName.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/ShipTo_Name").InnerText;
            textBoxShipToAddress.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/ShipTo_Address").InnerText;
            textBoxShipToAddress2.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/ShipTo_Address2").InnerText;
            textBoxShipToCity.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/ShipTo_City").InnerText;
            textBoxShipToState.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/ShipTo_State").InnerText;
            textBoxShipToZipCode.Text = xmlDoc.SelectSingleNode("//Demo/Invoice/ShipTo_ZipCode").InnerText;

            datePickerInvoiceDate.Focus();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            dataSetInvoice.WriteXml(pathXML);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(pathXML);
            xmlDoc.SelectSingleNode("//Demo/Invoice/InvoiceDate").InnerText = datePickerInvoiceDate.SelectedDate.ToString();
            xmlDoc.SelectSingleNode("//Demo/Invoice/InvoiceNumber").InnerText = textBoxNumber.Text;
            xmlDoc.SelectSingleNode("//Demo/Invoice/CustomerID").InnerText = textBoxCustomer.Text;
            xmlDoc.SelectSingleNode("//Demo/Invoice/BillTo_Name").InnerText = textBoxBillToName.Text;
            xmlDoc.SelectSingleNode("//Demo/Invoice/BillTo_Address").InnerText = textBoxBillToAddress.Text;
            xmlDoc.SelectSingleNode("//Demo/Invoice/BillTo_Address2").InnerText = textBoxBillToAddress2.Text;
            xmlDoc.SelectSingleNode("//Demo/Invoice/BillTo_City").InnerText = textBoxBillToCity.Text;
            xmlDoc.SelectSingleNode("//Demo/Invoice/BillTo_State").InnerText = textBoxBillToState.Text;
            xmlDoc.SelectSingleNode("//Demo/Invoice/BillTo_ZipCode").InnerText = textBoxBillToZipCode.Text;
            xmlDoc.SelectSingleNode("//Demo/Invoice/ShipTo_Name").InnerText = textBoxShipToName.Text;
            xmlDoc.SelectSingleNode("//Demo/Invoice/ShipTo_Address").InnerText = textBoxShipToAddress.Text;
            xmlDoc.SelectSingleNode("//Demo/Invoice/ShipTo_Address2").InnerText = textBoxShipToAddress2.Text;
            xmlDoc.SelectSingleNode("//Demo/Invoice/ShipTo_City").InnerText = textBoxShipToCity.Text;
            xmlDoc.SelectSingleNode("//Demo/Invoice/ShipTo_State").InnerText = textBoxShipToState.Text;
            xmlDoc.SelectSingleNode("//Demo/Invoice/ShipTo_ZipCode").InnerText = textBoxShipToZipCode.Text;
            xmlDoc.Save(pathXML);
            DialogResult = true;
            Close();
        }

        #endregion

    }
}
