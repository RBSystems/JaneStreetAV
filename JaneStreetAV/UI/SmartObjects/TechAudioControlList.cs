using Crestron.SimplSharpPro;
using UX.Lib2.Models;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class TechAudioControlList : UISubPageReferenceList
    {
        public TechAudioControlList(UIControllerWithSmartObjects uiController, SmartObject smartObject)
            : base(uiController, smartObject, 3, 1, 2, (list, index) => new TechAudioControlListItem(list, index))
        {

        }

        public uint AddItem(IAudioLevelControl control)
        {
            return base.AddItem(control.Name, control, true);
        }
    }
}