using System;
using JaneStreetAV.UI.Joins;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views
{
    public class TextFieldEntryViewController : ASubPage
    {
        private UITextEntry _textEntry;
        private TextEntryViewCallback _callback;

        public TextFieldEntryViewController(UIController uiController)
            : base(uiController, Digitals.SubPageTextEntry, "Text Entry", TimeSpan.FromSeconds(30))
        {
            _textEntry = new UITextEntry(this, Serials.TextEntryFieldText, Digitals.TextEntryEnter);
        }

        public void Show(string withText, TextEntryViewCallback callback)
        {
            _callback = callback;
            _textEntry.Text = withText;
            base.Show();
        }

        protected override void WillShow()
        {
            base.WillShow();

            _textEntry.KeyboardDidEnter += TextEntryOnKeyboardDidEnter;
        }

        protected override void WillHide()
        {
            base.WillHide();

            _textEntry.KeyboardDidEnter -= TextEntryOnKeyboardDidEnter;
        }

        protected override void DidShow()
        {
            base.DidShow();

            Device.BooleanInput[Digitals.TextEntryKeyboardFocusOn].Pulse(10);
        }

        private void TextEntryOnKeyboardDidEnter(UITextEntry textEntry, string text)
        {
            Hide();
            if (_callback != null)
            {
                _callback(text);
            }
        }
    }

    public delegate void TextEntryViewCallback(string text);
}