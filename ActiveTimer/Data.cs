using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveTimer
{
    public static class Data
    {


        private static ActiveTimerSettings _settings;

        public static ActiveTimerSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new ActiveTimerSettings()
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
