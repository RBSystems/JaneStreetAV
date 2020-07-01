using System.Text.RegularExpressions;
using UX.Lib2.Devices.Cisco.CallHistory;
using UX.Lib2.Devices.Cisco.Phonebook;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class VCDialListItem : UISubPageReferenceListItem
    {
        private object _linkedObject;

        public VCDialListItem(UISubPageReferenceList list, uint index) : base(list, index)
        {

        }

        public override object LinkedObject
        {
            get { return _linkedObject; }
            set
            {
                _linkedObject = value;

                var s = _linkedObject as string;
                if (s != null)
                {
                    SetAsHeaderItem(s);
                    return;
                }

                var c = _linkedObject as PhonebookItem;
                if (c != null)
                {
                    SetAsCallItem(c.Name, "");
                    return;
                }

                var h = _linkedObject as CallHistoryItem;
                if (h != null)
                {
                    SetAsCallItem(h.DisplayName, h.StartTime.ToString("t"));
                }
            }
        }

        private void SetAsHeaderItem(string text)
        {
            BoolInputSigs[3].BoolValue = false;
            BoolInputSigs[4].BoolValue = false;
            BoolInputSigs[2].BoolValue = true;
            StringInputSigs[1].StringValue = text;
        }

        private void SetAsCallItem(string mainLabel, string subLabel)
        {
            BoolInputSigs[2].BoolValue = false;
            BoolInputSigs[3].BoolValue = true;
            BoolInputSigs[4].BoolValue = true;
            StringInputSigs[1].StringValue = string.Format("<FONT size=\"28\" face=\"Segoe UI\" color=\"#ffffff\">{0}<BR>" +
                          "<FONT size=\"22\" color=\"#aaaaaa\">{1}</FONT></FONT>",
                mainLabel, subLabel);
            var matches = Regex.Matches(mainLabel, @"\b[a-zA-Z]");
            if (matches.Count == 0)
            {
                StringInputSigs[2].StringValue = "#";
                return;
            }
            var letters = string.Empty;
            foreach (Match match in matches)
            {
                letters = letters + match.Value;
                if (letters.Length >= 2) break;
            }
            StringInputSigs[2].StringValue = letters.ToUpper();
        }
    }
}