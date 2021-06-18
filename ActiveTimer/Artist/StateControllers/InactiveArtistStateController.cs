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
            main.ChangeState("Active");
        }

        public override void Tick()
        {

        }

        public override void OnEnter(object o)
        {
            main.Artist.ArtistState = "Inactive";



            if (Data.Settings.PlayChangeSound)
            {
                main.PlaySound("ring.wav");
            }

            //Color c = Color.FromArgb(255, 221, 44, 0);
            // MainBarModule.SetBarColor(c);
            main._host.SendMessage("MainBar", "color|||" + "221|||44|||0");
            main._host.SendMessage("ActiveTimer", "IsNotActive");
        }


        public override string GetTimerText()
        {
            return main.Artist.ActiveTime.ToString();

        }

        public override void OnTimeClicked()
        {
           main.ChangeState(typeof(PausedArtistStateController), "user");

        }
    }
}
