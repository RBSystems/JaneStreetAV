using System.Collections.Generic;
using UX.Lib2.Config;

namespace JaneStreetAV.Models.Config
{
    public class SwitcherConfig : AConfig
    {
        public string FrameType { get; set; }
        public List<SwitcherBladeConfig> InputBlades { get; set; }
        public List<SwitcherBladeConfig> OutputBlades { get; set; }
        public List<SwitcherCardConfig> InputCards { get; set; }
        public List<SwitcherCardConfig> OutputCards { get; set; }
        public List<SwitcherInputConfig> Inputs { get; set; }
        public List<SwitcherOutputConfig> Outputs { get; set; }
    }

    public class SwitcherCardConfig : SwitcherComponentConfig
    {

    }

    public class SwitcherBladeConfig : SwitcherComponentConfig
    {
        
    }

    public class SwitcherInputConfig : SwitcherInputOutputConfig
    {
        
    }

    public class SwitcherOutputConfig : SwitcherInputOutputConfig
    {
        
    }

    public abstract class SwitcherComponentConfig
    {
        public uint Number { get; set; }
        public string Type { get; set; }
    }

    public abstract class SwitcherInputOutputConfig : SwitcherComponentConfig
    {
        public string Name { get; set; }
        public string EndpointType { get; set; }
        public string Color { get; set; }
    }
}