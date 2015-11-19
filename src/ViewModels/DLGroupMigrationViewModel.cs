using System;
using System.ComponentModel;
using System.Windows.Threading;
using Hummingbird.Models;

namespace Hummingbird.ViewModels
{
    public class DlGroupMigrationViewModel : INotifyPropertyChanged
    {
        private static DlGroupMigrationViewModel _instance;
        private static readonly object Padlock = new object();
        private bool _backupControlsEnabled;
        private bool _bulkAddControlsEnabled;
        private DistributionList _currentlyActiveDl;
        private DistributionList _bulkAddDistributionList;
        private bool _isAliasValid;
        private bool _restoreControlsEnabled;

        public DlGroupMigrationViewModel()
        {
            BackupControlsEnabled = true;
            RestoreControlsEnabled = true;
            BulkAddControlsEnabled = true;
        }

        public static DlGroupMigrationViewModel Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new DlGroupMigrationViewModel());
                }
            }
            set { _instance = value; }
        }

        public bool IsAliasValid
        {
            get { return _isAliasValid; }
            set
            {
                if (_isAliasValid == value) return;
                _isAliasValid = value;
                NotifyPropertyChanged("IsAliasValid");
            }
        }

        public bool BackupControlsEnabled
        {
            get { return _backupControlsEnabled; }
            set
            {
                if (_backupControlsEnabled == value) return;
                _backupControlsEnabled = value;
                NotifyPropertyChanged("BackupControlsEnabled");
            }
        }

        public bool BulkAddControlsEnabled
        {
            get { return _bulkAddControlsEnabled; }
            set
            {
                if (_bulkAddControlsEnabled == value) return;
                _bulkAddControlsEnabled = value;
                NotifyPropertyChanged("BulkAddControlsEnabled");
            }
        }

        public DistributionList CurrentlyActiveDl
        {
            get { return _currentlyActiveDl; }
            set
            {
                if (_currentlyActiveDl == value) return;
                _currentlyActiveDl = value;
                NotifyPropertyChanged("CurrentlyActiveDl");
            }
        }

        public DistributionList BulkAddDistributionList
        {
            get { return _bulkAddDistributionList; }
            set
            {
                if (_bulkAddDistributionList == value) return;
                _bulkAddDistributionList = value;
                NotifyPropertyChanged("BulkAddDistributionList");
            }
        }

        public bool RestoreControlsEnabled
        {
            get { return _restoreControlsEnabled; }
            set
            {
                if (_restoreControlsEnabled == value) return;
                _restoreControlsEnabled = value;
                NotifyPropertyChanged("RestoreControlsEnabled");
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