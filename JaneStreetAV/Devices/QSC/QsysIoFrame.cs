using System;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Devices.Audio.QSC.Components;
using UX.Lib2.DeviceSupport;

namespace JaneStreetAV.Devices.QSC
{
    public class QsysIoFrame : IFusionAsset
    {
        private readonly IoStatus _statusComponent;
        private bool _deviceCommunicating;
        private string _componentName;

        public QsysIoFrame(IoStatus statusComponent)
        {
            _statusComponent = statusComponent;

            if (_statusComponent == null) throw new Exception("Component cannot be null");

            _componentName = _statusComponent.Name;
            _statusComponent.StatusChanged += StatusComponentOnStatusChanged;
            _deviceCommunicating = statusComponent.Status != "Not Present";
        }

        public string Name
        {
            get { return "Q-Sys IO Frame"; }
        }

        public string ManufacturerName
        {
            get { return "QSC"; }
        }

        public string ModelName
        {
            get
            {
                return string.IsNullOrEmpty(_statusComponent.NetworkId)
                    ? "IO Frame (ID Unknown)"
                    : _statusComponent.NetworkId;
            }
        }

        public string DiagnosticsName
        {
            get { return ModelName; }
        }

        public string Status
        {
            get { return _statusComponent.Status; }
        }

        public int StatusCode
        {
            get { return _statusComponent.StatusCode; }
        }

        public bool DeviceCommunicating
        {
            get { return _deviceCommunicating; }
            private set
            {
                if (_deviceCommunicating == value) return;

                _deviceCommunicating = value;

                if (DeviceCommunicatingChange == null) return;

                try
                {
                    DeviceCommunicatingChange(this, value);
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e);
                }
            }
        }

        public string DeviceAddressString
        {
            get { return _statusComponent.NetworkId; }
        }

        public string SerialNumber
        {
            get { return "Unknown"; }
        }

        public string VersionInfo
        {
            get { return "Unknown"; }
        }

        public event DeviceCommunicatingChangeHandler DeviceCommunicatingChange;

        public FusionAssetType AssetType
        {
            get { return FusionAssetType.AudioProcessor; }
        }

        private void StatusComponentOnStatusChanged(IoStatus statusComponent)
        {
            _deviceCommunicating = statusComponent.Status != "Not Present";
        }
    }
}