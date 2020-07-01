using JaneStreetAV.Models.Config;
using UX.Lib2.Models;

namespace JaneStreetAV.Models.Rooms
{
    public class RecRoom : MeetingRoom
    {
        public RecRoom(ASystem system, RoomConfig config)
            : base(system, config)
        {

        }

        public override bool AutoSourceSelectionEnabled
        {
            get { return false; }
        }

        public override void Blinds(uint channel, BlindsCommand command)
        {
            throw new System.NotImplementedException();
        }

        public override void RecallLightingScene(uint scene)
        {
            throw new System.NotImplementedException();
        }

        protected override void SourceLoadProcess(SourceBase previousSource, SourceBase newSource)
        {
            foreach (var display in Displays)
            {
                display.Source = newSource;
            }

            if(SystemSwitcher == null || Config.PgmAudioSwitcherOutput == 0) return;

            var source = newSource as ASource;

            if (source != null && source.SwitcherInputs.ContainsKey(1))
            {
                SystemSwitcher.RouteAudio(source.SwitcherInputs[1], Config.PgmAudioSwitcherOutput);
            }
            else
            {
                SystemSwitcher.RouteAudio(0, Config.PgmAudioSwitcherOutput);
            }
        }
    }
}