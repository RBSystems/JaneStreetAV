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

using System.Collections.Generic;

namespace JaneStreetAV.Models.Config.Templates
{
    public abstract class TemplateBase : SystemConfig
    {
        #region Fields
        #endregion

        #region Constructors

        protected TemplateBase()
        {
            Enabled = true;
            Id = 1;
            Name = string.Empty;
            IsDefault = true;

            UserInterfaces = new List<UserInterface>();

            Rooms = new List<RoomConfig>()
            {
                new RoomConfig()
                {
                    Enabled = true,
                    Id = 1,
                    DspConfig = new DspRoomConfig()
                    {
                        Enabled = false,
                        Id = 1
                    },
                    Displays = new List<DisplayConfig>(),
                    Sources = new List<SourceConfig>(),
                    Fusion = new FusionConfig
                    {
                        Enabled = true,
                        DeviceAddressNumber = 0xF1
                    }
                }
            };

            GlobalSources = new List<SourceConfig>();

            ValueStore = new Dictionary<string, object>
            {
                {"FireAlarmInterfaceAddress", "30.92.1.157"}
            };
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