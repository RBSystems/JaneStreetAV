using System.Linq;
using JaneStreetAV.UI.Joins;
using JaneStreetAV.UI.SmartObjects;
using UX.Lib2.Devices.Cisco.Conference;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.VC
{
    public class VCMainMenuViewController : VCSubViewControllerBase
    {
        private readonly CiscoLargeIconList _list;
        private readonly ButtonCollection _buttons;

        public VCMainMenuViewController(UIViewController parentViewController)
            : base(parentViewController, Digitals.SubPageVCMenu)
        {
            var ui = (UIControllerWithSmartObjects) parentViewController.UIController;
            _list = new CiscoLargeIconList(ui, ui.Device.SmartObjects[Joins.SmartObjects.VCMainMenuList]);
            _buttons = new ButtonCollection(_list);
            _list.AddItem(CiscoLargeIconListItemType.Call, "Call", Parent.DialView);
            _list.AddItem(CiscoLargeIconListItemType.Share, "Share", Parent.ShareView);
        }

        protected override void WillShow()
        {
            base.WillShow();
            _buttons.ButtonEvent += ButtonsOnButtonEvent;
        }

        protected override void WillHide()
        {
            base.WillHide();
            _buttons.ButtonEvent -= ButtonsOnButtonEvent;
        }

        private void ButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;

            if (PageNumber == Digitals.PageTechControl &&
                ((CiscoLargeIconListItem) _list[args.CollectionKey]).IconType == CiscoLargeIconListItemType.Share)
            {
                var sending = Codec.Conference.Presentation.LocalInstance.Count > 0 &&
                              Codec.Conference.Presentation.LocalInstance.First().Value.Mode != LocalSendingMode.Off;
                if (sending)
                {
                    Codec.Conference.Presentation.Stop();
                }
                else
                {
                    Codec.Conference.Presentation.Start(SendingMode.LocalRemote);
                }
                return;
            }

            var view = _list[args.CollectionKey].LinkedObject as IVisibleItem;
            
            if(view != null) view.Show();
        }
    }
}