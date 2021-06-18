using ActiveTimer.ViewModel;
using System.Text;

namespace ActiveTimer.Artist.StateControllers
{
    public class PausedArtistStateController : ArtistStateController
    {
        public PausedArtistStateController(ActiveTimerViewModel mainVM) : base(mainVM)
        {
            availableTransitionState = StateName;
        }

        public override void OnEnter(object o)
        {
            main.Artist.ArtistState = "Paused";

            if (o is string)
                main.TimeReason = (string)o;

            main._host.SendMessage("MainBar", "color|||" + "221|||44|||0");

            main._host.SendMessage("ActiveTimer", "IsNotActive");
        }

        private bool TransitionAvailable => !IsSameStateByName(availableTransitionState);

        public override string StateName => "Paused";

        private string availableTransitionState;

        public override bool IsTransitionAvailable(out string availableState)
        {
            availableTransitionState = StateName;
            availableState = StateName;

            if (!main.IsTimerPausedByUser() && main.InputReceivedThisTick)
            {
                string titlee = WinApi.GetWindowTitle().ToString().ToLowerInvariant();
                if (!main.IsTitleSameTitleAsPrevious(titlee))
                    if (main.IsTitleValidWindowTitleCapture(titlee))
                        if (Data.Settings.Blacklist.IsTitleAllowed(titlee, out BlacklistItem blacklistItem))
                        {
                            availableTransitionState = "Resumed";
                            availableState = "Resumed";
                            //main.ArtistResume.Execute(null);
                            return true;
                        }
            }
            return false;
        }

        public override void TransitionToNextState()
        {
            main.ChangeState(typeof(ResumedArtistStateController));
        }

        public override void Tick()
        {
        }

        public override string GetTimerText()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("|| ").Append(main.Artist.ActiveTime.ToString());
            if (!string.IsNullOrEmpty(main.TimeReason))
                sb.Append(" by ").Append(main.TimeReason);
            return sb.ToString();
        }

        public override void OnTimeClicked()
        {
            main.ChangeState(typeof(ResumedArtistStateController));
        }
    }
}