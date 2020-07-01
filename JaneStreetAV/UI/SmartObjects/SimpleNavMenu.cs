using Crestron.SimplSharpPro;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class SimpleNavMenu : UISubPageReferenceList
    {
        public SimpleNavMenu(UIControllerWithSmartObjects uiController, SmartObject smartObject)
            : base(uiController, smartObject, 1, 1, 1, (list, index) => new SimpleNaveMenuItem(list, index))
        {

        }
    }
}