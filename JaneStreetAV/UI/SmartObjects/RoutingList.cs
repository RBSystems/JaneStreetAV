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
using Crestron.SimplSharpPro.DM;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class RoutingList : UISubPageReferenceList
    {
        #region Fields
        #endregion

        #region Constructors

        public RoutingList(UIControllerWithSmartObjects uiController, SmartObject smartObject)
            : base(uiController, smartObject, 2, 4, 2, (list, index) => new RoutingListItem(list, index))
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

        public RoutingListItem AddItem(DMInput input, bool holdOffSettingListSize)
        {
            var index = base.AddItem(input.Name.StringValue, input, holdOffSettingListSize);
            ((RoutingListItem)this[index]).Number = input.Number;
            this[index].Visible = !string.IsNullOrEmpty(input.Name.StringValue);
            return this[index] as RoutingListItem;
        }

        public RoutingListItem AddItem(DMOutput output, bool holdOffSettingListSize)
        {
            var index = base.AddItem(output.Name.StringValue, output, holdOffSettingListSize);
            ((RoutingListItem)this[index]).Number = output.Number;
            this[index].Visible = !string.IsNullOrEmpty(output.Name.StringValue);

            return this[index] as RoutingListItem;
        }

        public RoutingListItem AddItem(uint inputOutput, string name, bool holdOffSettingListSize)
        {
            var index = base.AddItem(name, inputOutput, holdOffSettingListSize);
            ((RoutingListItem) this[index]).Number = inputOutput;
            this[index].Visible = !string.IsNullOrEmpty(name);

            return this[index] as RoutingListItem;
        }

        #endregion
    }
}