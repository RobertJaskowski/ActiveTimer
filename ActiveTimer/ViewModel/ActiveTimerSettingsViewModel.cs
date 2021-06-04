using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ActiveTimer.ViewModel
{
    public class ActiveTimerSettingsViewModel : BaseViewModel
    {
        #region Properties

        public bool CheckBoxBlacklistEnabled
        {
            get => Data.Settings.BlacklistEnabled;
            set
            {
                Data.Settings.BlacklistEnabled = value;

                OnPropertyChanged(nameof(CheckBoxBlacklistEnabled));
                SaveSettings();
            }
        }

        public bool PlaySoundCheckbox
        {
            get { return Data.Settings.PlayChangeSound; }
            set
            {
                Data.Settings.PlayChangeSound = value;

                OnPropertyChanged(nameof(PlaySoundCheckbox)); SaveSettings();
            }
        }

        public int PlaySoundVolumeSlider
        {
            get { return Data.Settings.PlayChangeVolume; }
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 100)
                    value = 100;

                Data.Settings.PlayChangeVolume = value;

                OnPropertyChanged(nameof(PlaySoundVolumeSlider)); SaveSettings();
            }
        }

        private ObservableCollection<BlacklistItem> _blacklistItems;

        public ObservableCollection<BlacklistItem> BlacklistItems
        {
            get
            {
                if (_blacklistItems == null)
                {
                    _blacklistItems = new ObservableCollection<BlacklistItem>(Data.Settings.BlacklistItems);
                }

                return _blacklistItems;
            }
            set
            {
                _blacklistItems = value;

                OnPropertyChanged(nameof(BlacklistItems));
            }
        }

        #endregion Properties

        #region Commands

        private ICommand _createNewBlacklistItem;

        public ICommand CreateNewBlacklistItem
        {
            get
            {
                if (_createNewBlacklistItem == null)
                    _createNewBlacklistItem = new RelayCommand(
                       (object o) =>
                       {
                           var k = new BlacklistItem("empty");
                           _blacklistItems.Add(k);

                           Data.Settings.BlacklistItems.Add(k);
                           SaveSettings();
                       },
                       (object o) =>
                       {
                           return true;
                       });

                return _createNewBlacklistItem;
            }
        }

        private RelayCommand _removeBlacklistItem;

        public RelayCommand RemoveBlacklistItem
        {
            get
            {
                if (_removeBlacklistItem == null)
                    _removeBlacklistItem = new RelayCommand(
                       (object o) =>
                       {
                           if (o is BlacklistItem)
                           {
                               BlacklistItems.Remove(o as BlacklistItem);

                               Data.Settings.BlacklistItems.Remove(o as BlacklistItem);
                               SaveSettings();
                           }
                       },
                       (object o) =>
                       {
                           return true;
                       });

                return _removeBlacklistItem;
            }
        }

        #endregion Commands

        private IModuleController _host;
        private ICoreModule coreModule;

        public ActiveTimerSettingsViewModel(IModuleController host, ICoreModule activeTimer)
        {
            _host = host;
            coreModule = activeTimer;
        }

        public void SaveSettings()
        {
            _host.SaveModuleInformation(coreModule.GetModuleName(), "ActiveTimerSettings", Data.Settings);
        }
    }
}