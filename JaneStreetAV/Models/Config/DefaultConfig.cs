using System.Collections.Generic;
using Crestron.SimplSharpPro.UI;

namespace JaneStreetAV.Models.Config
{
    public sealed class DefaultConfig : SystemConfig
    {
        /// <summary>
        /// Creates an instance of the default configuration for when the system doesn't find an existing config.
        /// </summary>
        public DefaultConfig()
        {
            IsDefault = true;
            Enabled = true;
            SystemType = SystemType.NotConfigured;

            Rooms = new List<RoomConfig>()
            {
                new RoomConfig()
            };
            
            UserInterfaces = new List<UserInterface>
            {
                new UserInterface
                {
                    Enabled = true,
                    Id = 1,
                    UIControllerType = UIControllerType.ControlRoom,
                    DeviceAddressNumber = 0x03,
                    DeviceAddressString = "0x03",
                    Name = "Default Panel",
                    DeviceType = typeof (XpanelForSmartGraphics).Name,
                    DefaultRoom = 1
                }
            };
        }
    }
}