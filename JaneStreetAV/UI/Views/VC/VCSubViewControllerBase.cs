using System;
using UX.Lib2.Devices.Cisco;
using UX.Lib2.UI;

namespace JaneStreetAV.UI.Views.VC
{
    public abstract class VCSubViewControllerBase : ASubPage
    {
        protected VCSubViewControllerBase(UIViewController parentViewController, uint joinNumber)
            : base(parentViewController, joinNumber, string.Empty, TimeSpan.Zero)
        {
        }

        public new VCViewController Parent
        {
            get { return base.Parent as VCViewController; }
        }

        protected CiscoTelePresenceCodec Codec
        {
            get { return Parent.Codec; }
        }

        public uint PageNumber
        {
            get { return Parent.Parent.VisibleJoin.Number; }
        }
    }
}