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
using Crestron.SimplSharp;
using JaneStreetAV.UI.Joins;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views
{
    public class ActionSheetDefault : UIActionSheet
    {
        #region Fields

        private readonly UILabel _timeRemainingLabel;
        private CTimer _timeTimer;

        #endregion

        #region Constructors

        public ActionSheetDefault(BaseUIController uiController)
            : this(uiController, Digitals.SubPageActionSheetDefault)
        {

        }

        public ActionSheetDefault(BaseUIController uiController, uint joinNumber)
            : base(uiController,
                new ASubPage(uiController, uiController.Device.BooleanInput[joinNumber], string.Empty, TimeSpan.Zero),
                uiController.Device.SmartObjects[Joins.SmartObjects.ActionSheetButtonsDefault],
                new UILabel(uiController.Device, Serials.ActionSheetTitle),
                new UILabel(uiController.Device, Serials.ActionSheetSubTitle))
        {
            _timeRemainingLabel = new UILabel(uiController.Device, Serials.ActionSheetTimeRemaining);
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

        protected override void ViewOnVisibilityChanged(IVisibleItem item, VisibilityChangeEventArgs args)
        {
            base.ViewOnVisibilityChanged(item, args);

            switch (args.EventType)
            {
                case VisibilityChangeEventType.WillShow:
                    if (_timeTimer == null || _timeTimer.Disposed)
                        _timeTimer =
                            new CTimer(
                                specific =>
                                {
                                    try
                                    {
                                        _timeRemainingLabel.SetText(
                                            new DateTime(TimeRemaining.Ticks).ToString(@"mm:ss"));
                                    }
                                    catch (Exception e)
                                    {
                                        _timeTimer.Dispose();
                                        _timeTimer = null;
                                    }
                                },
                                null, 100, 1000);
                    break;
                case VisibilityChangeEventType.DidHide:
                    if (_timeTimer != null && !_timeTimer.Disposed)
                        _timeTimer.Dispose();
                    _timeTimer = null;
                    break;
            }
        }

        #endregion
    }
}