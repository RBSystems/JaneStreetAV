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

using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class NavIconMenuItem : UISubPageReferenceListItem
    {
        #region Fields
        
        private JSIcons _icon;

        #endregion

        #region Constructors

        public NavIconMenuItem(UISubPageReferenceList list, uint index)
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

        public JSIcons Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                UShortInputSigs[2].UShortValue = (ushort) value;
            }
        }

        public bool IsBlank
        {
            get { return UShortInputSigs[1].UShortValue == 0; }
            set
            {
                if (value)
                {
                    BoolInputSigs[2].BoolValue = false;
                    BoolInputSigs[3].BoolValue = false;
                }
                UShortInputSigs[1].UShortValue = (ushort) (value ? 0 : 1);
            }
        }

        public override bool Feedback
        {
            get { return base.Feedback; }
            set
            {
                base.Feedback = value;
                BoolInputSigs[2].BoolValue = !value;
                BoolInputSigs[3].BoolValue = value;
            }
        }

        #endregion

        #region Methods
        #endregion
    }
}