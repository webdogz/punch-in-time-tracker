using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Resources;
using Webdogz.UI.Presentation;

namespace PunchIn.ViewModels
{
    /// <summary>
    /// A simple view model for configuring theme, font and accent colors.
    /// </summary>
    public class SettingsAppearanceViewModel
        : ViewModelBase
    {
        #region Private Fields
        private const string PaletteMetro = "metro";
        private const string PaletteWP = "windows phone";

        // 9 accent colors from metro design principles
        private Color[] metroAccentColors = new Color[]{
            Color.FromRgb(0x33, 0x99, 0xff),   // blue
            Color.FromRgb(0x00, 0xab, 0xa9),   // teal
            Color.FromRgb(0x33, 0x99, 0x33),   // green
            Color.FromRgb(0x8c, 0xbf, 0x26),   // lime
            Color.FromRgb(0xf0, 0x96, 0x09),   // orange
            Color.FromRgb(0xff, 0x45, 0x00),   // orange red
            Color.FromRgb(0xe5, 0x14, 0x00),   // red
            Color.FromRgb(0xff, 0x00, 0x97),   // magenta
            Color.FromRgb(0xa2, 0x00, 0xff),   // purple
        };

        // 20 accent colors from Windows Phone 8
        private Color[] wpAccentColors = new Color[]{
            Color.FromRgb(0xa4, 0xc4, 0x00),   // lime
            Color.FromRgb(0x60, 0xa9, 0x17),   // green
            Color.FromRgb(0x00, 0x8a, 0x00),   // emerald
            Color.FromRgb(0x00, 0xab, 0xa9),   // teal
            Color.FromRgb(0x1b, 0xa1, 0xe2),   // cyan
            Color.FromRgb(0x00, 0x50, 0xef),   // cobalt
            Color.FromRgb(0x6a, 0x00, 0xff),   // indigo
            Color.FromRgb(0xaa, 0x00, 0xff),   // violet
            Color.FromRgb(0xf4, 0x72, 0xd0),   // pink
            Color.FromRgb(0xd8, 0x00, 0x73),   // magenta
            Color.FromRgb(0xa2, 0x00, 0x25),   // crimson
            Color.FromRgb(0xe5, 0x14, 0x00),   // red
            Color.FromRgb(0xfa, 0x68, 0x00),   // orange
            Color.FromRgb(0xf0, 0xa3, 0x0a),   // amber
            Color.FromRgb(0xe3, 0xc8, 0x00),   // yellow
            Color.FromRgb(0x82, 0x5a, 0x2c),   // brown
            Color.FromRgb(0x6d, 0x87, 0x64),   // olive
            Color.FromRgb(0x64, 0x76, 0x87),   // steel
            Color.FromRgb(0x76, 0x60, 0x8a),   // mauve
            Color.FromRgb(0x87, 0x79, 0x4e),   // taupe
        };

        private string selectedPalette = PaletteWP;

        private Color selectedAccentColor;
        private LinkCollection themes = new LinkCollection();
        private Link selectedTheme;
        #endregion

        public SettingsAppearanceViewModel()
        {
            // add the default themes
            themes.Add(new Link { DisplayName = "dark", Source = AppearanceManager.DarkThemeSource });
            themes.Add(new Link { DisplayName = "light", Source = AppearanceManager.LightThemeSource });

            // add additional themes
            themes.Add(new Link { DisplayName = "underworld", Source = new Uri("/PunchIn;component/Assets/Themes/underworld.xaml", UriKind.Relative) });
            themes.Add(new Link { DisplayName = "hello-kitty", Source = new Uri("/PunchIn;component/Assets/Themes/hello-kitty.xaml", UriKind.Relative) });
            themes.Add(new Link { DisplayName = "my-background", Source = new Uri("/PunchIn;component/Assets/Themes/my-background.xaml", UriKind.Relative) });

            //GetAvailableUserThemes();

            SyncThemeAndColor();

            AppearanceManager.Current.PropertyChanged += OnAppearanceManagerPropertyChanged;
        }

        #region Theme Loader Methods
        private void GetAvailableUserThemes()
        {
            var relativePath = "Themes";
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            DirectoryInfo themesDir = new DirectoryInfo(path);

            if (themesDir.Exists)
            {
                foreach (FileInfo file in themesDir.GetFiles("*.xaml"))
                {
                    themes.Add(new Link { DisplayName = file.Name, Source = new Uri(Path.Combine("pack://siteoforigin:,,,/", relativePath, file.Name), UriKind.Absolute) });
                }
            }
        }
        private void LoadThemeResourceFromFile(Uri resorceUri)
        {
            //TODO: Implement external theme loading
            try
            {
                StreamResourceInfo info = Application.GetRemoteStream(resorceUri);
                System.Windows.Markup.XamlReader reader = new System.Windows.Markup.XamlReader();
                ResourceDictionary resource = (ResourceDictionary)reader.LoadAsync(info.Stream);
                AppearanceManager.Current.AddTheme(resource, true);
            }
            catch (IOException)
            {
                // do something
            }

        }
        #endregion

        #region Private Methods
        private void SyncThemeAndColor()
        {
            // synchronizes the selected viewmodel theme with the actual theme used by the appearance manager.
            Uri currentTheme = AppearanceManager.Current.ThemeSource;
            SelectedTheme = themes.FirstOrDefault(l => l.Source.Equals(currentTheme));

            // and make sure accent color is up-to-date
            SelectedAccentColor = AppearanceManager.Current.AccentColor;

            if (AppearanceManager.Current.UserSettingsLoaded)
            {
                Properties.Settings.Default.SelectedAccentColor = SelectedAccentColor;
                Properties.Settings.Default.SelectedThemeSource = SelectedTheme.Source;
                Properties.Settings.Default.Save();
            }
        }

        private void OnAppearanceManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ThemeSource" || e.PropertyName == "AccentColor") {
                SyncThemeAndColor();
            }
        }
        #endregion

        #region Properties
        public LinkCollection Themes
        {
            get { return themes; }
        }

        public string[] Palettes
        {
            get { return new string[] { PaletteMetro, PaletteWP }; }
        }

        public Color[] AccentColors
        {
            get { return selectedPalette == PaletteMetro ? metroAccentColors : wpAccentColors; }
        }

        public string SelectedPalette
        {
            get { return selectedPalette; }
            set
            {
                if (selectedPalette != value) {
                    selectedPalette = value;
                    OnPropertyChanged("AccentColors");

                    SelectedAccentColor = AccentColors.FirstOrDefault();
                }
            }
        }

        public Link SelectedTheme
        {
            get { return selectedTheme; }
            set
            {
                if (selectedTheme != value) {
                    selectedTheme = value;
                    OnPropertyChanged("SelectedTheme");

                    // and update the actual theme
                    AppearanceManager.Current.ThemeSource = value.Source;
                }
            }
        }

        public Color SelectedAccentColor
        {
            get { return selectedAccentColor; }
            set
            {
                if (selectedAccentColor != value) {
                    selectedAccentColor = value;
                    OnPropertyChanged("SelectedAccentColor");

                    AppearanceManager.Current.AccentColor = value;
                }
            }
        }
        #endregion
    }
}
