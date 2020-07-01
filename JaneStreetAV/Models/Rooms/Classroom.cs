using System;
using JaneStreetAV.Models.Config;
using SSMono.Threading.Tasks;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.DeviceSupport;
using UX.Lib2.Models;

namespace JaneStreetAV.Models.Rooms
{
    public class Classroom : VCRoom
    {
        private bool _autoMode;

        public Classroom(ASystem system, RoomConfig config)
            : base(system, config)
        {

        }

        protected override void SwitcherOnInputStatusChanged(ISwitcher switcher, SwitcherInputStatusChangeEventArgs args)
        {
            
        }

        public override void Blinds(uint channel, BlindsCommand command)
        {
            throw new NotImplementedException();
        }

        public override void RecallLightingScene(uint scene)
        {
            throw new NotImplementedException();
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

            Task task = null;

            switch (state)
            {
                case StandbyState.Off:
                    task = new Task(() =>
                    {
                        var vcSource = Sources[SourceType.VideoConference];
                        foreach (var display in Displays)
                        {
                            display.Source = vcSource;
                        }
                    });
                    break;
                case StandbyState.EnteringStandby:
                    break;
                case StandbyState.Halfwake:
                    break;
                case StandbyState.Standby:
                    task = new Task(() =>
                    {
                        foreach (var display in Displays)
                        {
                            display.Source = null;
                        }
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException("state");
            }

            if (task != null)
            {
                task.Start();
            }
        }
    }
}