/* License
 * ------------------------------------------------------------------------------
 * Copyright (c) 2017 UX Digital Systems Ltd
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * ------------------------------------------------------------------------------
 * UX.Digital
 * ----------
 * http://ux.digital
 * support@ux.digital
 */

using System;
using JaneStreetAV.UI.Joins;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views
{
    public class ASubPageWithCloseButton : ASubPage
    {
        #region Fields

        private readonly UIButton _backButton;

        #endregion

        #region Constructors

        public ASubPageWithCloseButton(UIViewController parentViewController, uint joinNumber, string title, TimeSpan timeout)
            : base(parentViewController, parentViewController.UIController.Device.BooleanInput[joinNumber], title, timeout)
        {
            _backButton = new UIButton(parentViewController.UIController, Digitals.SubPageBackButton, Serials.SubPageBackButtonTitle);
        }

        public ASubPageWithCloseButton(UIViewController parentViewController, uint joinNumber, string title)
            : base(parentViewController, parentViewController.UIController.Device.BooleanInput[joinNumber], title, TimeSpan.FromSeconds(30))
        {
            _backButton = new UIButton(parentViewController.UIController, Digitals.SubPageBackButton, Serials.SubPageBackButtonTitle);
        }

        public ASubPageWithCloseButton(BaseUIController uiController, uint joinNumber, string title)
            : base(uiController, uiController.Device.BooleanInput[joinNumber], title, TimeSpan.FromSeconds(30))
        {
            _backButton = new UIButton(uiController, Digitals.SubPageBackButton, Serials.SubPageBackButtonTitle);
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties
        #endregion

        #region Methods

        protected override void WillShow()
        {
            base.WillShow();
            _backButton.ButtonEvent += CloseButtonOnButtonEvent;
            _backButton.Text = SetBackButtonTitle();
        }

        protected override void WillHide()
        {
            base.WillHide();
            _backButton.ButtonEvent -= CloseButtonOnButtonEvent;
        }

        protected void SetBackButtonTitle(string title)
        {
            _backButton.Text = title;
        }

        protected virtual string SetBackButtonTitle()
        {
            var parentView = Parent as UIViewController;
            if (parentView != null)
            {
                return parentView.ViewName;
            }
            return UIController.CurrentPage.ViewName;
        }

        private void CloseButtonOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;
            BackButtonPushed();
        }

        protected virtual void BackButtonPushed()
        {
            Hide();
        }

        #endregion
    }
}