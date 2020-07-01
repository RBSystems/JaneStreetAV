using System;
using JaneStreetAV.Models.Config;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.Devices.Cisco.Conference;
using UX.Lib2.Models;

namespace JaneStreetAV.Models.Rooms
{
    public abstract class VCRoom : ARoom
    {
        protected VCRoom(ASystem system, RoomConfig config)
            : base(system, config)
        {

        }

        protected override void SourceLoadProcess(SourceBase previousSource, SourceBase newSource)
        {
            if (newSource != null)
                CloudLog.Debug("Loading source \"{0}\" in room \"{1}\"", newSource, this);

            

            if (newSource != null)
            {
                if (SystemSwitcher != null && Config.SwitcherOutputCodecContent > 0 &&
                    ((ASource) newSource).SwitcherInputs.ContainsKey(1))
                {
                    SystemSwitcher.RouteVideo(((ASource)newSource).SwitcherInputs[1], Config.SwitcherOutputCodecContent);
                    SystemSwitcher.RouteAudio(((ASource)newSource).SwitcherInputs[1], Config.SwitcherOutputCodecContent);
                }
                Codec.Conference.Presentation.Start(SendingMode.LocalRemote);
            }
            else
            {
                if (SystemSwitcher != null && Config.SwitcherOutputCodecContent > 0)
                {
                    SystemSwitcher.RouteVideo(0, Config.SwitcherOutputCodecContent);
                    SystemSwitcher.RouteAudio(0, Config.SwitcherOutputCodecContent);
                }
                Codec.Conference.Presentation.Stop();
            }

            try
            {
                FusionShouldUpdateCoreParameters();
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        public CiscoTelePresenceCodec Codec
        {
            get { return ((ASystem)System).Codecs[Id]; }
        }

        public override void Initialize()
        {
            base.Initialize();

            try
            {
                Codec.Standby.StateChange += StandbyOnStateChange;
            }
            catch (Exception e)
            {
                CloudLog.Error("Error initializing codec in {0}, {1}", GetType().Name, e.Message);
            }
        }

        protected abstract void StandbyOnStateChange(CiscoTelePresenceCodec codec, StandbyState state);
    }
}