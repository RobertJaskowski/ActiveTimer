using System;

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