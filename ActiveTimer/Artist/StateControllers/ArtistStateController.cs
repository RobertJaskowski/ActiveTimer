using ActiveTimer.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class ArtistStateController
{
    public abstract string StateName { get; }
    protected ActiveTimerViewModel main;

    public ArtistStateController(ActiveTimerViewModel mainVM)
    {
        main = mainVM;
    }
    public abstract bool IsTransitionAvailable(out string artistState);
    public abstract void TransitionToNextState();
    public abstract void Tick();

    public bool IsSameStateByName(string other)
    {
        if (other == null) return false;

        if (other.Equals(StateName))
            return true;

        return false;

    }
    public bool IsSameStateByName(ArtistStateController other)
    {
        if (other == null) return false;

        if (other.StateName.Equals(StateName))
            return true;

        return false;

    }
}
