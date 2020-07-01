using System;
using Crestron.SimplSharpPro.DeviceSupport;
using JaneStreetAV.UI.Joins;
using JaneStreetAV.UI.Views.ControlRoomPanel;
using UX.Lib2.Models;
using UX.Lib2.UI;

namespace JaneStreetAV.UI
{
    public class RoomUIController : BaseUIController
    {
        private readonly bool _startInTechMode;
        private readonly UserPageViewController _userPage;
        private readonly TechPageViewController _techPage;
        private readonly UIButton _hiddenFlipButton;

        public RoomUIController(SystemBase system, BasicTriListWithSmartObject device, RoomBase room, bool startInTechMode)
            : base(system, device, room)
        {
            _startInTechMode = startInTechMode;
            _userPage = new UserPageViewController(this);
            _techPage = new TechPageViewController(this);
            _hiddenFlipButton = new UIButton(this, Digitals.HeaderHiddenButton, Serials.PageName)
            {
                HoldTime = TimeSpan.FromSeconds(0.5)
            };
            _hiddenFlipButton.ButtonEvent += (button, args) =>
            {
                if(args.EventType != ButtonEventType.Held) return;
                if (_userPage.Visible)
                {
                    _techPage.Show();
                    SetHeaderTitle("Tech Control Panel");                    
                }
                else
                {
                    _userPage.Show();
                    if(Room == null) return;
                    SetHeaderTitle(Room.Name);
                }
            };
        }

        protected override void UIShouldShowSource(SourceBase source)
        {
            if (_startInTechMode && !_techPage.Visible)
            {
                _techPage.Show();
            }
        }

        protected override void UIShouldShowHomePage(ShowHomePageEventType eventType)
        {
            if (_startInTechMode)
            {
                _techPage.Show();
                return;
            }

            _userPage.Show();
        }

        protected override void RoomDidPowerOff(PowerOfFEventType eventType)
        {

        }

        protected override void OnSystemStartupProgressChange(SystemBase system, SystemStartupProgressEventArgs args)
        {

        }

        protected override void OnRoomChange(UIController uiController, RoomBase previousRoom, RoomBase newRoom)
        {
            base.OnRoomChange(uiController, previousRoom, newRoom);
            if(_techPage.Visible || newRoom == null) return;
            SetHeaderTitle(newRoom.Name);
        }

        public void SetHeaderTitle(string name)
        {
            _hiddenFlipButton.SetText(name);
        }
    }
}