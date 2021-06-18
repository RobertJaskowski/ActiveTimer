
using System;
using System.ComponentModel;

public class ArtistModel : INotifyPropertyChanged
{

    string _currentArtistState;
    public string ArtistState
    {
        get => _currentArtistState; set
        {
            _currentArtistState = value;
            OnPropertyChanged(nameof(ArtistState));
        }
    }
    //public bool ArtistActive
    //{
    //    get
    //    {
    //        return ArtistState.Equals("Active");
    //    }
    //    set
    //    {

    //        OnPropertyChanged(nameof(ArtistState));
    //    }
    //}todo remove


    TimeSpan _activeTime;
    public TimeSpan ActiveTime
    {
        get
        {
            return _activeTime;
        }
        set
        {
            _activeTime = value;
            OnPropertyChanged(nameof(ActiveTime));
        }
    }

    public ArtistModel(TimeSpan timespan)
    {
        ActiveTime = timespan;
        ArtistState = "Active";
    }




    #region INotifyPropertyChanged Members;
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;

        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion
}





