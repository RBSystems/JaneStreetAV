using Crestron.SimplSharpPro;
using UX.Lib2.DeviceSupport;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class PowerControlList : UISubPageReferenceList
    {
        public PowerControlList(UIControllerWithSmartObjects uiController, SmartObject smartObject)
            : base(uiController, smartObject, 2, 1, 1, (list, index) => new PowerControlListItem(list, index))
        {
        }

        public uint AddItem(IPowerDevice device)
        {
            return base.AddItem(device.Name, device, true);
        }
    }
}