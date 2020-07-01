using Crestron.SimplSharpPro;
using UX.Lib2.DeviceSupport;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class PowerControlListItem : UISubPageReferenceListItem
    {
        private IPowerDevice _device;

        public PowerControlListItem(UISubPageReferenceList list, uint index)
            : base(list, index)
        {
            ButtonEvent += OnButtonEvent;
        }

        public override object LinkedObject
        {
            get { return base.LinkedObject; }
            set
            {
                base.LinkedObject = value;

                var newDevice = value as IPowerDevice;

                if (_device == newDevice && _device != null)
                {
                    Feedback = _device.Power;
                    return;
                }

                if (_device != null)
                {
                    _device.PowerStatusChange -= DeviceOnPowerStatusChange;
                }

                _device = newDevice;

                if(_device == null) return;

                StringInputSigs[1].StringValue = _device.Name;
                _device.PowerStatusChange += DeviceOnPowerStatusChange;
                Feedback = _device.Power;
                BoolInputSigs[2].BoolValue = _device.PowerStatus != DevicePowerStatus.PowerCooling &&
                                             _device.PowerStatus != DevicePowerStatus.PowerWarming;
            }
        }

        private void DeviceOnPowerStatusChange(IPowerDevice device, DevicePowerStatusEventArgs args)
        {
            Feedback = args.NewPowerStatus == DevicePowerStatus.PowerWarming ||
                       args.NewPowerStatus == DevicePowerStatus.PowerOn;
            BoolInputSigs[2].BoolValue = device.PowerStatus != DevicePowerStatus.PowerCooling &&
                                         device.PowerStatus != DevicePowerStatus.PowerWarming;
        }

        private new void OnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;

            if (_device == null) return;
            var power = !_device.Power;
            button.Feedback = power;
            _device.Power = power;
        }
    }
}