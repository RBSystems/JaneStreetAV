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
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Crestron.SimplSharpPro.EthernetCommunication;
using Crestron.SimplSharpPro.UI;
using JaneStreetAV.Devices;
using JaneStreetAV.Devices.QSC;
using JaneStreetAV.Devices.Sources;
using JaneStreetAV.Models.Config;
using JaneStreetAV.SoftwareUpdate;
using JaneStreetAV.UI;
using JaneStreetAV.UI.Joins;
using JaneStreetAV.WebApp;
using JaneStreetAV.WebApp.Files;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Config;
using UX.Lib2.Devices.Audio.QSC;
using UX.Lib2.Devices.Audio.QSC.Components;
using UX.Lib2.Devices.Cameras.Sony;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.Devices.Extron;
using UX.Lib2.Devices.TV;
using UX.Lib2.DeviceSupport;
using UX.Lib2.Models;
using UX.Lib2.UI;

namespace JaneStreetAV.Models
{
    public abstract class ASystem : SystemBase
    {
        #region Fields

        private CTimer _heartBeatTimer;
        private bool _serviceMode;
        private bool _testScriptShouldRun;
        private readonly List<string> _bmsDiscoveredRoomIds = new List<string>();
        private ViscaOverIpSocket _viscaSocet;
        private readonly FireAlarmListener _fireAlarmListener;
        private readonly ReadOnlyDictionary<uint, OneBeyond> _oneBeyondControllers;

        #endregion

        #region Constructors

        protected ASystem(CrestronControlSystem controlSystem)
            : base(controlSystem, Assembly.GetExecutingAssembly())
        {
            Debug.WriteInfo("System.ctor()", "Started");

            BootStatus = "Loading System Type \"" + GetType().Name + "\"";
            BootProgressPercent = 5;

            SoftwareUpdate.SoftwareUpdate.UpdateShouldLoad += LoadUpdate;

            CrestronConsole.AddNewConsoleCommand(parameters =>
            {
                if (parameters.Trim() == "?")
                {
                    CrestronConsole.ConsoleCommandResponse(
                        "Start running a test script. Options here are as follows:\r\n" +
                        "  count  - set the number of times the script should loop and change sources\r\n" +
                        "  number - set the number to dial if the codec is selected\r\n" +
                        "for example: \"TestScriptStart count 10 number 1234");
                    return;
                }
                
                var args = new Dictionary<string, string>();

                try
                {
                    var matches = Regex.Matches(parameters, @"(?:(\w+)\s+(\w+))");

                    foreach (Match match in matches)
                    {
                        args[match.Groups[1].Value] = match.Groups[2].Value;
                    }
                }
                catch (Exception e)
                {
                    CrestronConsole.ConsoleCommandResponse("Error parsing args, {0}", e.Message);
                    return;
                }

                try
                {
                    TestScriptStart(args);
                }
                catch (Exception e)
                {
                    CrestronConsole.ConsoleCommandResponse("Error: {0}", e.Message);
                }
            }, "TestScriptStart", "Start a test script", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(parameters => TestScriptStop(), "TestScriptStop",
                "Stop running the test script", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(parameters =>
            {
                try
                {
                    var id = uint.Parse(parameters);
                    Rooms[1].Source = id == 0 ? null : Sources[id];
                    CrestronConsole.ConsoleCommandResponse("Selected source \"{0}\" in Room \"{1}\"", Sources[id],
                        Rooms[1].Name);
                }
                catch (Exception e)
                {
                    CrestronConsole.ConsoleCommandResponse("Error: {0}", e.Message);
                }
            }, "Source", "Select a source by ID for the first room", ConsoleAccessLevelEnum.AccessOperator);

            CrestronConsole.AddNewConsoleCommand(parameters =>
            {
                try
                {
                    foreach (var source in Sources)
                    {
                        CrestronConsole.ConsoleCommandResponse("Source {0}: {1}", source.Id, source.Name);
                    }
                }
                catch (Exception e)
                {
                    CrestronConsole.ConsoleCommandResponse("Error: {0}", e.Message);
                }
            }, "ListSources", "List sources by IDs", ConsoleAccessLevelEnum.AccessOperator);

            StartWebApp(9001);

            if (ConfigManager.Config.SystemType == SystemType.NotConfigured)
            {
                return;
            }

            var config = ConfigManager.Config;

            Debug.WriteInfo("System.ctor()", "Config loaded");

            #region System Switcher

            BootStatus = "Loading switcher configuration";
            BootProgressPercent = 6;

            if (config.SwitcherType != SystemSwitcherType.NotInstalled)
            {
                try
                {
                    switch (config.SwitcherType)
                    {
                        case SystemSwitcherType.BigDmFrame:
                            Switcher = new BigDmSwitcher(controlSystem as ControlSystem,
                                config.SwitcherConfig);
                            Switcher.InputStatusChanged += LocalSwitcherOnInputStatusChanged;
                            break;
                        case SystemSwitcherType.DmFrame:
                            Switcher = new DmSwitcher(controlSystem as ControlSystem,
                                config.SwitcherConfig);
                            Switcher.InputStatusChanged += LocalSwitcherOnInputStatusChanged;
                            break;
                    }
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e, "Error loading System Video Switcher");
                }
            }

            #endregion

            var controllers = config.OneBeyondAddresses.ToDictionary(address => address.Key,
                address => new OneBeyond(address.Value, address.Key));

            _oneBeyondControllers = new ReadOnlyDictionary<uint, OneBeyond>(controllers);

            #region DSP

            BootStatus = "Loading dsp configuration";
            BootProgressPercent = 7;

            if (config.DspConfig != null && config.DspConfig.Enabled)
            {
                Dsp = new QsysCore(new[] {config.DspConfig.DeviceAddressString}, config.DspConfig.Name,
                    config.DspConfig.Username, config.DspConfig.Password);
                Dsp.HasIntitialized += DspOnHasIntitialized;
            }

            #endregion

            #region Rooms

            BootStatus = "Loading Room Configs";
            BootProgressPercent = 10;

            foreach (var roomConfig in config.Rooms
                .Where(r => r.Enabled)
                .OrderBy(r => r.Id))
            {
                Debug.WriteInfo("System.ctor()", "Setting up {0} room \"{1}\"", config.SystemType, roomConfig.Name);
                try
                {
                    var room = (ARoom) Assembly.GetExecutingAssembly()
                        .GetType(roomConfig.RoomType)
                        .GetConstructor(new CType[] {GetType(), typeof (RoomConfig)})
                        .Invoke(new object[] {this, roomConfig});
                    WebApp.AddXpanelLink(string.Format("/dashboard/app/xpanel/xpanel?room={0}", room.Id), room.Name);
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e, "Failed to load room type \"{0}\"", roomConfig.RoomType);
                }
            }

            #endregion

            #region Global Sources

            if (config.GlobalSources != null)
            {
                foreach (var sourceConfig in config.GlobalSources
                    .Where(s => s.Enabled)
                    .OrderBy(s => s.Id))
                {
                    Debug.WriteInfo("System.ctor()", "Setting up global source \"{0}\", {1}", sourceConfig.Name,
                        sourceConfig.SourceType);

                    switch (sourceConfig.SourceType)
                    {
                        case SourceType.AppleTV:
                            var appleTV = new AppleTV(controlSystem.IROutputPorts[sourceConfig.DeviceAddressNumber]);
                            new GenericSource(this, sourceConfig, appleTV);
                            break;
                        case SourceType.PC:
                            new PCSource(this, sourceConfig);
                            break;
                        default:
                            new GenericSource(this, sourceConfig);
                            break;
                    }
                }
            }

            #endregion

            #region User Interfaces

            BootStatus = "Loading UI Controllers";
            BootProgressPercent = 15;
#if DEBUG
            Debug.WriteInfo("Setting up User Interfaces", "Count = {0}", config.UserInterfaces.Count(ui => ui.Enabled));
            Debug.WriteInfo(JToken.FromObject(config.UserInterfaces).ToString(Formatting.Indented));
#endif

            foreach (var uiConfig in config.UserInterfaces.Where(ui => ui.Enabled))
            {
                try
                {
                    Debug.WriteInfo("System.ctor()", "Setting up UIController \"{0}\", {1} 0x{2:X2}", uiConfig.Name,
                        uiConfig.DeviceType, uiConfig.DeviceAddressNumber);

                    if (uiConfig.UIControllerType == UIControllerType.RemoteMpc)
                    {
                        var isc = new ThreeSeriesTcpIpEthernetIntersystemCommunications(uiConfig.DeviceAddressNumber,
                            uiConfig.DeviceAddressString, controlSystem);
                        var mpcUi = new MpcUIController(this, isc, Rooms.ContainsKey(uiConfig.DefaultRoom)
                            ? Rooms[uiConfig.DefaultRoom]
                            : null);
                        mpcUi.Register();
                        continue;
                    }

                    var uiAssembly = Assembly.LoadFrom(@"Crestron.SimplSharpPro.UI.dll");
                    var type = uiAssembly.GetType("Crestron.SimplSharpPro.UI." + uiConfig.DeviceType);
                    BasicTriListWithSmartObject panel;
                    var ctor = type.GetConstructor(new CType[] {typeof (UInt32), typeof (ControlSystem)});
                    panel = (BasicTriListWithSmartObject)
                        ctor.Invoke(new object[] {uiConfig.DeviceAddressNumber, controlSystem});

                    var app = panel as CrestronApp;
                    if (app != null && !string.IsNullOrEmpty(uiConfig.DeviceAddressString))
                    {
                        app.ParameterProjectName.Value = uiConfig.DeviceAddressString;
                    }

                    if (panel != null)
                    {
                        panel.Description = uiConfig.Name;
                    }

                    UIController ui = null;

                    RoomBase defaultRoom;
                    switch (uiConfig.UIControllerType)
                    {
                        case UIControllerType.TechPanel:
                            defaultRoom = Rooms.ContainsKey(uiConfig.DefaultRoom)
                                ? Rooms[uiConfig.DefaultRoom]
                                : null;
                            ui = new RoomUIController(this, panel, defaultRoom, true);
                            break;
                        case UIControllerType.UserPanel:
                            defaultRoom = Rooms.ContainsKey(uiConfig.DefaultRoom)
                                ? Rooms[uiConfig.DefaultRoom]
                                : null;
                            ui = new RoomUIController(this, panel, defaultRoom, false);
                            break;
                    }

                    foreach (var room in uiConfig.AllowedRoomIds)
                    {
                        ui.AddRoomToAllowedRooms(Rooms[room]);
                    }

                    if (ui != null)
                        ui.Register();
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e, "Error loading UI device {0} 0x{1:X2}", uiConfig.DeviceType,
                        uiConfig.DeviceAddressNumber);
                }
            }
            
            UIControllers.SetupCustomTime(Serials.Time);
            UIControllers.SetupCustomDate(Serials.Date);

            #endregion

            if(this is FireAlarmMonitorSystem) return;

            var fireInterfaceAddress = ConfigManager.GetCustomValue("FireAlarmInterfaceAddress") as string;
            if (string.IsNullOrEmpty(fireInterfaceAddress)) return;
            _fireAlarmListener = new FireAlarmListener(fireInterfaceAddress);
            _fireAlarmListener.StatusChanged += FireAlarmListenerOnStatusChanged;
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public override AConfig Config
        {
            get { return ConfigManager.Config; }
        }

        public ISwitcher Switcher { get; private set; }

        public QsysCore Dsp { get; private set; }

        public List<string> BmsDiscoveredRoomIds
        {
            get { return _bmsDiscoveredRoomIds; }
        }

        public override string Name
        {
            get
            {
                if (ConfigManager.Config.SystemType == SystemType.NotConfigured)
                {
                    return Hostname;
                }
                return Rooms.Any() ? "Room " + Rooms.First().Name : Hostname;
            }
        }

        public abstract ReadOnlyDictionary<uint, ExtronSmp> Recorders { get; }

        public abstract ReadOnlyDictionary<uint, ICamera> Cameras { get; } 

        public override IEnumerable<IStatusMessageItem> StatusMessages
        {
            get
            {
                var items = new List<IStatusMessageItem>();

                foreach (var device in GetSystemDevices())
                {
                    if (device is UIController &&
                        (((UIController) device).IsXpanel || ((UIController) device).Device is CrestronApp))
                    {
                        items.Add(new StatusMessage(StatusMessageWarningLevel.Notice,
                            device.Name + " is " + (device.DeviceCommunicating ? "Online" : "Offline")));
                        continue;
                    }

                    if (device is CiscoTelePresenceCodec && device.DeviceCommunicating)
                    {
                        var vc = device as CiscoTelePresenceCodec;
                        items.AddRange(vc.Diagnostics.Cast<IStatusMessageItem>());
                    }

                    if (device is QsysIoFrame && device.DeviceCommunicating && ((QsysIoFrame)device).StatusCode != 0)
                    {
                        items.Add(new StatusMessage(StatusMessageWarningLevel.Warning,
                            ((QsysIoFrame) device).Status) {SourceDeviceName = ((QsysIoFrame) device).Name});
                    }

                    if (device.DeviceCommunicating)
                    {
                        items.Add(new StatusMessage(StatusMessageWarningLevel.Notice,
                            string.Format("{0}{1} is Online", device.Name,
                                string.IsNullOrEmpty(device.DeviceAddressString)
                                    ? ""
                                    : " (" + device.DeviceAddressString + ")")));
                        continue;
                    }

                    items.Add(new StatusMessage(StatusMessageWarningLevel.Error,
                        string.Format("{0}{1} is Offline", device.Name,
                            string.IsNullOrEmpty(device.DeviceAddressString)
                                ? ""
                                : " (" + device.DeviceAddressString + ")")));
                }

                if (ConfigManager.Config.SystemType == SystemType.NotConfigured)
                {
                    items.Add(new StatusMessage(StatusMessageWarningLevel.Error,
                        "System is not configured. Please check processor has correct IP address or create a config manually."));
                }

                var dm = Switcher as BigDmSwitcher;
                if (dm != null)
                {
                    var endpoints = dm.GetEndpoints();
                    foreach (var endpoint in endpoints)
                    {
                        var name = endpoint.Name;
                        var online = false;

                        var rxEndpoint = endpoint as EndpointReceiverBase;
                        if (rxEndpoint != null)
                        {
                            name = name + " on Output " + rxEndpoint.DMOutput.Number;
                            online = rxEndpoint.DMOutput.EndpointOnlineFeedback;
                        }
                        else
                        {
                            var txEndpoint = endpoint as EndpointTransmitterBase;
                            if (txEndpoint != null)
                            {
                                name = name + " on Input " + txEndpoint.DMInput.Number;
                                online = txEndpoint.DMInput.EndpointOnlineFeedback;

                                if (!online && SystemType == SystemType.Auditorium)
                                {
                                    switch (txEndpoint.DMInput.Number)
                                    {
                                        case 1:
                                        case 3:
                                            if (dm.Chassis.Inputs[txEndpoint.DMInput.Number + 1].EndpointOnlineFeedback)
                                            {
                                                continue;
                                            }
                                            break;
                                        case 2:
                                        case 4:
                                            if (dm.Chassis.Inputs[txEndpoint.DMInput.Number - 1].EndpointOnlineFeedback)
                                            {
                                                continue;
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(endpoint.Description))
                        {
                            name = name + " (" + endpoint.Description + ")";
                        }

                        if (endpoint.ConnectedIpList.Count > 0)
                        {
                            name = name + ", " + endpoint.ConnectedIpList.First();
                        }

                        items.Add(
                            new StatusMessage(
                                online ? StatusMessageWarningLevel.Notice : StatusMessageWarningLevel.Error,
                                online ? "{0} is online" : "{0} is offline", name));
                    }
                }

                return items.OrderByDescending(item => item.MessageLevel);
            }
        }

        public SystemType SystemType
        {
            get { return ConfigManager.Config.SystemType; }
        }

        public ViscaOverIpSocket ViscaSocket
        {
            get
            {
                if (_viscaSocet != null) return _viscaSocet;
                _viscaSocet = new ViscaOverIpSocket(EthernetAdapterType.EthernetUnknownAdapter);
                return _viscaSocet;
            }
        }

        public FireAlarmListener FireAlarmListener
        {
            get { return _fireAlarmListener; }
        }

        public abstract ReadOnlyDictionary<uint, CiscoTelePresenceCodec> Codecs { get; }

        public ReadOnlyDictionary<uint, OneBeyond> OneBeyondControllers
        {
            get { return _oneBeyondControllers; }
        }

        #endregion

        #region Methods

        public override IEnumerable<IDevice> GetSystemDevices()
        {
            var devices = Sources
                .Select(source => source.Device as IDevice)
                .Where(sourceDevice => sourceDevice != null).ToList();

            devices.AddRange(UIControllers.Cast<IDevice>());

            if (Dsp != null)
            {
                devices.Add(Dsp);
            }

            devices.AddRange(
                (from room in Rooms.Where(r => r is ARoom).Cast<ARoom>()
                    where room.IoFrameDevice != null
                    select room.IoFrameDevice).Cast<IDevice>());

            devices.AddRange(Displays.Where(d => d.Device != null)
                .Select(display => display.Device)
                .Cast<IDevice>());

            if (Codecs != null)
            {
                foreach (var codec in Codecs.Values.Where(codec => !devices.Contains(codec)))
                {
                    devices.Add(codec);
                }
            }

            if (Switcher != null)
            {
                devices.Add(Switcher);
            }

            return devices;
        }

        protected override void AppShouldRunUpgradeScripts()
        {
            try
            {
                if (ConfigManager.Config.SystemType == SystemType.NotConfigured) return;

                var ip = ConfigManager.GetCustomValue("FireAlarmInterfaceAddress") as string;

                if (ip == null)
                {
                    ConfigManager.SetCustomValue("FireAlarmInterfaceAddress", "30.92.1.157");
                }

                var config = ConfigManager.Config;

                if (config.DspFaderFilters == null)
                {
                    config.DspFaderFilters = new List<FaderItemFilter>
                    {
                        new FaderItemFilter {FilterButton = 1, FilterString = "Aud 1"},
                        new FaderItemFilter {FilterButton = 2, FilterString = "Aud 2"},
                        new FaderItemFilter {FilterButton = 3, FilterString = "Multi"},
                        new FaderItemFilter {FilterButton = 4, FilterString = "*"},
                    };
                }

                if (config.SystemType == SystemType.Auditorium && config.OneBeyondConfig == null)
                {
                    config.OneBeyondConfig = new OneBeyondConfig
                    {
                        ProcessorConfigs = new Dictionary<uint, OneBeyondProcessor>
                        {
                            {
                                1, new OneBeyondProcessor
                                {
                                    CameraConfigs = new List<OneBeyondProcessorConfig>
                                    {
                                        new OneBeyondProcessorConfig
                                        {
                                            CameraIds = new List<uint> {1, 4, 5},
                                            UsedWithConfigValue = 2,
                                        },
                                        new OneBeyondProcessorConfig
                                        {
                                            CameraIds = new List<uint> {1, 2, 4, 5, 6, 7},
                                            UsedWithConfigValue = 3,
                                        }
                                    }
                                }
                            },
                            {
                                2, new OneBeyondProcessor
                                {
                                    CameraConfigs = new List<OneBeyondProcessorConfig>
                                    {
                                        new OneBeyondProcessorConfig
                                        {
                                            CameraIds = new List<uint> {3, 6, 7},
                                            UsedWithConfigValue = 2,
                                        }
                                    }
                                }
                            }
                        },
                        Cameras = new List<OneBeyondCameraConfig>
                        {
                            new OneBeyondCameraConfig
                            {
                                Id = 1,
                                IpAddress = "30.92.1.176",
                                Name = "Audi 1 Tracking Centre"
                            },
                            new OneBeyondCameraConfig
                            {
                                Id = 2,
                                IpAddress = "30.92.1.175",
                                Name = "Audi 1 & 2 Tracking"
                            },
                            new OneBeyondCameraConfig
                            {
                                Id = 3,
                                IpAddress = "30.92.1.171",
                                Name = "Audi 2 Tracking Centre"
                            },
                            new OneBeyondCameraConfig
                            {
                                Id = 4,
                                IpAddress = "30.92.1.174",
                                Name = "Audi 1 Screen Left"
                            },
                            new OneBeyondCameraConfig
                            {
                                Id = 5,
                                IpAddress = "30.92.1.155",
                                Name = "Audi 1 Screen Right"
                            },
                            new OneBeyondCameraConfig
                            {
                                Id = 6,
                                IpAddress = "30.92.1.156",
                                Name = "Audi 2 Screen Left"
                            },
                            new OneBeyondCameraConfig
                            {
                                Id = 7,
                                IpAddress = "30.92.1.170",
                                Name = "Audi 2 Screen Right"
                            },
                        }
                    };
                }

                foreach (
                    var userInterface in
                        config.UserInterfaces.Where(
                            userInterface => userInterface.UIControllerType == UIControllerType.ControlRoom))
                {
                    userInterface.UIControllerType = UIControllerType.UserPanel;
                }

                ConfigManager.Save(config, ConfigManager.FilePath);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        public override string SystemTypeInfo
        {
            get { return ConfigManager.Config.SystemType.ToString(); }
        }

        protected override IEnumerable<InitializeProcess> GetSystemItemsToInitialize()
        {
            var items = new List<InitializeProcess>();

            if (Switcher != null)
            {
                items.Add(new InitializeProcess(Switcher.Init, "Initializing Central Matrix Switcher connections"));
            }

            if (Dsp != null)
            {
                items.Add(new InitializeProcess(InitializeDsp, "Connecting to audio DSP"));
            }

            items.AddRange(
                _oneBeyondControllers.Select(
                    oneBeyond =>
                        new InitializeProcess(oneBeyond.Value.Initialize,
                            "Initializing OneBeyond Controller " + oneBeyond.Key)));

            if (Codecs != null)
            {
                items.AddRange(Codecs.Select(codec =>
                    new InitializeProcess(codec.Value.Initialize, string.Format("Connecting to Codec {0}", codec.Key))));
            }

            if (FireAlarmListener != null)
            {
                items.Add(new InitializeProcess(FireAlarmListener.Start, "Starting fire alarm interface listener"));
            }

            items.Add(new InitializeProcess(SoftwareUpdate.SoftwareUpdate.ListenForUpdateServers,
                "Starting Software Update Listener"));

            return items;
        }

        protected virtual void InitializeDsp()
        {
            if (Dsp == null) return;
            var components = new List<string>();
            var config = ConfigManager.Config;
            foreach (var roomConfig in config.Rooms)
            {
                if (roomConfig.DspConfig == null) continue;
                var dspConfig = roomConfig.DspConfig;
                components.Add(dspConfig.PgmVolControlName);
                components.AddRange(dspConfig.OtherComponents);
            }
            if (ConfigManager.Config.DspFaderComponents != null)
            {
                foreach (var item in ConfigManager.Config.DspFaderComponents
                    .Where(item => !string.IsNullOrEmpty(item.ComponentName)))
                {
                    components.Add(item.ComponentName);
                    if (string.IsNullOrEmpty(item.TrafficLightBaseName)) continue;
                    components.Add(item.TrafficLightBaseName + ".signal");
                    components.Add(item.TrafficLightBaseName + ".good");
                    components.Add(item.TrafficLightBaseName + ".peak");
                }
            }
            components.Add("monitors.mixer");
            components.Add("room.mode");
            components.Add("fire.mute");
            Dsp.Initialize(components);
        }

        protected virtual void DspOnHasIntitialized(QsysCore core)
        {
            CloudLog.Notice(string.Format("Q-Sys Core: \"{0}\" Has Intitialized!", core.Name));
            foreach (var component in core)
            {
                foreach (var control in component)
                {
                    CloudLog.Info("Core Control: {0}", control.ToString());
                }
            }

            foreach (var itemConfig in ConfigManager.Config.DspFaderComponents.Where(c => !string.IsNullOrEmpty(c.TrafficLightBaseName)))
            {
                try
                {
                    core.GetChangeGroup("TrafficLights").Add(core[itemConfig.TrafficLightBaseName + ".signal"]);
                    core.GetChangeGroup("TrafficLights").Add(core[itemConfig.TrafficLightBaseName + ".good"]);
                    core.GetChangeGroup("TrafficLights").Add(core[itemConfig.TrafficLightBaseName + ".peak"]);
                }
                catch
                {
                    
                }
            }

            core.GetChangeGroup("TrafficLights").PollAuto(0.2);
        }

        protected override void OnWebAppStarted()
        {
            base.OnWebAppStarted();
            if (ConfigManager.Config.SystemType == SystemType.NotConfigured)
            {
                WebApp.AddUserPageLink(@"/dashboard/app/install", "Install Config", "flight_takeoff");
            }
            else
            {
                //WebApp.AddUserPageLink(@"/dashboard/app/xpanel/xpanel?room=1", "XPanel", "touch_app");
            }
            //WebApp.AddUserPageLink(@"/dashboard/app/softwareupdates", "Updates", "cloud_download");
            WebApp.AddRoute(@"/au/manifest", typeof(PanelUpdateManifestHandler));
            WebApp.AddRoute(@"/api/app/update/<method:\w+>", typeof(SoftwareUpdateRequestHandler));
            WebApp.AddRoute(@"/api/app/update", typeof(SoftwareUpdateRequestHandler));
            WebApp.AddRoute(@"/dashboard/app/xpanel/<page:xpanel>", typeof(DashboardXPanelHandler));
            WebApp.AddRoute(@"/dashboard/app/xpanel/<filepath:[\/\w\.\-\[\]\(\)\x20]+>", typeof(XPanelFileHandler));
            WebApp.AddRoute(@"/dashboard/app/<page:\w+>", typeof(AppDashboardHandler));
            WebApp.AddRoute(@"/dashboard/app/content/<page:\w+>", typeof(AppDashboardContentHandler));
            WebApp.AddRoute(@"/xpanel", typeof(XPanelRedirectHandler));
            WebApp.AddRoute(@"/static/xpanel/<filepath:[\/\w\.\-\[\]\(\)\x20]+>", typeof(XPanelFileHandler));
            WebApp.AddRoute(@"/static/app/<filepath:[\/\w\.\-\[\]\(\)\x20]+>", typeof(StaticFileHandler));
            WebApp.DashboardCustomScriptPath = @"/static/app/js/dashboard-app-scripts.js";
        }

        public override void SetConfig(JToken fromData)
        {
            ConfigManager.Save(fromData.ToObject<SystemConfig>(), ConfigManager.FilePath);
        }

        public override void LoadConfigTemplate(string id)
        {
            
        }

        public override void TestScriptProcess(IDictionary<string, string> args)
        {
            _testScriptShouldRun = true;
            var count = 0;
            var maxCount = 0;

            CloudLog.Notice("Test scripting process begin. Testing starts in 5 seconds");

            if (args.ContainsKey("count"))
            {
                try
                {
                    maxCount = int.Parse(args["count"]);
                }
                catch
                {
                    CloudLog.Warn("Test Script argument error, count is not a number. Script will run until cancelled!");
                }
            }

            Thread.Sleep(5000);

            var room = Rooms.First();
            var sources = room.Sources.ToArray();
            var sourceCount = sources.Length;
            var random = new Random();

            while (_testScriptShouldRun)
            {
                count++;
                CloudLog.Info("Test Script Loop {0}", count);

                while (true)
                {
                    var randomSourceIndex = random.Next(0, sourceCount);

                    SourceBase source = null;

                    if (randomSourceIndex == 0 && room.Source == null)
                    {
                        continue;
                    }
                    
                    if (randomSourceIndex > 0)
                    {
                        source = sources[randomSourceIndex - 1];
                        if (source == room.Source)
                        {
                            continue;
                        }
                    }

                    CloudLog.Info("Selecting random source: {0}", source != null ? source.ToString() : "None");

                    try
                    {
                        room.Source = source;
                    }
                    catch (Exception e)
                    {
                        CloudLog.Error("Error setting source in TestScriptProcess(), {0}", e.Message);
                    }

                    break;
                }

                CloudLog.Debug("Selected source {0}, waiting for 10 seconds",
                    room.Source != null ? room.Source.ToString() : "None");

                Thread.Sleep(10000);

                if (room.Source != null && room.Source.Device is CiscoTelePresenceCodec)
                {
                    var codec = room.Source.Device as CiscoTelePresenceCodec;

                    if (args.ContainsKey("dial") && args["dial"].Length > 0)
                    {
                        CloudLog.Debug("Source is Codec source, will attempt call to \"{0}\"", args["dial"]);

                        try
                        {
                            codec.Calls.DialNumber(args["number"],
                                (code, description, id) =>
                                    CloudLog.Debug("Dial result: {0} {1}, call id: {2}", code, description, id));
                        }
                        catch (Exception e)
                        {
                            CloudLog.Error("Could not dial call, {0}", e.Message);
                        }

                        CloudLog.Debug("Will wait for 60 seconds");

                        Thread.Sleep(60000);

                        CloudLog.Debug("Ending all calls");

                        codec.Calls.DisconnectAll();

                        Thread.Sleep(5000);
                    }
                }
                else
                {
                    Thread.Sleep(20000);
                }

                if (count != maxCount) continue;
                CloudLog.Notice("Test scripting count reached. Exiting loop.");
                return;
            }
        }

        public void TestScriptStop()
        {
            _testScriptShouldRun = false;
        }

        public override void TestScriptEnded()
        {
            _testScriptShouldRun = false;
        }

        private void LocalSwitcherOnInputStatusChanged(ISwitcher switcher, SwitcherInputStatusChangeEventArgs args)
        {
            foreach (var source in Sources.Cast<ASource>())
            {
                source.UpdateFromSwitcherVideoStatus(args.Number, args.HasVideo);
            }
        }

        private void FireAlarmListenerOnStatusChanged(FireAlarmListener listener, FireAlarmStatus status)
        {
            var mute = status == FireAlarmStatus.Alert;

            CloudLog.Warn("Fire Alarm Interface changed status to: {0}", status.ToString());

            try
            {
                PromptUsers(prompt => { }, "FIRE ALARM", "Please listen for announcements", 300, null,
                    new PromptAction
                    {
                        ActionName = "OK",
                        ActionType = PromptActionType.Acknowledge
                    });
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }

            if (Dsp != null && Dsp.ContainsComponentWithName("fire.mute"))
            {
                var muteGainBlock = Dsp["fire.mute"] as IAudioLevelControl;
                if (muteGainBlock != null && muteGainBlock.SupportsMute)
                {
                    CloudLog.Notice("Dsp mute {0} set to {1}", muteGainBlock.Name, mute ? "muted" : "unmuted");
                    muteGainBlock.Muted = mute;
                    return;
                }
            }

            foreach (var room in Rooms.Cast<ARoom>().Where(room => room.ProgramVolume != null))
            {
                CloudLog.Warn("Fire Alarm {0} program volume in Room {1}", mute ? "muted" : "unmuted", room.Name);
                room.ProgramVolume.Muted = mute;
            }
        }

        #endregion
    }

    public enum SystemType
    {
        NotConfigured,
        VcMeetingRoom,
        Auditorium,
        Classroom,
        RecArea,
        RoomMonitoring,
        FireAlarm
    }
}