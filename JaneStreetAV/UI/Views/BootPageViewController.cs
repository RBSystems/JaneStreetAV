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

using JaneStreetAV.UI.Joins;
using UX.Lib2.Models;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views
{
    public class BootPageViewController : APage
    {
        #region Fields

        private readonly UIGuage _progressGuage;
        private readonly UILabel _statusLabel;

        #endregion

        #region Constructors

        public BootPageViewController(BaseUIController uiController, uint pageNumber, string title)
            : base(uiController, pageNumber, title)
        {
            _progressGuage = new UIGuage(this, Analogs.PageBootProgressLevel);
            _statusLabel = new UILabel(this, Serials.PageBootStatus);
            uiController.System.SystemStartupProgressChange += SystemOnSystemStartupProgressChange;
        }

        private void SystemOnSystemStartupProgressChange(SystemBase system, SystemStartupProgressEventArgs args)
        {
            if (!Visible)
            {
                Show();
            }
            else
            {
                VisibleJoin.BoolValue = false;
                VisibleJoin.BoolValue = true;
            }

            _statusLabel.SetText(args.Message);
            _progressGuage.SetValue(args.ProgressLevel);
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
        #endregion
    }
}