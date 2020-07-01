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
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;
using JaneStreetAV.Devices.QSC;
using JaneStreetAV.Devices.Sources;
using JaneStreetAV.Models.Config;
using JaneStreetAV.UI.Joins;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Config;
using UX.Lib2.Devices.Audio.QSC;
using UX.Lib2.Devices.Audio.QSC.Components;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.Devices.Cisco.Cameras;
using UX.Lib2.Devices.Displays.Crestron;
using UX.Lib2.Devices.Displays.Samsung;
using UX.Lib2.Devices.Displays.Sony;
using UX.Lib2.Devices.Polycom;
using UX.Lib2.DeviceSupport;
using UX.Lib2.DeviceSupport.Relays;
using UX.Lib2.Models;
using UX.Lib2.UI;

namespace JaneStreetAV.Models
{
    public abstract class ARoom : RoomBase
    {
        #region Fields
        
        private readonly RoomConfig _config;
        private CTimer _fusionUpdateTimer;
        private QsysIoFrame _ioFrameDevice;
        private ASource _vcContentSource;

        #endregion

        #region Constructors

        protected ARoom(ASystem system, RoomConfig config)
            : base(system)
        {
            try
            {
                _config = config;
                Debug.WriteInfo("Loading Room Config", "Room {0} \"{1}\"", config.Id, config.Name);

                Name = string.IsNullOrEmpty(config.Name) ? string.Format("Room {0}", Id) : config.Name;

                DivisibleRoomType = config.DivisibleRoomType;

                SystemSwitcher = system.Switcher;

                if (system.Dsp != null)
                {
                    system.Dsp.HasIntitialized += DspOnHasIntitialized;
                }

                if (SystemSwitcher != null)
                {
                    SystemSwitcher.InputStatusChanged += SwitcherOnInputStatusChanged;
                }

                try
                {
                    if (config.Displays == null)
                    {
                        config.Displays = new List<DisplayConfig>();
                    }
                    foreach (var displayConfig in config.Displays
                        .Where(d => d.Enabled)
                        .OrderBy(d => d.Id))
                    {
                        Debug.WriteInfo("Loading Display Config", "{0} {1}", displayConfig.DisplayType,
                            displayConfig.DeviceAddressString);

                        switch (displayConfig.DisplayType)
                        {
                            case DisplayType.Samsung:
                                if (SystemSwitcher != null && displayConfig.DeviceConnectionType == DeviceConnectionType.Serial)
                                {
                                    var comports =
                                        SystemSwitcher.GetEndpointForOutput(displayConfig.SwitcherOutputIndex) as
                                            IComPorts;
                                    new ADisplay(this,
                                        new SamsungDisplay(displayConfig.Name, 1,
                                            new SamsungDisplayComPortHandler(comports.ComPorts[1])), displayConfig);
                                }
                                else
                                {
                                    new ADisplay(this,
                                        new SamsungDisplay(displayConfig.Name, 1,
                                            new SamsungDisplaySocket(displayConfig.DeviceAddressString)), displayConfig);
                                }
                                break;
                            case DisplayType.CrestronConnected:
                                var proj = new ADisplay(this,
                                    new CrestronConnectedDisplay(displayConfig.DeviceAddressNumber, system.ControlSystem,
                                        displayConfig.Name), displayConfig);
                                if (displayConfig.UsesRelaysForScreenControl && SystemSwitcher != null)
                                {
                                    var relayEndpoint =
                                        SystemSwitcher.GetEndpointForOutput(displayConfig.SwitcherOutputIndex) as
                                            IRelayPorts;
                                    if (relayEndpoint == null)
                                    {
                                        CloudLog.Error("Could not get relays for projector");
                                        break;
                                    }
                                    var relays = new UpDownRelays(relayEndpoint.RelayPorts[2],
                                        relayEndpoint.RelayPorts[1], UpDownRelayModeType.Momentary);
                                    relays.Register();
                                    proj.SetScreenController(relays);
                                }
                                break;
                            default:
                                new ADisplay(this, null, displayConfig);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    CloudLog.Error("Error setting up displays in Room {0}, {1}", Id, e.Message);
                }

                if (config.Sources != null)
                {
                    foreach (var sourceConfig in config.Sources
                        .Where(s => s.Enabled)
                        .OrderBy(s => s.Id))
                    {
                        try
                        {
                            ASource source;
                            switch (sourceConfig.SourceType)
                            {
                                case SourceType.PC:
                                    source = new PCSource(this, sourceConfig);
                                    break;
                                case SourceType.IPTV:
                                    source = new TVSource(this, sourceConfig);
                                    break;
                                default:
                                    source = new GenericSource(this, sourceConfig);
                                    break;
                            }

                            if (!String.IsNullOrEmpty(sourceConfig.GroupName))
                                source.AddToGroup(sourceConfig.GroupName);
                        }
                        catch (Exception e)
                        {
                            CloudLog.Error("Error setting up source \"{0}\" in Room {1}, {2}", sourceConfig.Name, Id,
                                e.Message);
                        }
                    }
                }

                foreach (var source in Sources.Cast<ASource>())
                {
                    if (source.Type == SourceType.Laptop)
                    {
                        source.VideoStatusChangeDetected += LaptopSourceOnVideoDetected;
                    }
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        public CalendarMeetingsUpdatedEventHandler MeetingsUpdated;

        #endregion

        #region Delegates
        #endregion

        #region Properties

        public bool HasVC
        {
            get { return Sources.OfSourceType(SourceType.VideoConference).Any(); }
        }

        public bool InCall
        {
            get
            {
                var result = false;
                return result;
            }
        }

        public IAudioLevelControl CurrentActivityVolume
        {
            get { return InCall ? ConferenceVolume : ProgramVolume; }
        }

        public IAudioLevelControl ProgramVolume
        {
            get
            {
                var dsp = ((ASystem) System).Dsp;
                if (dsp != null && Config.DspConfig != null && Config.DspConfig.Enabled)
                {
                    var componentName = Config.DspConfig.PgmVolControlName;
                    if (dsp.ContainsComponentWithName(componentName))
                    {
#if DEBUG
                        Debug.WriteInfo("Room.ProgramVolume", "Room {0}, DSP Level: {1}", Id, dsp[componentName].Name);
#endif
                        return dsp[componentName] as IAudioLevelControl;
                    }
                }

                if (!Displays.Any(d => d.Device is IAudioLevelDevice))
                {
#if DEBUG
                    Debug.WriteError("Room.ProgramVolume", "Room {0}, no displays as audio device!", Id);
#endif
                    return null;
                }

                var display = Displays.First(d => d.Device is IAudioLevelDevice).Device as IAudioLevelDevice;
                if (display == null || !display.AudioLevels.Any())
                {
#if DEBUG
                    Debug.WriteError("Room.ProgramVolume", "Room {0}, no displays with audio levels!", Id);
#endif
                    return null;
                }
#if DEBUG
                Debug.WriteInfo("Room.ProgramVolume", "Room {0}, Display Level: {1}", Id, display.ToString());
#endif
                return display.AudioLevels.First();
            }
        }

        public virtual ushort DefaultProgramVolumeLevel
        {
            get { return ushort.MaxValue/2; }
        }

        public virtual ushort DefaultConferenceVolumeLevel
        {
            get { return ushort.MaxValue/2; }
        }

        public IAudioLevelControl ConferenceVolume
        {
            get
            {
                

                return ProgramVolume;
            }
        }

        public IAudioLevelControl MicMute
        {
            get
            {
                return null;
            }
        }

        public SnapshotController AudioCombinerSnapshots
        {
            get
            {
                
                return null;
            }
        }

        public RoomConfig Config
        {
            get { return _config; }
        }

        public ISwitcher SystemSwitcher { get; protected set; }

        public QsysIoFrame IoFrameDevice
        {
            get { return _ioFrameDevice; }
        }

        public virtual bool AutoSourceSelectionEnabled
        {
            get { return !(System is AuditoriumSystem); }
        }

        public virtual bool ShouldStartOnTouchToStart
        {
            get { return AutoSourceSelectionEnabled; }
        }

        public bool HasBlindControls
        {
            get { return false; }
        }

        public bool HasLightingControls
        {
            get { return false; }
        }

        public bool HasHvacControls
        {
            get { return false; }
        }

        public bool HasRoomControls
        {
            get { return HasBlindControls || HasHvacControls || HasLightingControls; }
        }

        #endregion

        #region Methods

        protected abstract void SwitcherOnInputStatusChanged(ISwitcher switcher, SwitcherInputStatusChangeEventArgs args);

        protected override void SourceLoadStarted(SourceBase newSource)
        {
            
        }

        protected override void SourceLoadEnded()
        {
            
        }

        public virtual void RouteCodecPresentationSource(ASource value, bool isAuto)
        {
            
        }

        protected override void OnDivisibleStateChanged(RoomBase room, DivisibleStateChangeType changeType)
        {
            switch (changeType)
            {
                case DivisibleStateChangeType.Connected:
                    foreach (var uiController in room.UIControllers)
                    {
                        uiController.Room = this;
                    }

                    foreach (var display in room.Displays)
                    {
                        display.Source = Source;
                    }
                    break;
                case DivisibleStateChangeType.Disconnected:
                    foreach (var uiController in room.DefaultUIControllers)
                    {
                        uiController.ConnectToDefaultRoom();
                    }

                    foreach (var display in room.Displays)
                    {
                        display.Source = room.Source;
                    }
                    break;
            }
        }

        public override void PowerOff(bool askToConfirm, PowerOfFEventType eventType)
        {
            PowerOff(askToConfirm, eventType, Digitals.SubPageActionSheetPowerOff);
        }

        protected override void PowerOff(PowerOfFEventType eventType)
        {
            Source = null;
            foreach (var display in Displays.Where(display => display.Device != null))
            {
#if DEBUG
                CloudLog.Notice("{0} Power Off!", display.Device.Name);
#endif
                display.Device.Power = false;
            }
        }

        public override IEnumerable<IDevice> GetRoomDevices()
        {
            var systemDevices = System.GetSystemDevices().ToArray();

            var results =
                systemDevices.OfType<UIController>()
                    .Select(s => s)
                    .Where(s => s.DefaultRoom == this)
                    .Cast<IDevice>()
                    .ToList();

            results.AddRange(systemDevices.Where(systemDevice => !(systemDevice is UIController)));

            return results;
        }

        public IEnumerable<IPowerDevice> GetPowerDevices()
        {
            var results = new List<IPowerDevice>();
            results.AddRange(Displays.Where(d => d.Device != null)
                .Select(display => display.Device)
                .Cast<IPowerDevice>());
            return results;
        } 

        protected override uint FusionShouldRegister()
        {
            if (Config.Fusion != null && Config.Fusion.Enabled)
            {
                return Config.Fusion.DeviceAddressNumber;
            }

            return 0;
        }

        protected override void FusionShouldRegisterUserSigs()
        {
            Fusion.AddSig(eSigType.Bool, 1, "Room Booked", eSigIoMask.InputSigOnly);
            Fusion.AddSig(eSigType.Bool, 2, "In Call", eSigIoMask.InputSigOnly);
            Fusion.AddSig(eSigType.Bool, 3, "Volume Mute", eSigIoMask.InputSigOnly);
            Fusion.AddSig(eSigType.Bool, 4, "Mic Mute", eSigIoMask.InputSigOnly);

            Fusion.AddSig(eSigType.String, 1, "Current Source", eSigIoMask.InputSigOnly);
            Fusion.AddSig(eSigType.String, 2, "System ID", eSigIoMask.InputSigOnly);
            Fusion.AddSig(eSigType.String, 3, "Room Type", eSigIoMask.InputSigOnly);
            Fusion.AddSig(eSigType.String, 7, "Program Version", eSigIoMask.InputSigOnly);

            Fusion.AddSig(eSigType.UShort, 1, "Volume", eSigIoMask.InputSigOnly);

            _fusionUpdateTimer = new CTimer(FusionUpdatePollTimer, null, 60000, 10000);
        }

        private void FusionUpdatePollTimer(object userSpecific)
        {
            try
            {
                FusionShouldUpdateCoreParameters();
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        protected override void FusionShouldUpdateCoreParameters()
        {
            base.FusionShouldUpdateCoreParameters();

            if(Fusion == null || !Fusion.IsOnline) return;

            Fusion.UserDefinedBooleanSigDetails[2].InputSig.BoolValue = InCall;
            Fusion.UserDefinedStringSigDetails[1].InputSig.StringValue = Source != null ? Source.Name : "-";
            Fusion.UserDefinedStringSigDetails[2].InputSig.StringValue = ConfigManager.Config.SystemId;
            Fusion.UserDefinedStringSigDetails[3].InputSig.StringValue =
                ConfigManager.Config.SystemType.ToString().SplitCamelCase();

            if (Source != null && Source.Type == SourceType.VideoConference && ConferenceVolume != null)
            {
                Fusion.UserDefinedBooleanSigDetails[3].InputSig.BoolValue = ConferenceVolume.Muted;
                Fusion.UserDefinedUShortSigDetails[1].InputSig.UShortValue = ConferenceVolume.Level;
            }
            else if (ProgramVolume != null)
            {
                Fusion.UserDefinedBooleanSigDetails[3].InputSig.BoolValue = ProgramVolume.Muted;
                Fusion.UserDefinedUShortSigDetails[1].InputSig.UShortValue = ProgramVolume.Level;
            }

            if (MicMute != null)
            {
                Fusion.UserDefinedBooleanSigDetails[4].InputSig.BoolValue = MicMute.Muted;
            }
        }

        protected override void FusionShouldRegisterAssets()
        {
            foreach (var uiController in System.UIControllers
                .ForDefaultRoom(this)
                .Where(ui => !(ui.Device is XpanelForSmartGraphics)))
            {
                FusionAddAsset(uiController);
            }

            foreach (var display in Displays.Where(d => d.Device != null))
            {
                FusionAddAsset(display.Device);
            }

            foreach (var source in Sources.Where(s => s.Device != null))
            {
                var device = source.Device as IFusionAsset;

                if (device != null)
                {
                    FusionAddAsset(device);
                }

                var codec = device as CiscoTelePresenceCodec;

                if (codec != null)
                {
                    foreach (var camera in codec.Cameras.ConnectedCameras)
                    {
                        FusionAddAsset(camera);
                    }
                }
            }

            if (((ASystem)System).Dsp != null)
            {
                FusionAddAsset(((ASystem)System).Dsp);
            }

            if (((ASystem)System).Switcher != null)
            {
                FusionAddAsset(((ASystem)System).Switcher);
            }

            if (_ioFrameDevice != null)
            {
                FusionAddAsset(_ioFrameDevice);
            }
        }

        protected override void OnFusionOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            base.OnFusionOnlineStatusChange(currentDevice, args);

            if(args.DeviceOnLine == false) return;

            Fusion.UserDefinedStringSigDetails[7].InputSig.StringValue =
                Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        protected override void FusionRequestedPowerOn()
        {
            try
            {
                Source = Sources
                    .OrderByDescending(s => s.Type == SourceType.VideoConference)
                    .ThenByDescending(s => s.Type == SourceType.AirMedia)
                    .First();
            }
            catch (Exception e)
            {
                CloudLog.Error("{0}.FusionRequestedPowerOn(), {1}", GetType().Name, e.Message);
            }
        }

        public virtual void RouteVideoForCamera(Camera camera)
        {
            camera.UseAsMainVideoSource();
        }

        private void DspOnHasIntitialized(QsysCore core)
        {
            
        }

        protected virtual void LaptopSourceOnVideoDetected(ASource source, bool videoActive)
        {
            if(!System.Booted) return;

            var vcContentSource =
                Sources.Cast<ASource>()
                    .Where(s => s.Type != SourceType.VideoConference)
                    .OrderByDescending(s => s.VideoInputActive)
                    .ThenByDescending(s => s.Type == SourceType.Laptop)
                    .FirstOrDefault();

            RouteCodecPresentationSource(vcContentSource, true);

            if (!AutoSourceSelectionEnabled) return;

            SetupDisplays();

            if (Source != source && videoActive && !InCall)
            {
                Source = source;
            }
            else if(!InCall)
            {
                SelectDefaultSource();
            }
        }

        public override void Start()
        {
            SetupDisplays();
            SelectDefaultSource();
        }

        public virtual void SetupDisplays()
        {
            foreach (var display in Displays.Where(d => d.Device != null))
            {
                display.Device.Power = true;
                display.Device.SetInput(
                    Sources.Cast<ASource>()
                        .First(s => s.DisplayDeviceInput != DisplayDeviceInput.Unknown)
                        .DisplayDeviceInput);
            }
        }

        public virtual void SelectDefaultSource()
        {
            Source =
                Sources.Cast<ASource>()
                    .Where(s => s.Type != SourceType.VideoConference)
                    .OrderByDescending(s => s.VideoInputActive)
                    .ThenByDescending(s => s.Type == SourceType.Laptop)
                    .FirstOrDefault();
        }

        private void MeetingsOnMeetingsUpdated(IEnumerable<CalendarMeeting> meetings, CalendarMeeting currentMeeting, CalendarMeeting nextMeeting)
        {
            if (MeetingsUpdated != null)
            {
                try
                {
                    MeetingsUpdated(meetings, currentMeeting, nextMeeting);
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e);
                }
            }
        }

        public abstract void Blinds(uint channel, BlindsCommand command);

        public abstract void RecallLightingScene(uint scene);

        #endregion
    }

    public enum BlindsCommand
    {
        None,
        Up,
        Down,
        Stop
    }
}