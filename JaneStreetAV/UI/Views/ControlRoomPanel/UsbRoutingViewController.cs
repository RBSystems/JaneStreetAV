using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DM;
using JaneStreetAV.Devices;
using JaneStreetAV.Models;
using JaneStreetAV.UI.Joins;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.ControlRoomPanel
{
    public class UsbRoutingViewController : ASubPage
    {
        private readonly Dictionary<uint, UIDynamicButtonList> _lists;
        private readonly Dictionary<uint, ButtonCollection> _buttons;
        private readonly Dictionary<uint, uint> _pcInputs;

        public UsbRoutingViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageTechUsb, string.Empty, TimeSpan.Zero)
        {
            try
            {
                var ui = parentViewController.UIController as UIControllerWithSmartObjects;
                _lists = new Dictionary<uint, UIDynamicButtonList>();
                _buttons = new Dictionary<uint, ButtonCollection>();
                _pcInputs = new Dictionary<uint, uint>
                {
                    {1, 11},
                    {2, 12},
                    {3, 13},
                    {4, 14},
                };
                for (uint i = 0; i < 4; i++)
                {
                    var list = new UIDynamicButtonList(ui,
                        ui.Device.SmartObjects[Joins.SmartObjects.UsbRoutingList1 + i]);
                    _lists[i + 1] = list;
                    _buttons[i + 1] = new ButtonCollection(list);

                    for (uint j = 1; j <= 3; j++)
                    {
                        switch (j)
                        {
                            case 1:
                                list.AddItem("Aud 1 Lectern", new[] {1U, 2U});
                                break;
                            case 2:
                                list.AddItem("Aud 2 Lectern", new[] {3U, 4U});
                                break;
                            case 3:
                                list.AddItem("Control Room", new[] {10U});
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CloudLog.Error("Error loading {0}, {1}", GetType().Name, e.Message);
            }
        }

        protected override void WillShow()
        {
            base.WillShow();

            foreach (var buttonCollection in _buttons.Values)
            {
                buttonCollection.ButtonEvent += OnButtonEvent;
            }

            var switcher = ((ASystem)UIController.System).Switcher as DmSwitcherBase;

            switcher.Chassis.DMInputChange += ChassisOnDMInputChange;

            UpdateFeedback();
        }

        protected override void WillHide()
        {
            base.WillHide();

            foreach (var buttonCollection in _buttons.Values)
            {
                buttonCollection.ButtonEvent -= OnButtonEvent;
            }

            var switcher = ((ASystem)UIController.System).Switcher as DmSwitcherBase;

            switcher.Chassis.DMInputChange -= ChassisOnDMInputChange;
        }

        private void ChassisOnDMInputChange(Switch device, DMInputEventArgs args)
        {
            if (args.EventId == DMInputEventIds.UsbRoutedToEventId)
            {
                UpdateFeedback();
            }
        }

        private void OnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;

            var listIndex = _buttons.First(kvp => kvp.Value == args.Collection).Key;
            var routes = (uint[])_lists[listIndex][args.CollectionKey].LinkedObject;

            var switcher = ((ASystem)UIController.System).Switcher as DmSwitcherBase;

            if (switcher == null) return;
            var pcInput = switcher.Chassis.Inputs[_pcInputs[listIndex]];
            foreach (var dmRoute in routes.Select(route => switcher.Chassis.Inputs[route]))
            {
                switcher.RouteUsb(dmRoute, pcInput);
            }
        }

        public void UpdateFeedback()
        {
            var switcher = ((ASystem)UIController.System).Switcher as DmSwitcherBase;

            foreach (var list in _lists)
            {
                if (switcher == null) continue;
                var pcInput = switcher.Chassis.Inputs[_pcInputs[list.Key]];
                foreach (var item in list.Value.Where(i => i.LinkedObject != null))
                {
                    var routeValues = (uint[]) item.LinkedObject;
                    foreach (var route in routeValues.Select(routeValue => switcher.GetUsbRoute(switcher.Chassis.Inputs[routeValue])))
                    {
                        item.Feedback = route == pcInput;
                        if (route == pcInput)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}