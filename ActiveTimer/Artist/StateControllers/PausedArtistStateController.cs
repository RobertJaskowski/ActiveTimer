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
        }

        public override bool IsThisStateCurrentStateOfArtist()
        {
            return main.Artist.ArtistState == ArtistState.PAUSED;
        }


        private bool TransitionAvailable => availableTransitionState != ArtistState.PAUSED;
        private ArtistState availableTransitionState = ArtistState.PAUSED;

        public override bool IsTransitionAvailable(out ArtistState availableState)
        {
            availableTransitionState = ArtistState.PAUSED;
            availableState = ArtistState.PAUSED;

            if (!main.IsTimerPausedByUser() && main.InputReceivedThisTick)
            {
                string titlee = WinApi.GetWindowTitle().ToString().ToLowerInvariant();
                if (!main.IsTitleSameTitleAsPrevious(titlee))
                    if (main.IsTitleValidWindowTitleCapture(titlee))
                        if (Data.Settings.Blacklist.IsTitleAllowed(titlee, out BlacklistItem blacklistItem))
                        {
                            availableTransitionState = ArtistState.RESUMED;
                            availableState = ArtistState.RESUMED;
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
