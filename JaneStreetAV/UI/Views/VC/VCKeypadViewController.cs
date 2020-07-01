using System;
using JaneStreetAV.UI.Joins;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.VC
{
    public class VCKeypadViewController : ASubPage
    {
        private readonly UIKeypad _keypad;

        public VCKeypadViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageVCKeypad, string.Empty, TimeSpan.Zero)
        {
            _keypad = new UIKeypad(Device.SmartObjects[Joins.SmartObjects.KeyboardKeypad]);
        }

        public UIKeypad Keypad
        {
            get { return _keypad; }
        }
    }
}