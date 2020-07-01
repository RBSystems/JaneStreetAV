using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using JaneStreetAV.Models.Config;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Devices.Audio.QSC.Components;
using UX.Lib2.Models;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class AudioFaderListItem : UISubPageReferenceListItem
    {
        private readonly UILabel _nameLabel;
        private readonly UILabel _levelLabel;
        private readonly UISlider _slider;
        private readonly UIButton _upBtn;
        private readonly UIButton _dnBtn;
        private readonly UIButton _muteBtn;
        private Gain _gain;
        private string _name;
        private UIButton _cueBtn;
        private MixerItem _cueMixerItem;
        private SignalPresence _sigOk;
        private SignalPresence _sigGood;
        private SignalPresence _sigPeak;

        public AudioFaderListItem(UISubPageReferenceList list, uint index) : base(list, index)
        {
            _nameLabel = new UILabel(list.SmartObject, StringInputSigs[1].Name);
            _levelLabel = new UILabel(list.SmartObject, StringInputSigs[2].Name);
            _slider = new UISlider(list.SmartObject, UShortInputSigs[1].Name, UShortOutputSigs[1].Name,
                BoolOutputSigs[1].Name);
            _slider.AnalogValueChanged += SliderOnAnalogValueChanged;
            _upBtn = new UIButton(list.SmartObject, BoolOutputSigs[2].Name, BoolInputSigs[2].Name);
            _upBtn.ButtonEvent += OnButtonEvent;
            _dnBtn = new UIButton(list.SmartObject, BoolOutputSigs[3].Name, BoolInputSigs[3].Name);
            _dnBtn.ButtonEvent += OnButtonEvent;
            _muteBtn = new UIButton(list.SmartObject, BoolOutputSigs[4].Name, BoolInputSigs[4].Name);
            _muteBtn.ButtonEvent += OnButtonEvent;
            _cueBtn = new UIButton(list.SmartObject, BoolOutputSigs[5].Name, BoolInputSigs[5].Name)
            {
                EnableJoin = BoolInputSigs[6],
                HoldTime = TimeSpan.FromSeconds(0.5)
            };
            _cueBtn.ButtonEvent += CueBtnOnButtonEvent;
            UShortInputSigs[5].UShortValue = 6;
            UShortInputSigs[6].UShortValue = 3;
            UShortInputSigs[7].UShortValue = 0;
        }

        public new string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    return _name;
                }
                return _gain != null ? _gain.Name : base.Name;
            }
            set
            {
                base.Name = value;
                _name = value;
                _nameLabel.SetText(_name);
            }
        }

        public override object LinkedObject
        {
            get { return _gain; }
            set
            {
                base.LinkedObject = value;

                var gain = value as Gain;

                if (_gain == gain) return;

                if (_gain != null)
                {
                    _gain.LevelChange -= GainOnLevelChange;
                    _gain.MuteChange -= _muteBtn.SetFeedback;

                    if (_sigOk != null)
                    {
                        _sigOk.SignalPresenceChanged -= SetSignalOk;
                    }
                    if (_sigGood != null)
                    {
                        _sigGood.SignalPresenceChanged -= SetSignalGood;
                        if (_sigPeak != null)
                        {
                            _sigPeak.SignalPresenceChanged -= SetSignalPeak;
                        }
                    }
                }

                _gain = gain;

                if (_gain == null) return;
                _gain.LevelChange += GainOnLevelChange;
                _gain.MuteChange += _muteBtn.SetFeedback;
                _nameLabel.SetText(_gain.Name);
                _levelLabel.SetText(_gain.LevelString);
                _slider.SetValue(_gain.Level);
                _muteBtn.Feedback = _gain.Muted;

                var conf = ConfigManager.Config.DspFaderComponents.FirstOrDefault(c => c.ComponentName == _gain.Name);
                if (conf == null)
                {
                    BoolInputSigs[7].BoolValue = false;
                    return;
                }
                var trafficLightName = conf.TrafficLightBaseName;
                if (string.IsNullOrEmpty(trafficLightName))
                {
                    BoolInputSigs[7].BoolValue = false;
                    return;
                }
                var dsp = _gain.Core;
                if (!dsp.ContainsComponentWithName(trafficLightName + ".signal") ||
                    !dsp.ContainsComponentWithName(trafficLightName + ".good") ||
                    !dsp.ContainsComponentWithName(trafficLightName + ".peak")) return;
                _sigOk = dsp[trafficLightName + ".signal"] as SignalPresence;
                _sigOk.SignalPresenceChanged += SetSignalOk;
                _sigGood = dsp[trafficLightName + ".good"] as SignalPresence;
                _sigGood.SignalPresenceChanged += SetSignalGood;
                _sigPeak = dsp[trafficLightName + ".peak"] as SignalPresence;
                _sigPeak.SignalPresenceChanged += SetSignalPeak;
                BoolInputSigs[7].BoolValue = true;
            }

        }

        public MixerItem CueMixerItem
        {
            get { return _cueMixerItem; }
            set
            {
                if(_cueMixerItem == value) return;

                if (_cueMixerItem != null)
                {
                    _cueMixerItem.MuteChange -= CueMixerItemOnMuteChange;
                }

                _cueMixerItem = value;

                if (_cueMixerItem == null)
                {
                    _cueBtn.Disable();
                    return;
                }

                _cueMixerItem.MuteChange += CueMixerItemOnMuteChange;
                _cueBtn.SetFeedback(!_cueMixerItem.Muted);
                _cueBtn.Enable();
            }
        }

        public void SetColour(string code)
        {
            try
            {
                var regex = Regex.Match(code, @"#(\w{2})(\w{2})(\w{2})");
                if (!regex.Success) return;
                var red = int.Parse(regex.Groups[1].Value, NumberStyles.HexNumber);
                var green = int.Parse(regex.Groups[2].Value, NumberStyles.HexNumber);
                var blue = int.Parse(regex.Groups[3].Value, NumberStyles.HexNumber);
                UShortInputSigs[2].UShortValue = (ushort) red;
                UShortInputSigs[3].UShortValue = (ushort) green;
                UShortInputSigs[4].UShortValue = (ushort) blue;
            }
            catch (Exception e)
            {
                CloudLog.Error("Could not set color of colorchip from string \"{0}\", {1}", code, e.Message);
            }
        }

        private void SetSignalOk(bool value)
        {
            UShortInputSigs[5].UShortValue = (ushort) (6 + (value ? 1 : 0));
        }

        private void SetSignalGood(bool value)
        {
            UShortInputSigs[6].UShortValue = (ushort)(3 + (value ? 1 : 0));
        }

        private void SetSignalPeak(bool value)
        {
            UShortInputSigs[7].UShortValue = (ushort)(0 + (value ? 1 : 0));
        }

        private void SliderOnAnalogValueChanged(IAnalogTouch item, ushort value)
        {
            _gain.Level = value;
        }

        private new void OnButtonEvent(IButton button, ButtonEventArgs args)
        {
            switch (args.EventType)
            {
                case ButtonEventType.Pressed:
                    if (button == _dnBtn)
                    {
                        Decrement();
                        return;
                    }
                    if (button == _upBtn)
                    {
                        Increment();
                        return;
                    }
                    break;
                case ButtonEventType.Tapped:
                    break;
                case ButtonEventType.Held:
                    break;
                case ButtonEventType.Released:
                    if (button == _muteBtn)
                    {
                        var mute = !_gain.Muted;
                        button.Feedback = mute;
                        _gain.Muted = mute;
                        return;
                    }
                    break;
            }
        }

        private void Increment()
        {
            var cValue = _gain.GainValue;
            if (Math.Ceiling(cValue) - cValue > 0)
            {
                _gain.GainValue = (float) Math.Ceiling(cValue);
                return;
            }
            _gain.GainValue = cValue + 1;
        }

        private void Decrement()
        {
            var cValue = _gain.GainValue;
            if (cValue - Math.Floor(cValue) > 0)
            {
                _gain.GainValue = (float)Math.Floor(cValue);
                return;
            }
            _gain.GainValue = cValue - 1;
        }

        private void GainOnLevelChange(IAudioLevelControl control, ushort level)
        {
            _slider.SetValue(level);
            _levelLabel.SetText(control.LevelString);
        }

        private void CueBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType == ButtonEventType.Held && !_cueMixerItem.Muted)
            {
                if (_cueMixerItem != null)
                {
                    foreach (var input in _cueMixerItem.Mixer.Inputs.Where(i => i != _cueMixerItem))
                    {
                       input.Mute();
                    }
                }
            }

            if(args.EventType != ButtonEventType.Pressed) return;

            if (_cueMixerItem == null) return;
            var mute = !_cueMixerItem.Muted;
            _cueMixerItem.Muted = mute;
            button.SetFeedback(!mute);
        }

        private void CueMixerItemOnMuteChange(bool muted)
        {
            _cueBtn.SetFeedback(!muted);
        }
    }
}