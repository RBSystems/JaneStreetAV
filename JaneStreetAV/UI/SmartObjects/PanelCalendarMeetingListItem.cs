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

using UX.Lib2.Devices.Polycom;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class PanelCalendarMeetingListItem : UISubPageReferenceListItem
    {
        #region Fields
        
        private object _linkedObject;
        private readonly UILabel _timeLabel;
        private readonly UILabel _nameLabel;
        private readonly UILabel _hostLabel;

        #endregion

        #region Constructors

        public PanelCalendarMeetingListItem(UISubPageReferenceList list, uint index)
            : base(list, index)
        {
            _nameLabel = new UILabel(list.SmartObject, StringInputSigs[1].Name);
            _hostLabel = new UILabel(list.SmartObject, StringInputSigs[2].Name);
            _timeLabel = new UILabel(list.SmartObject, StringInputSigs[3].Name);
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

                var meeting = _linkedObject as CalendarMeeting;

                if (meeting == null)
                {
                    _hostLabel.Clear();
                    _timeLabel.Clear();
                    return;
                }

                _nameLabel.SetText(meeting.Name);
                _hostLabel.SetText(meeting.Organizer);
                _timeLabel.SetText(string.Format("{0:h:mm} {0:tt} - {1:h:mm} {1:tt}", meeting.StartTime, meeting.EndTime));

                BoolInputSigs[2].BoolValue = meeting.CanDial;
            }
        }

        #endregion

        #region Methods



        #endregion
    }
}