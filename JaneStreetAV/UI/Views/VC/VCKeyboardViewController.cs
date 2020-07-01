using System;
using JaneStreetAV.UI.Joins;
using JaneStreetAV.UI.Views.ControlRoomPanel;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.VC
{
    public class VCKeyboardViewController : ASubPage
    {
        private readonly ButtonCollection _keyboardButtons;
        private readonly UIButton _closeBtn;
        private readonly UIButton _shiftBtn;
        private readonly UIButton _callBtn;
        private bool _shiftMode;
        private readonly VCKeypadViewController _keypadView;
        private readonly UIButton _keypadModeBtn;

        public VCKeyboardViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageVCKeyboard, string.Empty, TimeSpan.Zero)
        {
            _closeBtn = new UIButton(this, Digitals.VCDialKeyboardCloseBtn);
            _shiftBtn = new UIButton(this, Digitals.VCDialKeyboardShiftBtn);
            _callBtn = new UIButton(this, Digitals.VCDialKeyboardCallBtn);
            _keyboardButtons = new ButtonCollection();

            var joinOffset = 0U;

            //joins here start at Digitals.VCDialKeyboardCharRangeStart
            for (var c = '0'; c <= '9'; c ++)
            {
                _keyboardButtons.Add(c, new UIButton(this, Digitals.VCDialKeyboardCharRangeStart + joinOffset));
                joinOffset++;
            }

            //joins here start at Digitals.VCDialKeyboardCharRangeStart + 10

            for (var c = 'a'; c <= 'z'; c++)
            {
                _keyboardButtons.Add(c, new UIButton(this, Digitals.VCDialKeyboardCharRangeStart + joinOffset));
                joinOffset++;
            }

            //joins here start at Digitals.VCDialKeyboardCharRangeStart + 35

            _keyboardButtons.Add(' ', new UIButton(this, Digitals.VCDialKeyboardCharRangeStart + joinOffset));
            joinOffset ++;

            _keyboardButtons.Add('-', new UIButton(this, Digitals.VCDialKeyboardCharRangeStart + joinOffset));
            joinOffset ++;

            _keyboardButtons.Add('_', new UIButton(this, Digitals.VCDialKeyboardCharRangeStart + joinOffset));
            joinOffset++;

            _keyboardButtons.Add('@', new UIButton(this, Digitals.VCDialKeyboardCharRangeStart + joinOffset));
            joinOffset++;

            _keyboardButtons.Add('/', new UIButton(this, Digitals.VCDialKeyboardCharRangeStart + joinOffset));
            joinOffset++;

            _keyboardButtons.Add('.', new UIButton(this, Digitals.VCDialKeyboardCharRangeStart + joinOffset));
            joinOffset++;

            _keyboardButtons.Add((char) 8, new UIButton(this, Digitals.VCDialKeyboardCharRangeStart + joinOffset)
            {
                HoldTime = TimeSpan.FromSeconds(0.5)
            });

            _keypadView = new VCKeypadViewController(this);
            _keypadModeBtn = new UIButton(this, Digitals.VCDialKeyboardKeypadToggleBtn);
        }

        public event KeyboardTypedValueEventHandler TypedValue;

        protected override void WillShow()
        {
            base.WillShow();
            _closeBtn.ButtonEvent += CloseBtnOnButtonEvent;
            _shiftBtn.ButtonEvent += ShiftBtnOnButtonEvent;
            _callBtn.ButtonEvent += CallBtnOnButtonEvent;
            _keyboardButtons.ButtonEvent += KeyboardButtonsOnButtonEvent;
            _keypadView.Keypad.ButtonEvent += KeypadOnButtonEvent;
            _keypadModeBtn.ButtonEvent += KeypadModeBtnOnButtonEvent;
        }

        protected override void WillHide()
        {
            base.WillHide();
            _closeBtn.ButtonEvent -= CloseBtnOnButtonEvent;
            _shiftBtn.ButtonEvent -= ShiftBtnOnButtonEvent;
            _callBtn.ButtonEvent -= CallBtnOnButtonEvent;
            _keyboardButtons.ButtonEvent -= KeyboardButtonsOnButtonEvent;
            _keypadView.Keypad.ButtonEvent -= KeypadOnButtonEvent;
            _keypadModeBtn.ButtonEvent -= KeypadModeBtnOnButtonEvent;
        }

        private void CallBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;
            ((VCDialViewController) Parent).AttemptCall();
        }

        private void ShiftBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            _shiftMode = button.IsPressed;
        }

        private void CloseBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType == ButtonEventType.Released) Hide();
        }

        private void KeyboardButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (TypedValue == null) return;
            var c = (char) args.CollectionKey;
            if (_shiftMode && c >= 'a' && c <= 'z')
            {
                c = (char) (c - 32);
            }
            TypedValue(args.EventType, c);
        }

        private void KeypadModeBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;

            if(_keypadView.Visible)
                _keypadView.Hide();
            else
                _keypadView.Show();
        }

        private void KeypadOnButtonEvent(UIKeypad keypad, UIKeypadButtonEventArgs args)
        {
            if (TypedValue == null) return;
            if (args.EventType == ButtonEventType.Held && args.KeypadButtonType == UIKeypadButtonType.Key0)
            {
                TypedValue(ButtonEventType.Tapped, '+');
            }
            else if (args.EventType == ButtonEventType.Tapped)
            {
                switch (args.KeypadButtonType)
                {
                    case UIKeypadButtonType.Key0:
                    case UIKeypadButtonType.Key1:
                    case UIKeypadButtonType.Key2:
                    case UIKeypadButtonType.Key3:
                    case UIKeypadButtonType.Key4:
                    case UIKeypadButtonType.Key5:
                    case UIKeypadButtonType.Key6:
                    case UIKeypadButtonType.Key7:
                    case UIKeypadButtonType.Key8:
                    case UIKeypadButtonType.Key9:
                        TypedValue(args.EventType, (char) ('0' + args.Value));
                        break;
                    case UIKeypadButtonType.Star:
                        TypedValue(args.EventType, '*');
                        break;
                    case UIKeypadButtonType.Hash:
                        TypedValue(args.EventType, '#');
                        break;
                }
            }
        }
    }

    public delegate void KeyboardTypedValueEventHandler(ButtonEventType eventType, char value);
}