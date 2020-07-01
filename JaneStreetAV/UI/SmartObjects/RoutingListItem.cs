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
using System.Globalization;
using System.Text.RegularExpressions;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class RoutingListItem : UISubPageReferenceListItem
    {
        private bool _active;
        private uint _number;

        #region Fields
        #endregion

        #region Constructors

        public RoutingListItem(UISubPageReferenceList list, uint index)
            : base(list, index)
        {
            SetColourTag("#666666");
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public RoutingItemListColor Color
        {
            get { return (RoutingItemListColor) UShortInputSigs[1].UShortValue; }
            set { UShortInputSigs[1].UShortValue = (ushort) value; }
        }

        public uint Number
        {
            get { return _number; }
            set
            {
                _number = value;
                StringInputSigs[2].StringValue = string.Format("<FONT color=\"FF8000\">{0:D2}</FONT>", _number);
            }
        }

        public bool Active
        {
            get { return _active; }
            set
            {
                if(_active == value) return;
                _active = value;
                StringInputSigs[2].StringValue = string.Format("<FONT color=\"{0}\">{1:D2}</FONT>",
                    _active ? "00FF00" : "FF8000", _number);
            }
        }

        #endregion

        #region Methods

        public void SetColourTag(string code)
        {
            try
            {
                var regex = Regex.Match(code, @"#(\w{2})(\w{2})(\w{2})");
                if (!regex.Success) return;
                var red = int.Parse(regex.Groups[1].Value, NumberStyles.HexNumber);
                var green = int.Parse(regex.Groups[2].Value, NumberStyles.HexNumber);
                var blue = int.Parse(regex.Groups[3].Value, NumberStyles.HexNumber);
                UShortInputSigs[2].UShortValue = (ushort)red;
                UShortInputSigs[3].UShortValue = (ushort)green;
                UShortInputSigs[4].UShortValue = (ushort)blue;
            }
            catch (Exception e)
            {
                CloudLog.Error("Could not set color of colorchip from string \"{0}\", {1}", code, e.Message);
            }
        }

        #endregion
    }

    public enum RoutingItemListColor
    {
        Blue,
        Green,
        Red
    }
}