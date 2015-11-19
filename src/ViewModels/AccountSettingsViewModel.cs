using System;
using System.ComponentModel;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Hummingbird.ViewModels
{
    [XmlRoot("HummingbirdSettings")]
    public class AccountSettingsViewModel : INotifyPropertyChanged
    {
        private static AccountSettingsViewModel _instance;
        private static readonly object Padlock = new object();
        private string _apiPrefix;
        private bool _isInternal;
        private string _serverUrl;

        public static AccountSettingsViewModel Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new AccountSettingsViewModel());
                }
            }
            set { _instance = value; }
        }

        public string ServerUrl
        {
            get { return _serverUrl; }
            set
            {
                if (_serverUrl == value) return;
                _serverUrl = value;
                NotifyPropertyChanged("ServerUrl");
            }
        }

        public string ApiPrefix
        {
            get { return _apiPrefix; }
            set
            {
                if (_apiPrefix == value) return;
                _apiPrefix = value;
                NotifyPropertyChanged("ApiPrefix");
            }
        }

        public bool IsInternal
        {
            get { return _isInternal; }
            set
            {
                if (_isInternal == value) return;
                _isInternal = value;
                NotifyPropertyChanged("IsInternal");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                await Dispatcher.CurrentDispatcher.BeginInvoke(new Action(
                    () => { PropertyChanged(this, new PropertyChangedEventArgs(info)); }));
            }
        }
    }
}