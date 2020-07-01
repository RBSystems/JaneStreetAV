using System;
using System.Collections.Generic;
using System.Linq;
using Automate_VX;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
using JaneStreetAV.Models.Config;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;

namespace JaneStreetAV.Devices
{
    public class OneBeyond
    {
        private readonly string _host;
        private readonly uint _processorId;
        private readonly AutomateVx _vx;
        private static bool _consoleRegistered;
        private Thread _loginThread;
        private bool _loggedIn;
        private bool _programStopping;
        private Dictionary<ushort, string> _layouts = new Dictionary<ushort, string>();
        private Dictionary<ushort, string> _configs = new Dictionary<ushort, string>(); 

        public OneBeyond(string host, uint processorId)
        {
            if (!_consoleRegistered)
            {
                _consoleRegistered = true;
                CrestronConsole.AddNewConsoleCommand(parameters =>
                {

                }, "VxTest", "Test a OneBeyond command", ConsoleAccessLevelEnum.AccessOperator);
            }

            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                _programStopping = type == eProgramStatusEventType.Stopping;
            };

            _host = host;
            _processorId = processorId;
            _vx = new AutomateVx();
            _vx.Debug(1);
            _vx.LoginStateCallbackEvent += LoginStateCallbackEvent;
            _vx.SystemStatusCallbackEvent += SystemStatusCallbackEvent;
            _vx.RecStatusCallbackEvent += RecStatusCallbackEvent;
            _vx.StreamStatusCallbackEvent += StreamStatusCallbackEvent;
            _vx.OutputStatusCallbackEvent += OutputStatusCallbackEvent;
            _vx.InitializedCallbackEvent += InitializedCallbackEvent;
            _vx.AutoSwitchStatusCallbackEvent += AutoSwitchStatusCallbackEvent;
            _vx.IsoRecStatusCallbackEvent += IsoRecStatusCallbackEvent;
            _vx.LayoutStatusCallbackEvent += LayoutStatusCallbackEvent;
            _vx.LayoutNamesCallbackEvent += LayoutNamesCallbackEvent;
            _vx.LayoutCountCallbackEvent += LayoutCountCallbackEvent;
            _vx.LayoutClearCallbackEvent += LayoutClearCallbackEvent;
            _vx.RoomConfigStatusCallbackEvent += RoomConfigStatusCallbackEvent;
            _vx.RoomConfigNamesCallbackEvent += RoomConfigNamesCallbackEvent;
            _vx.RoomConfigCountCallbackEvent += RoomConfigCountCallbackEvent;
            _vx.RoomConfigClearCallbackEvent += RoomConfigClearCallbackEvent;
            _vx.CameraSelectCallbackEvent += CameraSelectCallbackEvent;
            _vx.CameraPresetCallbackEvent += CameraPresetCallbackEvent;
            _vx.CopyFilesStatusCallbackEvent += CopyFilesStatusCallbackEvent;
            _vx.RecSpaceInfoCallbackEvent += RecSpaceInfoCallbackEvent;
            _vx.AutomateVxErrorCallbackEvent += AutomateVxErrorCallbackEvent;
            _vx.PluginStatusCallbackEvent += PluginStatusCallbackEvent;
        }

        public void Initialize()
        {
            var thread = new Thread(specific =>
            {
                _vx.Initialize(_host, 0, 0);
                return null;
            }, null);
        }

        public void GetAutoSwitchStatus()
        {
            _vx.GetAutoSwitchStatus();
        }

        public void GetLayouts()
        {
            _layouts.Clear();
            _vx.GetLayouts();
        }

        public void GetCurrentLayout()
        {
            _vx.GetCurrentLayout();
        }

        public void ChangeLayout(ushort layout)
        {
            _vx.ChangeLayout(layout);
        }

        public void GetRoomConfigs()
        {
            _configs.Clear();
            _vx.GetRoomConfigs();
        }

        public void GetCurrentRoomConfig()
        {
            _vx.GetCurrentRoomConfig();
        }

        public void ChangeRoomConfig(ushort config)
        {
            _vx.ChangeRoomConfig(config);
        }

        public void ManualSwitchCamera(ushort camera)
        {
            _vx.ManualSwitchCamera(camera);
        }

        public void GetCameraStatus()
        {
            _vx.GetCameraStatus();
        }

        public void GetRecordStatus()
        {
            _vx.GetRecordStatus();
        }

        public void GetRecordingSpace()
        {
            _vx.GetRecordingSpace();
        }

        public void GetStreamStatus()
        {
            _vx.GetStreamStatus();
        }

        public void GetIsoRecordStatus()
        {
            _vx.GetIsoRecordStatus();
        }

        public void ToggleAutoSwitch()
        {
            _vx.ToggleAutoSwitch();
        }

        public void ToggleStream()
        {
            _vx.ToggleStream();
        }

        public void StartRecord()
        {
            _vx.StartRecord();
        }

        public void StopRecord()
        {
            _vx.StopRecord();
        }

        public IEnumerable<OneBeyondCameraConfig> GetCamerasForConfig(ushort configValue)
        {
            var config = ConfigManager.Config.OneBeyondConfig;
            var cameras = new List<OneBeyondCameraConfig>();
            var c =
                config.ProcessorConfigs[_processorId].CameraConfigs.FirstOrDefault(
                    d => d.UsedWithConfigValue == configValue);
            if (c == null) return cameras;
            cameras.AddRange(c.CameraIds.Select(id => config.Cameras.First(cam => cam.Id == id)));
            return cameras;
        } 

        public event UpdateFeedbackEventHandler UpdateReceived;

        protected virtual void OnUpdateReceived(OneBeyond device, UpdateFeedbackEventArgs args)
        {
            var handler = UpdateReceived;
            if (handler == null) return;
            try
            {
                handler(device, args);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Error updating event");
            }
        }

        private void LoginStateCallbackEvent(ushort loginFb)
        {
            _loggedIn = Convert.ToBoolean(loginFb);
            Debug.WriteInfo("VX", "LoginStateCallbackEvent({0})", loginFb);
            if(loginFb > 0 || (_loginThread != null && _loginThread.ThreadState == Thread.eThreadStates.ThreadRunning)) return;
            _loginThread = new Thread(LoginProcess, null);
        }

        private object LoginProcess(object userSpecific)
        {
            while (!_loggedIn && !_programStopping)
            {
                Debug.WriteInfo("VX", "Waiting to login..");
                Thread.Sleep(5000);
                try
                {
                    Debug.WriteInfo("VX", "Attempting to login..");
                    _vx.Login(ConfigManager.Config.OneBeyondUsername, ConfigManager.Config.OneBeyondPassword);
                }
                catch(Exception e)
                {
                    CloudLog.Error("Could not login to VX, {0}", e.Message);
                }
            }
            if (!_programStopping)
            {
                Debug.WriteInfo("VX", "Logged in! Leaving login process..");
            }
            return null;
        }

        private void SystemStatusCallbackEvent(SimplSharpString status)
        {
            Debug.WriteInfo("VX", "SystemStatusCallbackEvent({0})", status);
        }

        private void RecStatusCallbackEvent(ushort status)
        {
            //Debug.WriteInfo("VX", "RecStatusCallbackEvent({0})", status);
            OnUpdateReceived(this, new UpdateFeedbackEventArgs(UpdateType.RecordStatusUpdated, status));
        }

        private void StreamStatusCallbackEvent(ushort status)
        {
            //Debug.WriteInfo("VX", "StreamStatusCallbackEvent({0})", status);
            OnUpdateReceived(this, new UpdateFeedbackEventArgs(UpdateType.StreamStatusUpdated, status));
        }

        private void OutputStatusCallbackEvent(ushort status)
        {
            Debug.WriteInfo("VX", "OutputStatusCallbackEvent({0})", status);
        }

        private void InitializedCallbackEvent(ushort init)
        {
            Debug.WriteInfo("VX", "InitializedCallbackEvent({0})", init);
        }

        private void AutoSwitchStatusCallbackEvent(ushort status)
        {
            //Debug.WriteInfo("VX", "AutoSwitchStatusCallbackEvent({0})", status);
            OnUpdateReceived(this, new UpdateFeedbackEventArgs(UpdateType.AutoSwitchStatusUpdated, status));
        }

        private void IsoRecStatusCallbackEvent(ushort status)
        {
            Debug.WriteInfo("VX", "IsoRecStatusCallbackEvent({0})", status);
        }

        private void LayoutStatusCallbackEvent(ushort status)
        {
            //Debug.WriteInfo("VX", "LayoutStatusCallbackEvent({0})", status);
            OnUpdateReceived(this, new UpdateFeedbackEventArgs(UpdateType.CurrentLayoutUpdated, status));
        }

        private void LayoutNamesCallbackEvent(ushort index, SimplSharpString layoutName)
        {
            //Debug.WriteInfo("VX", "LayoutNamesCallbackEvent({0}, {1})", index, layoutName);
            _layouts[index] = layoutName.ToString();
        }

        private void LayoutCountCallbackEvent(ushort number)
        {
            //Debug.WriteInfo("VX", "LayoutCountCallbackEvent({0})", number);
            OnUpdateReceived(this, new UpdateFeedbackEventArgs(UpdateType.LayoutsUpdated, _layouts));
        }

        private void LayoutClearCallbackEvent()
        {
            Debug.WriteInfo("VX", "LayoutClearCallbackEvent()");
        }

        private void RoomConfigStatusCallbackEvent(ushort status)
        {
            //Debug.WriteInfo("VX", "RoomConfigStatusCallbackEvent({0})", status);
            OnUpdateReceived(this, new UpdateFeedbackEventArgs(UpdateType.CurrentRoomConfigUpdated, status));
        }

        private void RoomConfigNamesCallbackEvent(ushort index, SimplSharpString configName)
        {
            //Debug.WriteInfo("VX", "RoomConfigNamesCallbackEvent({0}, {1})", index, configName);
            _configs[index] = configName.ToString();
        }

        private void RoomConfigCountCallbackEvent(ushort number)
        {
            //Debug.WriteInfo("VX", "RoomConfigCountCallbackEvent({0})", number);
            OnUpdateReceived(this, new UpdateFeedbackEventArgs(UpdateType.RoomConfigsUpdated, _configs));
        }

        private void RoomConfigClearCallbackEvent()
        {
            Debug.WriteInfo("VX", "RoomConfigClearCallbackEvent()");
        }

        private void CameraSelectCallbackEvent(ushort camera)
        {
            //Debug.WriteInfo("VX", "CameraSelectCallbackEvent({0})", camera);
            OnUpdateReceived(this, new UpdateFeedbackEventArgs(UpdateType.CameraSelectUpdated, camera));
        }

        private void CameraPresetCallbackEvent(ushort camera, ushort preset)
        {
            Debug.WriteInfo("VX", "CameraPresetCallbackEvent({0}, {1})", camera, preset);
        }

        private void CopyFilesStatusCallbackEvent(ushort status)
        {
            Debug.WriteInfo("VX", "CopyFilesStatusCallbackEvent({0})", status);
        }

        private void RecSpaceInfoCallbackEvent(ushort available, ushort total)
        {
            Debug.WriteInfo("VX", "RecSpaceInfoCallbackEvent({0}, {1})", available, total);
        }

        private void AutomateVxErrorCallbackEvent(ushort code, SimplSharpString error, SimplSharpString requestBody)
        {
            Debug.WriteWarn("VX", "AutomateVxErrorCallbackEvent({0}, {1}, {2})", code, error, requestBody);
        }

        private void PluginStatusCallbackEvent(SimplSharpString pluginName, ushort code, SimplSharpString requestBody)
        {
            Debug.WriteInfo("VX", "PluginStatusCallbackEvent({0}, {1}, {2})", pluginName, code, requestBody);
        }
    }

    public enum UpdateType
    {
        LayoutsUpdated,
        CurrentLayoutUpdated,
        RoomConfigsUpdated,
        CurrentRoomConfigUpdated,
        AutoSwitchStatusUpdated,
        CameraSelectUpdated,
        RecordStatusUpdated,
        StreamStatusUpdated
    }

    public class UpdateFeedbackEventArgs : EventArgs
    {
        public UpdateFeedbackEventArgs(UpdateType type, object value)
        {
            Type = type;
            Value = value;
        }

        public UpdateType Type { get; private set; }
        public ushort Index { get; private set; }
        public object Value { get; private set; }
    }

    public delegate void UpdateFeedbackEventHandler(OneBeyond device, UpdateFeedbackEventArgs args);
}