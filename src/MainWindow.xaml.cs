using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FirstFloor.ModernUI.Presentation;
using Hummingbird.Core;
using Hummingbird.ViewModels;

namespace Hummingbird
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            LoggingViewModel.Instance.Logger.Write("Successfully loaded.");

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Icon = new BitmapImage(new Uri("1427799497_61706.ico", UriKind.Relative));

            // Dominant color for the old Outlook logo.
            AppearanceManager.Current.AccentColor = Color.FromRgb(233, 151, 26);

            var settingsManager = new SettingsManager();
            AccountSettingsViewModel.Instance = settingsManager.GetUserSettings();

            LoggingViewModel.Instance.Logger.Write(string.Concat("IsInternal: ",
                AccountSettingsViewModel.Instance.IsInternal));
            LoggingViewModel.Instance.Logger.Write(string.Concat("Server: ", AccountSettingsViewModel.Instance.ServerUrl));
        }
    }
}