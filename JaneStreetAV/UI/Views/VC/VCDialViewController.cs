using System;
using System.Linq;
using Crestron.SimplSharpPro.CrestronThread;
using JaneStreetAV.UI.Joins;
using JaneStreetAV.UI.SmartObjects;
using UX.Lib2;
using UX.Lib2.Devices.Cisco.CallHistory;
using UX.Lib2.Devices.Cisco.Phonebook;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.VC
{
    public class VCDialViewController : VCSubViewControllerBase
    {
        private readonly UIButton _closeBtn;
        private readonly UILabel _textField;
        private readonly UIButton _textFieldButton;
        private readonly UIButton _textFieldClearButton;
        private readonly VCKeyboardViewController _keyboard;
        private string _text = string.Empty;
        private readonly VCDialList _list;
        private readonly ButtonCollection _listButtons;
        private Thread _listActionThread;
        private string _currentSearchString;
        private UIButton _dotComBtn;

        public VCDialViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageVCDial)
        {
            var ui = parentViewController.UIController as UIControllerWithSmartObjects;
            _list = new VCDialList(ui, ui.Device.SmartObjects[Joins.SmartObjects.VCDialList]);
            _listButtons = new ButtonCollection(_list);
            _closeBtn = new UIButton(this, Digitals.VCDialViewCloseBtn);
            _textField = new UILabel(this, Serials.VCDialTextField);
            _textFieldButton = new UIButton(this, Digitals.VCDialTextField);
            _textFieldClearButton = new UIButton(this, Digitals.VCDialTextFieldClear)
            {
                VisibleJoin = Device.BooleanInput[Digitals.VCDialTextFieldClearVisible]
            };
            _keyboard = new VCKeyboardViewController(this);
            _dotComBtn = new UIButton(this, Digitals.VCDialKeyboardDotComBtn);
        }

        protected override void WillShow()
        {
            base.WillShow();
            _keyboard.VisibilityChanged += KeyboardOnVisibilityChanged;
            _keyboard.TypedValue += KeyboardOnTypedValue;
            _closeBtn.ButtonEvent += CloseBtnOnButtonEvent;
            _textFieldButton.ButtonEvent += TextFieldButtonOnButtonEvent;
            _textFieldClearButton.ButtonEvent += TextFieldClearButtonOnButtonEvent;
            _listButtons.ButtonEvent += ListButtonsOnButtonEvent;
            _dotComBtn.ButtonEvent += DotComBtnOnButtonEvent;
            Clear();
            GetCallHistory();
        }

        protected override void WillHide()
        {
            base.WillHide();
            _keyboard.VisibilityChanged -= KeyboardOnVisibilityChanged;
            _keyboard.TypedValue -= KeyboardOnTypedValue;
            _closeBtn.ButtonEvent -= CloseBtnOnButtonEvent;
            _textFieldButton.ButtonEvent -= TextFieldButtonOnButtonEvent;
            _textFieldClearButton.ButtonEvent -= TextFieldClearButtonOnButtonEvent;
            _listButtons.ButtonEvent -= ListButtonsOnButtonEvent;
            _dotComBtn.ButtonEvent -= DotComBtnOnButtonEvent;
            _list.ClearList(true);
        }

        private void CloseBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType == ButtonEventType.Pressed) Parent.ResetView();
        }

        private void KeyboardOnTypedValue(ButtonEventType eventType, char value)
        {
            if(value == 8 && eventType == ButtonEventType.Held) SetText(string.Empty);

            if (eventType != ButtonEventType.Tapped) return;

            var str = _text;

            if (value == 8 && str.Length > 0)
            {
                str = str.Substring(0, _text.Length - 1);
            }
            else if (value != 8)
            {
                str = str + value;
            }

            if (str == _text) return;

            if (str.Length > 0)
            {
                if (str.Length == 1)
                {
                    _list.ClearList(true);
                    _list.AddItem("Searching directory..");
                    _list.SetListSizeToItemCount();
                }
                Search(str);
            }
            else
            {
                GetCallHistory();
            }
            SetText(str);
        }

        private void DotComBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Tapped) return;
            SetText(_text + ".com");
        }

        private void Clear()
        {
            SetText(string.Empty);
        }

        private void SetText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                SetText(string.Empty, "#777777", _keyboard.RequestedVisibleState);
                return;
            }
            SetText(text, "#ffffff", true);
        }

        private void SetText(string text, string colorHex, bool leftAlign)
        {
            var textToSet = text;
            _text = text;
            if (string.IsNullOrEmpty(textToSet))
            {
                textToSet = "Search or dial";
            }
            _textField.SetText("<P align=\"{0}\"><FONT size=\"32\" face=\"Segoe UI Light\" " +
                               "color=\"{1}\">{2}</FONT></P>", leftAlign ? "left" : "center", colorHex, textToSet);
        }

        private void TextFieldClearButtonOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType == ButtonEventType.Released) Clear();
        }

        private void TextFieldButtonOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType == ButtonEventType.Pressed) _keyboard.Show();
        }

        private void KeyboardOnVisibilityChanged(IVisibleItem item, VisibilityChangeEventArgs args)
        {
            switch (args.EventType)
            {
                case VisibilityChangeEventType.WillShow:
                    SetText(string.Empty);
                    break;
                case VisibilityChangeEventType.DidShow:
                    break;
                case VisibilityChangeEventType.WillHide:
                    break;
                case VisibilityChangeEventType.DidHide:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ListButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;

            var item = _list[args.CollectionKey].LinkedObject as CallHistoryItem;

            if (item != null)
            {
                Codec.Calls.DialNumber(item.CallbackNumber, (code, description, id) =>
                {
                    if (code == 0) return;

                    //Parent.DialView.Hide();

                    _textField.Clear();
                    ((BaseUIController)UIController).ActionSheetDefault.SetTimeOutInSeconds(5);
                    ((BaseUIController)UIController).ActionSheetDefault.Show(
                        (type, index) => { }, "<FONT color=\"#E00716\">Could not place call</FONT>",
                        "Error " + code + " - " + description, "OK");
                });
                return;
            }

            var contact = _list[args.CollectionKey].LinkedObject as PhonebookContact;
            if (contact != null && contact.ContactMethods.Any())
            {
                Codec.Calls.DialNumber(contact.ContactMethods.First().Value.Number, (code, description, id) =>
                {
                    if (code == 0) return;

                    //Parent.DialView.Hide();

                    _textField.Clear();
                    ((BaseUIController)UIController).ActionSheetDefault.SetTimeOutInSeconds(5);
                    ((BaseUIController)UIController).ActionSheetDefault.Show(
                        (type, index) => { }, "<FONT color=\"#E00716\">Could not place call</FONT>",
                        "Error " + code + " - " + description, "OK");
                });
            }
        }

        public void AttemptCall()
        {
            Codec.Calls.DialNumber(_text, (code, description, id) =>
            {
                if (code == 0) return;

                //Parent.DialView.Hide();

                _keyboard.Hide();
                ((BaseUIController)UIController).ActionSheetDefault.SetTimeOutInSeconds(5);
                ((BaseUIController)UIController).ActionSheetDefault.Show(
                    (type, index) => { }, "<FONT color=\"#E00716\">Could not place call</FONT>",
                    "Error " + code + " - " + description, "OK");
            });
        }

        private void Search(string searchString)
        {
            _currentSearchString = searchString;
            if (_listActionThread != null && _listActionThread.ThreadState == Thread.eThreadStates.ThreadRunning)
            {
                return;
            }

            _listActionThread = new Thread(specific =>
            {
                try
                {
                    while (true)
                    {
                        var results = Codec.Phonebook.Search(PhonebookType.Corporate, _currentSearchString,
                        (int)_list.MaxNumberOfItems - 1, 0);
                        _list.ClearList(true);
                        if (results.Count == 0)
                        {
                            _list.AddItem(string.Format("No contacts matching \"{0}\"", results.SearchString));
                            _list.SetListSizeToItemCount();
                            return null;
                        }
                        _list.AddItem(string.Format("Showing {0} of {1} matching contacts", results.Count, results.TotalRows));
                        foreach (var result in results)
                        {
                            _list.AddItem(result);
                        }
                        _list.SetListSizeToItemCount();

                        if (_currentSearchString == results.SearchString)
                        {
                            return null;
                        }
                    }
                }
                catch
                {
                    _list.SetListSizeToItemCount();
                    return null;
                }
            }, null);
        }

        private void GetCallHistory()
        {
            if (_listActionThread != null && _listActionThread.ThreadState == Thread.eThreadStates.ThreadRunning)
            {
                _listActionThread.Abort();
            }

            _listActionThread = new Thread(specific =>
            {
                var history = Codec.CallHistory.Get(Filter.All, 0, 10);
                _list.ClearList(true);
                if (history.Count == 0) return null;
                foreach (var result in history)
                {
                    Debug.WriteWarn("History", "{0}, {1} {2} {3} days ago", result.DisplayName, result.CallbackNumber,
                        result.StartTime, result.DaysAgo);
                }
                var daysAgo = history.First().DaysAgo;
                if (daysAgo == 0)
                {
                    _list.AddItem("Today");
                }
                else
                {
                    _list.AddItem(string.Format("{0} days ago", daysAgo));
                }
                foreach (var result in history)
                {
                    if (result.DaysAgo != daysAgo)
                    {
                        _list.AddItem(string.Format("{0} days ago", result.DaysAgo));
                        daysAgo = result.DaysAgo;
                    }
                    _list.AddItem(result);
                }
                _list.SetListSizeToItemCount();
                return null;
            }, null);
        }
    }
}