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

using UX.Lib2;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class HomeIconMenuItem : UISubPageReferenceListItem
    {
        #region Fields

        private HomeIconMode _iconMode;

        #endregion

        #region Constructors

        public HomeIconMenuItem(UISubPageReferenceList list, uint index)
            : base(list, index)
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

        public HomeIconMode IconMode
        {
            get { return _iconMode; }
            set
            {
                _iconMode = value;
                UShortInputSigs[1].UShortValue = (ushort) value;
#if DEBUG
                Debug.WriteInfo(GetType().Name + " Index " + Index, "IconMode set to {0} {1}", (ushort) value, value);
#endif
            }
        }

        #endregion

        #region Methods
        #endregion
    }

    public enum HomeIconMode
    {
        Dialpad,
        Contacts,
        Calendar,
        Phone,
        HDMI,
        Wireless,
        Desktop,
        Presentation,
        TV
    }
}