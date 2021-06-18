using ActiveTimer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveTimer.Artist.StateControllers
{
    public class PausedArtistStateController : ArtistStateController
    {
        public PausedArtistStateController(ActiveTimerViewModel mainVM) : base(mainVM)
        {
            availableTransitionState = StateName;
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
            main.ArtistResume.Execute(null);
        }

        public override void Tick()
        {

        }
    }
}
