using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.EthernetCommunication;
using JaneStreetAV.Models.Config;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.DeviceSupport;
using UX.Lib2.DeviceSupport.Relays;

namespace JaneStreetAV.Models.Rooms
{
    public class Auditorium : VCRoom
    {
        private bool _autoMode;

        private readonly Dictionary<uint, UpDownRelays> _blindRelays = new Dictionary<uint, UpDownRelays>();

        public Auditorium(ASystem system, RoomConfig config) : base(system, config)
        {
            if (((AuditoriumSystem) system).BlindsProcessorEIC == null)
            {
                ((AuditoriumSystem) system).BlindsProcessorEIC =
                    new ThreeSeriesTcpIpEthernetIntersystemCommunications(0x0a, "30.92.1.185", system.ControlSystem);
                ((AuditoriumSystem) system).BlindsProcessorEIC.Register();
            }
            var eic = ((AuditoriumSystem) system).BlindsProcessorEIC;
            switch (Id)
            {
                case 1:
                    _blindRelays[1] = new UpDownRelays(eic.BooleanInput[1], eic.BooleanInput[2],
                        UpDownRelayModeType.Momentary);
                    _blindRelays[2] = new UpDownRelays(eic.BooleanInput[3], eic.BooleanInput[4],
                        UpDownRelayModeType.Momentary);
                    break;
                case 2:
                    _blindRelays[1] = new UpDownRelays(system.ControlSystem.RelayPorts[1],
                        system.ControlSystem.RelayPorts[2], UpDownRelayModeType.Momentary);
                    _blindRelays[2] = new UpDownRelays(system.ControlSystem.RelayPorts[3],
                        system.ControlSystem.RelayPorts[4], UpDownRelayModeType.Momentary);
                    foreach (var relays in _blindRelays.Values)
                    {
                        relays.Register();
                    }
                    break;
            }
        }

        protected override void SwitcherOnInputStatusChanged(ISwitcher switcher, SwitcherInputStatusChangeEventArgs args)
        {
            
        }

        public override void Blinds(uint channel, BlindsCommand command)
        {
            if (!_blindRelays.ContainsKey(channel)) return;
            var relays = _blindRelays[channel];
            switch (command)
            {
                case BlindsCommand.Up:
                    relays.Up();
                    break;
                case BlindsCommand.Down:
                    relays.Down();
                    break;
                case BlindsCommand.Stop:
                    relays.StopUsingPulseBoth();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("command");
            }
        }

        public override void RecallLightingScene(uint scene)
        {
            //TODO Program Lights in Auditorium
        }

        public bool AutoMode
        {
            get { return _autoMode; }
            set
            {
                if(_autoMode == value) return;
                
                _autoMode = value;

                
            }
        }

        protected override void StandbyOnStateChange(CiscoTelePresenceCodec codec, StandbyState state)
        {
            if (!AutoMode) return;

            switch (state)
            {
                case StandbyState.Off:
                    break;
                case StandbyState.EnteringStandby:
                    break;
                case StandbyState.Halfwake:
                    break;
                case StandbyState.Standby:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("state");
            }
        }
    }
}