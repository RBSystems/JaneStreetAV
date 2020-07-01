using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
using JaneStreetAV.Devices;
using JaneStreetAV.Models;
using JaneStreetAV.Models.Config;
using JaneStreetAV.UI.Joins;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Devices.Cameras.Sony;
using UX.Lib2.DeviceSupport;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.ControlRoomPanel
{
    public class CameraControlViewController : ASubPage
    {
        private readonly ButtonCollection _processorSelectButtons;
        private OneBeyond _selectedProcessor;
        private readonly ButtonCollection _cameraButtons;
        private readonly UIDynamicButtonList _layoutsList;
        private readonly ButtonCollection _layoutsListButtons;
        private readonly UIDynamicButtonList _roomConfigsList;
        private readonly ButtonCollection _roomConfigsListButtons;
        private readonly UIDynamicButtonList _cameraSelectList;
        private readonly ButtonCollection _cameraSelectListButtons;
        private readonly UIButton _autoSwitchEnableButton;

        public CameraControlViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageTechCameras, "Camera Control", TimeSpan.Zero)
        {
            var tabs = new UITabBar((UIControllerWithSmartObjects) parentViewController.UIController,
                Device.SmartObjects[Joins.SmartObjects.CamTrackingProcessorSelectTabs]);
            _processorSelectButtons = new ButtonCollection(tabs);
            _cameraButtons = new ButtonCollection
            {
                {Digitals.CameraUp, new UIButton(this, Digitals.CameraUp)},
                {Digitals.CameraDown, new UIButton(this, Digitals.CameraDown)},
                {Digitals.CameraLeft, new UIButton(this, Digitals.CameraLeft)},
                {Digitals.CameraRight, new UIButton(this, Digitals.CameraRight)},
                {Digitals.CameraZoomIn, new UIButton(this, Digitals.CameraZoomIn)},
                {Digitals.CameraZoomOut, new UIButton(this, Digitals.CameraZoomOut)},
                {Digitals.CameraTrackingOff, new UIButton(this, Digitals.CameraTrackingOff)},
                {Digitals.CameraTrackingOn, new UIButton(this, Digitals.CameraTrackingOn)},
                {Digitals.CameraRecordBtn, new UIButton(this, Digitals.CameraRecordBtn)},
                {Digitals.CameraRecordStopBtn, new UIButton(this, Digitals.CameraRecordStopBtn)},
                {Digitals.CameraStreamBtn, new UIButton(this, Digitals.CameraStreamBtn)},
            };
            _layoutsList = new UIDynamicButtonList(
                (UIControllerWithSmartObjects) parentViewController.UIController,
                Device.SmartObjects[Joins.SmartObjects.CameraLayoutsList]);
            _layoutsListButtons = new ButtonCollection(_layoutsList);
            _roomConfigsList = new UIDynamicButtonList(
                (UIControllerWithSmartObjects)parentViewController.UIController,
                Device.SmartObjects[Joins.SmartObjects.CameraRoomConfigsList]);
            _roomConfigsListButtons = new ButtonCollection(_roomConfigsList);
            _cameraSelectList = new UIDynamicButtonList(
                (UIControllerWithSmartObjects)parentViewController.UIController,
                Device.SmartObjects[Joins.SmartObjects.CameraSelectList]);
            _cameraSelectListButtons = new ButtonCollection(_cameraSelectList);
            _autoSwitchEnableButton = new UIButton(this, Digitals.CameraAutoTracking);
        }

        private ReadOnlyDictionary<uint, OneBeyond> Processors
        {
            get { return ((ASystem) UIController.System).OneBeyondControllers; }
        }

        public OneBeyond SelectedProcessor
        {
            get { return _selectedProcessor; }
            set
            {
                if (_selectedProcessor == value)
                {
                    if (_selectedProcessor != null)
                    {
                        new Thread(specific => UpdateFeedback(specific as OneBeyond), _selectedProcessor);
                    }
                    return;
                }

                if (_selectedProcessor != null)
                {
                    _selectedProcessor.UpdateReceived -= SelectedProcessorOnUpdateReceived;
                }

                _selectedProcessor = value;

                if (_selectedProcessor == null)
                {
                    _processorSelectButtons.ClearInterlockedFeedback();
                    
                    // clear UI
                    
                    return;
                }

                _selectedProcessor.UpdateReceived += SelectedProcessorOnUpdateReceived;

                var key = Processors.First(kvp => kvp.Value == value).Key;

                if (_processorSelectButtons.ContainsKey(key))
                {
                    _processorSelectButtons.SetInterlockedFeedback(key);
                }

                new Thread(specific => UpdateFeedback(specific as OneBeyond), _selectedProcessor);
            }
        }

        private void SelectedProcessorOnUpdateReceived(OneBeyond device, UpdateFeedbackEventArgs args)
        {
            switch (args.Type)
            {
                case UpdateType.LayoutsUpdated:
                    _layoutsList.ClearList();
                    foreach (var item in ((Dictionary<ushort, string>) args.Value) )
                    {
                        _layoutsList.AddItem(item.Value, item.Key, true);
                    }
                    _layoutsList.SetListSizeToItemCount();
                    break;
                case UpdateType.CurrentLayoutUpdated:
                    _layoutsList.SetSelectedItem(_layoutsList[(ushort) args.Value]);
                    break;
                case UpdateType.RoomConfigsUpdated:
                    _roomConfigsList.ClearList();
                    foreach (var item in ((Dictionary<ushort, string>)args.Value))
                    {
                        _roomConfigsList.AddItem(item.Value, item.Key, true);
                    }
                    _roomConfigsList.SetListSizeToItemCount();
                    break;
                case UpdateType.CurrentRoomConfigUpdated:
                    _layoutsList.SetSelectedItem(_roomConfigsList[(ushort)args.Value]);
                    var cameras = SelectedProcessor.GetCamerasForConfig((ushort) args.Value);
                    _cameraSelectList.ClearList(true);
                    SelectedCamera = null;
                    foreach (var camera in cameras)
                    {
                        _cameraSelectList.AddItem(camera.Name, camera, true);
                    }
                    _cameraSelectList.SetListSizeToItemCount();
                    break;
                case UpdateType.AutoSwitchStatusUpdated:
                    _autoSwitchEnableButton.Feedback = Convert.ToBoolean(args.Value);
                    break;
                case UpdateType.CameraSelectUpdated:
                    _cameraSelectList.SetSelectedItem(_cameraSelectList[(ushort) args.Value]);
                    var cam = _cameraSelectList[(ushort) args.Value].LinkedObject as OneBeyondCameraConfig;
                    if (cam != null)
                    {
                        SelectedCamera = ((ASystem) UIController.System).Cameras[cam.Id];
                        Debug.WriteInfo("Selected camera", "{0} {1}", cam.Name, cam.IpAddress);
                    }
                    break;
                case UpdateType.RecordStatusUpdated:
                    _cameraButtons[Digitals.CameraRecordBtn].Feedback = Convert.ToBoolean(args.Value);
                    break;
                case UpdateType.StreamStatusUpdated:
                    _cameraButtons[Digitals.CameraStreamBtn].Feedback = Convert.ToBoolean(args.Value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ICamera SelectedCamera { get; set; }

        private object UpdateFeedback(OneBeyond selectedProcessor)
        {
            if(selectedProcessor == null) return null;

            try
            {
                selectedProcessor.GetRoomConfigs();
                selectedProcessor.GetCurrentRoomConfig();
                selectedProcessor.GetAutoSwitchStatus();
                selectedProcessor.GetLayouts();
                selectedProcessor.GetCurrentLayout();
                selectedProcessor.GetAutoSwitchStatus();
                selectedProcessor.GetCameraStatus();
                selectedProcessor.GetRecordStatus();
                selectedProcessor.GetRecordingSpace();
                selectedProcessor.GetStreamStatus();
                selectedProcessor.GetIsoRecordStatus();
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }

            return null;
        }

        protected override void WillShow()
        {
            base.WillShow();
            _processorSelectButtons.ButtonEvent += ProcessorSelectButtonsOnButtonEvent;
            _cameraButtons.ButtonEvent += CameraButtonsOnButtonEvent;
            _cameraSelectListButtons.ButtonEvent += CameraSelectListButtonsOnButtonEvent;
            _roomConfigsListButtons.ButtonEvent += RoomConfigsListButtonsOnButtonEvent;
            _layoutsListButtons.ButtonEvent += LayoutsListButtonsOnButtonEvent;
            _autoSwitchEnableButton.ButtonEvent += AutoSwitchEnableButtonOnButtonEvent;

            if (SelectedProcessor != null)
            {
                new Thread(specific => UpdateFeedback(specific as OneBeyond), _selectedProcessor);
                return;
            }

            var first = Processors.Values.FirstOrDefault();
            if (first != null)
            {
                SelectedProcessor = first;
            }
        }

        protected override void WillHide()
        {
            base.WillHide();
            _processorSelectButtons.ButtonEvent -= ProcessorSelectButtonsOnButtonEvent;
            _cameraButtons.ButtonEvent -= CameraButtonsOnButtonEvent;
            _cameraSelectListButtons.ButtonEvent -= CameraSelectListButtonsOnButtonEvent;
            _roomConfigsListButtons.ButtonEvent -= RoomConfigsListButtonsOnButtonEvent;
            _layoutsListButtons.ButtonEvent -= LayoutsListButtonsOnButtonEvent;
            _autoSwitchEnableButton.ButtonEvent -= AutoSwitchEnableButtonOnButtonEvent;
        }

        private void ProcessorSelectButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;
            if (_processorSelectButtons.ContainsKey(args.CollectionKey))
            {
                SelectedProcessor = Processors[args.CollectionKey];
            }
        }

        private void LayoutsListButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;
            SelectedProcessor.ChangeLayout((ushort) args.CollectionKey);
        }

        private void RoomConfigsListButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;
            var thread = new Thread(specific =>
            {
                SelectedProcessor.ChangeRoomConfig((ushort)specific);
                UpdateFeedback(SelectedProcessor);
                return null;
            }, args.CollectionKey);
        }

        private void CameraSelectListButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;
            SelectedProcessor.ManualSwitchCamera((ushort) args.CollectionKey);
        }

        private void AutoSwitchEnableButtonOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;
            SelectedProcessor.ToggleAutoSwitch();
        }

        private void CameraButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (SelectedCamera == null) return;
            switch (args.EventType)
            {
                case ButtonEventType.Pressed:
                    switch (args.CollectionKey)
                    {
                        case Digitals.CameraUp:
                            SelectedCamera.TiltUp();
                            break;
                        case Digitals.CameraDown:
                            SelectedCamera.TiltDown();
                            break;
                        case Digitals.CameraLeft:
                            SelectedCamera.PanLeft();
                            break;
                        case Digitals.CameraRight:
                            SelectedCamera.PanRight();
                            break;
                        case Digitals.CameraZoomIn:
                            SelectedCamera.ZoomIn();
                            break;
                        case Digitals.CameraZoomOut:
                            SelectedCamera.ZoomOut();
                            break;
                    }
                    break;
                case ButtonEventType.Tapped:
                    break;
                case ButtonEventType.Held:
                    break;
                case ButtonEventType.Released:
                    switch (args.CollectionKey)
                    {
                        case Digitals.CameraUp:
                        case Digitals.CameraDown:
                            SelectedCamera.TiltStop();
                            break;
                        case Digitals.CameraLeft:
                        case Digitals.CameraRight:
                            SelectedCamera.PanStop();
                            break;
                        case Digitals.CameraZoomIn:
                        case Digitals.CameraZoomOut:
                            SelectedCamera.ZoomStop();
                            break;
                        case Digitals.CameraTrackingOff:
                            ((ViscaCamera) SelectedCamera).RecallPreset(81);
                            break;
                        case Digitals.CameraTrackingOn:
                            ((ViscaCamera)SelectedCamera).RecallPreset(80);
                            break;
                        case Digitals.CameraRecordBtn:
                            SelectedProcessor.StartRecord();
                            break;
                        case Digitals.CameraRecordStopBtn:
                            SelectedProcessor.StopRecord();
                            break;
                        case Digitals.CameraStreamBtn:
                            SelectedProcessor.ToggleStream();
                            break;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}