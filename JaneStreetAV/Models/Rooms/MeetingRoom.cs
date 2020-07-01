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

using System.Linq;
using JaneStreetAV.Models.Config;
using UX.Lib2;
using UX.Lib2.DeviceSupport;
using UX.Lib2.Models;

namespace JaneStreetAV.Models.Rooms
{
    public abstract class MeetingRoom : ARoom
    {
        #region Constructors

        protected MeetingRoom(ASystem system, RoomConfig config)
            : base(system, config)
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

        protected ASource GetSourceWithInputNumber(uint inputNumber)
        {
            return
                Sources.Cast<ASource>()
                    .Where(source => source != null)
                    .FirstOrDefault(
                        source => source.SwitcherInputs.ContainsKey(1) && source.SwitcherInputs[1] == inputNumber);
        }

        protected override void SwitcherOnInputStatusChanged(ISwitcher switcher, SwitcherInputStatusChangeEventArgs args)
        {
            var source = GetSourceWithInputNumber(args.Number);
#if DEBUG
            Debug.WriteInfo("Switcher Input Status", "Input {0}, HasVideo = {1}", args.Number, args.HasVideo);
#endif
            if (source == null || !AutoSourceSelectionEnabled) return;

            if (Source == source && !args.HasVideo && source.Type == SourceType.Laptop &&
                Sources.OfSourceType(SourceType.AirMedia).Any())
            {
#if DEBUG
                Debug.WriteWarn("Current source is laptop... switching back to AirMedia");
#endif
                Source = Sources[SourceType.AirMedia];
            }

            else if (Source == null && args.HasVideo && source.Type == SourceType.Laptop)
            {
#if DEBUG
                Debug.WriteSuccess("System is off, switching to laptop");
#endif
                Source = source;
            }
        }

        #endregion
    }
}