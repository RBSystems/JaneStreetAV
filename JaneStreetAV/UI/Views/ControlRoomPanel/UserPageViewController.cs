using System;
using JaneStreetAV.Models;
using JaneStreetAV.UI.Joins;
using JaneStreetAV.UI.Views.VC;
using UX.Lib2.Models;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.ControlRoomPanel
{
    public class UserPageViewController : APage
    {
        private readonly VCViewController _vcView;
        private readonly UIButton _openPreviewBtn;
        private readonly UIButton _closePreviewBtn;
        private readonly UISlider _slider;
        private readonly UIButton _muteBtn;
        private IAudioLevelControl _volumeControl;

        public UserPageViewController(BaseUIController uiController)
            : base(uiController, Digitals.PageUser, string.Empty)
        {
            _vcView = new VCViewController(this);
            _openPreviewBtn = new UIButton(this, Digitals.UserPageVideoOpenBtn);
            _closePreviewBtn = new UIButton(this, Digitals.UserPageVideoCloseBtn);
            _slider = new UISlider(this, Analogs.UserPageVolumeSlider, true);
            _muteBtn = new UIButton(this, Digitals.UserPageVolumeMuteBtn);
            UIController.RoomChange += UIControllerOnRoomChange;
        }

        protected override void WillShow()
        {
            base.WillShow();

            Device.BooleanInput[Digitals.SubPageUserVideoFullscreen].BoolValue = false;
            Device.BooleanInput[Digitals.SubPageUserSideBar].BoolValue = true;

            _openPreviewBtn.ButtonEvent += OpenPreviewBtnOnButtonEvent;
            _closePreviewBtn.ButtonEvent += ClosePreviewBtnOnButtonEvent;
            _slider.AnalogValueChanged += SliderOnAnalogValueChanged;
            _muteBtn.ButtonEvent += MuteBtnOnButtonEvent;
            VolumeControl = ((ARoom) UIController.Room).ProgramVolume;
            if (VolumeControl != null)
            {
                _slider.SetValue(VolumeControl.Level);
                _muteBtn.SetFeedback(VolumeControl.Muted);
            }

            _vcView.Show();
        }

        protected override void WillHide()
        {
            base.WillHide();

            _openPreviewBtn.ButtonEvent -= OpenPreviewBtnOnButtonEvent;
            _closePreviewBtn.ButtonEvent -= ClosePreviewBtnOnButtonEvent;
            _slider.AnalogValueChanged -= SliderOnAnalogValueChanged;
            _muteBtn.ButtonEvent -= MuteBtnOnButtonEvent;
        }

        private IAudioLevelControl VolumeControl
        {
            get { return _volumeControl; }
            set
            {
                if(_volumeControl == value) return;

                if (_volumeControl != null)
                {
                    _volumeControl.LevelChange -= VolumeControlOnLevelChange;
                    _volumeControl.MuteChange -= _muteBtn.SetFeedback;
                }

                _volumeControl = value;

                if (_volumeControl == null) return;

                _volumeControl.LevelChange += VolumeControlOnLevelChange;
                _volumeControl.MuteChange += _muteBtn.SetFeedback;
                _slider.SetValue(_volumeControl.Level);
                _muteBtn.SetFeedback(_volumeControl.Muted);
            }
        }

        private void OpenPreviewBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType == ButtonEventType.Pressed)
            {
                Device.BooleanInput[Digitals.SubPageUserSideBar].BoolValue = false;
                Device.BooleanInput[Digitals.SubPageUserVideoFullscreen].BoolValue = true;
            }
        }

        private void ClosePreviewBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType == ButtonEventType.Pressed)
            {
                Device.BooleanInput[Digitals.SubPageUserVideoFullscreen].BoolValue = false;
                Device.BooleanInput[Digitals.SubPageUserSideBar].BoolValue = true;
            }
        }

        private void SliderOnAnalogValueChanged(IAnalogTouch item, ushort value)
        {
            if (_volumeControl != null)
            {
                _volumeControl.Level = value;
            }
        }

        private void VolumeControlOnLevelChange(IAudioLevelControl control, ushort level)
        {
            _slider.SetValue(level);
        }

        private void MuteBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType == ButtonEventType.Pressed && _volumeControl != null)
            {
                var mute = !_volumeControl.Muted;
                button.SetFeedback(mute);
                _volumeControl.Muted = mute;
            }
        }

        private void UIControllerOnRoomChange(UIController uiController, UIControllerRoomChangeEventArgs args)
        {
            if (args.NewRoom == null)
            {
                VolumeControl = null;
                return;
            }

            VolumeControl = ((ARoom) args.NewRoom).ProgramVolume;
        }
    }
}