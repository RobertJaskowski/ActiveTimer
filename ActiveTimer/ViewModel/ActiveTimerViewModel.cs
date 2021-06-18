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

        public readonly int maxSecAfkTime = 2;
        public int currentCheckingAfkTime = 0;

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

        public IModuleController _host;
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

            stateControllers = new List<ArtistStateController>
            {
                new ActiveArtistStateController(this),
                new InactiveArtistStateController(this),
                new ResumedArtistStateController(this),
                new PausedArtistStateController(this)
            };


            ArtistActivate?.Execute(null);
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

            //if (IsTimerPausedByUser())
            //    return;
            if (IsTitleDifferentAndValidTitleCapture(title))
                OnNewTitleCaptured(title);

            //var dataBl = Data.Settings.Blacklist;

            //if (dataBl.BlacklistEnabled)
            //    if (dataBl.BlacklistItems.Count > 0)
            //    {
            //        if (!Data.Settings.Blacklist.IsTitleAllowed(title, out BlacklistItem bl))
            //        {
            //            ArtistPause.Execute(bl.Rule);
            //            Debug.WriteLine(TimeReason);
            //            return;
            //        }
            //    }



            //if (Artist.ArtistState != ArtistState.ACTIVE)
            //    if (IsTimerPausedByUser())
            //    {
            //        ArtistResume.Execute(null);
            //    }

        }
        public void OnTitleCaptured(string title)
        {
            CurrentWindowTitle = title;

            if (!IsTitleSameTitleAsPrevious(title))
                OnNewTitleCaptured(title);
        }

        public void OnNewTitleCaptured(string title)
        {


            IsTransitionCheckedThisTick = true;
            if (currentTickStateController.IsTransitionAvailable(out ArtistState artistState))
            {
                IsTransitionAvailableThisTick = true;
                IsTransitionHappenedThisTick = true;
                currentTickStateController.TransitionToNextState();
            }
            else
                IsTransitionAvailableThisTick = false;



            PreviousWindowTitle = CurrentWindowTitle;

        }




        private void CreateArtistStateTimers()
        {
            DispatcherTimer timerArtistStateCheck = new DispatcherTimer();
            timerArtistStateCheck.Interval = TimeSpan.FromSeconds(1);
            timerArtistStateCheck.Tick += new EventHandler(TimerTick);
            timerArtistStateCheck.Start();
        }

        ArtistStateController prevTickStateController;
        ArtistStateController currentTickStateController;
        private bool IsControllerTickedThisTick;
        private bool IsTransitionCheckedThisTick;
        private bool IsTransitionHappenedThisTick;
        private bool IsTransitionAvailableThisTick;


        private ArtistStateController GetCurrentStateController()
        {
            return stateControllers.First((sc) => sc.IsThisStateCurrentStateOfArtist());
        }



        private void TimerTick(object sender, EventArgs e)
        {

            IsControllerTickedThisTick = false;
            IsTransitionCheckedThisTick = false;
            IsTransitionHappenedThisTick = false;
            InputReceivedThisTick = false;
            CheckAndSetForInputReceived();

            currentTickStateController = GetCurrentStateController();



            var tempTitle = GetWindowTitle().ToString().ToLowerInvariant();



            if (IsTitleValidWindowTitleCapture(tempTitle))
                OnTitleCaptured(tempTitle);

            if (!IsTransitionCheckedThisTick)
            {
                IsTransitionCheckedThisTick = true;
                if (currentTickStateController.IsTransitionAvailable(out ArtistState artistState))
                {
                    IsTransitionAvailableThisTick = true;
                }
                else
                    IsTransitionAvailableThisTick = false;
            }


            if (!IsTransitionHappenedThisTick && IsTransitionAvailableThisTick)
            {
                IsTransitionHappenedThisTick = true;
                currentTickStateController.TransitionToNextState();
            }

            if (!IsControllerTickedThisTick && !IsTransitionHappenedThisTick)
            {
                IsControllerTickedThisTick = true;
                currentTickStateController.Tick();
            }

            prevTickStateController = currentTickStateController;

        }

        public bool InputReceivedThisTick = false;
        public uint lastActive = 0;

        private bool CheckAndSetForInputReceived()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (UInt32)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                if (lastInputInfo.dwTime != lastActive)
                {
                    lastActive = lastInputInfo.dwTime;
                    InputReceivedThisTick = true;
                    return true;
                }
            }

            return false;
        }

        public float topPercentFilled = 0;
        public int timeSecToFillTopBar = 0;




        public void Stop()
        {
            _host.UnHookWindowSwitchEvent(WindowSwitched);
            Data.OnSettingsChanged -= OnSettingsChanged;
        }


        public bool IsCurrentActiveWindowTargetableValidWindow()
        {

            string title = WinApi.GetWindowTitle().ToString().ToLowerInvariant();

            if (!IsTitleValidWindowTitleCapture(title))
                return false;


            return true;
        }

        public bool IsTitleDifferentAndValidTitleCapture(string title)
        {
            return !IsTitleSameTitleAsPrevious(title) && IsTitleValidWindowTitleCapture(title);
        }

        public string CurrentWindowTitle;
        public string PreviousWindowTitle;
        public bool IsTitleSameTitleAsPrevious(string title)
        {
            if (PreviousWindowTitle == null)
                return false;

            if (PreviousWindowTitle.Equals(title))
                return true;
            return false;
        }

        public bool IsTimerPausedByUser()
        {
            if (TimeReason != null)
                if (TimeReason.ToLower().Contains("user"))
                    return true;
            return false;
        }

        public bool IsTitleValidWindowTitleCapture(string title)
        {
            if (string.IsNullOrEmpty(title))
                return false;

            if (IsTitleDesignTime(title))
                return false;

            if (IsTitleMainApplication(title))
                return false;

            if (IsTitleException(title))
                return false;

            return true;
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
            return title.Contains("task switching") || title.Contains("search");
        }
    }
}