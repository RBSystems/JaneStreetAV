using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.EthernetCommunication;
using JaneStreetAV.Models.Config;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.DeviceSupport;
using UX.Lib2.DeviceSupport.Relays;
using UX.Lib2.Models;

namespace JaneStreetAV.Models.Rooms
{
    public class MultiPurpose : VCRoom
    {
        private bool _autoMode;

        private readonly Dictionary<uint, UpDownRelays> _blindRelays = new Dictionary<uint, UpDownRelays>();

        public MultiPurpose(ASystem system, RoomConfig config)
            : base(system, config)
        {
            var eic = ((AuditoriumSystem)system).BlindsProcessorEIC;
            _blindRelays[1] = new UpDownRelays(eic.BooleanInput[5], eic.BooleanInput[6],
                        UpDownRelayModeType.Momentary);
            _blindRelays[2] = new UpDownRelays(eic.BooleanInput[7], eic.BooleanInput[8],
                UpDownRelayModeType.Momentary);
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

        protected override void StandbyOnStateChange(CiscoTelePresenceCodec codec, StandbyState state)
        {

            switch (state)
            {
                case StandbyState.Off:
                    foreach (var display in Displays.Where(d => d.Device != null))
                    {
                        display.Device.Power = true;
                    }
                    break;
                case StandbyState.EnteringStandby:
                    break;
                case StandbyState.Halfwake:
                    break;
                case StandbyState.Standby:
                    foreach (var display in Displays.Where(d => d.Device != null))
                    {
                        display.Device.Power = false;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("state");
            }
        }
    }
}