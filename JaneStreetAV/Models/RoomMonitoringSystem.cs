using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.Net.Https;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using JaneStreetAV.Models.Config;
using SSMono;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Models;

namespace JaneStreetAV.Models
{
    public class RoomMonitoringSystem : MeetingSpaceSystem
    {
        private readonly ReadOnlyDictionary<uint, string> _codecAddresses;
        private Thread _pollingThread;
        private bool _programStopping;

        private static readonly HttpsClient Client = new HttpsClient()
        {
            IncludeHeaders = false,
            HostVerification = false,
            PeerVerification = false,
            KeepAlive = false,
            Verbose = false,
            Timeout = 5
        };

        public RoomMonitoringSystem(CrestronControlSystem controlSystem) : base(controlSystem)
        {
            var addresses = ConfigManager.Config.CodecAddresses;

            if (addresses == null) return;

            _codecAddresses = new ReadOnlyDictionary<uint, string>(addresses);
        }

        protected override IEnumerable<InitializeProcess> GetSystemItemsToInitialize()
        {
            var items = base.GetSystemItemsToInitialize().ToList();
            items.Add(new InitializeProcess(StartPollingCodecs, "Starting Cisco codec polling"));
            return items;
        }

        private void StartPollingCodecs()
        {
            if(_pollingThread != null) return;

            _pollingThread = new Thread(CodecPollingProcess, null, Thread.eThreadStartOptions.CreateSuspended)
            {
                Name = "Codec Polling",
                Priority = Thread.eThreadPriority.MediumPriority
            };
            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                _programStopping = type == eProgramStatusEventType.Stopping;
            };
            _pollingThread.Start();
        }

        private object CodecPollingProcess(object userSpecific)
        {
            while (!_programStopping)
            {
                foreach (var valuePair in _codecAddresses)
                {
                    var roomId = valuePair.Key;
                    var address = valuePair.Value;

                    try
                    {
                        var uri = new UriBuilder("https", address, 443, "status.xml").Uri;
                        Debug.WriteInfo("Polling Codec", "Room {0}: {1}", roomId, uri.ToString());
                        var request = new HttpsClientRequest { Url = new UrlParser(uri.ToString()) };
                        var username = ConfigManager.Config.CodecUsername;
                        var password = ConfigManager.Config.CodecPassword;
                        var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
                        request.Header.AddHeader(new HttpsHeader("Authorization", "Basic " + auth));
                        var response = Client.Dispatch(request);
                        var reader = new XmlReader(response.ContentString);
                        var doc = XDocument.Load(reader);
                        var status = doc.Element("Status");
                        Debug.WriteSuccess("Codec Online", status.Element("UserInterface").Element("ContactInfo").Element("Name").Value);
                        Debug.WriteSuccess("Mute", status.Element("Audio").Element("Microphones").Element("Mute").Value);
                        Debug.WriteSuccess("In Call", status.Elements("Call").Any().ToString());
                        Debug.WriteSuccess("Uptime", TimeSpan.FromSeconds(int.Parse(status.Element("SystemUnit").Element("Uptime").Value)).ToPrettyFormat());
                        Debug.WriteSuccess("Standby", "Status = {0}", status.Element("Standby").Element("State").Value != "Standby");
                        CrestronConsole.PrintLine("");
                    }
                    catch (Exception e)
                    {
                        CloudLog.Error("Error polling codec at {0}, {1}", address, e.Message);
                    }
                }

                Thread.Sleep(30000);
                CrestronEnvironment.AllowOtherAppsToRun();
            }

            CloudLog.Notice("Leaving thread: {0}", Thread.CurrentThread.Name);

            return null;
        }
    }
}