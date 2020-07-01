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

using UX.Lib2.Models;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class SystemStatusListItem : UISubPageReferenceListItem
    {
        private object _linkedObject;

        #region Fields
        #endregion

        #region Constructors

        public SystemStatusListItem(UISubPageReferenceList list, uint index)
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

        public override object LinkedObject
        {
            get { return _linkedObject; }
            set
            {
                _linkedObject = value;

                var item = _linkedObject as IStatusMessageItem;

                if (item == null) return;

                UShortInputSigs[1].UShortValue = (ushort) item.MessageLevel;

                switch (item.MessageLevel)
                {
                    case StatusMessageWarningLevel.Ok:
                        StringInputSigs[1].StringValue = string.Format("<FONT color=\"#4CAF50\">All systems go!</FONT>",
                            item.MessageLevel.ToString());
                        break;
                    case StatusMessageWarningLevel.Notice:
                        StringInputSigs[1].StringValue = string.Format("<FONT color=\"#4E7AB5\">Notice</FONT>",
                            item.MessageLevel.ToString());
                        break;
                    case StatusMessageWarningLevel.Warning:
                        StringInputSigs[1].StringValue = string.Format("<FONT color=\"#BA9B48\">Warning</FONT>",
                            item.MessageLevel.ToString());
                        break;
                    case StatusMessageWarningLevel.Error:
                        StringInputSigs[1].StringValue = string.Format("<FONT color=\"#C74343\">Critical</FONT>",
                            item.MessageLevel.ToString());
                        break;
                }

                var message = string.Empty;

                if (item.SourceDeviceName.Length > 0)
                {
                    message = string.Format("<B>{0}:</B> ", item.SourceDeviceName);
                }

                message = message + item.MessageString;

                StringInputSigs[2].StringValue = message;
            }
        }

        #endregion

        #region Methods
        #endregion
    }
}