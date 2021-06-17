using ActiveTimer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveTimer.Artist.StateControllers
{
    public class ResumedArtistStateController : ArtistStateController
    {
        public ResumedArtistStateController(ActiveTimerViewModel mainVM) : base(mainVM)
        {
        }

        public override bool IsThisStateCurrentStateOfArtist()
        {
            return main.Artist.ArtistState == ArtistState.RESUMED;
        }


        private bool TransitionAvailable => availableTransitionState != ArtistState.RESUMED;
        private ArtistState availableTransitionState = ArtistState.RESUMED;

        public override bool IsTransitionAvailable(out ArtistState availableState)
        {
            availableTransitionState = ArtistState.RESUMED;
            availableState = ArtistState.RESUMED;


            if (main.InputReceivedThisTick)
            {

                availableTransitionState = ArtistState.ACTIVE;
                availableState = ArtistState.ACTIVE;

                //main.ArtistActivate.Execute(null);
                return false;
            }
            return true;
        }

        public override void TransitionToNextState()
        {
            main.ArtistActivate.Execute(null);
        }

        public override void Tick()
        {
            main.ActiveTimeUpdate1Sec.Execute(null);
            main.TimeReason = "";
        }
    }
}
