/* License
 * ------------------------------------------------------------------------------
 * Copyright (c) 2017 UX Digital Systems Ltd
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * ------------------------------------------------------------------------------
 * UX.Digital
 * ----------
 * http://ux.digital
 * support@ux.digital
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharp.Reflection;
using Newtonsoft.Json.Linq;
using SSMono.IO;
using SSMono.Net;
using SSMono.Net.Http;
using SSMono.Threading.Tasks;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;
using Thread = Crestron.SimplSharpPro.CrestronThread.Thread;

namespace JaneStreetAV.SoftwareUpdate
{
    public static class SoftwareUpdate
    {
        #region Fields

        private static HttpClient _httpClient = new HttpClient();
        private static UDPServer _udpServer;
        private static bool _programStopping;
        private static Thread _listenThread;
        private static string _serverAddress = string.Empty;
        private static List<AppFileInfo> _updates;
        private static CTimer _checkUpdatesTimer;
        private static UpdateStatus _status;
        private static int _progress;
        private static bool _downloadCancelled;
        private static Version _pushedVersion;
        private static Thread _pushUpdateProcessThread;

        #endregion

        #region Constructors

        static SoftwareUpdate()
        {
            
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        public static event SoftwareUpdateShouldLoadEventHandler UpdateShouldLoad;

        #endregion

        #region Delegates
        #endregion

        public enum UpdateStatus
        {
            NotRunning,
            Pending,
            Waiting,
            Downloading,
            Downloaded,
            Loading,
            Failed,
            Cancelled
        }

        #region Properties

        public static bool UpdateAvailable
        {
            get
            {
                return Updates != null && Updates.Any();
            }
        }

        public static AppFileInfo LatestUpdate
        {
            get
            {
                return Updates == null ? null : Updates.FirstOrDefault();
            }
        } 

        public static IEnumerable<AppFileInfo> Updates
        {
            get
            {
                if (_updates == null)
                {
                    return null;
                }

                return
                    _updates
                        .Where(update => !update.IsDebug)
                        .Where(update => update.VersionIsNewerThanRunningVersion)
                        .OrderByDescending(update => new Version(update.Version));
            }
        }

        public static IEnumerable<AppFileInfo> RollBacks
        {
            get
            {
                if (_updates == null)
                {
                    return null;
                }

                return
                    _updates
                        .Where(update => !update.IsDebug)
                        .Where(update => update.VersionIsOlderThanRunningVersion)
                        .OrderByDescending(update => new Version(update.Version));
            }
        }

        public static IEnumerable<AppFileInfo> DebugBuilds
        {
            get
            {
                if (_updates == null)
                {
                    return null;
                }

                return
                    _updates
                        .Where(update => update.IsDebug)
                        .OrderByDescending(update => new Version(update.Version));
            }
        }

        public static UpdateStatus Status
        {
            get { return _status; }
        }

        public static bool Busy
        {
            get
            {
                switch (Status)
                {
                    case UpdateStatus.Cancelled:
                    case UpdateStatus.Failed:
                    case UpdateStatus.NotRunning:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public static int Progress
        {
            get { return _progress; }
        }

        public static bool Ready
        {
            get { return !String.IsNullOrEmpty(_serverAddress); }
        }

        public static string ServerAddress
        {
            get { return _serverAddress; }
        }

        #endregion

        #region Methods

        public static void GetAppUpdates()
        {
            if (string.IsNullOrEmpty(_serverAddress))
            {
                throw new Exception("No update server found");
            }

            var attemptCount = 0;
            while (attemptCount < 2)
            {
                attemptCount ++;
                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var appName = assembly.GetName();

                    var url = string.Format("http://{0}/api/software/app_updates?app={1}", _serverAddress, appName.Name);
                    var response = _httpClient.GetAsync(url).Await();

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        CloudLog.Error("Error getting software update, {0}", response.StatusCode);
                        return;
                    }

                    var data = JToken.Parse(response.Content.ReadAsStringAsync().Await());
                    _updates = data["response"]["files"].Select(fileData => fileData.ToObject<AppFileInfo>())
                        .Where(f => new Version(f.Version) != appName.Version)
                        .ToList();
                    return;
                }
                catch (Exception e)
                {
                    if (attemptCount == 1)
                    {
                        CloudLog.Warn("Could not Get App Updates, disposing old httpclient and making new one");
                        _httpClient.Dispose();
                        _httpClient = new HttpClient();
                    }
                    else
                    {
                        throw new Exception(
                            string.Format("Could not get information request from update server, {0}", e.Message), e);
                    }
                }
            }
        }

        public static JArray GetPanelUpdates()
        {
            if (string.IsNullOrEmpty(_serverAddress))
            {
                throw new Exception("No update server found");
            }

            var data =
                JObject.Parse(
                    _httpClient.GetAsync(string.Format("http://{0}/api/software/panel_updates", _serverAddress))
                        .Await()
                        .Content.ReadAsStringAsync()
                        .Await());
            return data["response"]["files"] as JArray;
        }

        public static void ListenForUpdateServers()
        {
            if(_udpServer != null) return;
            
            _udpServer = new UDPServer(IPAddress.Any, 15000, 1000);
            _udpServer.EnableUDPServer();

            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                _programStopping = type == eProgramStatusEventType.Stopping;

                if (!_programStopping) return;

                try
                {
                    _udpServer.DisableUDPServer();
                    _udpServer.Dispose();
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e);
                }
                if (_listenThread.ThreadState == Thread.eThreadStates.ThreadRunning)
                {
                    _listenThread.Abort();
                }
            };

            _listenThread = new Thread(ProcessUdpData, null)
            {
                Name = "Software Update Listener"
            };
        }

        private static object ProcessUdpData(object o)
        {
            var buffer = new byte[1000];
            var byteIndex = 0;

            while (!_programStopping)
            {
                try
                {
                    var count = _udpServer.ReceiveData();
                    for (var i = 0; i < count; i++)
                    {
                        var b = _udpServer.IncomingDataBuffer[i];
                        if (b == 13)
                        {
                            try
                            {
                                var str = Encoding.ASCII.GetString(buffer, 0, byteIndex);

                                var match = Regex.Match(str, @"UpdateServer: *([\w-.]+)");
                                if (match.Success)
                                {
                                    _serverAddress = match.Groups[1].Value;

                                    if (_checkUpdatesTimer == null)
                                    {
                                        _checkUpdatesTimer = new CTimer(specific =>
                                        {
                                            try
                                            {
                                                GetAppUpdates();
                                            }
                                            catch (Exception e)
                                            {
                                                CloudLog.Error("Could not check for updates, {0}", e.Message);
                                            }
                                        }, null, 1000,
                                            (long) TimeSpan.FromHours(1).TotalMilliseconds);
                                    }
                                    byteIndex = 0;
                                    continue;
                                }

                                match = Regex.Match(str, @"UpdatePush\s+Version:\s*([\d\.]+)");
                                if (match.Success)
                                {
                                    Debug.WriteWarn("Update Push!!! Version", match.Groups[1].Value);
                                    CloudLog.Notice("Software Update Push command received. Version = {0}", match.Groups[1].Value);
                                    _pushedVersion = new Version(match.Groups[1].Value);

                                    if (_pushedVersion.CompareTo(Assembly.GetExecutingAssembly().GetName().Version) == 0)
                                    {
                                        CloudLog.Notice("New version is current so will skip the push!");
                                    }
                                    else
                                    {
                                        if (_pushUpdateProcessThread != null &&
                                            _pushUpdateProcessThread.ThreadState == Thread.eThreadStates.ThreadRunning)
                                        {
                                            byteIndex = 0;
                                            continue;
                                        }
                                        Debug.WriteInfo("Update push ok to run, launching process now");
                                        CloudLog.Info("Starting software update push process", match.Groups[1].Value);
                                        _pushUpdateProcessThread = new Thread(UpdateFromPushedUpdateProcess, null);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                CloudLog.Exception(e);
                            }

                            byteIndex = 0;
                        }
                        else
                        {
                            buffer[byteIndex] = b;
                            byteIndex++;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is ThreadAbortException)
                    {
                        return null;
                    }
                    CloudLog.Exception(e);
                }
            }

            return null;
        }

        public static void ResetDownloads()
        {
            var dir = new SSMono.IO.DirectoryInfo(InitialParametersClass.ProgramDirectory.ToString());
            foreach (var fileInfo in dir.GetFiles("*.cpz"))
            {
                Debug.WriteWarn("Removing existing package file", fileInfo.FullName);
                fileInfo.Delete();
            }
            _status = UpdateStatus.NotRunning;
            _progress = 0;
        }

        public static void DownloadCancel()
        {
            _downloadCancelled = true;
        }

        public static void OnUpdateShouldLoad()
        {
            SetStatusLoading();
            var handler = UpdateShouldLoad;
            if (handler != null) handler();
        }

        public static int Download(string url)
        {
            Debug.WriteInfo("SoftwareUpdate.Download() Called");

            if (_status == UpdateStatus.Pending)
            {
                throw new Exception("Download process already running");
            }
            try
            {
                ResetDownloads();

                _downloadCancelled = false;
                _status = UpdateStatus.Pending;
                _progress = 10;
                var attemptCount = 0;
                
                while (true)
                {
                    if (_downloadCancelled)
                    {
                        Debug.WriteWarn("SoftwareUpdate.Download()", "Cancelled");
                        _status = UpdateStatus.Cancelled;
                        return -1;
                    }

                    attemptCount++;

                    if (attemptCount > 10)
                    {
                        Debug.WriteWarn("SoftwareUpdate.Download()", "Too many attempts, cancelling");
                        _status = UpdateStatus.Failed;
                        _httpClient.Dispose();
                        _httpClient = new HttpClient();
                        return -3;
                    }

                    var getTask = _httpClient.GetAsync(url);
                    Debug.WriteInfo("SoftwareUpdate.Download()", "Awaiting response");
                    var response = getTask.Await();
                    var message = response.EnsureSuccessStatusCode();
                    Debug.WriteWarn("SoftwareUpdate.Download()", "Status = {0}", message.StatusCode.ToString());
                    if (message.StatusCode == HttpStatusCode.OK)
                    {
                        _status = UpdateStatus.Waiting;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                        continue;
                    }

                    if (response.Content.Headers.ContentLength == null)
                    {
                        Debug.WriteWarn("SoftwareUpdate.Download()", "ContentLength is null");
                        Thread.Sleep(5000);
                        continue;
                    }
                    var totalBytes = (long)response.Content.Headers.ContentLength;

                    Debug.WriteInfo("SoftwareUpdate.Download()", "Received {0} bytes, {1}", totalBytes,
                        Tools.PrettyByteSize(totalBytes, 2));

                    if (_downloadCancelled)
                    {
                        Debug.WriteWarn("SoftwareUpdate.Download()", "Cancelled");
                        _status = UpdateStatus.Cancelled;
                        return -1;
                    }

                    using (var contentStream = response.Content.ReadAsStreamAsync().Await())
                    {
                        _status = UpdateStatus.Downloading;
                        var totalBytesRead = 0L;
                        var readCount = 0L;
                        var buffer = new byte[8192];
                        var isMoreToRead = true;

                        using (
                            var fileStream = new FileStream(InitialParametersClass.ProgramDirectory + @"\update.cpz",
                                FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            try
                            {
                                do
                                {
                                    var bytesRead = contentStream.ReadAsync(buffer, 0, buffer.Length).Await();
                                    if (bytesRead == 0)
                                    {
                                        isMoreToRead = false;
                                        _progress = (int) Tools.ScaleRange(totalBytesRead, 0, totalBytes, 0, 100);
                                        Debug.WriteInfo("Download progress:", "{0}%", _progress);
                                        continue;
                                    }

                                    fileStream.WriteAsync(buffer, 0, bytesRead).Await();

                                    totalBytesRead += bytesRead;
                                    readCount += 1;

                                    if (readCount%100 != 0) continue;
                                    _progress = (int) Tools.ScaleRange(totalBytesRead, 0, totalBytes, 0, 100);
                                    Debug.WriteInfo("Download progress:", "{0}%", _progress);
                                } while (isMoreToRead);
                                Debug.WriteSuccess("Download complete");
                                _status = UpdateStatus.Downloaded;
                                _progress = 100;
                                return 0;
                            }
                            catch (Exception e)
                            {
                                CloudLog.Exception(e);
                                _status = UpdateStatus.Failed;
                                _httpClient.Dispose();
                                _httpClient = new HttpClient();
                                return -2;
                            }
                        }
                    }
                }
            }
            catch
            {
                _status = UpdateStatus.Failed;
                return -2;
            }
        }

        public static object UpdateFromPushedUpdateProcess(object o)
        {
            var random = new Random();
            var attemptCount = 0;

            if (_status == UpdateStatus.Failed)
            {
                CloudLog.Notice("Previous update had failed, clearing status!");
                _status = UpdateStatus.NotRunning;
                Thread.Sleep(1000);
            }

            while (_status == UpdateStatus.NotRunning && !_programStopping)
            {
                try
                {
                    attemptCount ++;

                    if (attemptCount == 1)
                    {
                        var ms = random.Next((int) TimeSpan.FromSeconds(30).TotalMilliseconds,
                            (int) TimeSpan.FromMinutes(20).TotalMilliseconds);

                        Debug.WriteSuccess("Update push process running", "waiing for {0}",
                            TimeSpan.FromMilliseconds(ms).ToString());

                        CrestronEnvironment.AllowOtherAppsToRun();
                        Thread.Sleep(ms);
                    }
                    else
                    {
                        Thread.Sleep(60000);
                    }

                    if (attemptCount > 5)
                    {
                        CloudLog.Error("Software update push failed after 5 attempts. Exiting process");
                        _status = UpdateStatus.Failed;
                        return null;
                    }

                    if (_status != UpdateStatus.NotRunning)
                    {
                        return null;
                    }

                    Debug.WriteInfo("Getting current list of updates...");

                    try
                    {
                        GetAppUpdates();
                    }
                    catch (Exception e)
                    {
                        CloudLog.Exception(e);
                        _httpClient.Dispose();
                        _httpClient = new HttpClient();
                    }

                    var info = _updates.FirstOrDefault(f => new Version(f.Version).CompareTo(_pushedVersion) == 0);

                    if (info != null)
                    {
                        Debug.WriteInfo("Found update available at URL", info.Url);
                        CloudLog.Info("Software update push can be downloaded from: {0}", info.Url);
                        var downloadResult = Download(info.Url);
                        if (downloadResult == 0)
                        {
                            try
                            {
                                OnUpdateShouldLoad();
                            }
                            catch (Exception e)
                            {
                                CloudLog.Exception(e);
                                continue;
                            }
                            return null;
                        }
                        
                        _status = UpdateStatus.NotRunning;

                        CloudLog.Warn("Attempt {0} to download update failed, Error {1}", attemptCount, downloadResult);
                        continue;
                    }

                    CloudLog.Error("Pushed update failed, could not find an update matching version {0}", _pushedVersion);
                    _status = UpdateStatus.Failed;
                    return null;
                }
                catch (Exception e)
                {
                    if (attemptCount <= 5) continue;
                    CloudLog.Error("Pushed update failed to get version {0}, {1}", _pushedVersion, e.Message);
                    _status = UpdateStatus.Failed;
                    return null;
                }
            }

            return null;
        }

        private static void SetStatusLoading()
        {
            CloudLog.Notice("SoftwareUpdate calling system to update from CPZ!");
            _status = UpdateStatus.Loading;
        }

        #endregion
    }

    public delegate void SoftwareUpdateShouldLoadEventHandler();
}