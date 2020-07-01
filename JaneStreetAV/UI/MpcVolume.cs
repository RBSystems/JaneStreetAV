using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using UX.Lib2;
using UX.Lib2.Models;
using UX.Lib2.UI;

namespace JaneStreetAV.UI
{
    public class MpcVolume
    {

        private readonly UIButton _volUpBtn;
        private readonly UIButton _volDownBtn;
        private readonly UIButton _muteBtn;
        private readonly UIGuage _volGuage;
        private IAudioLevelControl _volumeControl;

        public MpcVolume(MpcUIController mpc)
        {

            _volUpBtn = new UIButton(mpc, 12) {HoldTime = TimeSpan.FromSeconds(0.5)};
            _volUpBtn.ButtonEvent += OnOtherButtonEvent;
            _volDownBtn = new UIButton(mpc, 11) {HoldTime = TimeSpan.FromSeconds(0.5)};
            _volDownBtn.ButtonEvent += OnOtherButtonEvent;
            _muteBtn = new UIButton(mpc, 13);
            _muteBtn.ButtonEvent += OnOtherButtonEvent;
            _volGuage = new UIGuage(mpc, 1);
        }

        public MpcVolume(MPC3x101Touchscreen mpc)
        {

            _volUpBtn = new UIButton(mpc, 2) { HoldTime = TimeSpan.FromSeconds(0.5) };
            _volUpBtn.ButtonEvent += OnOtherButtonEvent;
            _volDownBtn = new UIButton(mpc, 3) { HoldTime = TimeSpan.FromSeconds(0.5) };
            _volDownBtn.ButtonEvent += OnOtherButtonEvent;
            _muteBtn = new UIButton(mpc, 1);
            _muteBtn.ButtonEvent += OnOtherButtonEvent;
            _volGuage = new UIGuage(mpc, 1);
        }

        public IAudioLevelControl VolumeControl
        {
            set
            {
                if(_volumeControl == value) return;

                if (_volumeControl != null)
                {
                    _volumeControl.LevelChange -= VolumeControlOnLevelChange;
                    _volumeControl.MuteChange -= _muteBtn.SetFeedback;
                }

                _volumeControl = value;
                
                if(_volumeControl == null) return;

                _volumeControl.LevelChange += VolumeControlOnLevelChange;
                _volumeControl.MuteChange += _muteBtn.SetFeedback;
                _muteBtn.SetFeedback(_volumeControl.Muted);
                _volGuage.SetValue(_volumeControl.Level);
            }
            get { return _volumeControl; }
        }

        private void VolumeControlOnLevelChange(IAudioLevelControl control, ushort level)
        {
            _volGuage.SetValue(level);
        }

        private void OnOtherButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType == ButtonEventType.Pressed)
            {
                if (button == _volUpBtn) VolumeUp();
                else if (button == _volDownBtn) VolumeDown();
                else if (button == _muteBtn)
                {
                    if(_volumeControl == null) return;
                    var mute = !_volumeControl.Muted;
                    button.Feedback = mute;
                    _volumeControl.Muted = mute;
                }
            }
            else if (args.EventType == ButtonEventType.Held)
            {
                if (button == _volUpBtn)
                {
                    new Thread(specific =>
                    {
                        while (((UIButton) specific).IsPressed)
                        {
                            VolumeUp();
                            Thread.Sleep(500);
                        }
                        return null;
                    }, button);
                }
                else if (button == _volDownBtn)
                {
                    new Thread(specific =>
                    {
                        while (((UIButton)specific).IsPressed)
                        {
                            VolumeDown();
                            Thread.Sleep(500);
                        }
                        return null;
                    }, button);
                }
            }
        }

        private void VolumeUp()
        {
            if (_volumeControl == null) return;

            var percent = Tools.ScaleRange(_volumeControl.Level, ushort.MinValue, ushort.MaxValue, 0, 100);
            var newPercent = percent + 5;
            if (newPercent > 100)
            {
                _volumeControl.Level = ushort.MaxValue;
            }
            else
            {
                _volumeControl.Level = (ushort) Tools.ScaleRange(newPercent, 0, 100, ushort.MinValue, ushort.MaxValue);
            }
        }

        private void VolumeDown()
        {
            if (_volumeControl == null) return;

            var percent = Tools.ScaleRange(_volumeControl.Level, ushort.MinValue, ushort.MaxValue, 0, 100);
            if (percent < 5)
            {
                _volumeControl.Level = ushort.MinValue;
                return;
            }
            _volumeControl.Level = (ushort)Tools.ScaleRange(percent - 5, 0, 100, ushort.MinValue, ushort.MaxValue);
        }
    }
}