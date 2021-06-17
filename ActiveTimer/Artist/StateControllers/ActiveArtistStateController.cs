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
        }


        public override bool IsThisStateCurrentStateOfArtist()
        {
            return main.Artist.ArtistState == ArtistState.ACTIVE;
        }

        private bool TransitionAvailable => availableTransitionState != ArtistState.ACTIVE;
        private BlacklistItem transitionBlacklistItem;
        private ArtistState availableTransitionState = ArtistState.ACTIVE;


        public override bool IsTransitionAvailable(out ArtistState artistState)
        {
            transitionBlacklistItem = null;
            availableTransitionState = ArtistState.ACTIVE;
            artistState = ArtistState.ACTIVE;

            string title = main.CurrentWindowTitle;
            if (!main.IsTitleSameTitleAsPrevious(title))
                if (main.IsTitleValidWindowTitleCapture(title))
                    if (!Data.Settings.Blacklist.IsTitleAllowed(title, out BlacklistItem blacklistItemOnFalse))
                    {
                        transitionBlacklistItem = blacklistItemOnFalse;
                        availableTransitionState = ArtistState.PAUSED;
                        artistState = ArtistState.PAUSED;

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
                    availableTransitionState = ArtistState.INACTIVE;
                    artistState = ArtistState.INACTIVE;

                    //main.ArtistDeactivate.Execute(null);
                    return true;

                }
            }

            return false;
        }
        public override void TransitionToNextState()
        {
            switch (availableTransitionState)
            {
                case ArtistState.PAUSED:
                    main.ArtistPause.Execute(transitionBlacklistItem.Rule);

                    break;
                case ArtistState.INACTIVE:
                    main.ArtistDeactivate.Execute(null);

                    break;
            }
        }


        public override void Tick()
        {




            main.ActiveTimeUpdate1Sec.Execute(null);


            if (main.timeSecToFillTopBar == 0)
                return;

            float rest = (float)(main.Artist.ActiveTime.TotalSeconds % (main.timeSecToFillTopBar));
            main.topPercentFilled = Utils.ToProcentage(rest, 0, main.timeSecToFillTopBar);

            main._host.SendMessage("MainBar", "value|||" + main.topPercentFilled);
        }
    }
}
