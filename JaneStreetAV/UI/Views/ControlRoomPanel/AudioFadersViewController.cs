using System;
using System.Linq;
using JaneStreetAV.Models;
using JaneStreetAV.Models.Config;
using JaneStreetAV.UI.Joins;
using JaneStreetAV.UI.SmartObjects;
using UX.Lib2;
using UX.Lib2.Devices.Audio.QSC.Components;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.ControlRoomPanel
{
    public class AudioFadersViewController : ASubPage
    {
        private readonly AudioFaderList _faders;
        private readonly ButtonCollection _tabButtonsGroups;
        private readonly ButtonCollection _tabButtonsFilter;
        private uint _group;
        private string _filterString;

        public AudioFadersViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageTechAudioFaders, string.Empty, TimeSpan.Zero)
        {
            var ui = parentViewController.UIController as UIControllerWithSmartObjects;
            _faders = new AudioFaderList(ui, ui.Device.SmartObjects[Joins.SmartObjects.AudioFaderList]);
            var tabs = new UITabBar(ui, ui.Device.SmartObjects[Joins.SmartObjects.AudioFaderListTabs]);
            _tabButtonsGroups = new ButtonCollection(tabs);
            tabs = new UITabBar(ui, ui.Device.SmartObjects[Joins.SmartObjects.AudioFaderListTabsFilter]);
            _tabButtonsFilter = new ButtonCollection(tabs);
        }

        protected override void WillShow()
        {
            base.WillShow();
            _tabButtonsGroups.ButtonEvent += TabButtonsGroupsOnButtonGroupsEvent;
            _tabButtonsFilter.ButtonEvent += TabButtonsFilterOnButtonEvent;
            Device.BooleanInput[Digitals.AudioFilterTabsVisible].BoolValue = UIController.System is AuditoriumSystem;
            if (_faders.NumberOfItems == 0)
            {
                InitFaders(1);
            }
        }

        protected override void WillHide()
        {
            base.WillHide();
            _tabButtonsGroups.ButtonEvent -= TabButtonsGroupsOnButtonGroupsEvent;
            _tabButtonsFilter.ButtonEvent -= TabButtonsFilterOnButtonEvent;
        }

        public static FaderItemColor TryGetColor(string colorInfo)
        {
            try
            {
                var colors = ConfigManager.Config.DspFaderColors;
                var color = colors.FirstOrDefault(c => c.Name == colorInfo);
                if (color != null)
                {
                    return color;
                }
                return colors.FirstOrDefault(c => c.ColorHex.ToLower() == colorInfo.ToLower());
            }
            catch
            {
                return null;
            }
        }

        private void InitFaders(uint group)
        {
            var dsp = ((ASystem) UIController.System).Dsp;
            _faders.ClearList(false);

            _tabButtonsGroups.SetInterlockedFeedback(group);
            _group = group;

            var items = ConfigManager.Config.DspFaderComponents
                .Where(f => f.Group == group || group == 0)
                .ToDictionary(conf => conf, conf => TryGetColor(conf.Color));
            var confList = items
                .Where(item => item.Value != null)
                .OrderByDescending(item => item.Value.Priority)
                .ThenBy(item => item.Key.Name)
                .Select(item => item.Key)
                .ToList();
            confList.AddRange(items.Where(item => item.Value == null).Select(item => item.Key));

            foreach (var conf in confList)
            {
                if (string.IsNullOrEmpty(conf.ComponentName) || !dsp.ContainsComponentWithName(conf.ComponentName))
                    continue;
                var gain = dsp[conf.ComponentName] as Gain;
                if (gain == null) continue;
                var item = (AudioFaderListItem) _faders[_faders.AddItem(gain)];
                if (!string.IsNullOrEmpty(conf.Name))
                {
                    item.Name = conf.Name;
                }
                var color = TryGetColor(conf.Color);
                if (color != null)
                {
                    item.SetColour(color.ColorHex);
                }
                if (dsp.ContainsComponentWithName("monitors.mixer") && conf.MixerIndex > 0)
                {
                    var cueMixer = (Mixer) dsp["monitors.mixer"];
                    item.CueMixerItem = cueMixer.Inputs[conf.MixerIndex];
                }
                else
                {
                    item.CueMixerItem = null;
                }
            }

            if (string.IsNullOrEmpty(_filterString))
            {
                _faders.SetListSizeToItemCount();
                return;
            }

            string[] names;

            if (_filterString == "*")
            {
                var otherNames =
                    ConfigManager.Config.DspFaderFilters.Where(f => f.FilterString != _filterString)
                        .Select(f => f.FilterString).ToArray();
                names = (from fader in _faders
                             where otherNames.All(name => !fader.Name.Contains(name))
                             select fader.Name).ToArray();
            }
            else
            {
                names = (from fader in _faders
                    where fader.Name.Contains(_filterString)
                    select fader.Name).ToArray();
            }

            Debug.WriteInfo("Filtered", string.Join(", ", names));

            foreach (var fader in _faders)
            {
                if (!names.Contains(fader.Name))
                {
                    fader.Hide();
                }
                else
                {
                    fader.Show();
                }
            }

            _faders.SetListSizeToItemCount();
        }

        private void TabButtonsGroupsOnButtonGroupsEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;
            InitFaders(args.CollectionKey);
        }

        private void TabButtonsFilterOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;

            if (button.Feedback)
            {
                _filterString = string.Empty;
                button.Feedback = false;
                InitFaders(_group);
                return;
            }

            var filter = ConfigManager.Config.DspFaderFilters.FirstOrDefault(f => f.FilterButton == args.CollectionKey);
            if (filter == null) return;

            _filterString = filter.FilterString;
            _tabButtonsFilter.SetInterlockedFeedback(args.CollectionKey);
            InitFaders(_group);
        }
    }
}