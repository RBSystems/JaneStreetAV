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
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;
using JaneStreetAV.Models.Config;
using JaneStreetAV.UI;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.DeviceSupport;
using UX.Lib2.Models;

namespace JaneStreetAV.Models
{
    public abstract class ASource : SourceBase
    {
        #region Fields

        private readonly Dictionary<uint, uint> _switcherInputs = new Dictionary<uint, uint>();
        private readonly SourceConfig _config;
        private bool _videoInputActive;

        #endregion

        #region Constructors

        protected ASource(ASystem system, SourceConfig config, ISourceDevice device)
            : base(system, config.SourceType, device)
        {
            _config = config;
            Name = config.Name;
            IconName = config.Icon.ToString();
            DisplayDeviceInput = config.DisplayInput;
            if (config.SwitcherInputIndex > 0)
            {
                _switcherInputs[1] = config.SwitcherInputIndex;
            }
            if (config.SwitcherInputIndexSecondary > 0)
            {
                _switcherInputs[2] = config.SwitcherInputIndexSecondary;
            }
            DmEndpointSourceInput = config.DmEndpointSourceSelect;
            AvailabilityType = config.AvailabilityType;
            DmpsAudioInput = config.DmpsAudioInput;
        }

        protected ASource(ARoom room, SourceConfig config, ISourceDevice device)
            : base(room, config.SourceType, device)
        {
            _config = config;
            Name = config.Name;
            IconName = config.Icon.ToString();
            DisplayDeviceInput = config.DisplayInput;
            if (config.SwitcherInputIndex > 0)
            {
                _switcherInputs[1] = config.SwitcherInputIndex;
            }
            if (config.SwitcherInputIndexSecondary > 0)
            {
                _switcherInputs[2] = config.SwitcherInputIndexSecondary;
            }
            DmEndpointSourceInput = config.DmEndpointSourceSelect;
            AvailabilityType = config.AvailabilityType;
            DmpsAudioInput = config.DmpsAudioInput;
        }

        protected ASource(DisplayBase display, SourceConfig config, ISourceDevice device)
            : base(display, config.DisplayInput, config.SourceType, device)
        {
            _config = config;
            Name = config.Name;
            IconName = config.Icon.ToString();
            AvailabilityType = config.AvailabilityType;
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        public event SourceVideoDetectedEventHandler VideoStatusChangeDetected;

        #endregion

        #region Delegates
        #endregion

        #region Properties

        public ReadOnlyDictionary<uint, uint> SwitcherInputs
        {
            get { return new ReadOnlyDictionary<uint, uint>(_switcherInputs); }
        }

        public uint DmEndpointSourceInput { get; private set; }

        public SourceConfig Config
        {
            get { return _config; }
        }

        public bool VideoInputActive
        {
            get { return _videoInputActive; }
        }

        public eDmps34KAudioOutSource DmpsAudioInput { get; private set; }

        public JSIcons Icon
        {
            get
            {
                try
                {
                    return (JSIcons) Enum.Parse(typeof (JSIcons), IconName, false);
                }
                catch
                {
                    return JSIcons.Laptop;
                }
            }
        }

        #endregion

        #region Methods

        public void UpdateFromSwitcherVideoStatus(uint input, bool videoActive)
        {
            if (!SwitcherInputs.ContainsKey(1) || SwitcherInputs[1] != input) return;

            if(_videoInputActive == videoActive) return;

            _videoInputActive = videoActive;

            CloudLog.Debug("Source: \"{0}\", Video Present on input {1} = {2}", Name, input, videoActive);

            OnVideoStatusChangeDetected(this, _videoInputActive);
        }

        protected virtual void OnVideoStatusChangeDetected(ASource source, bool videoActive)
        {
            var handler = VideoStatusChangeDetected;
            if (handler != null)
            {
                try
                {
                    handler(source, videoActive);
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e);
                }
            }
        }

        #endregion
    }

    public delegate void SourceVideoDetectedEventHandler(ASource source, bool videoActive);
}