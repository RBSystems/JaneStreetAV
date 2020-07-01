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
using UX.Lib2.Devices.Polycom;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.SmartObjects
{
    public class DgeCalendarMeetingListItem : UISubPageReferenceListItem
    {
        #region Fields
        
        private object _linkedObject;
        private UILabel _timeLabel;
        private UILabel _infoLabel;

        #endregion

        #region Constructors

        public DgeCalendarMeetingListItem(UISubPageReferenceList list, uint index)
            : base(list, index)
        {
            _timeLabel = new UILabel(list.SmartObject, StringInputSigs[1].Name);
            _infoLabel = new UILabel(list.SmartObject, StringInputSigs[2].Name);
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
                    _infoLabel.Clear();
                    return;
                }

                if (meeting.StartTime.Date == DateTime.Today)
                {
                    _timeLabel.SetText(
                        "<FONT size=\"32\" face=\"Arial\" color=\"#ffffff\">{0:dddd} {0:HH:mm} - {1:HH:mm}</FONT>",
                        meeting.StartTime, meeting.EndTime);
                }
                else
                {
                    _timeLabel.SetText(
                        "<FONT size=\"32\" face=\"Arial\" color=\"#ffffff\">Today {0:HH:mm} - {1:HH:mm}</FONT>",
                        meeting.StartTime, meeting.EndTime);
                }
                _infoLabel.SetText("<FONT size=\"32\" face=\"Arial\" color=\"#ffffff\">" +
                                   "<FONT color=\"#666666\">|&nbsp;&nbsp;&nbsp;</FONT>{0}<FONT color=\"#666666\">" +
                                   "&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;</FONT>{1}</FONT>",
                    meeting.Organizer, meeting.Name);
            }
        }

        #endregion

        #region Methods



        #endregion
    }
}