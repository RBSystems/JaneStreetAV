using System;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using JaneStreetAV.Devices.Sources;
using JaneStreetAV.Models;
using JaneStreetAV.Models.Rooms;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Devices.Audio.QSC;
using UX.Lib2.Models;
using UX.Lib2.UI;
using ButtonEventArgs = UX.Lib2.UI.ButtonEventArgs;
using IButton = UX.Lib2.UI.IButton;

namespace JaneStreetAV.UI
{
    public class MpcUIController : UIController
    {
        private readonly ButtonCollection _sourceButtons;
        private readonly ButtonCollection _audioButtons;
        private readonly UIButton _powerBtn;
        private readonly MpcVolume _level;
        private readonly ButtonCollection _chanButtons;

        public MpcUIController(SystemBase system, BasicTriList comms, RoomBase defaultRoom)
            : base(system, comms, defaultRoom)
        {
            _sourceButtons = new ButtonCollection
            {
                {1, new UIButton(this, 1)},
                {2, new UIButton(this, 2)},
                {3, new UIButton(this, 4)},
            };

            _chanButtons = new ButtonCollection
            {
                {1, new UIButton(this, 3)},
                {2, new UIButton(this, 6)}
            };

            _sourceButtons.ButtonEvent += SourceButtonsOnButtonEvent;
            _chanButtons.ButtonEvent += ChanButtonsOnButtonEvent;
            _powerBtn = new UIButton(this, 10);
            _powerBtn.ButtonEvent += PowerBtnOnButtonEvent;
            _level = new MpcVolume(this);
            ((ASystem) system).Dsp.HasIntitialized += DspOnHasIntitialized;
        }

        public MpcUIController(SystemBase system, RoomBase defaultRoom)
            : base(system, system.ControlSystem.MPC3x201TouchscreenSlot, defaultRoom)
        {
            var mpc = system.ControlSystem.MPC3x201TouchscreenSlot;
            mpc.Register();

            _sourceButtons = new ButtonCollection
            {
                {1, new UIButton(this, 5)},
                {2, new UIButton(this, 6)}
            };

            _chanButtons = new ButtonCollection
            {
                {1, new UIButton(this, 7)},
                {2, new UIButton(this, 10)}
            };

            _audioButtons = new ButtonCollection
            {
                {1, new UIButton(this, 8)},
                {2, new UIButton(this, 9)}
            };

            _sourceButtons.ButtonEvent += SourceButtonsOnButtonEvent;
            _chanButtons.ButtonEvent += ChanButtonsOnButtonEvent;
            _audioButtons.ButtonEvent += AudioButtonsOnButtonEvent;
            _powerBtn = new UIButton(this, 10);
            _powerBtn.ButtonEvent += PowerBtnOnButtonEvent;
            _level = new MpcVolume(mpc);
            ((ASystem)system).Dsp.HasIntitialized += DspOnHasIntitialized;
        }

        private void DspOnHasIntitialized(QsysCore core)
        {
            if(Room == null) return;
#if DEBUG
            Debug.WriteInfo("MPC DspOnHasIntitialized", "Getting audio level...");
#endif
            _level.VolumeControl = ((ARoom) Room).ProgramVolume;
        }

        private void SourceButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if(args.EventType != ButtonEventType.Released) return;

            try
            {
                if (args.CollectionKey == 3 && Room.OtherRooms.Count > 0 && Room.OtherRooms.First().Source != null)
                {
                    Room.Source = Room.OtherRooms.First().Source;
                    return;
                }

                Room.Source = Room.Sources.ElementAt((int) args.CollectionKey - 1);
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        private void ChanButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            switch (args.EventType)
            {
                case ButtonEventType.Pressed:
                    var tvSource = Room.Source as TVSource;
                    if(tvSource == null) return;
                    button.Feedback = true;
                    switch (args.CollectionKey)
                    {
                        case 1:
                            tvSource.ChannelUp();
                            break;
                        case 2:
                            tvSource.ChannelDown();
                            break;
                    }
                    break;
                case ButtonEventType.Tapped:
                    break;
                case ButtonEventType.Held:
                    break;
                case ButtonEventType.Released:
                    button.Feedback = false;
                    break;
            }
        }

        private void AudioButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;

            var router = ((Gym) Room).AudioRouter;
            if (router != null)
            {
                router.SelectedInput = args.CollectionKey;
                args.Collection.SetInterlockedFeedback(args.CollectionKey);
            }
        }

        private void PowerBtnOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType == ButtonEventType.Released)
            {
                Room.PowerOff(false, PowerOfFEventType.UserRequest);
            }
        }

        protected override void OnRoomChange(UIController uiController, RoomBase previousRoom, RoomBase newRoom)
        {
            base.OnRoomChange(uiController, previousRoom, newRoom);
#if DEBUG
            Debug.WriteInfo("MPC OnRoomChange", "Getting audio level...");
#endif
            var room = newRoom as ARoom;
            if (room == null || room.ProgramVolume == null) return;

            if (_level != null)
            {
                _level.VolumeControl = room.ProgramVolume;
            }
        }

        protected override void OnRoomSourceChange(RoomBase room, RoomSourceChangeEventArgs args)
        {
            base.OnRoomSourceChange(room, args);

            if (args.NewSource == null)
            {
                _sourceButtons.ClearInterlockedFeedback();
                return;
            }

            if (Room.OtherRooms.Count > 0 && Room.OtherRooms.First().Source == args.NewSource)
            {
                _sourceButtons.SetInterlockedFeedback(3);
                return;
            }

            uint sourceIndex = 0;
            foreach (var s in Room.Sources.TakeWhile(s => s != args.NewSource))
            {
                sourceIndex++;
            }
            _sourceButtons.SetInterlockedFeedback(sourceIndex + 1);
        }

        protected override void UIShouldShowSource(SourceBase source)
        {
            
        }

        protected override void UIShouldShowHomePage(ShowHomePageEventType eventType)
        {
            
        }

        protected override void RoomDidPowerOff(PowerOfFEventType eventType)
        {
            
        }

        protected override void UIShouldShowPrompt(UserPrompt prompt)
        {
            
        }

        protected override void Initialize()
        {
            
        }

        protected override void OnSystemStartupProgressChange(SystemBase system, SystemStartupProgressEventArgs args)
        {
            
        }
    }
}