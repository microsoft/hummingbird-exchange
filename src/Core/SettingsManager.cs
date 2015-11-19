using System;
using System.IO;
using System.Xml.Serialization;
using Hummingbird.ViewModels;

namespace Hummingbird.Core
{
    public class SettingsManager
    {
        public SettingsManager()
        {
            var localPath = AppDomain.CurrentDomain.BaseDirectory;
            AppPath = Directory.CreateDirectory(Path.Combine(localPath, AppSetup.SettingsContainerName)).FullName;
        }

        private string AppPath { get; }

        /// <summary>
        ///     Store user settings in a local file.
        /// </summary>
        /// <param name="settings">Existing settings.</param>
        /// <returns></returns>
        public bool StoreUserSettings(AccountSettingsViewModel settings)
        {
            bool isSuccessful;

            try
            {
                var serializer = new XmlSerializer(typeof (AccountSettingsViewModel));
                using (var writer = new StreamWriter(Path.Combine(AppPath, AppSetup.SettingsFileName)))
                {
                    serializer.Serialize(writer, settings);
                }
                isSuccessful = true;

                LoggingViewModel.Instance.Logger.Write(string.Concat("StoreUserSettings:Success ",
                    AccountSettingsViewModel.Instance.ServerUrl,
                    Environment.NewLine, AccountSettingsViewModel.Instance.IsInternal, Environment.NewLine,
                    AccountSettingsViewModel.Instance.ApiPrefix));
            }
            catch (Exception exception)
            {
                LoggingViewModel.Instance.Logger.Write(string.Concat("StoreUserSettings:Error ", exception.Message,
                    Environment.NewLine,
                    exception.StackTrace));

                isSuccessful = false;
            }

            return isSuccessful;
        }

        /// <summary>
        ///     Deserializes settings stored in a local file.
        /// </summary>
        /// <returns></returns>
        public AccountSettingsViewModel GetUserSettings()
        {
            var settings = new AccountSettingsViewModel();

            try
            {
                var serializer = new XmlSerializer(typeof (AccountSettingsViewModel));
                if (File.Exists(Path.Combine(AppPath, AppSetup.SettingsFileName)))
                {
                    using (var reader = new StreamReader(Path.Combine(AppPath, AppSetup.SettingsFileName)))
                    {
                        settings = (AccountSettingsViewModel) serializer.Deserialize(reader);
                    }
                }

                LoggingViewModel.Instance.Logger.Write("GetUserSettings:OK");
            }
            catch (Exception exception)
            {
                LoggingViewModel.Instance.Logger.Write(string.Concat("StoreUserSettings:Error ", exception.Message,
                    Environment.NewLine,
                    exception.StackTrace));
            }

            return settings;
        }
    }
}