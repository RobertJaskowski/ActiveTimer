using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveTimer
{
    public static class Data
    {
        public static Action OnSettingsChanged;

        private static ActiveTimerSettings _settings;

        public static ActiveTimerSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new ActiveTimerSettings()
                    {

                    };

                }

                if (_settings.Blacklist == null)
                {
                    _settings.Blacklist = new Blacklist()
                    {
                        BlacklistEnabled = true,
                        BlacklistItems = new List<BlacklistItem>()
                    };
                }


                return _settings;
            }
            internal set
            {
                _settings = value;


            }
        }
    }
}
