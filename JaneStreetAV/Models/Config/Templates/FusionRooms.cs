using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp.Reflection;
using Newtonsoft.Json.Linq;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Models;

namespace JaneStreetAV.Models.Config.Templates
{
    public class FusionRooms : SystemConfig
    {
        public FusionRooms()
        {
            try
            {
                var app = Assembly.GetExecutingAssembly();
                var resource =
                    app.GetManifestResourceStream(
                        app.GetManifestResourceNames().First(name => name.EndsWith("fusionrooms.csv")));
                var parser = new CsvParser(resource);
                parser.Parse();
                var data = new Dictionary<string, List<JToken>>();

                foreach (var roomName in parser.Results.Select(result => result["Room"]))
                {
                    if (!data.ContainsKey(roomName))
                    {
                        data.Add(roomName, new List<JToken>());
                    }

                    var room = data[roomName];

                    room.Add(JToken.FromObject(new
                    {
                        Make = data["Make"],
                        Model = data["Model"],
                        IpAddress = data["IpAddress"]
                    }));
                }
                var count = 0U;
                foreach (var room in data)
                {
                    count++;
                    Rooms.Add(new RoomConfig
                    {
                        Name = room.Key,
                        Id = count,
                        Enabled = true,
                        Fusion = new FusionConfig
                        {
                            Enabled = false,
                            Id = IpIdFactory.Create(IpIdFactory.DeviceType.Fusion)
                        },
                        FusionMonitoringInfo = room.Value
                    });
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }
    }
}