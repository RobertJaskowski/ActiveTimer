using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveTimer
{
    [Serializable]
    public class ActiveTimerSettings
    {
        public Blacklist Blacklist;
        public bool PlayChangeSound;
        public int PlayChangeVolume;
    }
}