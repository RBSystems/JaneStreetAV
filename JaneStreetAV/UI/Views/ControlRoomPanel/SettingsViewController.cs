using System;
using JaneStreetAV.Models;
using JaneStreetAV.UI.Joins;
using UX.Lib2.Devices.Audio.QSC.Components;
using UX.Lib2.Devices.Cisco.Video;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.ControlRoomPanel
{
    public class SettingsViewController : ASubPage
    {
        private readonly ButtonCollection _autoModeButtons;
        private readonly ButtonCollection _roomModeTabButtons;
        private readonly ButtonCollection _roomModeLayoutButtons;
        private readonly UIButton _saveRoomModeButton;
        private readonly UIButton _revertRoomModeButton;
        private readonly UIButton _codec1DualBtn;
        private uint _uiRoomModeLayout;

        public SettingsViewController(UIViewController parentViewController, uint joinNumber, string title)
            : base(parentViewController, joinNumber, title, TimeSpan.Zero)
        {
            var ui = UIController as UIControllerWithSmartObjects;
            _autoModeButtons = new ButtonCollection
            {
                {1, new UIButton(this, Digitals.AutoModeRoom1Btn)},
                {2, new UIButton(this, Digitals.AutoModeRoom2Btn)},
            };
            var tabBar = new UITabBar(ui, ui.Device.SmartObjects[Joins.SmartObjects.AudioPresetsTabs]);
            _roomModeTabButtons = new ButtonCollection(tabBar);
            _roomModeLayoutButtons = new ButtonCollection
            {
                {1, new UIButton(this, Digitals.RoomModeLayout1Btn)},
                {2, new UIButton(this, Digitals.RoomModeLayout2Btn)},
                {3, new UIButton(this, Digitals.RoomModeLayout3Btn)},
            };
            _saveRoomModeButton = new UIButton(this, Digitals.RoomModeSaveBtn)
            {
                EnableJoin = Device.BooleanInput[Digitals.RoomModeChangeEnable]
            };
            _revertRoomModeButton = new UIButton(this, Digitals.RoomModeRevertBtn)
            {
                EnableJoin = Device.BooleanInput[Digitals.RoomModeChangeEnable]
            };
            _codec1DualBtn = new UIButton(this, Digitals.Codec1DualModeBtn);
        }

        protected override void WillShow()
        {
            base.WillShow();
            _autoModeButtons.ButtonEvent += AutoModeButtonsOnButtonEvent;
            _roomModeTabButtons.ButtonEvent += RoomModeTabButtonsOnButtonEvent;
            _saveRoomModeButton.ButtonEvent += SaveRoomModeButtonOnButtonEvent;
            _revertRoomModeButton.ButtonEvent += RevertRoomModeButtonOnButtonEvent;
            _codec1DualBtn.ButtonEvent += Codec1DualBtnOnButtonEvent;
            _codec1DualBtn.Feedback = ((ASystem) UIController.System).Codecs[1].Video.Monitors == VideoMonitors.Dual;
            if (RoomModes != null)
            {
                _roomModeTabButtons.SetInterlockedFeedback((uint) RoomModes.GetMatch());
            }
        }

        protected override void WillHide()
        {
            base.WillHide();
            _autoModeButtons.ButtonEvent -= AutoModeButtonsOnButtonEvent;
            _roomModeTabButtons.ButtonEvent -= RoomModeTabButtonsOnButtonEvent;
            _saveRoomModeButton.ButtonEvent -= SaveRoomModeButtonOnButtonEvent;
            _revertRoomModeButton.ButtonEvent -= RevertRoomModeButtonOnButtonEvent;
            _codec1DualBtn.ButtonEvent -= Codec1DualBtnOnButtonEvent;
        }

        private SnapshotController RoomModes
        {
            get
            {
                var dsp = ((ASystem) UIController.System).Dsp;
                if (dsp.ContainsComponentWithName("room.mode"))
                {
                    return dsp["room.mode"] as SnapshotController;
                }
                return null;
            }
        }

        private uint RoomMode
        {
            get { return (uint) RoomModes.GetMatch(); }
            set
            {
                RoomModes.Load((int) value);
                _saveRoomModeButton.Enabled = _uiRoomModeLayout != RoomMode;
            }
        }

        private uint UIRoomModeLayout
        {
            get { return _uiRoomModeLayout; }
            set
            {
                _uiRoomModeLayout = value;
                _roomModeTabButtons.SetInterlockedFeedback(_uiRoomModeLayout);
                _roomModeLayoutButtons[1].Feedback = _uiRoomModeLayout > 2;
                _roomModeLayoutButtons[2].Feedback = _uiRoomModeLayout > 1;
                _roomModeLayoutButtons[3].Feedback = _uiRoomModeLayout == 2 || _uiRoomModeLayout == 3;
                _saveRoomModeButton.Enabled = _uiRoomModeLayout != RoomMode;
            }
        }

        private void RoomModeTabButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;

            UIRoomModeLayout = args.CollectionKey;
        }

        private void RevertRoomModeButtonOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;
            UIRoomModeLayout = RoomMode;
        }

        private void SaveRoomModeButtonOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;
            RoomMode = UIRoomModeLayout;
        }

        private void AutoModeButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Pressed) return;

            button.Feedback = !button.Feedback;
        }

        private void Codec1DualBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            var codec = ((ASystem) UIController.System).Codecs[1];
            var dualMode = codec.Video.Monitors == VideoMonitors.Dual;
            if (dualMode)
            {
                button.Feedback = false;
                codec.Video.MonitorsSet(Video.VideoMonitorsCommand.Single);
            }
            else
            {
                button.Feedback = true;
                codec.Video.MonitorsSet(Video.VideoMonitorsCommand.Single);
            }
        }
    }
}