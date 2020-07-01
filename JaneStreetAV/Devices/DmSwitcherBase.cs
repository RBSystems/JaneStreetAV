using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.DeviceSupport;

namespace JaneStreetAV.Devices
{
    public abstract class DmSwitcherBase : ISwitcher
    {
        protected string _connectedIp = "0.0.0.0";
        protected Dictionary<uint, bool> _outInit = new Dictionary<uint, bool>(); 

        public Switch Chassis { get; protected set; }

        public ControlSystem ControlSystem { get; protected set; }

        protected void OnDmInputChange(Switch device, DMInputEventArgs args)
        {
            //Debug.WriteInfo(GetType().Name, "OnDmInputChange - {0}, {1}", Tools.GetDmInputEventIdName(args.EventId), args.Number);
            if (args.EventId == DMInputEventIds.VideoDetectedEventId)
            {
#if DEBUG
                Debug.WriteSuccess("DM Input {0}, Video detected = {1}", args.Number,
                    device.Inputs[args.Number].VideoDetectedFeedback.BoolValue);
#endif
                if (InputStatusChanged != null)
                    InputStatusChanged(this, new SwitcherInputStatusChangeEventArgs(this, args.Number));
            }
        }

        protected void OnDmOutputChange(Switch device, DMOutputEventArgs args)
        {
            //Debug.WriteInfo(GetType().Name, "OnDmOutputChange - {0}, {1}", Tools.GetDmOutputEventIdName(args.EventId), args.Number);
            if (args.EventId == DMOutputEventIds.VideoOutEventId)
            {
#if DEBUG
                Debug.WriteSuccess("DM Output {0}, Video routed = {1}", args.Number,
                    device.Outputs[args.Number].VideoOutFeedback != null
                        ? device.Outputs[args.Number].VideoOutFeedback.Number
                        : 0);
#endif
                if (_outInit[args.Number]) return;

                _outInit[args.Number] = true;

                var input = device.Outputs[args.Number].VideoOutFeedback;

                if (input == null) return;

                device.Outputs[args.Number].VideoOut = input;
                device.VideoEnter.BoolValue = true;
                device.VideoEnter.BoolValue = false;
                Debug.WriteInfo(GetType().Name, "Set output {0} to {1}", args.Number, input.Number);
            }
        }

        protected void Chassis_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            Debug.WriteInfo(GetType().Name, "{0} Online = {1}", Chassis, args.DeviceOnLine);
            if (args.DeviceOnLine)
            {
                Chassis.VideoEnter.BoolValue = false;
                Chassis.USBEnter.BoolValue = false;
                Chassis.EnableUSBBreakaway.BoolValue = true;
                /*foreach (DMOutput output in Chassis.Outputs)
                {
                    Debug.WriteInfo(GetType().Name, "Dm on init, route {0} to {1}",
                        output.VideoOutFeedback != null ? output.VideoOutFeedback.Number : 0, output.Number);
                    output.VideoOut = output.VideoOutFeedback;
                }
                 * 
                 * 
                Chassis.VideoEnter.Pulse(1);
                Chassis.USBEnter.Pulse(1);*/

                var info = currentDevice.ConnectedIpList.FirstOrDefault();
                if (info != null)
                {
                    _connectedIp = info.DeviceIpAddress;
                }

                foreach (var input in Chassis.Inputs)
                {
                    if (InputStatusChanged != null)
                        InputStatusChanged(this, new SwitcherInputStatusChangeEventArgs(this, input.Number));
                }
            }

            try
            {
                if (DeviceCommunicatingChange != null)
                {
                    DeviceCommunicatingChange(this, DeviceCommunicating);
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        #region ISwitcher Members

        public void RouteVideo(uint input, uint output)
        {
            Debug.WriteWarn("*** DM.RouteVideo( " + input + " -> " + output + " ) ***");
            Chassis.Outputs[output].VideoOut = input > 0 ? Chassis.Inputs[input] : null;
            Chassis.VideoEnter.Pulse(1);
        }

        public void RouteAudio(uint input, uint output)
        {
            Debug.WriteWarn("*** DM.RouteAudio( " + input + " -> " + output + " ) ***");
            Chassis.Outputs[output].AudioOut = input > 0 ? Chassis.Inputs[input] : null;

            ((DmMDMnxn)Chassis).AudioEnter.Pulse(1);
        }

        public uint GetVideoInput(uint output)
        {
            if (Chassis.Outputs[output].VideoOutFeedback != null)
                return Chassis.Outputs[output].VideoOutFeedback.Number;
            return 0;
        }

        public uint GetAudioInput(uint output)
        {
            if (Chassis.Outputs[output].AudioOutFeedback != null)
                return Chassis.Outputs[output].AudioOutFeedback.Number;
            return 0;
        }

        public bool InputIsActive(uint input)
        {
            return Chassis.Inputs[input].VideoDetectedFeedback.BoolValue;
        }

        public event SwitcherInputStatusChangedEventHandler InputStatusChanged;

        public void Init()
        {

        }

        public bool SupportsDMEndPoints
        {
            get { return true; }
        }

        public IEnumerable<DMEndpointBase> GetEndpoints()
        {
            var result = new List<DMEndpointBase>();
            foreach (var io in Chassis.Inputs)
            {
                var endpoint = io.Endpoint as DMEndpointBase;
                if (endpoint == null) continue;
                result.Add(endpoint);
            }
            foreach (var io in Chassis.Outputs)
            {
                var endpoint = io.Endpoint as DMEndpointBase;
                if (endpoint == null) continue;
                result.Add(endpoint);
            }
            return result;
        }

        public EndpointReceiverBase GetEndpointForOutput(uint output)
        {
            try
            {
                var dmOutput = Chassis.Outputs[output];
                return dmOutput.Endpoint as EndpointReceiverBase;
            }
            catch
            {
                return null;
            }
        }

        public HdmiInputWithCEC GetHdmiCecInput(uint input)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void RouteUsb(DMInput input, DMInputOutputBase route)
        {
            input.USBRoutedTo = route;
            Chassis.USBEnter.Pulse(10);
        }

        public void RouteUsb(DMOutput output, DMInputOutputBase route)
        {
            output.USBRoutedTo = route;
            Chassis.USBEnter.Pulse(10);
        }

        public DMInputOutputBase GetUsbRoute(DMInput input)
        {
            return input.USBRoutedToFeedback;
        }

        public DMInputOutputBase GetUsbRoute(DMOutput output)
        {
            return output.USBRoutedToFeedback;
        }

        public string Name { get { return Chassis.Name; } }
        public string ManufacturerName { get { return "Crestron"; } }
        public string ModelName { get { return Chassis.Name; } }

        public string DiagnosticsName
        {
            get { return Chassis.ToString(); }
        }

        public bool DeviceCommunicating
        {
            get { return Chassis.IsOnline; }
        }

        public string DeviceAddressString
        {
            get { return _connectedIp; }
        }

        public string SerialNumber { get { return "Unknown"; } }
        public string VersionInfo { get { return "Unknown"; } }

        public event DeviceCommunicatingChangeHandler DeviceCommunicatingChange;

        public FusionAssetType AssetType { get { return FusionAssetType.VideoSwitcher; } }
    }

}