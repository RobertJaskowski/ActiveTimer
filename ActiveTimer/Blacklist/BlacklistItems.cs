﻿using System.Collections.Generic;
using System.Configuration;

namespace ActiveTimer
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class BlacklistItems : List<BlacklistItem>
    {
        public BlacklistItems()
        { }
    }
}