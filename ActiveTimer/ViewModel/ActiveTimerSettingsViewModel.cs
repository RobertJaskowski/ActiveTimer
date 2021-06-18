using Caravansary.SDK;
using System.Collections.Specialized;
using System.Linq;

namespace ActiveTimer.ViewModel
{
    public class ActiveTimerSettingsViewModel : PageModelBase
    {
        #region Properties

        public bool CheckBoxBlacklistEnabled
        {
            get => Data.Settings.Blacklist.BlacklistEnabled;
            set
            {
                Data.Settings.Blacklist.BlacklistEnabled = value;

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

        private TrulyObservableCollection<BlacklistItem> _blacklistItems;

        public TrulyObservableCollection<BlacklistItem> BlacklistItems
        {
            get
            {
                if (_blacklistItems == null)
                {
                    _blacklistItems = new(Data.Settings.Blacklist.BlacklistItems);
                }

                return _blacklistItems;
            }
            set
            {
                _blacklistItems = value;

                Data.Settings.Blacklist.BlacklistItems = _blacklistItems.ToList();

                OnPropertyChanged(nameof(BlacklistItems));
                SaveSettings();
            }
        }

        #endregion Properties

        #region Commands

        private RelayCommand _createNewBlacklistItem;

        public RelayCommand CreateNewBlacklistItem
        {
            get
            {
                if (_createNewBlacklistItem == null)
                    _createNewBlacklistItem = new RelayCommand(
                       (object o) =>
                       {
                           var k = new BlacklistItem("empty");
                           BlacklistItems.Add(k);
                           Data.Settings.Blacklist.BlacklistItems.Add(k);

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

                               Data.Settings.Blacklist.BlacklistItems.Remove(o as BlacklistItem);
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
            BlacklistItems.CollectionChanged += ColectionChanged;
        }

        private void ColectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SaveSettings();
        }

        public void SaveSettings()
        {
            _host.SaveModuleInformation(coreModule.GetModuleName(), "ActiveTimerSettings", Data.Settings);
            Data.OnSettingsChanged?.Invoke();
        }
    }
}