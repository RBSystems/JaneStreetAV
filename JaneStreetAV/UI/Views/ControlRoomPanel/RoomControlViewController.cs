using System;
using System.Collections.Generic;
using System.Linq;
using JaneStreetAV.Models;
using JaneStreetAV.UI.Joins;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.ControlRoomPanel
{
    public class RoomControlViewController : ASubPage
    {
        private readonly Dictionary<uint, ButtonCollection> _lightsTabButtons;
        private readonly Dictionary<uint, ButtonCollection> _blindsTabButtons;

        public RoomControlViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageTechLighting, string.Empty, TimeSpan.Zero)
        {
            _lightsTabButtons = new Dictionary<uint, ButtonCollection>
            {
                {
                    1,
                    new ButtonCollection(new UITabBar((UIControllerWithSmartObjects) parentViewController.UIController,
                        Device.SmartObjects[Joins.SmartObjects.Aud1LightsTabButtons]))
                },
                {
                    2,
                    new ButtonCollection(new UITabBar((UIControllerWithSmartObjects) parentViewController.UIController,
                        Device.SmartObjects[Joins.SmartObjects.Aud2LightsTabButtons]))
                },
                {
                    3,
                    new ButtonCollection(new UITabBar((UIControllerWithSmartObjects) parentViewController.UIController,
                        Device.SmartObjects[Joins.SmartObjects.MultiLightsTabButtons]))
                }
            };
            _blindsTabButtons = new Dictionary<uint, ButtonCollection>
            {
                {
                    1,
                    new ButtonCollection(new UITabBar((UIControllerWithSmartObjects) parentViewController.UIController,
                        Device.SmartObjects[Joins.SmartObjects.Aud1SolarBlindsTabButtons]))
                },
                {
                    2,
                    new ButtonCollection(new UITabBar((UIControllerWithSmartObjects) parentViewController.UIController,
                        Device.SmartObjects[Joins.SmartObjects.Aud1BlackoutBlindsTabButtons]))
                },
                {
                    3,
                    new ButtonCollection(new UITabBar((UIControllerWithSmartObjects) parentViewController.UIController,
                        Device.SmartObjects[Joins.SmartObjects.Aud2SolarBlindsTabButtons]))
                },
                {
                    4,
                    new ButtonCollection(new UITabBar((UIControllerWithSmartObjects) parentViewController.UIController,
                        Device.SmartObjects[Joins.SmartObjects.Aud2BlackoutBlindsTabButtons]))
                },
                {
                    5,
                    new ButtonCollection(new UITabBar((UIControllerWithSmartObjects) parentViewController.UIController,
                        Device.SmartObjects[Joins.SmartObjects.MultiSolarBlindsTabButtons]))
                },
                {
                    6,
                    new ButtonCollection(new UITabBar((UIControllerWithSmartObjects) parentViewController.UIController,
                        Device.SmartObjects[Joins.SmartObjects.MultiBlackoutBlindsTabButtons]))
                }
            };
        }

        protected override void WillShow()
        {
            base.WillShow();
            foreach (var blindsTabButton in _blindsTabButtons)
            {
                blindsTabButton.Value.ButtonEvent += BlindsOnButtonEvent;
            }
            foreach (var lightsTabButton in _lightsTabButtons)
            {
                lightsTabButton.Value.ButtonEvent += LightsOnButtonEvent;
            }
        }

        protected override void WillHide()
        {
            base.WillHide();
            foreach (var blindsTabButton in _blindsTabButtons)
            {
                blindsTabButton.Value.ButtonEvent -= BlindsOnButtonEvent;
            }
            foreach (var lightsTabButton in _lightsTabButtons)
            {
                lightsTabButton.Value.ButtonEvent -= LightsOnButtonEvent;
            }
        }

        private void LightsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;
            var roomIndex = _lightsTabButtons.First(kvp => kvp.Value == args.Collection).Key;
            var room = UIController.System.Rooms[roomIndex] as ARoom;
            try
            {
                room.RecallLightingScene(args.CollectionKey);
            }
            catch (Exception e)
            {
                CloudLog.Warn("Could not control lights, {0}", e.Message);
            }
        }

        private void BlindsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;
            var key = _blindsTabButtons.First(kvp => kvp.Value == args.Collection).Key;
            var roomIndex = (key/2) + (key%2);
            var room = UIController.System.Rooms[roomIndex] as ARoom;
            var channel = key - ((roomIndex - 1)*2);
            try
            {
                room.Blinds(channel, (BlindsCommand) args.CollectionKey);
            }
            catch (Exception e)
            {
                CloudLog.Warn("Could not control blinds, {0}", e.Message);
            }
        }
    }
}