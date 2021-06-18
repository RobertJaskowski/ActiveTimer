using ActiveTimer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveTimer.Artist.StateControllers
{
    public class ActiveArtistStateController : ArtistStateController
    {
        public ActiveArtistStateController(ActiveTimerViewModel mainVM) : base(mainVM)
        {
            availableTransitionState = StateName;
        }


        private bool TransitionAvailable => !IsSameStateByName(availableTransitionState);

        public override string StateName => "Active";


        private BlacklistItem transitionBlacklistItem;
        private string availableTransitionState;


        public override bool IsTransitionAvailable(out string nextTransitionStateAvailable)
        {
            transitionBlacklistItem = null;
            nextTransitionStateAvailable = "";

            string title = main.CurrentWindowTitle;
            if (!main.IsTitleSameTitleAsPrevious(title))
                if (main.IsTitleValidWindowTitleCapture(title))
                    if (!Data.Settings.Blacklist.IsTitleAllowed(title, out BlacklistItem blacklistItemOnFalse))
                    {
                        transitionBlacklistItem = blacklistItemOnFalse;
                        availableTransitionState = "Paused";
                        nextTransitionStateAvailable = "Paused";

                        //main.ArtistPause.Execute(blacklistItem.Rule);
                        return true;
                    }
            if (main.InputReceivedThisTick)
            {
                main.currentCheckingAfkTime = 0;
            }
            else
            {
                main.currentCheckingAfkTime++;
                if (main.currentCheckingAfkTime > main.maxSecAfkTime)
                {
                    main.currentCheckingAfkTime = 0;

                    transitionBlacklistItem = null;
                    availableTransitionState = "Inactive";
                    nextTransitionStateAvailable = "Inactive";

                    //main.ArtistDeactivate.Execute(null);
                    return true;

                }
            }

            return false;
        }
        public override void TransitionToNextState()
        {

            main.ChangeState(availableTransitionState, transitionBlacklistItem.Rule);

        }


        public override void Tick()
        {
            main.UpdateActiveTime(1);


            if (main.timeSecToFillTopBar == 0)
                return;

            float rest = (float)(main.Artist.ActiveTime.TotalSeconds % (main.timeSecToFillTopBar));
            main.topPercentFilled = Utils.ToProcentage(rest, 0, main.timeSecToFillTopBar);

            main._host.SendMessage("MainBar", "value|||" + main.topPercentFilled);
        }

        public override void OnEnter(object o)
        {
            main.Artist.ArtistState = "Active";


            if (Data.Settings.PlayChangeSound)
            {
                main.PlaySound("Rise02.wav");
            }

            main._host.SendMessage("MainBar", "color|||" + "178|||255|||89");
            main._host.SendMessage("ActiveTimer", "IsActive");
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
