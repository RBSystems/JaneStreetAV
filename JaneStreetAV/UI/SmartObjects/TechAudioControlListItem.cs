using UX.Lib2.Devices.Audio.QSC.Components;
using UX.Lib2.Models;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class TechAudioControlListItem : UISubPageReferenceListItem
    {
        private IAudioLevelControl _linkedObject;
        private ButtonCollection _buttons;

        public TechAudioControlListItem(UISubPageReferenceList list, uint index) : base(list, index)
        {
            _buttons = new ButtonCollection
            {
                {1, new UIButton(list.SmartObject, BoolOutputSigs[1].Name, BoolInputSigs[1].Name)},
                {2, new UIButton(list.SmartObject, BoolOutputSigs[2].Name, BoolInputSigs[2].Name)},
                {3, new UIButton(list.SmartObject, BoolOutputSigs[3].Name, BoolInputSigs[3].Name)}
            };
            _buttons.ButtonEvent += ButtonsOnButtonEvent;
        }

        public override object LinkedObject
        {
            get { return _linkedObject; }
            set
            {
                if(_linkedObject == value) return;

                if (_linkedObject != null)
                {
                    _linkedObject.LevelChange -= OnLevelChange;
                    _linkedObject.MuteChange -= OnMuteChange;
                }

                _linkedObject = value as IAudioLevelControl;

                if (_linkedObject == null) return;

                _linkedObject.LevelChange += OnLevelChange;
                _linkedObject.MuteChange += OnMuteChange;

                StringInputSigs[2].StringValue = _linkedObject.LevelString;
                BoolInputSigs[3].BoolValue = _linkedObject.Muted;
            }
        }

        private void ButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Pressed || _linkedObject == null) return;

            var control = _linkedObject as Gain;

            if(control == null) return;

            switch (args.CollectionKey)
            {
                case 1:
                    control.GainValue = control.GainValue + 1;
                    break;
                case 2:
                    control.GainValue = control.GainValue - 1;
                    break;
                case 3:
                    var mute = !control.Muted;
                    control.Muted = mute;
                    button.Feedback = mute;
                    break;
            }
        }

        private void OnMuteChange(bool muted)
        {
            BoolInputSigs[3].BoolValue = muted;
        }

        private void OnLevelChange(IAudioLevelControl control, ushort level)
        {
            StringInputSigs[2].StringValue = control.LevelString;            
        }
    }
}