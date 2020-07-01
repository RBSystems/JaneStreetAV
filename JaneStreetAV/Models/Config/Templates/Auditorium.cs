using System.Collections.Generic;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Blades;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Crestron.SimplSharpPro.UI;
using UX.Lib2.Config;

namespace JaneStreetAV.Models.Config.Templates
{
    public class Auditorium : TemplateBase
    {
        public Auditorium()
        {
            SystemType = SystemType.Auditorium;

            UserInterfaces = new List<UserInterface>
            {
                new UserInterface
                {
                    Enabled = true,
                    Id = 1,
                    DeviceAddressNumber = 0x04,
                    DeviceAddressString = "0x04",
                    DeviceType = typeof (DmDge200C).Name,
                    Name = string.Format("Lectern 1 Panel"),
                    UIControllerType = UIControllerType.UserPanel,
                    DefaultRoom = 1
                },
                new UserInterface
                {
                    Enabled = true,
                    Id = 2,
                    DeviceAddressNumber = 0x05,
                    DeviceAddressString = "0x05",
                    DeviceType = typeof (DmDge200C).Name,
                    Name = string.Format("Lectern 2 Panel"),
                    UIControllerType = UIControllerType.UserPanel,
                    DefaultRoom = 2
                }
            };

            Rooms = new List<RoomConfig>()
            {
                new RoomConfig()
                {
                    Enabled = true,
                    Id = 1,
                    Name = "Auditorium 1",
                    RoomType = typeof (Rooms.Auditorium).FullName,
                    DspConfig = new DspRoomConfig()
                    {
                        Enabled = true,
                        Id = 1,
                        PgmVolControlName = "Aud1.pgmmix.level"
                    },
                    Displays = new List<DisplayConfig>
                    {
                        new DisplayConfig
                        {
                            Enabled = true,
                            Id = 1,
                            Name = "Video Wall",
                            DisplayType = DisplayType.Generic
                        }
                    },
                    Sources = new List<SourceConfig>
                    {

                    },
                    Fusion = new FusionConfig
                    {
                        Enabled = true,
                        DeviceAddressNumber = 0xF1
                    }
                },
                new RoomConfig()
                {
                    Enabled = true,
                    Id = 2,
                    Name = "Auditorium 2",
                    RoomType = typeof (Rooms.Auditorium).FullName,
                    DspConfig = new DspRoomConfig()
                    {
                        Enabled = true,
                        Id = 1,
                        PgmVolControlName = "Aud2.pgmmix.level"
                    },
                    Displays = new List<DisplayConfig>
                    {
                        new DisplayConfig
                        {
                            Enabled = true,
                            Id = 1,
                            Name = "Video Wall",
                            DisplayType = DisplayType.Generic
                        }
                    },
                    Sources = new List<SourceConfig>
                    {

                    },
                    Fusion = new FusionConfig
                    {
                        Enabled = true,
                        DeviceAddressNumber = 0xF2
                    }
                }
            };

            GlobalSources = new List<SourceConfig>();

            TriplePlayServerAddress = "30.93.1.126";

            SwitcherType = SystemSwitcherType.BigDmFrame;

            SwitcherConfig = new SwitcherConfig
            {
                Enabled = true,
                FrameType = typeof (DmMd64x64).FullName,
                Id = 0x80,
                Name = "DM Switcher Config",
                InputBlades = new List<SwitcherBladeConfig>(),
                OutputBlades = new List<SwitcherBladeConfig>(),
                Inputs = new List<SwitcherInputConfig>(),
                Outputs = new List<SwitcherOutputConfig>()
            };

            for(uint nBlade = 1; nBlade <= 3; nBlade ++)
            {
                SwitcherConfig.InputBlades.Add(new SwitcherBladeConfig
                {
                    Number = nBlade,
                    Type = typeof(Dmb4kIC).FullName
                });
            }

            for (uint nBlade = 4; nBlade <= 6; nBlade++)
            {
                SwitcherConfig.InputBlades.Add(new SwitcherBladeConfig
                {
                    Number = nBlade,
                    Type = typeof(Dmb4kIHd).FullName
                });
            }

            for (uint nBlade = 1; nBlade <= 4; nBlade++)
            {
                SwitcherConfig.OutputBlades.Add(new SwitcherBladeConfig
                {
                    Number = nBlade,
                    Type = typeof(Dmb4KOC).FullName
                });
            }

            for (uint nBlade = 5; nBlade <= 8; nBlade++)
            {
                SwitcherConfig.OutputBlades.Add(new SwitcherBladeConfig
                {
                    Number = nBlade,
                    Type = typeof(Dmb4KOHD).FullName
                });
            }

            for (var input = 1U; input <= 23; input ++)
            {
                var inputConfig = new SwitcherInputConfig
                {
                    Number = input,
                    Name = "Input " + input,
                    EndpointType = string.Empty
                };

                if (input <= 5 || input == 10)
                {
                    inputConfig.EndpointType = typeof (DmTx4k302C).FullName;
                }
                else
                {
                    inputConfig.EndpointType = typeof(DmTx4kz202C).FullName;
                }

                SwitcherConfig.Inputs.Add(inputConfig);
            }

            for (var input = 24U; input <= 36; input++)
            {
                var inputConfig = new SwitcherInputConfig
                {
                    Number = input,
                    Name = "Input " + input,
                    EndpointType = string.Empty
                };

                SwitcherConfig.Inputs.Add(inputConfig);
            }

            foreach (var inputConfig in SwitcherConfig.Inputs)
            {
                switch (inputConfig.Number)
                {
                    case 1: inputConfig.Name = "Aud 1 Lectern Left"; break;
                    case 2: inputConfig.Name = "Aud 1 Lectern Right"; break;
                    case 3: inputConfig.Name = "Aud 2 Lectern Left"; break;
                    case 4: inputConfig.Name = "Aud 2 Lectern Right"; break;
                    case 5: inputConfig.Name = "MPR Lectern"; break;
                    case 6: inputConfig.Name = "MPR Laptop Front"; break;
                    case 7: inputConfig.Name = "MPR Laptop Rear"; break;
                    case 8: inputConfig.Name = "MPR Cam Audience"; break;
                    case 9: inputConfig.Name = "MPR Cam Presenter"; break;
                    case 10: inputConfig.Name = "Control Room Laptop"; break;
                    case 11: inputConfig.Name = "Aud 1 Windows PC"; break;
                    case 12: inputConfig.Name = "Aud 1 Linux PC"; break;
                    case 13: inputConfig.Name = "Aud 2 Windows PC"; break;
                    case 14: inputConfig.Name = "Aud 2 Linux PC"; break;
                    case 15: inputConfig.Name = ""; break;
                    case 16: inputConfig.Name = ""; break;
                    case 17: inputConfig.Name = "Aud 1 Cam Stage"; break;
                    case 18: inputConfig.Name = "Aud 1&2 Cam Stage"; break;
                    case 19: inputConfig.Name = "Aud 1 Cam Audience L"; break;
                    case 20: inputConfig.Name = "Aud 1 Cam Audience R"; break;
                    case 21: inputConfig.Name = "Aud 2 Cam Stage"; break;
                    case 22: inputConfig.Name = "Aud 2 Cam Audience L"; break;
                    case 23: inputConfig.Name = "Aud 2 Cam Audience R"; break;
                    case 24: inputConfig.Name = "Group Output"; break;
                    case 25: inputConfig.Name = "IPTV 1"; break;
                    case 26: inputConfig.Name = "IPTV 2"; break;
                    case 27: inputConfig.Name = "IPTV 3"; break;
                    case 28: inputConfig.Name = "Ante Space Signage 1"; break;
                    case 29: inputConfig.Name = "Ante Space Signage 2"; break;
                    case 30: inputConfig.Name = "Aud 1 Video Multi Mix"; break;
                    case 31: inputConfig.Name = "Aud 2 Video Multi Mix"; break;
                    case 32: inputConfig.Name = "MultiViewer"; break;
                    case 33: inputConfig.Name = "Aud 1 Codec Single"; break;
                    case 34: inputConfig.Name = "Aud 1 Codec Dual"; break;
                    case 35: inputConfig.Name = "Aud 2 Codec"; break;
                    case 36: inputConfig.Name = "MPR Codec"; break;
                }

                if (inputConfig.Name.StartsWith("Aud 1"))
                {
                    inputConfig.Color = "Blue";
                }
                else if (inputConfig.Name.StartsWith("Aud 2"))
                {
                    inputConfig.Color = "Orange";
                }
                else if (inputConfig.Name.StartsWith("MPR"))
                {
                    inputConfig.Color = "Green";
                }
            }

            for (var output = 1U; output <= 28; output++)
            {
                var outputConfig = new SwitcherOutputConfig
                {
                    Number = output,
                    Name = "Output " + output,
                    EndpointType = string.Empty
                };

                switch (output)
                {
                    case 1:
                    case 2:
                    case 5:
                    case 6:
                    case 13:
                    case 14:
                        outputConfig.EndpointType = typeof (DmRmc4kzScalerC).FullName;
                        break;
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                        break;
                    default:
                        if (output < 15)
                        {
                            continue;                            
                        }
                        break;
                }

                SwitcherConfig.Outputs.Add(outputConfig);
            }

            for (var output = 33U; output <= 64; output++)
            {
                var outputConfig = new SwitcherOutputConfig
                {
                    Number = output,
                    Name = "Output " + output,
                    EndpointType = string.Empty
                };

                SwitcherConfig.Outputs.Add(outputConfig);
            }

            foreach (var outputConfig in SwitcherConfig.Outputs)
            {
                switch (outputConfig.Number)
                {
                    case 1: outputConfig.Name = "Aud 1 Repeater Left"; break;
                    case 2: outputConfig.Name = "Aud 1 Repeater Right"; break;
                    case 3: outputConfig.Name = ""; break;
                    case 4: outputConfig.Name = ""; break;
                    case 5: outputConfig.Name = "Aud 2 Repeater Left"; break;
                    case 6: outputConfig.Name = "Aud 2 Repeater Right"; break;
                    case 7: outputConfig.Name = ""; break;
                    case 8: outputConfig.Name = ""; break;
                    case 9: outputConfig.Name = "Aud 1 Monitor Lectern Left"; break;
                    case 10: outputConfig.Name = "Aud 1 Monitor Lectern Right"; break;
                    case 11: outputConfig.Name = "Aud 2 Monitor Lectern Left"; break;
                    case 12: outputConfig.Name = "Aud 2 Monitor Lectern Right"; break;
                    case 13: outputConfig.Name = "MPR Projector"; break;
                    case 14: outputConfig.Name = "MPR Lectern"; break;
                    case 15: outputConfig.Name = "AV Control Monitor 1"; break;
                    case 16: outputConfig.Name = "AV Control Monitor 2"; break;
                    case 17: outputConfig.Name = "AV Control Monitor 3"; break;
                    case 18: outputConfig.Name = "AV Control Monitor 4"; break;
                    case 19: outputConfig.Name = "AV Control Monitor 5"; break;
                    case 20: outputConfig.Name = "Recorder 1 Source 1"; break;
                    case 21: outputConfig.Name = "Recorder 1 Source 2"; break;
                    case 22: outputConfig.Name = "Recorder 2 Source 1"; break;
                    case 23: outputConfig.Name = "Recorder 2 Source 2"; break;
                    case 24: outputConfig.Name = "Ante Space 1-L"; break;
                    case 25: outputConfig.Name = "Ante Space 1-R"; break;
                    case 26: outputConfig.Name = "Ante Space 2-L"; break;
                    case 27: outputConfig.Name = "Ante Space 2-R"; break;
                    case 28: outputConfig.Name = "Group Feed"; break;
                    case 29: outputConfig.Name = ""; break;
                    case 30: outputConfig.Name = ""; break;
                    case 31: outputConfig.Name = ""; break;
                    case 32: outputConfig.Name = ""; break;
                    case 33: outputConfig.Name = "Aud 1 Video Wall + Audio"; break;
                    case 34: outputConfig.Name = "Aud 2 Video Wall + Audio"; break;
                    case 35: outputConfig.Name = "MPR Audio"; break;
                    case 36: outputConfig.Name = "Ante Space 1 Audio"; break;
                    case 37: outputConfig.Name = "Ante Space 2 Audio"; break;
                    case 38: outputConfig.Name = "MPR Codec Cam 2"; break;
                    case 39: outputConfig.Name = "Video Multi Mix 1-5"; break;
                    case 40: outputConfig.Name = "Video Multi Mix 1-6"; break;
                    case 41: outputConfig.Name = "Video Multi Mix 1-7"; break;
                    case 42: outputConfig.Name = "Video Multi Mix 1-8"; break;
                    case 43: outputConfig.Name = "MultiView 1"; break;
                    case 44: outputConfig.Name = "MultiView 2"; break;
                    case 45: outputConfig.Name = "MultiView 3"; break;
                    case 46: outputConfig.Name = "MultiView 4"; break;
                    case 47: outputConfig.Name = "Aud 1 Codec Cam"; break;
                    case 48: outputConfig.Name = "Aud 1 Codec Content"; break;
                    case 49: outputConfig.Name = "Aud 2 Codec Cam"; break;
                    case 50: outputConfig.Name = "Aud 2 Codec Content"; break;
                    case 51: outputConfig.Name = "MPR Codec Cam"; break;
                    case 52: outputConfig.Name = "MPR Codec Content"; break;
                    case 53: outputConfig.Name = "Video Multi Mix 1-1"; break;
                    case 54: outputConfig.Name = "Video Multi Mix 1-2"; break;
                    case 55: outputConfig.Name = "Video Multi Mix 1-3"; break;
                    case 56: outputConfig.Name = "Video Multi Mix 1-4"; break;
                    case 57: outputConfig.Name = "Video Multi Mix 2-1"; break;
                    case 58: outputConfig.Name = "Video Multi Mix 2-2"; break;
                    case 59: outputConfig.Name = "Video Multi Mix 2-3"; break;
                    case 60: outputConfig.Name = "Video Multi Mix 2-4"; break;
                    case 61: outputConfig.Name = "Video Multi Mix 2-5"; break;
                    case 62: outputConfig.Name = "Video Multi Mix 2-6"; break;
                    case 63: outputConfig.Name = "Video Multi Mix 2-7"; break;
                    case 64: outputConfig.Name = "Video Multi Mix 2-8"; break;
                }

                if (outputConfig.Name.StartsWith("Aud 1"))
                {
                    outputConfig.Color = "Blue";
                }
                else if (outputConfig.Name.StartsWith("Aud 2"))
                {
                    outputConfig.Color = "Orange";
                }
                else if (outputConfig.Name.StartsWith("MPR"))
                {
                    outputConfig.Color = "Green";
                }
                else if (outputConfig.Name.StartsWith("Record"))
                {
                    outputConfig.Color = "Red";
                }
                else if (outputConfig.Name.StartsWith("Ante Space 1"))
                {
                    outputConfig.Color = "Purple";
                }
                else if (outputConfig.Name.StartsWith("Ante Space 2"))
                {
                    outputConfig.Color = "Yellow";
                }
            }

            DspConfig = new DspDeviceConfig
            {
                Enabled = true,
                DeviceAddressString = "30.92.1.164",
                DeviceConnectionType = DeviceConnectionType.Network,
                Username = null,
                Password = null,
                Name = "Q-Sys Core"
            };

            DspFaderColors = new List<FaderItemColor>
            {
                new FaderItemColor
                {
                    Name = "Orange",
                    ColorHex = "#ff8000",
                    Priority = 80,
                },
                new FaderItemColor
                {
                    Name = "Cyan",
                    ColorHex = "#0080FF",
                    Priority = 40,
                },
                new FaderItemColor
                {
                    Name = "Red",
                    ColorHex = "#ff0000",
                    Priority = 3,
                },
                new FaderItemColor
                {
                    Name = "Pink",
                    ColorHex = "#ff00ff",
                    Priority = 4,
                },
                new FaderItemColor
                {
                    Name = "Yellow",
                    ColorHex = "#ffff00",
                    Priority = 50,
                },
                new FaderItemColor
                {
                    Name = "Green",
                    ColorHex = "#00ff00",
                    Priority = 70,
                },
                new FaderItemColor
                {
                    Name = "Purple",
                    ColorHex = "#8000ff",
                    Priority = 60,
                },
                new FaderItemColor
                {
                    Name = "Blue",
                    ColorHex = "#0000ff",
                    Priority = 100,
                }
            };

            DspFaderComponents = new List<FaderItemConfig>
            {
                new FaderItemConfig
                {
                    Name = "Aud 1 PGM Mix",
                    ComponentName = "Aud1.pgmmix.level",
                    Color = "Blue",
                    Group = 3,
                    MixerIndex = 1
                },
                new FaderItemConfig
                {
                    Name = "Aiud 1 Mic Mix",
                    ComponentName = "Aud1.srmix.level",
                    Color = "Blue",
                    Group = 3,
                    MixerIndex = 2
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 Patch 1",
                    ComponentName = "Aud1.patchbay1.level",
                    Color = "Blue",
                    Group = 3,
                    MixerIndex = 3
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 Patch 2",
                    ComponentName = "Aud1.patchbay2.level",
                    Color = "Blue",
                    Group = 3,
                    MixerIndex = 4
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 PGM Mix",
                    ComponentName = "Aud2.pgmmix.level",
                    Color = "Orange",
                    Group = 3,
                    MixerIndex = 5
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 Mic Mix",
                    ComponentName = "Aud2.srmix.level",
                    Color = "Orange",
                    Group = 3,
                    MixerIndex = 6
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 Patch 1",
                    ComponentName = "Aud2.patchbay1.level",
                    Color = "Orange",
                    Group = 3,
                    MixerIndex = 7
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 Patch 2",
                    ComponentName = "Aud2.patchbay2.level",
                    Color = "Orange",
                    Group = 3,
                    MixerIndex = 8
                },
                new FaderItemConfig
                {
                    Name = "MPR PGM Mic",
                    ComponentName = "Multi.pgmmix.level",
                    Color = "Green",
                    Group = 3,
                    MixerIndex = 9
                },
                new FaderItemConfig
                {
                    Name = "MPR VC Mix",
                    ComponentName = "Aud2.vcmix.level",
                    Color = "Green",
                    Group = 3,
                    MixerIndex = 10
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 PGM",
                    ComponentName = "Aud1.pgm.gain",
                    Color = "Blue",
                    Group = 1,
                    MixerIndex = 11
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 VC",
                    ComponentName = "Aud1.vc.gain",
                    Color = "Blue",
                    Group = 1,
                    MixerIndex = 12
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 Patch 2301",
                    ComponentName = "Aud1.PatchBay.in1.gain",
                    Color = "Cyan",
                    Group = 1,
                    MixerIndex = 13
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 Patch 2302",
                    ComponentName = "Aud1.PatchBay.in2.gain",
                    Color = "Cyan",
                    Group = 1,
                    MixerIndex = 14
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 PGM",
                    ComponentName = "Aud2.pgm.gain",
                    Color = "Orange",
                    Group = 1,
                    MixerIndex = 15
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 VC",
                    ComponentName = "Aud2.vc.gain",
                    Color = "Orange",
                    Group = 1,
                    MixerIndex = 16
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 Patch 2301",
                    ComponentName = "Aud2.PatchBay.in1.gain",
                    Color = "Cyan",
                    Group = 1,
                    MixerIndex = 17
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 Patch 2302",
                    ComponentName = "Aud2.PatchBay.in2.gain",
                    Color = "Cyan",
                    Group = 1,
                    MixerIndex = 18
                },
                new FaderItemConfig
                {
                    Name = "MPR PGM",
                    ComponentName = "Multi.pgm.gain",
                    Color = "Green",
                    Group = 1,
                    MixerIndex = 19
                },
                new FaderItemConfig
                {
                    Name = "MPR VC",
                    ComponentName = "Multi.vc.gain",
                    Color = "Green",
                    Group = 1,
                    MixerIndex = 20
                },
                new FaderItemConfig
                {
                    Name = "Ante 01-202 PGM",
                    ComponentName = "01.202.pgm.gain",
                    Color = "Purple",
                    Group = 1,
                    MixerIndex = 21
                },
                new FaderItemConfig
                {
                    Name = "Ante 01-266 PGM",
                    ComponentName = "01.266.pgm.gain",
                    Color = "Yellow",
                    Group = 1,
                    MixerIndex = 22
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 Handheld",
                    ComponentName = "Aud1.hh.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 23,
                    TrafficLightBaseName = "Aud1.hh"
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 Lapel 1",
                    ComponentName = "Aud1.lapel1.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 24,
                    TrafficLightBaseName = "Aud1.lapel1"
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 Lapel 2",
                    ComponentName = "Aud1.lapel2.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 25,
                    TrafficLightBaseName = "Aud1.lapel2"
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 Lapel 3",
                    ComponentName = "Aud1.lapel3.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 26,
                    TrafficLightBaseName = "Aud1.lapel3"
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 Fbox Left 1",
                    ComponentName = "Aud1.fba1.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 27,
                    TrafficLightBaseName = "Aud1.fba1"
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 Fbox Left 2",
                    ComponentName = "Aud1.fba2.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 28,
                    TrafficLightBaseName = "Aud1.fba2"
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 Fbox Right 1",
                    ComponentName = "Aud1.fbb1.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 29,
                    TrafficLightBaseName = "Aud1.fbb1"
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 Fbox Right 2",
                    ComponentName = "Aud1.fbb2.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 30,
                    TrafficLightBaseName = "Aud1.fbb2"
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 Handheld",
                    ComponentName = "Aud2.hh.boost",
                    Color = "Orange",
                    Group = 2,
                    MixerIndex = 31,
                    TrafficLightBaseName = "Aud2.hh"
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 Lapel 1",
                    ComponentName = "Aud2.lapel1.boost",
                    Color = "Orange",
                    Group = 2,
                    MixerIndex = 32,
                    TrafficLightBaseName = "Aud2.lapel1"
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 Lapel 2",
                    ComponentName = "Aud2.lapel2.boost",
                    Color = "Orange",
                    Group = 2,
                    MixerIndex = 33,
                    TrafficLightBaseName = "Aud2.lapel2"
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 Lapel 3",
                    ComponentName = "Aud2.lapel3.boost",
                    Color = "Orange",
                    Group = 2,
                    MixerIndex = 34,
                    TrafficLightBaseName = "Aud2.lapel3"
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 Fbox Left 1",
                    ComponentName = "Aud2.fba1.boost",
                    Color = "Orange",
                    Group = 2,
                    MixerIndex = 35,
                    TrafficLightBaseName = "Aud2.fba1"
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 Fbox Left 2",
                    ComponentName = "Aud2.fba2.boost",
                    Color = "Orange",
                    Group = 2,
                    MixerIndex = 36,
                    TrafficLightBaseName = "Aud2.fba2"
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 Fbox Right 1",
                    ComponentName = "Aud2.fbb1.boost",
                    Color = "Orange",
                    Group = 2,
                    MixerIndex = 37,
                    TrafficLightBaseName = "Aud2.fbb1"
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 Fbox Right 2",
                    ComponentName = "Aud2.fbb2.boost",
                    Color = "Orange",
                    Group = 2,
                    MixerIndex = 38,
                    TrafficLightBaseName = "Aud2.fbb2"
                },
                new FaderItemConfig
                {
                    Name = "MPR Fbox 1",
                    ComponentName = "Multi.fba1.boost",
                    Color = "Green",
                    Group = 2,
                    MixerIndex = 39,
                    TrafficLightBaseName = "Multi.fba1"
                },
                new FaderItemConfig
                {
                    Name = "MPR Fbox 2",
                    ComponentName = "Multi.fba2.boost",
                    Color = "Green",
                    Group = 2,
                    MixerIndex = 40,
                    TrafficLightBaseName = "Multi.fba2"
                },
                new FaderItemConfig
                {
                    Name = "Monitor Level",
                    ComponentName = "ControlRoom.level",
                    Color = "Cyan",
                    Group = 4,
                    MixerIndex = 0
                },
                new FaderItemConfig
                {
                    Name = "Ambient Mic Level",
                    ComponentName = "ambient.level",
                    Color = "Cyan",
                    Group = 4,
                    MixerIndex = 41
                },
                new FaderItemConfig
                {
                    Name = "H/Phones 2401",
                    ComponentName = "PatchBay.out1.level",
                    Color = "Cyan",
                    Group = 4,
                    MixerIndex = 44,
                    TrafficLightBaseName = "PatchBay.out1"
                },
                new FaderItemConfig
                {
                    Name = "H/Phones 2402",
                    ComponentName = "PatchBay.out2.level",
                    Color = "Cyan",
                    Group = 4,
                    MixerIndex = 45,
                    TrafficLightBaseName = "PatchBay.out2"
                },
                new FaderItemConfig
                {
                    Name = "Flexi 2403",
                    ComponentName = "PatchBay.out3.level",
                    Color = "Cyan",
                    Group = 4,
                    MixerIndex = 46,
                    TrafficLightBaseName = "PatchBay.out3"
                },
                new FaderItemConfig
                {
                    Name = "Flexi 2404",
                    ComponentName = "PatchBay.out4.level",
                    Color = "Cyan",
                    Group = 4,
                    MixerIndex = 47,
                    TrafficLightBaseName = "PatchBay.out4"
                },
                new FaderItemConfig
                {
                    Name = "Aud 1 1Beyond",
                    ComponentName = "Aud1.1beyond.level",
                    Color = "Red",
                    Group = 5,
                    MixerIndex = 48,
                    TrafficLightBaseName = "Aud1.1beyond"
                },
                new FaderItemConfig
                {
                    Name = "Aud 2 1Beyond",
                    ComponentName = "Aud2.1beyond.level",
                    Color = "Red",
                    Group = 5,
                    MixerIndex = 49,
                    TrafficLightBaseName = "Aud2.1beyond"
                },
                new FaderItemConfig
                {
                    Name = "Extron Rec 1",
                    ComponentName = "extron.recorder1.level",
                    Color = "Red",
                    Group = 5,
                    MixerIndex = 50,
                    TrafficLightBaseName = "extron.recorder1"
                },
                new FaderItemConfig
                {
                    Name = "Extron Rec 2",
                    ComponentName = "extron.recorder2.level",
                    Color = "Red",
                    Group = 2,
                    MixerIndex = 51,
                    TrafficLightBaseName = "extron.recorder2"
                },
                new FaderItemConfig
                {
                    Name = "Ante 01-202 Mix",
                    ComponentName = "Ante.202.level",
                    Color = "Purple",
                    Group = 3,
                    MixerIndex = 42
                },
                new FaderItemConfig
                {
                    Name = "Ante 01-266 Mix",
                    ComponentName = "Ante.266.level",
                    Color = "Yellow",
                    Group = 3,
                    MixerIndex = 43
                },
            };

            DspFaderFilters = new List<FaderItemFilter>
            {
                new FaderItemFilter
                {
                    FilterButton = 1,
                    FilterString = "Aud 1"
                },
                new FaderItemFilter
                {
                    FilterButton = 2,
                    FilterString = "Aud 2"
                },
                new FaderItemFilter
                {
                    FilterButton = 3,
                    FilterString = "Multi"
                },
                new FaderItemFilter
                {
                    FilterButton = 4,
                    FilterString = "*"
                },
            };

            CodecAddresses = new Dictionary<uint, string>
            {
                {1, "30.92.0.153"},
                {2, "30.92.0.152"},
                {3, "30.92.0.151"},
            };

            CodecUsername = "crestron";
            CodecPassword = "fountain mutter earthquake haircut";

            OneBeyondAddresses = new Dictionary<uint, string>
            {
                {1, "30.92.1.186"},
                {2, "30.92.1.187"}
            };
            OneBeyondUsername = "admin";
            OneBeyondPassword = "1beyond";
        }
    }
}