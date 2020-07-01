using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class CiscoLargeIconListItem : UISubPageReferenceListItem
    {
        public CiscoLargeIconListItem(UISubPageReferenceList list, uint index)
            : base(list, index)
        {

        }

        public CiscoLargeIconListItemType IconType
        {
            get { return (CiscoLargeIconListItemType) UShortInputSigs[1].UShortValue; }
            set { UShortInputSigs[1].UShortValue = (ushort) value; }
        }
    }

    public enum CiscoLargeIconListItemType
    {
        Call,
        Share
    }
}