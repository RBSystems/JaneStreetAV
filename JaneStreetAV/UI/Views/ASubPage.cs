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
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views
{
    public class ASubPage : UIViewController
    {
        #region Fields

        private string _title = string.Empty;
        private readonly TimeSpan _defaultTimeOut;

        #endregion

        #region Constructors

        public ASubPage(UIController uiController, BoolInputSig visibleJoin, string title, TimeSpan timeOut)
            : base(uiController, visibleJoin)
        {
            _title = title;
            _defaultTimeOut = timeOut;
        }

        public ASubPage(UIController uiController, BoolInputSig visibleJoin, IVisibleItem parentVisibleItem, string title, TimeSpan timeOut)
            : base(uiController, visibleJoin, parentVisibleItem)
        {
            _title = title;
            _defaultTimeOut = timeOut;
        }

        public ASubPage(UIViewController parentViewController, BoolInputSig visibleJoin, string title, TimeSpan timeOut)
            : base(parentViewController, visibleJoin)
        {
            _title = title;
            _defaultTimeOut = timeOut;
        }

        public ASubPage(UIController uiController, IVisibleItem parentVisibleItem, string title, TimeSpan timeOut)
            : base(uiController, parentVisibleItem)
        {
            _title = title;
            _defaultTimeOut = timeOut;
        }

        protected ASubPage(UIController uiController, uint joinNumber, string title, TimeSpan timeOut)
            : base(uiController, uiController.Device.BooleanInput[joinNumber])
        {
            _title = title;
            _defaultTimeOut = timeOut;
        }

        protected ASubPage(UIViewController parentViewController, uint joinNumber, string title, TimeSpan timeOut)
            : base(parentViewController, parentViewController.UIController.Device.BooleanInput[joinNumber])
        {
            _title = title;
            _defaultTimeOut = timeOut;
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
            get { return UIController.Device as BasicTriListWithSmartObject; }
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

        public override void Show()
        {
            if (_defaultTimeOut > TimeSpan.Zero)
            {
                Show(_defaultTimeOut);
            }
            else
            {
                base.Show();
            }
        }

        protected override void WillShow()
        {
            if (!String.IsNullOrEmpty(ViewName))
            {
                Device.StringInput[Serials.SubPageTitle].StringValue = ViewName;
            }
        }

        protected override void DidShow()
        {

        }

        protected override void WillHide()
        {

        }

        protected override void DidHide()
        {

        }

        #endregion
    }
}