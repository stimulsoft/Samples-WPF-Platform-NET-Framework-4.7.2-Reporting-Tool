using Stimulsoft.Base;
using Stimulsoft.Base.Drawing;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Stimulsoft.Base.Theme;

namespace Changing_the_Viewer_and_Designer_Theme
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

            switch (StiUXTheme.Appearance)
            {
                case StiThemeAppearance.Auto:
                    comboBoxAppearance.SelectedIndex = 0;
                    break;

                case StiThemeAppearance.Light:
                    comboBoxAppearance.SelectedIndex = 1;
                    break;

                case StiThemeAppearance.Dark:
                    comboBoxAppearance.SelectedIndex = 2;
                    break;
            }

            switch (StiUXTheme.AccentColor)
            {
                case StiThemeAccentColor.Auto:
                    comboBoxAccentColor.SelectedIndex = 0;
                    break;

                case StiThemeAccentColor.Blue:
                    comboBoxAccentColor.SelectedIndex = 1;
                    break;

                case StiThemeAccentColor.Violet:
                    comboBoxAccentColor.SelectedIndex = 2;
                    break;

                case StiThemeAccentColor.Carmine:
                    comboBoxAccentColor.SelectedIndex = 3;
                    break;

                case StiThemeAccentColor.Teal:
                    comboBoxAccentColor.SelectedIndex = 4;
                    break;

                case StiThemeAccentColor.Green:
                    comboBoxAccentColor.SelectedIndex = 5;
                    break;

                case StiThemeAccentColor.Orange:
                    comboBoxAccentColor.SelectedIndex = 6;
                    break;
            }

            isInit = true;
        }

        #region Fields
        private bool isInit = false;
        #endregion

        #region Handelrs
        private void ButtonShowDesigner_Click(object sender, RoutedEventArgs e)
        {
            var report = new StiReport();
            report.DesignV2WithWpf();
        }

        private void ButtonShowViewer_Click(object sender, RoutedEventArgs e)
        {
            var report = new StiReport();
            report.ShowWithWpf();
        }

        private void ComboBoxAppearance_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!isInit) return;

            var appearance = StiThemeAppearance.Light;
            switch (comboBoxAppearance.SelectedIndex)
            {
                case 0:
                    appearance = StiThemeAppearance.Auto;
                    break;

                case 1:
                    appearance = StiThemeAppearance.Light;
                    break;

                case 2:
                    appearance = StiThemeAppearance.Dark;
                    break;
            }

            StiUXTheme.ApplyNewTheme(appearance, StiUXTheme.AccentColor);
        }

        private void ComboBoxAccentColor_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!isInit) return;

            var accentColor = StiThemeAccentColor.Blue;
            switch (comboBoxAccentColor.SelectedIndex)
            {
                case 0:
                    accentColor = StiThemeAccentColor.Auto;
                    break;

                case 1:
                    accentColor = StiThemeAccentColor.Blue;
                    break;

                case 2:
                    accentColor = StiThemeAccentColor.Violet;
                    break;

                case 3:
                    accentColor = StiThemeAccentColor.Carmine;
                    break;

                case 4:
                    accentColor = StiThemeAccentColor.Teal;
                    break;

                case 5:
                    accentColor = StiThemeAccentColor.Green;
                    break;

                case 6:
                    accentColor = StiThemeAccentColor.Orange;
                    break;
            }

            StiUXTheme.ApplyNewTheme(StiUXTheme.Appearance, accentColor);
        }
        #endregion
    }
}
