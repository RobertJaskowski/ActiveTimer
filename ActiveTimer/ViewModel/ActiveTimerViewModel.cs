using ActiveTimer.Artist.StateControllers;
using Caravansary.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static ActiveTimer.WinApi;

namespace ActiveTimer.ViewModel
{
    public class ActiveTimerViewModel
    {
        #region INotifyPropertyChanged Members;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged Members;

        #region Properties

        public readonly int maxSecAfkTime = 2;

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

        public String ArtistTimeString => currentTickStateController.GetTimerText().ToString();

        private Visibility _resetButtonVisible;

        public Visibility ResetButtonVisible
        {
            get => _resetButtonVisible;
            set
            {
                _resetButtonVisible = value;
                OnPropertyChanged(nameof(ResetButtonVisible));
            }
        }

        #endregion Properties

        #region Commands

        public ICommand ActiveTimeClicked { get; }
        public ICommand ResetButton { get; }

        #endregion Commands

        public IModuleController _host;
        private ICoreModule _core;

        private List<ArtistStateController> stateControllers;

        public ActiveTimerViewModel(IModuleController host, ICoreModule activeTimer)
        {
            _host = host;
            _core = activeTimer;

            _artistModel = new ArtistModel(TimeSpan.FromSeconds(0));

            ActiveTimeClicked = new RelayCommand((e) => currentTickStateController.OnTimeClicked());
            ResetButton = new RelayCommand((e) => Artist.ActiveTime = TimeSpan.FromSeconds(0));

            CreateArtistStateTimers();

            _host.HookWindowSwitchEvent(WindowSwitched);

            ActiveTimer.OnMinViewEnteredEvent += OnMinViewEntered;
            ActiveTimer.OnFullViewEnteredEvent += OnFullViewEntered;

            stateControllers = new List<ArtistStateController>
            {
                new ActiveArtistStateController(this),
                new InactiveArtistStateController(this),
                new ResumedArtistStateController(this),
                new PausedArtistStateController(this)
            };

            Artist.PropertyChanged += OnArtistPropertyChanged;

            currentTickStateController = GetStateController("Active");
        }

        public void UpdateActiveTime(int second = 1)
        {
            Artist.ActiveTime += TimeSpan.FromSeconds(second);
            _host.OnEventTriggered(_core.ModuleName, "TimeUpdate", Artist.ActiveTime);
        }

        public void ChangeState(Type toType, object data = null)
        {
            var s = GetStateController(toType);
            ChangeState(s, data);
        }

        public void ChangeState(string stateName, object data = null)
        {
            var s = GetStateController(stateName);
            ChangeState(s, data);
        }

        private void ChangeState(ArtistStateController toState, object data = null)
        {
            currentTickStateController = toState;
            currentTickStateController.OnEnter(data);

            RefreshTimeView();
        }

        public ArtistStateController GetStateController(Type type)
        {
            return stateControllers?.Find((e) => e.GetType() == type);
        }

        public ArtistStateController GetStateController(string name)
        {
            return stateControllers?.Find((e) => e.IsSameStateByName(name));
        }

        private void OnArtistPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ArtistTimeString));
        }

        private void OnMinViewEntered()
        {
            ResetButtonVisible = Visibility.Collapsed;
        }

        private void OnFullViewEntered()
        {
            ResetButtonVisible = Visibility.Visible;
        }

        public void PlaySound(string soundName)
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
            if (currentTickStateController.IsTransitionAvailable(out string artistState))
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

        private ArtistStateController prevTickStateController;
        private ArtistStateController currentTickStateController;
        private bool IsControllerTickedThisTick;
        private bool IsTransitionCheckedThisTick;
        private bool IsTransitionHappenedThisTick;
        private bool IsTransitionAvailableThisTick;

        public int currentCheckingAfkTime = 0;

        private ArtistStateController GetCurrentStateController()
        {
            return stateControllers.First((sc) => sc.IsSameStateByName(Artist.ArtistState));
        }

        private void TimerTick(object sender, EventArgs e)
        {
            IsControllerTickedThisTick = false;
            IsTransitionCheckedThisTick = false;
            IsTransitionHappenedThisTick = false;
            InputReceivedThisTick = false;
            CheckAndSetForInputReceived();

            currentTickStateController = GetCurrentStateController();

            var tempTitle = WinApi.GetWindowTitle().ToString().ToLowerInvariant();

            if (IsTitleValidWindowTitleCapture(tempTitle))
                OnTitleCaptured(tempTitle);

            if (!IsTransitionCheckedThisTick)
            {
                IsTransitionCheckedThisTick = true;
                if (currentTickStateController.IsTransitionAvailable(out string artistState))
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

            RefreshTimeView();
        }

        private void RefreshTimeView()
        {
            OnPropertyChanged(nameof(ArtistTimeString));
        }

        public float topPercentFilled = 0;
        public int timeSecToFillTopBar = 0;

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

        public void Stop()
        {
            _host.UnHookWindowSwitchEvent(WindowSwitched);
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