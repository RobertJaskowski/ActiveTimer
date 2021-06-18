using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveTimer
{
    [Serializable]
    public class Blacklist
    {
        public bool BlacklistEnabled;
        public List<BlacklistItem> BlacklistItems;


        public bool BlacklistSplitItemsRequireRefreshing = true;

        private List<BlacklistItem> _chrome;

        public List<BlacklistItem> ChromeTitles
        {
            get
            {
                AssignBlacklistedItems();
                return _chrome;
            }
        }

        private List<BlacklistItem> _windows;
        public List<BlacklistItem> WindowsTitles
        {
            get
            {
                AssignBlacklistedItems();
                return _windows;
            }
        }

        private void AssignBlacklistedItems()
        {
            if (!BlacklistSplitItemsRequireRefreshing)
                return;
            BlacklistSplitItemsRequireRefreshing = false;

            _chrome = new List<BlacklistItem>();
            _windows = new List<BlacklistItem>();
            foreach (var item in BlacklistItems)
            {
                if (ChromeHelper.IsTitleChrome(item.Rule))
                {
                    if (ChromeHelper.IsChromeTab(item.Rule.ToLower(), out string settingsTabUrl))
                    {
                        ChromeTitles.Add(item);
                    }
                    else
                    {
                        WindowsTitles.Add(item);
                    }
                }
                else
                {
                    WindowsTitles.Add(item);
                }
            }
        }

        private bool IsTitleBlacklisted(string title, out BlacklistItem foundBlacklistItem)
        {
            foundBlacklistItem = BlacklistItems?.Find((blItem) => title.Contains(blItem.Rule.ToLowerInvariant()));
            return foundBlacklistItem != null;

        }

        private bool IsTitleBlackListedSplit(string title,out BlacklistItem foundBlacklistItem)
        {
            AssignBlacklistedItems();

            if (IsTitleBlacklistedFromWindowsBlacklistedTitles(title, out foundBlacklistItem))
                return true;
            

            if (IsTitleBlacklistedFromChromeBlacklistedTitles(title, out foundBlacklistItem))
                return true;


            return false;
        }

        private bool IsTitleBlacklistedFromChromeBlacklistedTitles(string title, out BlacklistItem foundBlacklistItem)
        {
            foundBlacklistItem = ChromeTitles?.Find((chromeTitleBlocked) =>
             {
                 ChromeHelper.SplitProcessAndParameter(chromeTitleBlocked.Rule, out string process, out string settingsTabUrl);
                 if (title.Contains(settingsTabUrl.ToLowerInvariant()))
                     return true;
                 return false;
             });
            return foundBlacklistItem != null;
        }

        private bool IsTitleBlacklistedFromWindowsBlacklistedTitles(string title, out BlacklistItem foundBlacklistItem)
        {
            foundBlacklistItem = WindowsTitles?.Find((windowTitleBlocked) => title.Contains(windowTitleBlocked.Rule.ToLowerInvariant()));
            return foundBlacklistItem != null;
        }

        public bool IsTitleAllowed(string title, out BlacklistItem blacklistedItem)
        {
            if(IsTitleBlacklisted(title, out blacklistedItem))
            {
                return false;
            }
            return true;

        }


    }
}
