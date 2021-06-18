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
            availableTransitionState = StateName;

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
            main.ArtistActivate.Execute(null);
        }

        public override void Tick()
        {
            main.ActiveTimeUpdate1Sec.Execute(null);
            main.TimeReason = "";
        }
    }
}
