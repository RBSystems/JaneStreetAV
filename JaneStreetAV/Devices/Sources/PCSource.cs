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
using JaneStreetAV.Models;
using JaneStreetAV.Models.Config;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Models;

namespace JaneStreetAV.Devices.Sources
{
    public class PCSource : ASource
    {
        #region Fields

        private byte[] _usbExtenderMacAddress;

        #endregion

        #region Constructors

        public PCSource(ASystem system, SourceConfig config)
            : base(system, config, null)
        {
            TryParseMacAddressFromConfigString(config);
        }

        public PCSource(ARoom room, SourceConfig config)
            : base(room, config, null)
        {
            TryParseMacAddressFromConfigString(config);
        }

        public PCSource(DisplayBase display, SourceConfig config)
            : base(display, config, null)
        {
            TryParseMacAddressFromConfigString(config);
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public byte[] UsbExtenderMacAddress
        {
            get { return _usbExtenderMacAddress; }
        }

        #endregion

        #region Methods

        public void TryParseMacAddressFromConfigString(SourceConfig config)
        {
            if(string.IsNullOrEmpty(config.DeviceAddressString)) return;

            if(config.DeviceAddressString.Length != 6) return;

            try
            {
                _usbExtenderMacAddress = new byte[3];

                for (int i = 0; i < 6; i = i + 2)
                {
                    var byteString = config.DeviceAddressString.Substring(i, 2);

                    var b = Byte.Parse(byteString, NumberStyles.HexNumber);

                    _usbExtenderMacAddress[i/2] = b;
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        #endregion
    }
}