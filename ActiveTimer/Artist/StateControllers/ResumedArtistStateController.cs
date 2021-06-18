using ActiveTimer.ViewModel;
using System.Text;

namespace ActiveTimer.Artist.StateControllers
{
    public class ResumedArtistStateController : ArtistStateController
    {
        public ResumedArtistStateController(ActiveTimerViewModel mainVM) : base(mainVM)
        {
            availableTransitionState = StateName;
        }

        public override void OnEnter(object o)
        {
            main.Artist.ArtistState = "Resumed";
            if (o is string)
                main.TimeReason = "";

            main._host.SendMessage("MainBar", "color|||" + "221|||44|||0");

            main._host.SendMessage("ActiveTimer", "IsActive");
        }

        private bool TransitionAvailable => !IsSameStateByName(availableTransitionState);

        public override string StateName => "Resumed";

        private string availableTransitionState;

        public override bool IsTransitionAvailable(out string availableState)
        {
            availableTransitionState = StateName;
            availableState = StateName;

            if (main.InputReceivedThisTick)
            {
                availableTransitionState = "Active";
                availableState = "Active";

                //main.ArtistActivate.Execute(null);
                return false;
            }
            return true;
        }

        public override void TransitionToNextState()
        {
            main.ChangeState(typeof(ActiveArtistStateController));
        }

        public override void Tick()
        {
            main.TimeReason = "";
            TransitionToNextState();
        }

        public override string GetTimerText()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("|> ").Append(main.Artist.ActiveTime.ToString());
            if (!string.IsNullOrEmpty(main.TimeReason))
                sb.Append(" by ").Append(main.TimeReason);
            return sb.ToString();
        }

        public override void OnTimeClicked()
        {
        }
    }
}