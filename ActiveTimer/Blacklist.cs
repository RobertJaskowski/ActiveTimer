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

        private BlacklistItem IsTitleBlackListed(string title)
        {
            BlacklistItem ret = IsTitleBlacklistedFromWindowsBlacklistedTitles(title);
            if (ret != null)
                return ret;

            ret = IsTitleBlacklistedFromChromeBlacklistedTitles(title);
            if (ret != null)
            {
                return ret;
            }

            return ret;
        }

        private BlacklistItem IsTitleBlacklistedFromChromeBlacklistedTitles(string title)
        {

            //foreach (BlacklistItem item in chrome)
            //{
            //    SplitProcessAndParameter(item.Rule, out string process, out string settingsTabUrl);
            //    if (title.Contains(settingsTabUrl.ToLower()))
            //    {
            //        ArtistPause.Execute(settingsTabUrl);


            //        return;
            //    }
            //}


            return ChromeTitles?.Find((chromeTitleBlocked) =>
            {
                ChromeHelper.SplitProcessAndParameter(chromeTitleBlocked.Rule, out string process, out string settingsTabUrl);
                if (title.Contains(settingsTabUrl.ToLowerInvariant()))
                    return true;
                return false;
            });
        }

        private BlacklistItem IsTitleBlacklistedFromWindowsBlacklistedTitles(string title)
        {

            //foreach (BlacklistItem item in windows)
            //{
            //    if (title.Contains(item.Rule.ToLower(CultureInfo.InvariantCulture)))
            //    {
            //        ArtistPause.Execute(item.Rule);
            //        Debug.WriteLine(TimeReason);
            //        return;
            //    }
            //}

            return WindowsTitles?.Find((windowTitleBlocked) => title.Contains(windowTitleBlocked.Rule.ToLowerInvariant()));
        }

        public bool IsTitleAllowed(string title, out BlacklistItem blacklistItem)
        {
            blacklistItem = null;

            BlacklistItem bl = IsTitleBlackListed(title);
            if (bl != null)
            {
                blacklistItem = bl;
                return false;
            }

            return true;

        }
        

    }
}
