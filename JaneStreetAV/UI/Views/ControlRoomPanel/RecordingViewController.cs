using System;
using System.Collections.Generic;
using JaneStreetAV.Models;
using JaneStreetAV.UI.Joins;
using UX.Lib2;
using UX.Lib2.Devices.Extron;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.ControlRoomPanel
{
    public class RecordingViewController : ASubPage
    {
        private readonly Dictionary<uint, ButtonCollection> _buttons = new Dictionary<uint, ButtonCollection>();
        private readonly Dictionary<uint, UILabel> _timeLabels = new Dictionary<uint, UILabel>();
        private readonly Dictionary<uint, UILabel> _timeRemainingLabels = new Dictionary<uint, UILabel>();
        private readonly Dictionary<uint, UILabel> _storageLabels = new Dictionary<uint, UILabel>();
        private readonly Dictionary<uint, UILabel> _spaceRemainingLabels = new Dictionary<uint, UILabel>(); 

        public RecordingViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageTechRecording, "Recording Control", TimeSpan.Zero)
        {
            _timeLabels[1] = new UILabel(this, Serials.ExtronRec1Time);
            _timeLabels[2] = new UILabel(this, Serials.ExtronRec2Time);
            _timeRemainingLabels[1] = new UILabel(this, Serials.ExtronRec1TimeRemaining);
            _timeRemainingLabels[2] = new UILabel(this, Serials.ExtronRec2TimeRemaining);
            _storageLabels[1] = new UILabel(this, Serials.ExtronRec1Storage);
            _storageLabels[2] = new UILabel(this, Serials.ExtronRec2Storage);
            _spaceRemainingLabels[1] = new UILabel(this, Serials.ExtronRec1BytesAvailable);
            _spaceRemainingLabels[2] = new UILabel(this, Serials.ExtronRec2BytesAvailable);
            _buttons[1] = new ButtonCollection();
            _buttons[2] = new ButtonCollection();
            _buttons[1].Add(Digitals.ExtronRec1RecordBtn, new UIButton(this, Digitals.ExtronRec1RecordBtn)
            {
                EnableJoin = Device.BooleanInput[Digitals.ExtronRec1Enable]
            });
            _buttons[2].Add(Digitals.ExtronRec2RecordBtn, new UIButton(this, Digitals.ExtronRec2RecordBtn)
            {
                EnableJoin = Device.BooleanInput[Digitals.ExtronRec2Enable]
            });
            _buttons[1].Add(Digitals.ExtronRec1StopBtn, new UIButton(this, Digitals.ExtronRec1StopBtn));
            _buttons[2].Add(Digitals.ExtronRec2StopBtn, new UIButton(this, Digitals.ExtronRec2StopBtn));
        }

        protected override void WillShow()
        {
            base.WillShow();

            _buttons[1].ButtonEvent += OnRec1ButtonEvent;
            _buttons[2].ButtonEvent += OnRec2ButtonEvent;

            var system = UIController.System as ASystem;
            if (system == null) return;
            system.Recorders[1].StatusUpdated += OnRec1StatusUpdated;
            system.Recorders[2].StatusUpdated += OnRec2StatusUpdated;
        }

        protected override void WillHide()
        {
            base.WillHide();
            
            _buttons[1].ButtonEvent -= OnRec1ButtonEvent;
            _buttons[2].ButtonEvent -= OnRec2ButtonEvent;

            var system = UIController.System as ASystem;
            if (system == null) return;
            system.Recorders[1].StatusUpdated -= OnRec1StatusUpdated;
            system.Recorders[2].StatusUpdated -= OnRec2StatusUpdated;
        }

        private void OnRec1StatusUpdated(ExtronSmp device)
        {
            _timeLabels[1].SetText("{0:D2}:{1:D2}:{2:D2}", (int)device.Timer.TotalHours, device.Timer.Minutes, device.Timer.Seconds);
            _timeRemainingLabels[1].SetText(((int)device.TimeAvailable.TotalMinutes) + " mins");
            _storageLabels[1].SetText(device.StorageLocation.ToString());
            _spaceRemainingLabels[1].SetText(Tools.PrettyByteSize(device.BytesAvailable, 1));
            if (device.RecordStatus == ExtronSmp.eRecordStatus.Setup)
            {
                _buttons[1][Digitals.ExtronRec1RecordBtn].Feedback = true;
                _buttons[1][Digitals.ExtronRec1RecordBtn].Enabled = false;
            }
            else
            {
                _buttons[1][Digitals.ExtronRec1RecordBtn].Feedback = device.RecordStatus ==
                                                                     ExtronSmp.eRecordStatus.Recording;
                _buttons[1][Digitals.ExtronRec1RecordBtn].Enabled = true;
            }
        }

        private void OnRec2StatusUpdated(ExtronSmp device)
        {
            _timeLabels[2].SetText("{0:D2}:{1:D2}:{2:D2}", (int)device.Timer.TotalHours, device.Timer.Minutes, device.Timer.Seconds);
            _timeRemainingLabels[2].SetText(((int) device.TimeAvailable.TotalMinutes) + " mins");
            _storageLabels[2].SetText(device.StorageLocation.ToString());
            _spaceRemainingLabels[2].SetText(Tools.PrettyByteSize(device.BytesAvailable, 1));
            if (device.RecordStatus == ExtronSmp.eRecordStatus.Setup)
            {
                _buttons[2][Digitals.ExtronRec2RecordBtn].Feedback = true;
                _buttons[2][Digitals.ExtronRec2RecordBtn].Enabled = false;
            }
            else
            {
                _buttons[2][Digitals.ExtronRec2RecordBtn].Feedback = device.RecordStatus ==
                                                                     ExtronSmp.eRecordStatus.Recording;
                _buttons[2][Digitals.ExtronRec2RecordBtn].Enabled = true;
            }
        }

        private void OnRec1ButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;
            switch (args.CollectionKey)
            {
                case Digitals.ExtronRec1RecordBtn:
                    ((ASystem)UIController.System).Recorders[1].Record();
                    break;
                case Digitals.ExtronRec1StopBtn:
                    ((ASystem)UIController.System).Recorders[1].Stop();
                    break;
            }
        }

        private void OnRec2ButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;
            switch (args.CollectionKey)
            {
                case Digitals.ExtronRec2RecordBtn:
                    ((ASystem)UIController.System).Recorders[2].Record();
                    break;
                case Digitals.ExtronRec2StopBtn:
                    ((ASystem)UIController.System).Recorders[2].Stop();
                    break;
            }
        }
    }
}