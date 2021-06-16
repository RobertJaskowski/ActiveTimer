using ActiveTimer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static WinApi;

namespace ActiveTimer.ViewModel
{
    public class ActiveTimerViewModel : ObservableObject
    {
        #region Properties

        private const int maxSecAfkTime = 2;
        private int currentCheckingAfkTime = 0;

        private ArtistModel _artistModel;

        public ArtistModel Artist
        {
            get => _artistModel;
            set
            {
                _artistModel = value;
                OnPropertyChanged(nameof(Artist));
            }
        }

        private string _timeReason;

        public String TimeReason
        {
            get => _timeReason;
            set
            {
                _timeReason = value;
                OnPropertyChanged(nameof(TimeReason));
            }
        }

        private string _artistTimeString;

        public String ArtistTimeString
        {
            get
            {
                return _artistTimeString;
            }
            set
            {
                switch (Artist.ArtistState)
                {
                    case ArtistState.ACTIVE:
                        _artistTimeString = value;
                        break;

                    case ArtistState.INACTIVE:
                        _artistTimeString = value;
                        break;

                    case ArtistState.PAUSED:
                        _artistTimeString = "|| " + value + (!string.IsNullOrEmpty(TimeReason) ? " by " + TimeReason : "");
                        break;

                    case ArtistState.RESUMED:
                        _artistTimeString = "|> " + value + (!string.IsNullOrEmpty(TimeReason) ? " by " + TimeReason : "");
                        break;
                }
                OnPropertyChanged(nameof(ArtistTimeString));
            }
        }

        private Visibility _resetButtonVisible;

        public Visibility ResetButtonVisible
        {
            get { return _resetButtonVisible; }
            set { _resetButtonVisible = value; OnPropertyChanged(nameof(ResetButtonVisible)); }
        }

        #endregion Properties

        #region Commands

        private ICommand _activeTimeUpdate;

        public ICommand ActiveTimeUpdate1Sec
        {
            get
            {
                if (_activeTimeUpdate == null)

                    _activeTimeUpdate = new RelayCommand(
                       (object o) =>
                       {
                           Artist.ActiveTime += TimeSpan.FromSeconds(1);

                           ArtistTimeString = Artist.ActiveTime.ToString();

                           _host.OnEventTriggered(_core.ModuleName, "TimeUpdate", Artist.ActiveTime);
                       },
                       (object o) =>
                       {
                           return true;
                       });

                return _activeTimeUpdate;
            }
        }

        private ICommand _activeTimeClicked;

        public ICommand ActiveTimeClicked
        {
            get
            {
                if (_activeTimeClicked == null)
                    _activeTimeClicked = new RelayCommand(
                       (object o) =>
                       {

                           if (Artist.ArtistState == ArtistState.ACTIVE || Artist.ArtistState == ArtistState.INACTIVE)
                           {
                               ArtistPause.Execute("user");
                           }
                           else if (Artist.ArtistState == ArtistState.PAUSED)
                           {
                               ArtistResume.Execute(null);
                           }
                       },
                       (object o) =>
                       {
                           return true;
                       });

                return _activeTimeClicked;
            }
        }

        private ICommand _artistActivate;

        public ICommand ArtistActivate
        {
            get
            {
                if (_artistActivate == null)
                    _artistActivate = new RelayCommand(
                       (object o) =>
                       {
                           Artist.ArtistState = ArtistState.ACTIVE;


                           if (Data.Settings.PlayChangeSound)
                           {
                               PlaySound("Rise02.wav");
                           }

                           _host.SendMessage("MainBar", "color|||" + "178|||255|||89");
                           _host.SendMessage("ActiveTimer", "IsActive");
                       },
                       (object o) =>
                       {
                           return true;
                       });

                return _artistActivate;
            }
        }

        private ICommand _artistDeactivate;

        public ICommand ArtistDeactivate
        {
            get
            {
                if (_artistDeactivate == null)
                    _artistDeactivate = new RelayCommand(
                       (object o) =>
                       {
                           Artist.ArtistState = ArtistState.INACTIVE;


                           ArtistTimeString = Artist.ActiveTime.ToString();

                           if (Data.Settings.PlayChangeSound)
                           {
                               PlaySound("ring.wav");
                           }

                           //Color c = Color.FromArgb(255, 221, 44, 0);
                           // MainBarModule.SetBarColor(c);
                           _host.SendMessage("MainBar", "color|||" + "221|||44|||0");
                           _host.SendMessage("ActiveTimer", "IsNotActive");
                       },
                       (object o) =>
                       {
                           return true;
                       });

                return _artistDeactivate;
            }
        }

        private ICommand _artistPause;

        public ICommand ArtistPause
        {
            get
            {
                if (_artistPause == null)
                    _artistPause = new RelayCommand(
                       (object o) =>
                       {
                           Artist.ArtistState = ArtistState.PAUSED;

                           if (o is string)
                               TimeReason = (string)o;

                           ArtistTimeString = Artist.ActiveTime.ToString();


                           _host.SendMessage("MainBar", "color|||" + "221|||44|||0");

                           _host.SendMessage("ActiveTimer", "IsNotActive");
                       },
                       (object o) =>
                       {
                           return true;
                       });

                return _artistPause;
            }
        }

        private ICommand _artistResume;

        public ICommand ArtistResume
        {
            get
            {
                if (_artistResume == null)
                    _artistResume = new RelayCommand(
                       (object o) =>
                       {
                           Artist.ArtistState = ArtistState.RESUMED;
                           if (o is string)
                               TimeReason = "";

                           ArtistTimeString = Artist.ActiveTime.ToString();

                           _host.SendMessage("MainBar", "color|||" + "221|||44|||0");

                           _host.SendMessage("ActiveTimer", "IsActive");

                       },
                       (object o) =>
                       {
                           return true;
                       });

                return _artistResume;
            }
        }

        private ICommand _resetButton;

        public ICommand ResetButton
        {
            get
            {
                if (_resetButton == null)
                    _resetButton = new RelayCommand(
                       (object o) =>
                       {
                           Artist.ActiveTime = TimeSpan.FromSeconds(0);
                       },
                       (object o) =>
                       {
                           return true;
                       });

                return _resetButton;
            }
        }

        #endregion Commands

        private IModuleController _host;
        private ICoreModule _core;

        public ActiveTimerViewModel(IModuleController host, ICoreModule activeTimer)
        {
            _host = host;
            _core = activeTimer;

            _artistModel = new ArtistModel(TimeSpan.FromSeconds(0));

            CreateArtistStateTimers();

            _host.HookWindowSwitchEvent(WindowSwitched);

            ActiveTimer.OnMinViewEnteredEvent += OnMinViewEntered;
            ActiveTimer.OnFullViewEnteredEvent += OnFullViewEntered;

            Data.OnSettingsChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged()
        {
            splitBlacklist.BlacklistItemsRequireRefreshing = true;
        }

        private void OnMinViewEntered()
        {
            ResetButtonVisible = Visibility.Collapsed;
        }

        private void OnFullViewEntered()
        {
            ResetButtonVisible = Visibility.Visible;
        }

        private void PlaySound(string soundName)
        {
            if (!File.Exists("Modules/ActiveTimer/Resources/" + soundName)) return;

            MediaPlayer player = new MediaPlayer();
            player.Open(new Uri("Modules/ActiveTimer/Resources/" + soundName, UriKind.Relative));
            player.Volume = Data.Settings.PlayChangeVolume / 100.0f;
            player.Play();
        }


        private void WindowSwitched(WindowSwitchedArgs e)
        {
            string title = e.Title.ToLower().Trim();
            Debug.WriteLine("window switched callback " + title);

            OnNewTitleFound(title);
            return;





        }

        private BlacklistItem IsTitleBlackListed(string title)
        {
            BlacklistItem ret = IsTitleBlacklistedFromWindowsBlacklistedTitles(title);
            if (ret != null)
                return ret;

            ret = IsTitleBlacklistedFromChromeBlacklistedTitles(title);
            if (ret != null)
            {
                return ret;
            }

            return ret;
        }

        private BlacklistItem IsTitleBlacklistedFromChromeBlacklistedTitles(string title)
        {

            //foreach (BlacklistItem item in chrome)
            //{
            //    SplitProcessAndParameter(item.Rule, out string process, out string settingsTabUrl);
            //    if (title.Contains(settingsTabUrl.ToLower()))
            //    {
            //        ArtistPause.Execute(settingsTabUrl);


            //        return;
            //    }
            //}


            return splitBlacklist.chrome.Find((chromeTitleBlocked) =>
            {
                SplitProcessAndParameter(chromeTitleBlocked.Rule, out string process, out string settingsTabUrl);
                if (title.Contains(settingsTabUrl.ToLowerInvariant()))
                    return true;
                return false;
            });
        }

        private BlacklistItem IsTitleBlacklistedFromWindowsBlacklistedTitles(string title)
        {

            //foreach (BlacklistItem item in windows)
            //{
            //    if (title.Contains(item.Rule.ToLower(CultureInfo.InvariantCulture)))
            //    {
            //        ArtistPause.Execute(item.Rule);
            //        Debug.WriteLine(TimeReason);
            //        return;
            //    }
            //}

            return splitBlacklist.windows.Find((windowTitleBlocked) => title.Contains(windowTitleBlocked.Rule.ToLowerInvariant()));
        }


        private SplitBlacklist splitBlacklist = new();

        private class SplitBlacklist
        {
            public bool BlacklistItemsRequireRefreshing = true;


            private List<BlacklistItem> _chrome;

            public List<BlacklistItem> chrome
            {
                get
                {
                    AssignBlacklistedItems();
                    return _chrome;
                }
            }

            private List<BlacklistItem> _windows;
            public List<BlacklistItem> windows
            {
                get
                {
                    AssignBlacklistedItems();
                    return _windows;
                }
            }

            private void AssignBlacklistedItems()
            {
                if (!BlacklistItemsRequireRefreshing)
                    return;
                BlacklistItemsRequireRefreshing = false;

                _chrome = new List<BlacklistItem>();
                _windows = new List<BlacklistItem>();
                foreach (var item in Data.Settings.BlacklistItems)
                {
                    if (IsTitleChrome(item.Rule))
                    {
                        if (IsChromeTab(item.Rule.ToLower(), out string settingsTabUrl))
                        {
                            chrome.Add(item);
                        }
                        else
                        {
                            windows.Add(item);
                        }
                    }
                    else
                    {
                        windows.Add(item);
                    }
                }
            }
        }

        public void OnNewTitleFound(string title)
        {
            if (IsTimerPausedByUser())
                return;

            if (IsTitleExcludedFromWindowTitleCapture(title))
                return;

            if (IsSameProcessTitle(title))
                return;

            lastWindow = title;

            if (Data.Settings.BlacklistEnabled)
                if (Data.Settings.BlacklistItems.Count > 0)
                {
                    if (!IsTitleAllowed(title, out BlacklistItem bl))
                    {
                        ArtistPause.Execute(bl.Rule);
                        Debug.WriteLine(TimeReason);
                        return;
                    }
                }



            if (Artist.ArtistState != ArtistState.ACTIVE)
                if (IsTimerPausedByUser())
                {
                    ArtistResume.Execute(null);
                }
        }

        private bool IsCurrentActiveWindowAllowedWindow()
        {
            string title = WinApi.GetWindowTitle().ToString().ToLowerInvariant();

            return IsTitleAllowed(title, out BlacklistItem bl);
        }

        public bool IsTitleAllowed(string title, out BlacklistItem blacklistItem)
        {
            blacklistItem = null;

            if (IsTitleExcludedFromWindowTitleCapture(title)) return false;

            BlacklistItem bl = IsTitleBlackListed(title);
            if (bl != null)
            {
                blacklistItem = bl;
                return false;
            }

            return true;

        }

        public static bool IsChromeTab(string title)
        {
            if (!IsTitleChrome(title)) return false;

            SplitProcessAndParameter(title, out string process, out string param);
            return !string.IsNullOrEmpty(param);
        }

        public static void SplitProcessAndParameter(string stringToSplit, out string process, out string parameter)
        {
            if (stringToSplit.Contains(':'))
            {
                List<string> spl = stringToSplit.Split(':').ToList<string>();
                if (spl[1].Length > 0)
                {
                    process = spl[0];
                    parameter = spl[1];
                }
                else
                {
                    process = spl[0];
                    parameter = "";
                }
            }
            else
            {
                process = stringToSplit;
                parameter = "";
            }
        }

        public static bool IsChromeTab(string title, out string url)
        {
            if (!IsTitleChrome(title))
            {
                url = "";
                return false;
            }
            SplitProcessAndParameter(title, out string process, out url);
            return !string.IsNullOrEmpty(url);
        }

        private void CreateArtistStateTimers()
        {
            DispatcherTimer timerArtistStateCheck = new DispatcherTimer();
            timerArtistStateCheck.Interval = TimeSpan.FromSeconds(1);
            timerArtistStateCheck.Tick += new EventHandler(TimerTick);
            timerArtistStateCheck.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {


            CheckStateChanges();

            switch (Artist.ArtistState)
            {
                case ArtistState.ACTIVE:
                    OnArtistStateActiveTick();
                    break;

                case ArtistState.INACTIVE:
                    OnArtistStateInactiveTick();
                    break;

                case ArtistState.PAUSED:
                    break;

                case ArtistState.RESUMED:
                    OnArtistStateResumeTick();
                    break;
            }
        }

        private void CheckStateChanges()
        {
            inputReceivedThisTick = false;
            CheckIfAnyInputReceived();

            if (Artist.ArtistState == ArtistState.PAUSED)
            {
                if (!IsTimerPausedByUser() && inputReceivedThisTick && IsCurrentActiveWindowAllowedWindow())
                    ArtistResume.Execute(null);
                return;
            }
            else if (!Artist.ArtistActive)
            {
                if (inputReceivedThisTick)
                {
                    ArtistActivate.Execute(null);
                }

            }
            else
            {

                if (inputReceivedThisTick)
                {
                    currentCheckingAfkTime = 0;
                }
                else
                {
                    currentCheckingAfkTime++;
                    if (currentCheckingAfkTime > maxSecAfkTime)
                    {
                        currentCheckingAfkTime = 0;
                        ArtistDeactivate.Execute(null);
                    }
                }
            }
        }

        private bool inputReceivedThisTick = false;
        private uint lastActive = 0;

        private bool CheckIfAnyInputReceived()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (UInt32)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                if (lastInputInfo.dwTime != lastActive)
                {
                    lastActive = lastInputInfo.dwTime;
                    inputReceivedThisTick = true;
                    return true;
                }
            }

            return false;
        }

        private float topPercentFilled = 0;
        public int timeSecToFillTopBar = 0;

        private void OnArtistStateActiveTick()
        {
            ActiveTimeUpdate1Sec.Execute(null);


            if (timeSecToFillTopBar == 0)
                return;

            float rest = (float)(Artist.ActiveTime.TotalSeconds % (timeSecToFillTopBar));
            topPercentFilled = Utils.ToProcentage(rest, 0, timeSecToFillTopBar);

            _host.SendMessage("MainBar", "value|||" + topPercentFilled);
        }

        private void OnArtistStateInactiveTick()
        {
        }

        private void OnArtistStateResumeTick()
        {
            ActiveTimeUpdate1Sec.Execute(null);


            TimeReason = "";
        }

        public void Stop()
        {
            _host.UnHookWindowSwitchEvent(WindowSwitched);
            Data.OnSettingsChanged -= OnSettingsChanged;
        }

        private bool IsTitleExcludedFromWindowTitleCapture(string title)
        {
            return IsTitleDesignTime(title) || IsTitleMainApplication(title) || IsTitleException(title);
        }

        private bool IsTitleDesignTime(string title)
        {
            return title.Contains("visual");
        }

        private bool IsTitleMainApplication(string title)
        {
            return title.Contains("caravansary") || title.Contains("caravaneer");
        }

        private bool IsTitleException(string title)
        {
            return title.Contains("task switching");
        }


        public static bool IsTitleChrome(string title)
        {
            return title.Contains("chrome");
        }

        private string lastWindow;
        private bool IsSameProcessTitle(string title)
        {
            if (lastWindow == null)
                return false;

            if (lastWindow.Equals(title))
                return true;
            return false;
        }

        private bool IsTimerPausedByUser()
        {
            if (TimeReason != null)
                if (TimeReason.ToLower().Contains("user"))
                    return true;
            return false;
        }
    }
}