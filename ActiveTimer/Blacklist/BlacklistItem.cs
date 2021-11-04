using System;
using System.ComponentModel;

namespace ActiveTimer
{
    [Serializable]
    public class BlacklistItem : INotifyPropertyChanged
    {
        private string _rule;

        public string Rule
        {
            get
            {
                return _rule;
            }
            set
            {
                _rule = value;
                OnPropertyChanged(nameof(Rule));
            }
        }

        public BlacklistItem()
        { }

        public BlacklistItem(string rule)
        {
            Rule = rule;
        }

        #region INotifyPropertyChanged Members;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged Members;
    }
}