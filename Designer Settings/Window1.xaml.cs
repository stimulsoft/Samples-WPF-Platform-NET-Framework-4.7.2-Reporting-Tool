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
using Stimulsoft.Report.WpfDesign;
using Stimulsoft.Base.Services;
using System.Reflection;

namespace WpfDesignerSettings
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        #region Fields
        private StiWpfDesignerControl designerControl;

        private StiWpfStandardToolbarService standardToolbarService;
        private StiWpfBordersToolbarService bordersToolbarService;
        private StiWpfFormattingToolbarService formattingToolbarService;
        private StiWpfLayoutToolbarService layoutToolbarService;
        private StiWpfStatusBarService statusBarService;
        private StiWpfStyleToolbarService styleToolbarService;
        private StiWpfZoomToolbarService zoomToolbarService;
        private StiWpfMainMenuService mainMenuService;
        private StiWpfDictionaryPanelService dictionaryPanelService;
        #endregion

        public Window1()
        {
            StiOptions.Wpf.CurrentTheme = StiOptions.Wpf.Themes.Office2013Theme;

            InitializeComponent();
            designerControl = new StiWpfDesignerControl();

            #region Init Services
            StiServiceContainer cont = designerControl.Services.GetServices(typeof(StiWpfToolbarService));
            foreach (StiService service in cont)
            {
                if (service is StiWpfStandardToolbarService)
                    standardToolbarService = service as StiWpfStandardToolbarService;
                else if (service is StiWpfBordersToolbarService)
                    bordersToolbarService = service as StiWpfBordersToolbarService;
                else if (service is StiWpfFormattingToolbarService)
                    formattingToolbarService = service as StiWpfFormattingToolbarService;
                else if (service is StiWpfLayoutToolbarService)
                    layoutToolbarService = service as StiWpfLayoutToolbarService;
                else if (service is StiWpfStatusBarService)
                    statusBarService = service as StiWpfStatusBarService;
                else if (service is StiWpfStyleToolbarService)
                    styleToolbarService = service as StiWpfStyleToolbarService;
                else if (service is StiWpfZoomToolbarService)
                    zoomToolbarService = service as StiWpfZoomToolbarService;
            }

            cont = designerControl.Services.GetServices(typeof(StiWpfMainMenuService));
            foreach (StiService service in cont)
            {
                if (service is StiWpfMainMenuService)
                    mainMenuService = service as StiWpfMainMenuService;
            }

            cont = StiConfig.Services.GetServices(typeof(StiWpfPanelService));
            foreach (StiService service in cont)
            {
                if (service is StiWpfDictionaryPanelService)
                    dictionaryPanelService = service as StiWpfDictionaryPanelService;
            }
            #endregion

            InitDesigner();
        }

        #region Methods
        private void InitDesigner()
        {
            buttonApplyChanges.IsEnabled = false;

            rootGrid.Children.Clear();
            designerControl = new StiWpfDesignerControl();
            designerControl.SetValue(Grid.ColumnProperty, 1);
            rootGrid.Children.Add(designerControl);

            InitStandardToolbarPanel();
            InitBordersToolbarPanel();
            InitFormattingToolbarPanel();
            InitLayoutToolbarPanel();
            InitStatusBarPanel();
            InitStyleToolbar();
            InitZoomToolbar();
            InitMainMenuService();
            InitDictionaryPanel();
        }
        #endregion

        #region Handlers
        private void buttonApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            InitDesigner();
        }
        #endregion

        #region StandardToolbar
        private void cbStandardToolbar_Click(object sender, RoutedEventArgs e)
        {
            if (standardToolbarService == null)
                return;

            var checkBox = (CheckBox)sender;
            string tag = (string)checkBox.Tag;
            var pi = typeof(StiWpfStandardToolbarService).GetProperty(tag);

            if (pi != null)
                pi.SetValue(standardToolbarService, checkBox.IsChecked.Value, null);

            buttonApplyChanges.IsEnabled = true;
        }

        private void InitStandardToolbarPanel()
        {
            if (standardToolbarService == null)
                return;

            int index = -1;
            while(++index < standardToolbarPanel.Children.Count)
            {
                CheckBox checkBox = standardToolbarPanel.Children[index] as CheckBox;
                if (checkBox != null)
                {
                    PropertyInfo pi = typeof(StiWpfStandardToolbarService).GetProperty((string)checkBox.Tag);

                    if (pi != null)
                        checkBox.IsChecked = (bool)pi.GetValue(standardToolbarService, null);
                }
            }
        }
        #endregion

        #region BordersToolbarService
        private void cbBordersToolbar_Click(object sender, RoutedEventArgs e)
        {
            if (bordersToolbarService == null)
                return;

            CheckBox checkBox = (CheckBox)sender;
            string tag = (string)checkBox.Tag;
            PropertyInfo pi = typeof(StiWpfBordersToolbarService).GetProperty(tag);

            if (pi != null)
                pi.SetValue(bordersToolbarService, checkBox.IsChecked.Value, null);

            buttonApplyChanges.IsEnabled = true;
        }

        private void InitBordersToolbarPanel()
        {
            if (bordersToolbarService == null)
                return;

            foreach (CheckBox checkBox in bordersToolbarPanel.Children)
            {
                PropertyInfo pi = typeof(StiWpfBordersToolbarService).GetProperty((string)checkBox.Tag);

                if (pi != null)
                    checkBox.IsChecked = (bool)pi.GetValue(bordersToolbarService, null);
            }
        }
        #endregion

        #region FormattingToolbar
        private void cbFormattingToolbar_Click(object sender, RoutedEventArgs e)
        {
            if (formattingToolbarService == null)
                return;

            CheckBox checkBox = (CheckBox)sender;
            string tag = (string)checkBox.Tag;
            PropertyInfo pi = typeof(StiWpfFormattingToolbarService).GetProperty(tag);

            if (pi != null)
                pi.SetValue(formattingToolbarService, checkBox.IsChecked.Value, null);

            buttonApplyChanges.IsEnabled = true;
        }

        private void InitFormattingToolbarPanel()
        {
            if (formattingToolbarService == null)
                return;

            foreach (CheckBox checkBox in bordersFormattingToolbar.Children)
            {
                PropertyInfo pi = typeof(StiWpfFormattingToolbarService).GetProperty((string)checkBox.Tag);

                if (pi != null)
                    checkBox.IsChecked = (bool)pi.GetValue(formattingToolbarService, null);
            }
        }
        #endregion

        #region LayoutToolbar
        private void cbLayoutToolbar_Click(object sender, RoutedEventArgs e)
        {
            if (layoutToolbarService == null)
                return;

            CheckBox checkBox = (CheckBox)sender;
            string tag = (string)checkBox.Tag;
            PropertyInfo pi = typeof(StiWpfLayoutToolbarService).GetProperty(tag);

            if (pi != null)
                pi.SetValue(layoutToolbarService, checkBox.IsChecked.Value, null);

            buttonApplyChanges.IsEnabled = true;
        }

        private void InitLayoutToolbarPanel()
        {
            if (layoutToolbarService == null)
                return;

            foreach (CheckBox checkBox in layoutToolbarServiceToolbar.Children)
            {
                PropertyInfo pi = typeof(StiWpfLayoutToolbarService).GetProperty((string)checkBox.Tag);

                if (pi != null)
                    checkBox.IsChecked = (bool)pi.GetValue(layoutToolbarService, null);
            }
        }
        #endregion

        #region StatusBar
        private void cbStatusBar_Click(object sender, RoutedEventArgs e)
        {
            if (statusBarService == null)
                return;

            CheckBox checkBox = (CheckBox)sender;
            string tag = (string)checkBox.Tag;
            PropertyInfo pi = typeof(StiWpfStatusBarService).GetProperty(tag);

            if (pi != null)
                pi.SetValue(statusBarService, checkBox.IsChecked.GetValueOrDefault(), null);

            buttonApplyChanges.IsEnabled = true;
        }

        private void InitStatusBarPanel()
        {
            if (statusBarService == null)
                return;

            foreach (CheckBox checkBox in statusBarServiceToolbar.Children)
            {
                PropertyInfo pi = typeof(StiWpfStatusBarService).GetProperty((string)checkBox.Tag);

                if (pi != null)
                    checkBox.IsChecked = (bool)pi.GetValue(statusBarService, null);
            }
        }
        #endregion

        #region StatusBar
        private void cbStyleToolbar_Click(object sender, RoutedEventArgs e)
        {
            if (styleToolbarService == null)
                return;

            CheckBox checkBox = (CheckBox)sender;
            string tag = (string)checkBox.Tag;
            PropertyInfo pi = typeof(StiWpfStyleToolbarService).GetProperty(tag);

            if (pi != null)
                pi.SetValue(styleToolbarService, checkBox.IsChecked.Value, null);

            buttonApplyChanges.IsEnabled = true;
        }

        private void InitStyleToolbar()
        {
            if (styleToolbarService == null)
                return;

            foreach (CheckBox checkBox in statusBarServiceToolbar.Children)
            {
                PropertyInfo pi = typeof(StiWpfStyleToolbarService).GetProperty((string)checkBox.Tag);

                if (pi != null)
                    checkBox.IsChecked = (bool)pi.GetValue(styleToolbarService, null);
            }
        }
        #endregion

        #region StatusBar
        private void cbZoomToolbar_Click(object sender, RoutedEventArgs e)
        {
            if (zoomToolbarService == null)
                return;

            CheckBox checkBox = (CheckBox)sender;
            string tag = (string)checkBox.Tag;
            PropertyInfo pi = typeof(StiWpfZoomToolbarService).GetProperty(tag);

            if (pi != null)
                pi.SetValue(zoomToolbarService, checkBox.IsChecked.Value, null);

            buttonApplyChanges.IsEnabled = true;
        }

        private void InitZoomToolbar()
        {
            if (zoomToolbarService == null)
                return;

            foreach (CheckBox checkBox in zoomToolbarServiceToolbar.Children)
            {
                PropertyInfo pi = typeof(StiWpfZoomToolbarService).GetProperty((string)checkBox.Tag);

                if (pi != null)
                    checkBox.IsChecked = (bool)pi.GetValue(zoomToolbarService, null);
            }
        }
        #endregion

        #region StiWpfMainMenuService
        private void InitMainMenuService()
        {
            if (mainMenuService == null)
                return;

            int index = -1;
            while (++index < mainMenuServiceToolbar.Children.Count)
            {
                CheckBox checkBox = mainMenuServiceToolbar.Children[index] as CheckBox;
                if (checkBox != null)
                {
                    PropertyInfo pi = typeof(StiWpfMainMenuService).GetProperty((string)checkBox.Tag);

                    if (pi != null)
                        checkBox.IsChecked = (bool)pi.GetValue(mainMenuService, null);
                } 
            }
        }

        private void cbMainMenuServiceToolbar_Click(object sender, RoutedEventArgs e)
        {
            if (mainMenuService == null)
                return;

            CheckBox checkBox = (CheckBox)sender;
            string tag = (string)checkBox.Tag;
            PropertyInfo pi = typeof(StiWpfMainMenuService).GetProperty(tag);

            if (pi != null)
                pi.SetValue(mainMenuService, checkBox.IsChecked.Value, null);

            buttonApplyChanges.IsEnabled = true;
        }
        #endregion

        #region StiWpfDictionaryPanelService
        private void InitDictionaryPanel()
        {
            var type = typeof(StiOptions.Designer.Panels.Dictionary);
            foreach (CheckBox checkBox in dictionaryPanelServiceToolbar.Children)
            {
                PropertyInfo pi = type.GetProperty((string)checkBox.Tag);

                if (pi != null)
                    checkBox.IsChecked = (bool)pi.GetValue(null, null);
            }
        }

        private void cbDictionaryPanel_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            string tag = (string)checkBox.Tag;
            PropertyInfo pi = typeof(StiOptions.Designer.Panels.Dictionary).GetProperty(tag);

            if (pi != null)
                pi.SetValue(null, checkBox.IsChecked.Value, null);

            buttonApplyChanges.IsEnabled = true;
        }
        #endregion
    }
}