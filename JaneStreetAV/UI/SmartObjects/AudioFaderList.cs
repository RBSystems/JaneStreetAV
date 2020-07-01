using Crestron.SimplSharpPro;
using UX.Lib2.Devices.Audio.QSC.Components;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class AudioFaderList : UISubPageReferenceList
    {
        public AudioFaderList(UIControllerWithSmartObjects uiController, SmartObject smartObject)
            : base(uiController, smartObject, 7, 7, 2, (list, index) => new AudioFaderListItem(list, index))
        {

        }

        public uint AddItem(Gain gainControl)
        {
            return base.AddItem(gainControl.Name, gainControl, true);
        }   
    }
}