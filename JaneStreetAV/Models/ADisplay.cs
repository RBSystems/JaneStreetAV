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
using JaneStreetAV.Devices;
using JaneStreetAV.Models.Config;
using JaneStreetAV.UI;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.DeviceSupport;
using UX.Lib2.Models;

namespace JaneStreetAV.Models
{
    public class ADisplay : DisplayBase
    {
        #region Fields

        private readonly DisplayConfig _config;

        #endregion

        #region Constructors

        public ADisplay(SystemBase system, DisplayDeviceBase displayDevice, DisplayConfig config)
            : base(system, displayDevice)
        {
            _config = config;
            Name = _config.Name;
            Position = config.Position;
        }

        public ADisplay(RoomBase room, DisplayDeviceBase displayDevice, DisplayConfig config)
            : base(room, displayDevice)
        {
            _config = config;
            Name = _config.Name;
            Position = config.Position;
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public DisplayConfig Config
        {
            get { return _config; }
        }

        public DisplayPosition Position { get; private set; }

        #endregion

        #region Methods

        protected override void OnSourceChange(SourceBase source)
        {
            base.OnSourceChange(source);

            var switcher = ((ARoom)Room).SystemSwitcher;
            if (switcher == null) return;

            var s = source as ASource;
            if (s != null && s.SwitcherInputs.ContainsKey(2) && Position == DisplayPosition.Right)
            {
                switcher.RouteVideo(s.SwitcherInputs[2], _config.SwitcherOutputIndex);
            }
            else if (s != null && s.SwitcherInputs.ContainsKey(1))
            {
                switcher.RouteVideo(s.SwitcherInputs[1], _config.SwitcherOutputIndex);
            }
            else if (s == null)
            {
                switcher.RouteVideo(0, _config.SwitcherOutputIndex);
            }
        }

        #endregion
    }

    public enum DisplayPosition
    {
        Left,
        Center,
        Right
    }
}