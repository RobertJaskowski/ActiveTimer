using ActiveTimer.ViewModel;

namespace ActiveTimer
{
    public abstract class ArtistStateController
    {
        public abstract string StateName { get; }
        protected ActiveTimerViewModel main;

        public ArtistStateController(ActiveTimerViewModel mainVM)
        {
            main = mainVM;
        }

        public abstract void OnEnter(object o);

        public abstract bool IsTransitionAvailable(out string artistState);

        public abstract void TransitionToNextState();

        public abstract void Tick();

        public abstract void OnTimeClicked();

        public abstract string GetTimerText();

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
}