using Crestron.SimplSharpPro;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class CiscoLargeIconList : UISubPageReferenceList
    {
        public CiscoLargeIconList(UIControllerWithSmartObjects uiController, SmartObject smartObject)
            : base(uiController, smartObject, 1, 1, 1, (list, index) => new CiscoLargeIconListItem(list, index))
        {

        }

        public uint AddItem(CiscoLargeIconListItemType iconType, string name, IVisibleItem view)
        {
            var index = AddItem(name, view, false);
            ((CiscoLargeIconListItem) this[index]).IconType = iconType;
            return index;
        }
    }
}