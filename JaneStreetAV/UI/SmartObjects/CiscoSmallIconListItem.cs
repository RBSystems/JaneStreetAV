using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class CiscoSmallIconListItem : UISubPageReferenceListItem
    {
        public CiscoSmallIconListItem(UISubPageReferenceList list, uint index)
            : base(list, index)
        {

        }

        public CiscoSmallIconListItemType IconType
        {
            get { return (CiscoSmallIconListItemType)UShortInputSigs[1].UShortValue; }
            set { UShortInputSigs[1].UShortValue = (ushort) value; }
        }
    }

    public enum CiscoSmallIconListItemType
    {
        MicMute,
        EndCall,
        Keypad,
        Hold,
        Share
    }
}