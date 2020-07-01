using System;
using System.Linq;
using JaneStreetAV.Models;
using JaneStreetAV.UI.Joins;
using JaneStreetAV.UI.SmartObjects;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.ControlRoomPanel
{
    public class PowerViewController : ASubPage
    {
        private ButtonCollection _buttons;
        private readonly PowerControlList _controls;

        public PowerViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageTechPower, string.Empty, TimeSpan.Zero)
        {
            var ui = parentViewController.UIController as UIControllerWithSmartObjects;
            _controls = new PowerControlList(ui, ui.Device.SmartObjects[Joins.SmartObjects.PowerItemsList]);
        }

        protected override void WillShow()
        {
            base.WillShow();
            _controls.ClearList(true);
            foreach (
                var device in
                    UIController.System.Rooms.OfType<ARoom>()
                        .Select(room => room.GetPowerDevices())
                        .SelectMany(devices => devices))
            {
                _controls.AddItem(device);
            }

            _controls.SetListSizeToItemCount();
        }

        protected override void WillHide()
        {
            base.WillHide();
        }
    }
}