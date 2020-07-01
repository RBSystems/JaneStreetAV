using System;
using System.Linq;
using JaneStreetAV.Models;
using JaneStreetAV.Models.Rooms;
using JaneStreetAV.UI.Joins;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.Devices.Cisco.Conference;
using UX.Lib2.Models;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.VC
{
    public class VCShareViewController : VCSubViewControllerBase
    {
        private readonly UIButton _closeBtn;
        private readonly UIDynamicButtonList _list;
        private readonly ButtonCollection _sourceButtons;

        public VCShareViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageVCShare)
        {
            var ui = parentViewController.UIController as UIControllerWithSmartObjects;
            _list = new UIDynamicButtonList(ui, ui.Device.SmartObjects[Joins.SmartObjects.VCShareSourceList]);
            _sourceButtons = new ButtonCollection(_list);
            _closeBtn = new UIButton(this, Digitals.VCDialViewCloseBtn);
        }

        private CiscoTelePresenceCodec Codec
        {
            get { return ((VCRoom) UIController.Room).Codec; }
        }

        private bool CodecPresActive 
        {
            get
            {
                return Codec.Conference.Presentation.LocalInstance.Any() &&
                       Codec.Conference.Presentation.LocalInstance.First().Value.Mode != LocalSendingMode.Off;
            }
        }

        private ASource ActiveSource
        {
            get
            {
                if (!CodecPresActive)
                {
                    return null;
                }

                return UIController.Source as ASource;
            }
        }

        protected override void WillShow()
        {
            base.WillShow();
            _closeBtn.ButtonEvent += CloseBtnOnButtonEvent;
            _sourceButtons.ButtonEvent += SourceButtonsOnButtonEvent;
            
            Codec.Conference.Presentation.StatusChange += PresentationOnStatusChange;

            _list.ClearList(true);
            foreach (var source in UIController.Room.Sources
                .Where(s => s.Type != SourceType.VideoConference)
                .Cast<ASource>())
            {
                var index = _list.AddItem(source.Name, source, true);
                _list[index].IconNumber = (ushort) source.Icon;
            }
            _list.SetListSizeToItemCount();
            UpdateFeedback();
        }

        protected override void WillHide()
        {
            base.WillHide();
            _closeBtn.ButtonEvent -= CloseBtnOnButtonEvent;
            _sourceButtons.ButtonEvent -= SourceButtonsOnButtonEvent;
            Codec.Conference.Presentation.StatusChange -= PresentationOnStatusChange;
        }

        private void PresentationOnStatusChange(CodecApiElement element, string[] propertyNamesWhichUpdated)
        {
            UpdateFeedback();
        }

        private void UpdateFeedback()
        {
            _list.SetSelectedItem(ActiveSource);
        }

        private void CloseBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType == ButtonEventType.Pressed) Parent.ResetView();
        }

        private void SourceButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;

            var source = _list[args.CollectionKey].LinkedObject as ASource;

            if (source == ActiveSource)
            {
                Codec.Conference.Presentation.Stop();
            }
            else if (UIController.Source == source)
            {
                Codec.Conference.Presentation.Start(SendingMode.LocalRemote);
            }
            else
            {
                UIController.Room.Source = source;
                UpdateFeedback();
            }
        }
    }
}