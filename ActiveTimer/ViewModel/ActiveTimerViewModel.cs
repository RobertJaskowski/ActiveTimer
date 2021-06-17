using ActiveTimer;
using ActiveTimer.Artist.StateControllers;
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

        private List<ArtistStateController> stateControllers;

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

            stateControllers = new List<ArtistStateController>();
            stateControllers.Add(new ActiveArtistStateController());
            stateControllers.Add(new InactiveArtistStateController());
            stateControllers.Add(new ResumedArtistStateController());
            stateControllers.Add(new PausedArtistStateController());
        }

        private void OnSettingsChanged()
        {
            Data.Settings.Blacklist.BlacklistSplitItemsRequireRefreshing = true;
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

            OnTitleStateCheck(title);
            return;



        }

        public void OnTitleStateCheck(string title)
        {
            if (IsTimerPausedByUser())
                return;

            if (IsTitleExcludedFromWindowTitleCapture(title))
                return;

            if (IsSameProcessTitle(title))
                return;

            lastWindow = title;

            var dataBl = Data.Settings.Blacklist;

            if (dataBl.BlacklistEnabled)
                if (dataBl.BlacklistItems.Count > 0)
                {
                    if (!Data.Settings.Blacklist.IsTitleAllowed(title, out BlacklistItem bl))
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

        private bool IsCurrentActiveWindowAllowedWindow(out BlacklistItem blacklistedRuleOnFalse)
        {
            blacklistedRuleOnFalse = null;
            string title = WinApi.GetWindowTitle().ToString().ToLowerInvariant();

            var ret = Data.Settings.Blacklist.IsTitleAllowed(title, out BlacklistItem bl);
            blacklistedRuleOnFalse = bl;
            return ret;
        }
        private bool IsCurrentActiveWindowAllowedWindow()
        {
            string title = WinApi.GetWindowTitle().ToString().ToLowerInvariant();

            return Data.Settings.Blacklist.IsTitleAllowed(title, out BlacklistItem bl);
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

            switch (Artist.ArtistState)
            {
                case ArtistState.ACTIVE:
                    string title = GetWindowTitle().ToString().ToLowerInvariant();
                    if (!IsSameProcessTitle(title))
                        if (IsCurrentActiveWindowAllowedWindow(out BlacklistItem blacklistItem))
                            ArtistPause.Execute(blacklistItem.Rule);

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
                    break;
                case ArtistState.PAUSED:
                    if (!IsTimerPausedByUser() && inputReceivedThisTick)
                    {
                        string titlee = GetWindowTitle().ToString().ToLowerInvariant();
                        if (!IsSameProcessTitle(titlee))
                            if (IsCurrentActiveWindowAllowedWindow())
                                ArtistResume.Execute(null);
                    }
                    break;

                default://inactive / resumed
                    if (inputReceivedThisTick)
                    {
                        ArtistActivate.Execute(null);
                    }
                    break;

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

        public bool IsTitleExcludedFromWindowTitleCapture(string title)
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
    }
}