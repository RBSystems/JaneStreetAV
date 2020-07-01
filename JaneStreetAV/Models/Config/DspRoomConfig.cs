using System.Collections.Generic;
using UX.Lib2.Config;

namespace JaneStreetAV.Models.Config
{
    public class DspRoomConfig : AConfig
    {
        public DspRoomConfig()
        {
            OtherComponents = new List<string>();
        }

        public string PgmVolControlName { get; set; }
        public List<string> OtherComponents { get; set; }
    }
}