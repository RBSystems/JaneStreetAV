using Crestron.SimplSharpPro;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class CiscoSmallIconList : UISubPageReferenceList
    {
        public CiscoSmallIconList(UIControllerWithSmartObjects uiController, SmartObject smartObject)
            : base(uiController, smartObject, 1, 1, 1, (list, index) => new CiscoSmallIconListItem(list, index))
        {

        }

        public uint AddItem(CiscoSmallIconListItemType iconType, string name)
        {
            var index = AddItem(name, iconType, false);
            ((CiscoSmallIconListItem)this[index]).IconType = iconType;
            return index;
        }
    }
}