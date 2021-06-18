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
            availableTransitionState = StateName;

        }


        private bool TransitionAvailable => !IsSameStateByName(availableTransitionState);

        public override string StateName => "Inactive";

        private string availableTransitionState;

        public override bool IsTransitionAvailable(out string artistState)
        {
            artistState = StateName;


            if (main.InputReceivedThisTick)
            {
                //main.ArtistActivate.Execute(null);

                availableTransitionState = "Active";
                artistState = "Active";


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
