using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.CrestronThread;
using JaneStreetAV.Models;
using JaneStreetAV.Models.Rooms;
using JaneStreetAV.UI.Joins;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.Devices.Cisco.Video;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.VC
{
    public class VCViewController : ASubPage
    {
        private readonly ButtonCollection _tabs;
        private CiscoTelePresenceCodec _codec;
        private readonly VCMainMenuViewController _mainMenu;
        private readonly VCDialViewController _dialView;
        private readonly VCInCallViewController _callView;
        private readonly VCShareViewController _shareView;
        private readonly List<IVisibleItem> _views = new List<IVisibleItem>();
        private UIButton _codec1DualModeBtn;

        public VCViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageVCBase, string.Empty, TimeSpan.Zero)
        {
            var ui = parentViewController.UIController as UIControllerWithSmartObjects;
            _tabs = new ButtonCollection(new UITabBar(ui, ui.Device.SmartObjects[Joins.SmartObjects.VCSelectCodecTabs]));

            _dialView = new VCDialViewController(this);
            _shareView = new VCShareViewController(this);
            _callView = new VCInCallViewController(this);
            _mainMenu = new VCMainMenuViewController(this);
            _views.Add(_dialView);
            _views.Add(_shareView);
            _views.Add(_callView);
            _views.Add(_mainMenu);
            foreach (var view in _views)
            {
                view.VisibilityChanged += ViewOnVisibilityChanged;
            }
            _codec1DualModeBtn = new UIButton(this, Digitals.Codec1DualModeBtn)
            {
                VisibleJoin = Device.BooleanInput[Digitals.Codec1DualModeBtnVisible]
            };
        }

        protected override void WillShow()
        {
            base.WillShow();
            _tabs.ButtonEvent += TabsOnButtonEvent;
            Device.BooleanInput[Digitals.VCCodecTabsVisible].BoolValue = Parent.VisibleJoin.Number ==
                                                                         Digitals.PageTechControl &&
                                                                         ((ASystem) UIController.System).Codecs.Count >
                                                                         1;

            _codec1DualModeBtn.Visible = Parent.VisibleJoin.Number == Digitals.PageTechControl && Codec != null &&
                                         ((ASystem) UIController.System).Codecs.First(kvp => kvp.Value == _codec).Key ==
                                         1;
            _codec1DualModeBtn.ButtonEvent += Codec1DualModeBtnOnButtonEvent;

            if (Codec == null)
            {
                if (UIController.Room != null && ((VCRoom)UIController.Room).Codec != null)
                {
                    Codec = ((VCRoom) UIController.Room).Codec;
                }
                else
                {
                    Codec = ((ASystem) UIController.System).Codecs[1];
                }
            }
            else
            {
                new Thread(specific =>
                {
                    ((CiscoTelePresenceCodec)specific).StartSession();
                    return null;
                }, Codec);
                
                ResetView();
            }

            UIController.Activity += UIControllerOnActivity;
        }

        protected override void WillHide()
        {
            base.WillHide();
            _tabs.ButtonEvent -= TabsOnButtonEvent;
            UIController.Activity -= UIControllerOnActivity;
            _codec1DualModeBtn.ButtonEvent -= Codec1DualModeBtnOnButtonEvent;
        }

        public CiscoTelePresenceCodec Codec
        {
            get { return _codec; }
            private set
            {
                if(_codec == value) return;

                if (_codec != null)
                {
                    _codec.Calls.CallStatusChange -= CallsOnCallStatusChange;
                }

                foreach (var view in _views)
                {
                    view.Hide();
                }

                _codec = value;

                if (_codec == null) return;

                _codec.Calls.CallStatusChange += CallsOnCallStatusChange;

                new Thread(specific =>
                {
                    ((CiscoTelePresenceCodec) specific).StartSession();
                    return null;
                }, _codec);

                var codecIndex = ((ASystem) UIController.System).Codecs.First(kvp => kvp.Value == _codec).Key;
                _tabs.SetInterlockedFeedback(codecIndex);

                ResetView();

                _codec1DualModeBtn.Visible = Parent.VisibleJoin.Number == Digitals.PageTechControl && Codec != null &&
                                         ((ASystem)UIController.System).Codecs.First(kvp => kvp.Value == _codec).Key ==
                                         1;

                if (_codec1DualModeBtn.Visible && Codec != null)
                {
                    _codec1DualModeBtn.Feedback = Codec.Video.Monitors == VideoMonitors.Dual;
                }
            }
        }

        public VCDialViewController DialView
        {
            get { return _dialView; }
        }

        public VCInCallViewController CallView
        {
            get { return _callView; }
        }

        public VCShareViewController ShareView
        {
            get { return _shareView; }
        }

        private void TabsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Pressed) return;

            Codec = ((ASystem) UIController.System).Codecs[args.CollectionKey];
        }

        private void Codec1DualModeBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            var dualMode = Codec.Video.Monitors == VideoMonitors.Dual;
            if (dualMode)
            {
                button.Feedback = false;
                Codec.Video.MonitorsSet(Video.VideoMonitorsCommand.Single);
            }
            else
            {
                button.Feedback = true;
                Codec.Video.MonitorsSet(Video.VideoMonitorsCommand.Single);
            }
        }

        private void CallsOnCallStatusChange(CiscoTelePresenceCodec codec, CallStatusEventType eventType, Call call)
        {
            if(!RequestedVisibleState) return;
            if (eventType == CallStatusEventType.NewCall && !_callView.Visible)
            {
                _callView.Show(call);
            }
        }

        public void ResetView()
        {
            if (Codec.Calls.ConnectedCount == 0)
            {
                _mainMenu.Show();
            }
            else
            {
                _callView.Show(Codec.Calls.Connected.First());
            }
        }

        private void ViewOnVisibilityChanged(IVisibleItem item, VisibilityChangeEventArgs args)
        {
            if (args.EventType == VisibilityChangeEventType.WillShow)
            {
                foreach (var view in _views.Where(v => v.Visible && v != item))
                {
                    view.Hide();
                }
            }
        }

        private void UIControllerOnActivity(UIController uiController)
        {
            if(Codec == null || Codec.Standby.State == StandbyState.Off) return;

            Codec.Standby.Wake();
        }
    }
}