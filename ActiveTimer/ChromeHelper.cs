using System.Collections.Generic;
using System.Linq;

namespace ActiveTimer
{
    public static class ChromeHelper
    {
        public static bool IsChromeTab(string title)
        {
            if (!IsTitleChrome(title)) return false;

            SplitProcessAndParameter(title, out string process, out string param);
            return !string.IsNullOrEmpty(param);
        }

        public static void SplitProcessAndParameter(string stringToSplit, out string process, out string parameter)
        {
            if (stringToSplit.Contains(':'))
            {
                List<string> spl = stringToSplit.Split(':').ToList<string>();
                if (spl[1].Length > 0)
                {
                    process = spl[0];
                    parameter = spl[1];
                }
                else
                {
                    process = spl[0];
                    parameter = "";
                }
            }
            else
            {
                process = stringToSplit;
                parameter = "";
            }
        }

        public static bool IsChromeTab(string title, out string url)
        {
            if (!IsTitleChrome(title))
            {
                url = "";
                return false;
            }
            SplitProcessAndParameter(title, out string process, out url);
            return !string.IsNullOrEmpty(url);
        }

        public static bool IsTitleChrome(string title)
        {
            return title.Contains("chrome");
        }
    }
}