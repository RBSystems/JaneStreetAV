using System;
using System.Collections.Generic;
using System.Linq;
using JaneStreetAV.Devices.Sources;
using JaneStreetAV.Models;
using JaneStreetAV.Models.Config;
using JaneStreetAV.UI.Joins;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Devices.TriplePlay;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.ControlRoomPanel
{
    public class TVControlViewController : ASubPage
    {
        private readonly UIDynamicButtonList _channelList;
        private readonly ButtonCollection _channelButtons;
        private readonly ButtonCollection _tabs;
        private uint _currentId = 0;

        private readonly Dictionary<uint, uint> _receiverIds = new Dictionary<uint, uint>
        {
            {1, 7},
            {2, 8},
            {3, 5},
        };

        public TVControlViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageTechTV, string.Empty, TimeSpan.Zero)
        {
            var ui = parentViewController.UIController as UIControllerWithSmartObjects;
            var tabBar = new UITabBar(ui, ui.Device.SmartObjects[Joins.SmartObjects.TVReceiverSelectTabs]);
            _tabs = new ButtonCollection(tabBar);
            _channelList = new UIDynamicButtonList(ui, ui.Device.SmartObjects[Joins.SmartObjects.TVChannelList]);
            _channelButtons = new ButtonCollection(_channelList);
            if (parentViewController.UIController.System is AuditoriumSystem)
            {
                _receiverIds = new Dictionary<uint, uint>
                {
                    {1, 7},
                    {2, 8},
                    {3, 5},
                };
            }
            else if (parentViewController.UIController.System is ClassroomSystem)
            {
                _receiverIds = new Dictionary<uint, uint>
                {
                    {1, 11}
                };
            }
            else
            {
                _receiverIds = new Dictionary<uint, uint>();
                var count = 0U;
                foreach (var source in parentViewController.UIController.System.Sources.Where(s => s is TVSource).Cast<TVSource>())
                {
                    count ++;
                    _receiverIds[count] = source.ClientId;
                }
            }
        }

        protected override void WillShow()
        {
            base.WillShow();

            _tabs.ButtonEvent += TabsOnButtonEvent;
            _channelButtons.ButtonEvent += ChannelButtonsOnButtonEvent;

            if (_currentId == 0)
            {
                _tabs.SetInterlockedFeedback(1);
                _currentId = _receiverIds[1];
            }

            Device.BooleanInput[Digitals.IptvTabsVisible].BoolValue = _receiverIds.Count > 1;

            TripleCare.GetChannels(ConfigManager.Config.TriplePlayServerAddress, -1, (success, channels) =>
            {
                if (success && channels != null)
                {
                    var channelOrder = channels
                        .OrderBy(c => c.Number)
                        .Take((int) _channelList.MaxNumberOfItems)
                        .ToArray();

                    CloudLog.Debug("Received {0} channels from server", channelOrder.Count());
                    try
                    {
                        _channelList.ClearList(true);
                        foreach (var channel in channelOrder)
                        {
                            _channelList.AddItem(channel.Name, channel, true);
                        }
                        _channelList.SetListSizeToItemCount();
                    }
                    catch (Exception e)
                    {
                        CloudLog.Exception(e, "Error loading channels into list");
                    }
                }
                else
                {
                    CloudLog.Warn("Could not load channel list from server");
                }
            });
        }

        protected override void WillHide()
        {
            base.WillHide();

            _tabs.ButtonEvent -= TabsOnButtonEvent;
            _channelButtons.ButtonEvent -= ChannelButtonsOnButtonEvent;
        }

        private void TabsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;

            _currentId = _receiverIds[args.CollectionKey];
            _tabs.SetInterlockedFeedback(args.CollectionKey);
        }

        private void ChannelButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Pressed) return;

            try
            {
                var channel = _channelList[args.CollectionKey].LinkedObject as Channel;
                TripleCare.SetChannel(ConfigManager.Config.TriplePlayServerAddress, _currentId, channel.Number);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Error trying to set channel");
            }
        }
    }
}