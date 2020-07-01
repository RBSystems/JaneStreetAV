using JaneStreetAV.Models.Config;
using UX.Lib2.DeviceSupport;
using UX.Lib2.Models;

namespace JaneStreetAV.Models.Rooms
{
    public class MonitoredRoom : ARoom
    {
        public MonitoredRoom(ASystem system, RoomConfig config) : base(system, config)
        {
        }

        protected override void SourceLoadProcess(SourceBase previousSource, SourceBase newSource)
        {
            
        }

        protected override void SwitcherOnInputStatusChanged(ISwitcher switcher, SwitcherInputStatusChangeEventArgs args)
        {
            
        }

        public override void Blinds(uint channel, BlindsCommand command)
        {
            throw new System.NotImplementedException();
        }

        public override void RecallLightingScene(uint scene)
        {
            throw new System.NotImplementedException();
        }
    }
}