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
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using JaneStreetAV.UI.Joins;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.UI;
using ButtonEventArgs = UX.Lib2.UI.ButtonEventArgs;
using IButton = UX.Lib2.UI.IButton;

namespace JaneStreetAV.UI.Views
{
    public abstract class APage : UIPageViewController
    {
        #region Fields

        private string _title = string.Empty;
        private readonly UIButton _backButton;

        #endregion

        #region Constructors

        protected APage(BaseUIController uiController, uint pageNumber, string title)
            : base(uiController, pageNumber)
        {
            _title = title;
            _backButton = new UIButton(this, Digitals.PageBackButton, Serials.PageBackButtonTitle);
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public new BasicTriListWithSmartObject Device
        {
            get { return UIController.Device; }
        }

        public new BaseUIController UIController
        {
            get { return base.UIController as BaseUIController; }
        }

        public override string ViewName
        {
            get { return _title; }
        }

        #endregion

        #region Methods

        protected override void OnSigChange(GenericBase owner, SigEventArgs args)
        {
            
        }

        protected override void WillShow()
        {
            _backButton.ButtonEvent += BackButtonOnButtonEvent;

            Device.StringInput[Serials.PageTitle].StringValue = ViewName;
        }

        protected override void DidShow()
        {
            _backButton.Text = SetBackButtonTitle();
        }

        protected override void WillHide()
        {
            _backButton.ButtonEvent -= BackButtonOnButtonEvent;
        }

        protected override void DidHide()
        {

        }

        private void BackButtonOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;
            BackButtonPressed();
        }

        protected virtual void BackButtonPressed()
        {
            Back();
        }

        protected virtual string SetBackButtonTitle()
        {
            try
            {
                if (PreviousPage != null && !String.IsNullOrEmpty(PreviousPage.ViewName))
                {
                    return PreviousPage.ViewName;
                }
            }
            catch (Exception e)
            {
                CloudLog.Error("Error in {0}.SetBackButtonTitle(), {1}", GetType().Name, e.Message);
            }

            return string.Empty;
        }

        public void SetViewName(string title)
        {
            _title = title;
        }

        #endregion
    }
}