using ActiveTimer.View;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using ActiveTimer.ViewModel;

namespace ActiveTimer
{
    public class ActiveTimer : ICoreModule
    {

        public string ModuleName
        {
            get
            {
                return "ActiveTimer";
            }
        }

        private UserControl _view;
        public UserControl View
        {
            get
            {
                if (_view == null)
                {
                    _view = new ActiveTimerView();
                    _view.DataContext = new ActiveTimerViewModel(_host, this);
                }

                return _view;
            }
            set { _view = value; }
        }



        private UserControl _settingsView;
        public UserControl SettingsView
        {
            get
            {
                if (_settingsView == null)
                {
                    _settingsView = new ActiveTimerSettingsView();
                    _settingsView.DataContext = new ActiveTimerSettingsViewModel(_host, this);
                }

                return _settingsView;
            }
            set { _settingsView = value; }
        }





        private IModuleController _host;
        public void Init(IModuleController host)
        {
            _host = host;
            Data.Settings = _host.LoadModuleInformation<ActiveTimerSettings>(GetModuleName(), "ActiveTimerSettings");


        }

        public void Start()
        {
            Debug.WriteLine(ModuleName + " started");
        }


        public void Stop()
        {
            ((ActiveTimerViewModel)_view.DataContext).Stop();


            Debug.WriteLine(ModuleName + " stopped");

        }

        public void ReceiveMessage(string message)
        {
            Debug.WriteLine(ModuleName + " received " + message);


        }

        public void OnMinViewEntered()
        {

        }

        public void OnFullViewEntered()
        {

        }

        public ModulePosition GetModulePosition()
        {
            return ModulePosition.MID;
        }

        public UserControl GetModuleUserControlView()
        {
            return View;
        }

        public UserControl GetSettingsUserControlView()
        {
            return SettingsView;
        }

        public string GetModuleName()
        {
            return ModuleName;
        }
    }
}
