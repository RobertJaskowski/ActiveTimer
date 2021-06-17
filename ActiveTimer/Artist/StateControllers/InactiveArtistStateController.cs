using ActiveTimer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveTimer.Artist.StateControllers
{
    public class InactiveArtistStateController : ArtistStateController
    {
        public InactiveArtistStateController(ActiveTimerViewModel mainVM) : base(mainVM)
        {
        }

        public override bool IsThisStateCurrentStateOfArtist()
        {
            return main.Artist.ArtistState == ArtistState.INACTIVE;
        }

        private bool TransitionAvailable => availableTransitionState != ArtistState.INACTIVE;
        private ArtistState availableTransitionState = ArtistState.INACTIVE;

        public override bool IsTransitionAvailable(out ArtistState artistState)
        {
            artistState = ArtistState.INACTIVE;


            if (main.InputReceivedThisTick)
            {
                //main.ArtistActivate.Execute(null);

                availableTransitionState = ArtistState.ACTIVE;
                artistState = ArtistState.ACTIVE;

                return true;
            }
            return false;
        }

        public override void TransitionToNextState()
        {
            main.ArtistActivate.Execute(null);
        }

        public override void Tick()
        {
            if (main.InputReceivedThisTick)
            {
                main.ArtistActivate.Execute(null);
            }
        }
    }
}
