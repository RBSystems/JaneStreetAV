using System.Collections.Generic;
using System.Linq;
using JaneStreetAV.Models;
using JaneStreetAV.UI.Joins;
using JaneStreetAV.UI.SmartObjects;
using JaneStreetAV.UI.Views.VC;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.ControlRoomPanel
{
    public class TechPageViewController : APage
    {
        private readonly NavIconMenu _navList;
        private readonly ButtonCollection _navButtons;
        private readonly List<IVisibleItem> _subPages;

        public TechPageViewController(BaseUIController uiController)
            : base(uiController, Digitals.PageTechControl, string.Empty)
        {
            _navList = new NavIconMenu(uiController, uiController.Device.SmartObjects[1]);
            _navButtons = new ButtonCollection(_navList);

            _subPages = new List<IVisibleItem>
            {
                new DMRoutingViewController(this, Digitals.SubPageTechDmRouting, "AV Routing"),
                new SettingsViewController(this, Digitals.SubPageTechSettings, "System Settings"),
                new RecordingViewController(this),
                new PowerViewController(this),
                new RoomControlViewController(this),
                new TVControlViewController(this),
                new AudioFadersViewController(this),
                new VCViewController(this),
                new UsbRoutingViewController(this),
                new CameraControlViewController(this),
            };

            _navList.AddItem(JSIcons.Power, "Power Control", _subPages.FirstOrDefault(s => s is PowerViewController));
            _navList.AddItem(JSIcons.Video, "DM Routing",
                _subPages.FirstOrDefault(s => s.VisibleJoin.Number == Digitals.SubPageTechDmRouting));
            _navList.AddItem(JSIcons.AudioLevels, "Audio Control", _subPages.FirstOrDefault(s => s is AudioFadersViewController));
            if (UIController.System is ClassroomSystem || UIController.System is AuditoriumSystem)
            {
                _navList.AddItem(JSIcons.VideoConf, "Video Conference",
                    _subPages.FirstOrDefault(s => s is VCViewController));
                _navList.AddItem(JSIcons.Camera, "Cameras",
                    _subPages.FirstOrDefault(s => s is CameraControlViewController));
                _navList.AddItem(JSIcons.Recording, "Recording", _subPages.FirstOrDefault(s => s is RecordingViewController));
            }
            _navList.AddItem(JSIcons.TV, "IPTV Control", _subPages.FirstOrDefault(s => s is TVControlViewController));
            if (UIController.System is AuditoriumSystem)
            {
                _navList.AddItem(JSIcons.Lights, "Room Control",
                    _subPages.FirstOrDefault(s => s is RoomControlViewController));
                _navList.AddItem(JSIcons.Presets, "USB Routing",
                    _subPages.FirstOrDefault(s => s is UsbRoutingViewController));
                _navList.AddItem(JSIcons.Settings, "System Settings",
                    _subPages.FirstOrDefault(s => s.VisibleJoin.Number == Digitals.SubPageTechSettings));
            }
        }

        protected override void WillShow()
        {
            base.WillShow();

            _navButtons.ButtonEvent += NavButtonsOnButtonEvent;
        }

        protected override void WillHide()
        {
            base.WillHide();

            _navButtons.ButtonEvent -= NavButtonsOnButtonEvent;
        }

        private void NavButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Pressed) return;

            _navList.SetSelectedItem(button as UISubPageReferenceListItem);

            var view = _navList[args.CollectionKey].LinkedObject as IVisibleItem;

            foreach (var item in _subPages.Where(item => item != view))
            {
                item.Hide();
            }

            if (view == null) return;

            view.Show();
        }
    }
}