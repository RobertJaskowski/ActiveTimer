using ActiveTimer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class ArtistStateController
{
    protected ActiveTimerViewModel main;

    public ArtistStateController(ActiveTimerViewModel mainVM)
    {
        main = mainVM;
    }
    public abstract bool IsThisStateCurrentStateOfArtist();
    public abstract bool IsTransitionAvailable(out ArtistState artistState);
    public abstract void TransitionToNextState();
    public abstract void Tick();

}
