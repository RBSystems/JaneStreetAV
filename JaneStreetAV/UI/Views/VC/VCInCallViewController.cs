using System;
using System.Linq;
using Crestron.SimplSharp;
using JaneStreetAV.UI.Joins;
using JaneStreetAV.UI.SmartObjects;
using UX.Lib2;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.Devices.Cisco.Conference;
using UX.Lib2.UI;
using Call = UX.Lib2.Devices.Cisco.Call;

namespace JaneStreetAV.UI.Views.VC
{
    public class VCInCallViewController : VCSubViewControllerBase
    {
        private readonly CiscoSmallIconList _list;
        private readonly ButtonCollection _buttons;
        private readonly UILabel _statusLabel;
        private readonly UILabel _nameLabel;
        private readonly UILabel _timerLabel;
        private Call _call;
        private CTimer _timer;

        public VCInCallViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageVCInCall)
        {
            var ui = (UIControllerWithSmartObjects) parentViewController.UIController;
            _list = new CiscoSmallIconList(ui, ui.Device.SmartObjects[Joins.SmartObjects.VCCallActionList]);
            _buttons = new ButtonCollection(_list);
            _list.AddItem(CiscoSmallIconListItemType.MicMute, "Privacy");
            _list.AddItem(CiscoSmallIconListItemType.Keypad, "Keypad");
            _list.AddItem(CiscoSmallIconListItemType.Hold, "Hold");
            _list.AddItem(CiscoSmallIconListItemType.Share, "Share");
            _list.AddItem(CiscoSmallIconListItemType.EndCall, "End Call");
            _list[2].Hide();
            _list[3].Hide();
            _statusLabel = new UILabel(this, Serials.VCCallInfoStatus);
            _nameLabel = new UILabel(this, Serials.VCCallInfoName);
            _timerLabel = new UILabel(this, Serials.VCCallTimerText);
        }

        public void Show(Call call)
        {
            if(_call != null) return;
            _call = call;
            base.Show();
        }

        protected override void WillShow()
        {
            base.WillShow();
            _buttons.ButtonEvent += ButtonsOnButtonEvent;
            if(_call == null) return;

            _call.Codec.Calls.CallStatusChange += CallsOnCallStatusChange;
            _call.Codec.Audio.Microphones.MuteChange += _list[1].SetFeedback;
            _call.Codec.Conference.Presentation.StatusChange += PresentationOnStatusChange;

            UpdateStatusText(_call.Status);
            _nameLabel.SetText(_call.DisplayName);
            _list[3].SetFeedback(_call.OnHold);

            if (_call.Connected)
            {
                _list[2].Show();
                _list[3].Show();
                StartCallTimer();
            }
        }

        protected override void WillHide()
        {
            base.WillHide();
            _buttons.ButtonEvent -= ButtonsOnButtonEvent;
            if(_call == null) return;

            if (_timer != null)
            {
                _timer.Stop();
                _timerLabel.Clear();
            }
            _list[2].Hide();
            _list[3].Hide();

            _call.Codec.Calls.CallStatusChange -= CallsOnCallStatusChange;
            _call.Codec.Audio.Microphones.MuteChange -= _list[1].SetFeedback;
            _call.Codec.Conference.Presentation.StatusChange -= PresentationOnStatusChange;
            _call = null;
        }

        private void ButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;

            var type = (CiscoSmallIconListItemType) _list[args.CollectionKey].LinkedObject;

            switch (type)
            {
                case CiscoSmallIconListItemType.MicMute:
                    var mute = !_call.Codec.Audio.Microphones.Muted;
                    button.Feedback = mute;
                    _call.Codec.Audio.Microphones.Muted = mute;
                    break;
                case CiscoSmallIconListItemType.EndCall:
                    _call.Disconnect();
                    break;
                case CiscoSmallIconListItemType.Keypad:
                    break;
                case CiscoSmallIconListItemType.Hold:
                    if (_call.Status == CallStatus.OnHold)
                    {
                        _call.Resume();
                    }
                    else
                    {
                        _call.Hold();
                    }
                    break;
                case CiscoSmallIconListItemType.Share:
                    if (PageNumber == Digitals.PageUser)
                    {
                        Parent.ShareView.Show();
                        return;
                    }
                    var state = _call.Codec.Conference.Presentation.LocalInstance.Count > 0 &&
                                Codec.Conference.Presentation.LocalInstance.First().Value.Mode != LocalSendingMode.Off;
                    if (!state)
                    {
                        _call.Codec.Conference.Presentation.Start(SendingMode.LocalRemote);
                    }
                    else
                    {
                        _call.Codec.Conference.Presentation.Stop();
                    }
                    button.Feedback = !state;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StartCallTimer()
        {
            if (_timer == null)
            {
                _timer = new CTimer(UpdateTimer, null, 1000, 1000);
            }
            else
            {
                _timer.Reset(1000, 1000);
            }
        }

        private void UpdateTimer(object userSpecific)
        {
            if (_call == null)
            {
                _timerLabel.Clear();
                return;
            }

            _timerLabel.SetText(_call.Duration.ToString());
        }

        private void CallsOnCallStatusChange(CiscoTelePresenceCodec codec, CallStatusEventType eventType, Call call)
        {
            if(call != _call) return;

            if (eventType == CallStatusEventType.Ended)
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timerLabel.Clear();
                }
                Parent.ResetView();
                return;
            }

            if (eventType == CallStatusEventType.StatusUpdated && call.Connected)
            {
                _list[2].Show();
                _list[3].Show();
                StartCallTimer();
            }

            UpdateStatusText(call.Status);
            _nameLabel.SetText(call.DisplayName);
            _list[3].SetFeedback(call.OnHold);
        }

        private void UpdateStatusText(CallStatus status)
        {
            switch (status)
            {
                case CallStatus.Dialling:
                    _statusLabel.Text = "Calling ";
                    break;
                case CallStatus.EarlyMedia:
                    _statusLabel.Text = "Connecting to ";
                    break;
                case CallStatus.Connecting:
                    _statusLabel.Text = "Calling ";
                    break;
                default:
                    _statusLabel.Text = _call.Status.ToString().SplitCamelCase() + " ";
                    break;
            }
        }

        private void PresentationOnStatusChange(CodecApiElement element, string[] propertyNamesWhichUpdated)
        {
            var presentation = element as Presentation;
            if(presentation == null) return;
            _list[4].Feedback = presentation.LocalInstance.Count > 0 &&
                                presentation.LocalInstance.First().Value.Mode != LocalSendingMode.Off;
        }
    }
}