using Crestron.SimplSharpPro;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class VCDialList : UISubPageReferenceList
    {
        public VCDialList(UIControllerWithSmartObjects uiController, SmartObject smartObject)
            : base(uiController, smartObject, 4, 1, 2, (list, index) => new VCDialListItem(list, index))
        {
        }

        public override uint AddItem(object item)
        {
            return base.AddItem(item, true);
        }
    }
}