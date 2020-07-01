using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.DM;
using JaneStreetAV.Devices;
using JaneStreetAV.Models;
using JaneStreetAV.Models.Config;
using JaneStreetAV.UI.Joins;
using JaneStreetAV.UI.SmartObjects;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.ControlRoomPanel
{
    public class DMRoutingViewController : ASubPage
    {
        private readonly RoutingList _inputList;
        private readonly RoutingList _outputList;
        private readonly ButtonCollection _inputListButtons;
        private readonly ButtonCollection _outputListButtons;
        private readonly UIButton _takeButton;
        private readonly UIButton _cancelButton;
        private readonly DmSwitcherBase _switcher;
        private bool _programStopping;
        private Thread _updateFeedbackThread;
        private readonly CEvent _updateFeedbackEvent = new CEvent();
        private DMInput _selectedInput;
        private List<DMOutput> _routedOutputsForSelectedInput;
        private readonly Dictionary<DMOutput, DMInput> _outputsToChange = new Dictionary<DMOutput, DMInput>();
        private SwitcherConfig _config;

        public DMRoutingViewController(UIViewController parentViewController, uint joinNumber, string title)
            : base(parentViewController, joinNumber, title, TimeSpan.Zero)
        {
            CrestronEnvironment.ProgramStatusEventHandler += CrestronEnvironmentOnProgramStatusEventHandler;
            var uiController = parentViewController.UIController as UIControllerWithSmartObjects;
            if (uiController == null)
            {
                CloudLog.Error("Could not load smartobject for {0}", GetType().Name);
                return;
            }

            _inputList = new RoutingList(uiController,
                uiController.Device.SmartObjects[Joins.SmartObjects.DmRoutingInputsList]);
            _outputList = new RoutingList(uiController,
                uiController.Device.SmartObjects[Joins.SmartObjects.DmRoutingOutputsList]);
            _inputListButtons = new ButtonCollection(_inputList);
            _outputListButtons = new ButtonCollection(_outputList);

            _takeButton = new UIButton(this, Digitals.DmRoutingTakeButton)
            {
                EnableJoin = Device.BooleanInput[Digitals.DmRoutingTakeButtonEnable]
            };
            _cancelButton = new UIButton(this, Digitals.DmRoutingCancelButton)
            {
                EnableJoin = Device.BooleanInput[Digitals.DmRoutingCancelButtonEnable]
            };

            _switcher = ((ASystem)UIController.System).Switcher as DmSwitcherBase;

            if (_switcher == null)
            {
                CloudLog.Error("Invalid switcher type for {0}", GetType().Name);
                return;
            }

            var config = ConfigManager.Config.SwitcherConfig;

            UpdateLists(config);
        }

        private void UpdateLists(SwitcherConfig config)
        {
            if(_config == config) return;

            _config = config;
            
            _inputList.ClearList(true);
            _outputList.ClearList(true);

            var inputs = config.Inputs.ToDictionary(conf => conf, conf => AudioFadersViewController.TryGetColor(conf.Color));
            var ipConfList = inputs
                .Where(item => item.Value != null)
                .OrderByDescending(item => item.Value.Priority)
                .ThenBy(item => item.Key.Name)
                .Select(item => item.Key)
                .ToList();
            ipConfList.AddRange(inputs.Where(item => item.Value == null).Select(item => item.Key));

            foreach (var input in ipConfList)
            {
                var item = _inputList.AddItem(_switcher.Chassis.Inputs[input.Number], true);
                if (item == null) continue;

                try
                {
                    if (!string.IsNullOrEmpty(input.Color))
                    {
                        item.SetColourTag(AudioFadersViewController.TryGetColor(input.Color).ColorHex);
                    }
                }
                catch
                {
                    item.SetColourTag("#333333");
                }
            }

            var outputs = config.Outputs.ToDictionary(conf => conf, conf => AudioFadersViewController.TryGetColor(conf.Color));
            var opConfList = outputs
                .Where(item => item.Value != null)
                .OrderByDescending(item => item.Value.Priority)
                .ThenBy(item => item.Key.Name)
                .Select(item => item.Key)
                .ToList();
            opConfList.AddRange(outputs.Where(item => item.Value == null).Select(item => item.Key));

            foreach (var output in opConfList)
            {
                var item = _outputList.AddItem(_switcher.Chassis.Outputs[output.Number], true);
                if (item == null) continue;

                try
                {
                    if (!string.IsNullOrEmpty(output.Color))
                    {
                        item.SetColourTag(AudioFadersViewController.TryGetColor(output.Color).ColorHex);
                    }
                }
                catch
                {
                    item.SetColourTag("#333333");
                }
            }

            _inputList.SetListSizeToItemCount();
            _outputList.SetListSizeToItemCount();
        }

        private void CrestronEnvironmentOnProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            _programStopping = programEventType == eProgramStatusEventType.Stopping;
            if (_programStopping)
            {
                _updateFeedbackEvent.Set();
            }
        }

        protected override void WillShow()
        {
            base.WillShow();

            _inputListButtons.ButtonEvent += InputListButtonsOnButtonEvent;
            _outputListButtons.ButtonEvent += OutputListButtonsOnButtonEvent;
            _takeButton.ButtonEvent += TakeButtonOnButtonEvent;
            _cancelButton.ButtonEvent += CancelButtonOnButtonEvent;
            _switcher.Chassis.DMInputChange += ChassisOnDmInputChange;
            try
            {
                //UpdateSyncStatusFeedback();
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }

            UpdateLists(ConfigManager.Config.SwitcherConfig);

            if (_updateFeedbackThread != null && _updateFeedbackThread.ThreadState == Thread.eThreadStates.ThreadRunning)
                return;

            _updateFeedbackThread = new Thread(UpdateFeedbackProcess, null)
            {
                Priority = Thread.eThreadPriority.LowestPriority,
                Name = "Routing UI Feedback Process"
            };
        }

        protected override void WillHide()
        {
            base.WillHide();

            _inputListButtons.ButtonEvent -= InputListButtonsOnButtonEvent;
            _outputListButtons.ButtonEvent -= OutputListButtonsOnButtonEvent;
            _takeButton.ButtonEvent -= TakeButtonOnButtonEvent;
            _cancelButton.ButtonEvent -= CancelButtonOnButtonEvent;
            _switcher.Chassis.DMInputChange -= ChassisOnDmInputChange;
        }

        private void InputListButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Pressed) return;

            SelectInput(_inputList[args.CollectionKey].LinkedObject as DMInput);
        }

        private void OutputListButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Pressed) return;

            var output = _outputList[args.CollectionKey].LinkedObject as DMOutput;

            if (output != null && _selectedInput != null)
            {
                if (_outputsToChange.ContainsKey(output) && _routedOutputsForSelectedInput.Contains(output))
                {
                    _outputsToChange.Remove(output);

                }
                else if (_outputsToChange.ContainsKey(output) && _outputsToChange[output] == _selectedInput)
                {
                    _outputsToChange.Remove(output);
                }
                else if (_routedOutputsForSelectedInput.Contains(output))
                {
                    _outputsToChange[output] = null;
                }
                else
                {
                    _outputsToChange[output] = _selectedInput;
                }

                _updateFeedbackEvent.Set();
            }
            else if (output != null && output.VideoOutFeedback != null)
            {
                SelectInput(output.VideoOutFeedback);
            }
        }

        private void CancelButtonOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;

            _outputsToChange.Clear();
            SelectInput(null);
        }

        private void TakeButtonOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;

            foreach (var route in _outputsToChange)
            {
                route.Key.VideoOut = route.Value;
            }
            _switcher.Chassis.VideoEnter.Pulse(1);

            _outputsToChange.Clear();
        }

        private void SelectInput(DMInput input)
        {
            _selectedInput = input;
            _inputList.SetSelectedItem(input);
            _updateFeedbackEvent.Set();
        }

        private void ChassisOnDmInputChange(Switch device, DMInputEventArgs args)
        {
            if(args.EventId != DMInputEventIds.VideoDetectedEventId) return;
            UpdateFeedbackProcess(args.Number);
        }

        private void UpdateSyncStatusFeedback()
        {
            foreach (var input in _switcher.Chassis.Inputs)
            {
                UpdateSyncStatusFeedback(input.Number);
            }
        }

        private void UpdateSyncStatusFeedback(uint inputNumber)
        {
            var item = _inputList.Cast<RoutingListItem>().FirstOrDefault(i => i.Number == inputNumber);
            if (item != null)
            {
                item.Active = _switcher.Chassis.Inputs[inputNumber].VideoDetectedFeedback.BoolValue;
            }
        }

        private class ItemState
        {
            public bool Feedback { get; set; }
            public RoutingItemListColor Color { get; set; }
        }

        private object UpdateFeedbackProcess(object userSpecific)
        {
            CloudLog.Notice("Started " + Thread.CurrentThread.Name);

            var feedback = true;

            var itemStates = new Dictionary<RoutingListItem, ItemState>();

            while (RequestedVisibleState && !_programStopping && _switcher.Chassis.IsOnline)
            {
                if (_selectedInput != null)
                {
                    _routedOutputsForSelectedInput = new List<DMOutput>();
                    foreach (
                        var output in
                            _switcher.Chassis.Outputs.Values.Where(
                                o => o.VideoOutFeedback != null && o.VideoOutFeedback == _selectedInput))
                    {
                        _routedOutputsForSelectedInput.Add(output);
                    }

                    foreach (var item in _outputList.Cast<RoutingListItem>().TakeWhile(item => item.LinkedObject != null))
                    {
                        var output = (DMOutput)item.LinkedObject;
                        var alreadyRouted = _routedOutputsForSelectedInput.Contains(output);
                        var willChange = _outputsToChange.Keys.Contains(output);

                        itemStates[item] = new ItemState {Color = RoutingItemListColor.Blue, Feedback = false};

                        if (alreadyRouted && willChange)
                        {
                            itemStates[item].Color = RoutingItemListColor.Red;
                            itemStates[item].Feedback = feedback;
                        }
                        else if (willChange && _outputsToChange[output] == _selectedInput)
                        {
                            itemStates[item].Color = RoutingItemListColor.Green;
                            itemStates[item].Feedback = feedback;
                        }
                        else if (alreadyRouted)
                        {
                            itemStates[item].Feedback = true;
                        }
                    }
                }
                else
                {
                    foreach (var item in _outputList.Cast<RoutingListItem>().TakeWhile(item => item.LinkedObject != null))
                    {
                        itemStates[item] = new ItemState {Color = RoutingItemListColor.Blue, Feedback = false};
                    }
                }

                foreach (var itemState in itemStates)
                {
                    itemState.Key.Feedback = itemState.Value.Feedback;
                    itemState.Key.Color = itemState.Value.Color;
                }

                _cancelButton.Enabled = _outputsToChange.Count > 0 || _selectedInput != null;
                _takeButton.Enabled = _outputsToChange.Count > 0;

                if (_updateFeedbackEvent.Wait(500))
                {
                    feedback = true;
                }
                else
                {
                    feedback = !feedback;
                }
            }

            if (RequestedVisibleState)
            {
                Debug.WriteInfo("Showing DM offline prompt on " + UIController.ToString());
                ((BaseUIController) UIController).ActionSheetDefault.Show((type, args) => { }, "Switcher Offline",
                    "The DM Switcher is offline", "OK");
            }

            CloudLog.Notice("Exiting " + Thread.CurrentThread.Name);

            return null;
        }
    }
}