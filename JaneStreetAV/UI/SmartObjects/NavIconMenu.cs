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

using Crestron.SimplSharpPro;
using UX.Lib2;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class NavIconMenu : UISubPageReferenceList
    {
        #region Fields
        #endregion

        #region Constructors

        public NavIconMenu(UIControllerWithSmartObjects uiController, SmartObject smartObject)
            : base(uiController, smartObject, 3, 2, 1, (list, index) => new NavIconMenuItem(list, index))
        {

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

        public NavIconMenuItem AddItem(JSIcons iconMode, string title, object linkedObject)
        {
            var index = AddItem(title, linkedObject);

            ((NavIconMenuItem)this[index]).Icon = iconMode;
            ((NavIconMenuItem)this[index]).IsBlank = false;

            return this[index] as NavIconMenuItem;
        }

        public NavIconMenuItem AddBlankItem(object linkedObject)
        {
            var index = AddItem(string.Empty, linkedObject);

            ((NavIconMenuItem)this[index]).IsBlank = false;

            return this[index] as NavIconMenuItem;
        }

        #endregion
    }
}