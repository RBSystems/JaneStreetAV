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

using JaneStreetAV.Models;
using JaneStreetAV.Models.Config;
using UX.Lib2.DeviceSupport;
using UX.Lib2.Models;

namespace JaneStreetAV.Devices.Sources
{
    public class GenericSource : ASource
    {
        #region Fields
        #endregion

        #region Constructors

        public GenericSource(ASystem system, SourceConfig config)
            : base(system, config, null)
        {
        }

        public GenericSource(ARoom room, SourceConfig config)
            : base(room, config, null)
        {
        }

        public GenericSource(DisplayBase display, SourceConfig config)
            : base(display, config, null)
        {
        }

        public GenericSource(ASystem system, SourceConfig config, ISourceDevice device)
            : base(system, config, device)
        {
        }

        public GenericSource(ARoom room, SourceConfig config, ISourceDevice device)
            : base(room, config, device)
        {
        }

        public GenericSource(DisplayBase display, SourceConfig config, ISourceDevice device)
            : base(display, config, device)
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
        #endregion
    }
}