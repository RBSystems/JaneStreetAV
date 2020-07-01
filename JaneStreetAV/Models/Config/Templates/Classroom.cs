using System.Collections.Generic;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Crestron.SimplSharpPro.UI;
using JaneStreetAV.UI;
using UX.Lib2.Config;
using UX.Lib2.Models;

namespace JaneStreetAV.Models.Config.Templates
{
    public class Classroom : TemplateBase
    {
        public Classroom()
        {
            SystemType = SystemType.Classroom;

            UserInterfaces = new List<UserInterface>
            {
                new UserInterface
                {
                    Enabled = true,
                    Id = 1,
                    DeviceAddressNumber = 0x04,
                    DeviceAddressString = "0x04",
                    DeviceType = typeof (DmDge200C).Name,
                    Name = string.Format("Room Touchpanel"),
                    UIControllerType = UIControllerType.UserPanel,
                    DefaultRoom = 1
                },
                new UserInterface
                {
                    Enabled = true,
                    Id = 2,
                    DeviceAddressNumber = 0x05,
                    DeviceAddressString = "0x05",
                    DeviceType = typeof (Tsw1060).Name,
                    Name = string.Format("Rack Touchpanel"),
                    UIControllerType = UIControllerType.TechPanel,
                    DefaultRoom = 1
                }
            };

            Rooms = new List<RoomConfig>()
            {
                new RoomConfig()
                {
                    Enabled = true,
                    Id = 1,
                    RoomType = typeof (Rooms.Classroom).FullName,
                    DspConfig = new DspRoomConfig()
                    {
                        Enabled = true,
                        Id = 1,
                        PgmVolControlName = "classroom.pgm.level"
                    },
                    Displays = new List<DisplayConfig>
                    {
                        new DisplayConfig
                        {
                            Enabled = true,
                            Id = 1,
                            Name = "Display 1",
                            SwitcherOutputIndex = 1,
                            DisplayType = DisplayType.Samsung
                        },
                        new DisplayConfig
                        {
                            Enabled = true,
                            Id = 2,
                            Name = "Display 2",
                            SwitcherOutputIndex = 2,
                            DisplayType = DisplayType.Samsung
                        },
                        new DisplayConfig
                        {
                            Enabled = true,
                            Id = 3,
                            Name = "Display 3",
                            SwitcherOutputIndex = 3,
                            DisplayType = DisplayType.Samsung
                        },
                        new DisplayConfig
                        {
                            Enabled = true,
                            Id = 4,
                            Name = "Display 4",
                            SwitcherOutputIndex = 4,
                            DisplayType = DisplayType.Samsung
                        },
                        new DisplayConfig
                        {
                            Enabled = true,
                            Id = 5,
                            Name = "Display 5",
                            SwitcherOutputIndex = 5,
                            DisplayType = DisplayType.Samsung
                        },
                        new DisplayConfig
                        {
                            Enabled = true,
                            Id = 6,
                            Name = "Display 6",
                            SwitcherOutputIndex = 6,
                            DisplayType = DisplayType.Samsung
                        },
                        new DisplayConfig
                        {
                            Enabled = true,
                            Id = 7,
                            Name = "Lectern",
                            SwitcherOutputIndex = 10,
                            DisplayType = DisplayType.Generic
                        }
                    },
                    Sources = new List<SourceConfig>
                    {
                        new SourceConfig
                        {
                            Id = 1,
                            Name = "Video Conference",
                            Icon = JSIcons.Laptop,
                            SwitcherInputIndex = 13,
                            SwitcherInputIndexSecondary = 14,
                            SourceType = SourceType.VideoConference,
                            Enabled = true
                        },
                        new SourceConfig
                        {
                            Id = 2,
                            Name = "Lectern",
                            Icon = JSIcons.Laptop,
                            SwitcherInputIndex = 1,
                            SourceType = SourceType.Laptop,
                            Enabled = true
                        },
                        new SourceConfig
                        {
                            Id = 3,
                            Name = "PC 1",
                            Icon = JSIcons.Laptop,
                            SwitcherInputIndex = 3,
                            SourceType = SourceType.PC,
                            Enabled = true
                        },
                        new SourceConfig
                        {
                            Id = 4,
                            Name = "PC 2",
                            Icon = JSIcons.Laptop,
                            SwitcherInputIndex = 4,
                            SourceType = SourceType.PC,
                            Enabled = true
                        },
                        new SourceConfig
                        {
                            Id = 5,
                            Name = "PC 3",
                            Icon = JSIcons.Laptop,
                            SwitcherInputIndex = 5,
                            SourceType = SourceType.PC,
                            Enabled = true
                        }
                    },
                    SwitcherOutputCodecContent = 20,
                    Fusion = new FusionConfig
                    {
                        Enabled = true,
                        DeviceAddressNumber = 0xF1
                    }
                }
            };

            GlobalSources = new List<SourceConfig>();

            SwitcherType = SystemSwitcherType.DmFrame;

            CodecAddresses = new Dictionary<uint, string>()
            {
                {1, "30.93.0.177"}
            };

            CodecUsername = "admin";
            CodecPassword = "";

            DspConfig = new DspDeviceConfig
            {
                Enabled = true,
                DeviceAddressString = "30.93.1.218",
                DeviceConnectionType = DeviceConnectionType.Network,
                Name = "Q-Sys Core"
            };

            SwitcherConfig = new SwitcherConfig
            {
                Enabled = true,
                FrameType = typeof (DmMd32x32Cpu3).FullName,
                Id = 0x80,
                Name = "DM Switcher Config",
                InputCards = new List<SwitcherCardConfig>(),
                OutputCards = new List<SwitcherCardConfig>(),
                Inputs = new List<SwitcherInputConfig>(),
                Outputs = new List<SwitcherOutputConfig>()
            };

            for (uint nCard = 1; nCard <= 8; nCard++)
            {
                SwitcherConfig.InputCards.Add(new SwitcherCardConfig
                {
                    Number = nCard,
                    Type = typeof(Dmc4kzC).FullName
                });
            }

            for (uint nCard = 9; nCard <= 14; nCard++)
            {
                SwitcherConfig.InputCards.Add(new SwitcherCardConfig
                {
                    Number = nCard,
                    Type = typeof(Dmc4kzHd).FullName
                });
            }

            for (uint nCard = 1; nCard <= 5; nCard++)
            {
                SwitcherConfig.OutputCards.Add(new SwitcherCardConfig
                {
                    Number = nCard,
                    Type = typeof(Dmc4kzCoHdSingle).FullName
                });
            }

            for (uint nCard = 6; nCard <= 10; nCard++)
            {
                SwitcherConfig.OutputCards.Add(new SwitcherCardConfig
                {
                    Number = nCard,
                    Type = typeof(Dmc4kzHdoSingle).FullName
                });
            }

            for (var input = 1U; input <= 14; input ++)
            {
                var inputConfig = new SwitcherInputConfig
                {
                    Number = input,
                    Name = "Input " + input,
                    EndpointType = string.Empty
                };

                switch (input)
                {
                    case 1:
                        inputConfig.EndpointType = typeof (DmTx4k302C).FullName;
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                        inputConfig.EndpointType = typeof(DmTx4k202C).FullName;
                        break;
                    case 9:
                        continue;
                    default:
                        break;
                }

                SwitcherConfig.Inputs.Add(inputConfig);
            }

            foreach (var inputConfig in SwitcherConfig.Inputs)
            {
                switch (inputConfig.Number)
                {
                    case 1: inputConfig.Name = "Lectern"; break;
                    case 2: inputConfig.Name = "Aux Input Plate"; break;
                    case 3: inputConfig.Name = "PC 1"; break;
                    case 4: inputConfig.Name = "PC 2"; break;
                    case 5: inputConfig.Name = "PC 3"; break;
                    case 6: inputConfig.Name = "Cam 1"; break;
                    case 7: inputConfig.Name = "Cam 2"; break;
                    case 8: inputConfig.Name = "Cam 3"; break;
                    case 10: inputConfig.Name = "IPTV"; break;
                    case 11: inputConfig.Name = "Cam Processor"; break;
                    case 12: inputConfig.Name = "Multiview"; break;
                    case 13: inputConfig.Name = "Codec Single"; break;
                    case 14: inputConfig.Name = "Codec Dual"; break;
                }
            }

            for (var output = 1U; output <= 10; output++)
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
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        outputConfig.EndpointType = typeof (DmRmc4kzScalerC).FullName;
                        break;
                }

                SwitcherConfig.Outputs.Add(outputConfig);
            }

            for (var output = 11U; output <= 21; output++)
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
                    case 1: outputConfig.Name = "R1 Display 1"; break;
                    case 2: outputConfig.Name = "R1 Display 2"; break;
                    case 3: outputConfig.Name = "R1 Display 3"; break;
                    case 4: outputConfig.Name = "R2 Display 1"; break;
                    case 5: outputConfig.Name = "R2 Display 2"; break;
                    case 6: outputConfig.Name = "R2 Display 3"; break;
                    case 7: outputConfig.Name = "Rack Monitor"; break;
                    case 8: outputConfig.Name = "Recorder 1"; break;
                    case 9: outputConfig.Name = "Recorder 2"; break;
                    case 10: outputConfig.Name = "Lectern"; break;
                    case 11: outputConfig.Name = "Cam Processor 1"; break;
                    case 12: outputConfig.Name = "Cam Processor 2"; break;
                    case 13: outputConfig.Name = "Cam Processor 3"; break;
                    case 14: outputConfig.Name = "Cam Processor 4"; break;
                    case 15: outputConfig.Name = "Multiview 1"; break;
                    case 16: outputConfig.Name = "Multiview 2"; break;
                    case 17: outputConfig.Name = "Multiview 3"; break;
                    case 18: outputConfig.Name = "Multiview 4"; break;
                    case 19: outputConfig.Name = "Codec Cam"; break;
                    case 20: outputConfig.Name = "Codec Content"; break;
                    case 21: outputConfig.Name = "PGM Audio"; break;
                }
            }

            DspFaderColors = new List<FaderItemColor>
            {
                new FaderItemColor
                {
                    ColorHex = "#ff8000",
                    Name = "Orange",
                    Priority = 1
                },
                new FaderItemColor
                {
                    ColorHex = "#0080FF",
                    Name = "Cyan",
                    Priority = 2
                },
                new FaderItemColor
                {
                    ColorHex = "#ff0000",
                    Name = "Red",
                    Priority = 3
                },
                new FaderItemColor
                {
                    ColorHex = "#ff00ff",
                    Name = "Pink",
                    Priority = 4
                },
                new FaderItemColor
                {
                    ColorHex = "#ffff00",
                    Name = "Yellow",
                    Priority = 5
                },
                new FaderItemColor
                {
                    ColorHex = "#00ff00",
                    Name = "Green",
                    Priority = 6
                },
                new FaderItemColor
                {
                    ColorHex = "#8000ff",
                    Name = "Purple",
                    Priority = 7
                },
                new FaderItemColor
                {
                    ColorHex = "#0000ff",
                    Name = "Blue",
                    Priority = 8
                },
            };

            DspFaderComponents = new List<FaderItemConfig>
            {
                new FaderItemConfig
                {
                    Name = "PGM Mix",
                    ComponentName = "Class.pgmmix.level",
                    Color = "Blue",
                    Group = 3,
                    MixerIndex = 1
                },
                new FaderItemConfig
                {
                    Name = "Mic Mix",
                    ComponentName = "Class.srmix.level",
                    Color = "Blue",
                    Group = 3,
                    MixerIndex = 2
                },
                new FaderItemConfig
                {
                    Name = "Patch Bay 1",
                    ComponentName = "Class.patchbay1.level",
                    Color = "Blue",
                    Group = 3,
                    MixerIndex = 3
                },
                new FaderItemConfig
                {
                    Name = "Patch Bay 2",
                    ComponentName = "Class.patchbay2.level",
                    Color = "Blue",
                    Group = 3,
                    MixerIndex = 4
                },
                new FaderItemConfig
                {
                    Name = "PGM",
                    ComponentName = "Class.pgm.gain",
                    Color = "Blue",
                    Group = 1,
                    MixerIndex = 5
                },
                new FaderItemConfig
                {
                    Name = "VC",
                    ComponentName = "Class.vc.gain",
                    Color = "Blue",
                    Group = 1,
                    MixerIndex = 6
                },
                new FaderItemConfig
                {
                    Name = "Patch Bay In 1",
                    ComponentName = "Class.PatchBay.in1.gain",
                    Color = "Cyan",
                    Group = 1,
                    MixerIndex = 7
                },
                new FaderItemConfig
                {
                    Name = "Patch Bay In 2",
                    ComponentName = "Class.PatchBay.in2.gain",
                    Color = "Cyan",
                    Group = 1,
                    MixerIndex = 8
                },
                new FaderItemConfig
                {
                    Name = "Handheld",
                    ComponentName = "Class.hh.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 9,
                    TrafficLightBaseName = "Class.HH"
                },
                new FaderItemConfig
                {
                    Name = "Lapel",
                    ComponentName = "Class.lapel.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 10,
                    TrafficLightBaseName = "Class.lapel"
                },
                new FaderItemConfig
                {
                    Name = "Floorbox A",
                    ComponentName = "Class.fba.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 11,
                    TrafficLightBaseName = "Class.fba"
                },
                new FaderItemConfig
                {
                    Name = "Floorbox B",
                    ComponentName = "Class.fbb.boost",
                    Color = "Blue",
                    Group = 2,
                    MixerIndex = 12,
                    TrafficLightBaseName = "Class.fbb"
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
                    Name = "Patch Bay Out 1",
                    ComponentName = "PatchBay.out1.level",
                    Color = "Cyan",
                    Group = 4,
                    MixerIndex = 13,
                    TrafficLightBaseName = "patchbay1"
                },
                new FaderItemConfig
                {
                    Name = "Patch Bay Out 2",
                    ComponentName = "PatchBay.out2.level",
                    Color = "Cyan",
                    Group = 4,
                    MixerIndex = 14,
                    TrafficLightBaseName = "patchbay2"
                },
                new FaderItemConfig
                {
                    Name = "1Beyond",
                    ComponentName = "Class.1beyond.level",
                    Color = "Red",
                    Group = 5,
                    MixerIndex = 15,
                    TrafficLightBaseName = "1beyond"
                },
                new FaderItemConfig
                {
                    Name = "Extron Recorder 1",
                    ComponentName = "extron.recorder1.level",
                    Color = "Red",
                    Group = 5,
                    MixerIndex = 16,
                    TrafficLightBaseName = "extron.recorder1"
                },
                new FaderItemConfig
                {
                    Name = "Extron Recorder 2",
                    ComponentName = "extron.recorder2.level",
                    Color = "Red",
                    Group = 5,
                    MixerIndex = 17,
                    TrafficLightBaseName = "extron.recorder2"
                },
            };

            TriplePlayServerAddress = "30.93.1.126";
        }
    }
}