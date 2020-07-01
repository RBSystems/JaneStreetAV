/* License
 * ------------------------------------------------------------------------------
 * Copyright (c) 2017 UX Digital Systems Ltd
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * ------------------------------------------------------------------------------
 * UX.Digital
 * ----------
 * http://ux.digital
 * support@ux.digital
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;
using JaneStreetAV.UI.Joins;
using JaneStreetAV.UI.Views;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Models;
using UX.Lib2.UI;
using ButtonEventArgs = UX.Lib2.UI.ButtonEventArgs;
using IButton = UX.Lib2.UI.IButton;

namespace JaneStreetAV.UI
{
    public abstract class BaseUIController : UIControllerWithSmartObjects
    {
        #region Fields

        private readonly Dictionary<uint, UIActionSheet> _actionSheets = new Dictionary<uint, UIActionSheet>(); 
        private UserPrompt _currentPrompt;
        private readonly UILabel _roomNameLabel;
        private readonly UILabel _roomContactNumberLabel;
        private readonly BootPageViewController _bootPage;
        private readonly ASubPage _restartingView;
        private readonly UILabel _restartingText;
        private CTimer _auWaitTimer;
        private readonly UIButton _homeButton;
        private readonly UIButton _backButton;
        private readonly TextFieldEntryViewController _textEntryView;

        #endregion

        #region Constructors

        protected BaseUIController(SystemBase system, BasicTriListWithSmartObject device, RoomBase room)
            : base(system, device, room)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var streamName =
                    assembly.GetManifestResourceNames()
                        .FirstOrDefault(n => n.Contains(".sgd") && n.Contains(device.Name)) ??
                    assembly.GetManifestResourceNames()
                        .FirstOrDefault(
                            n => n.Contains(".sgd") && n.Contains("XPanel") && device is XpanelForSmartGraphics) ??
                    assembly.GetManifestResourceNames()
                        .FirstOrDefault(
                            n => n.Contains(".sgd") && n.Contains("CrestronApp") && device is CrestronApp) ??
                    assembly.GetManifestResourceNames()
                        .FirstOrDefault(n => n.Contains(".sgd"));
                CloudLog.Info("Loading Smartgraphics for {0} ID {1}, File: {2}", GetType().Name, Id, streamName);
                var stream = assembly.GetManifestResourceStream(streamName);
                LoadSmartObjects(stream);
            }
            catch
            {
                CloudLog.Error("Could not load SGD file for {0}", this);
            }

            _roomNameLabel = new UILabel(this, Serials.RoomName);
            _roomContactNumberLabel = new UILabel(this, Serials.RoomContactNumber);

            _bootPage = new BootPageViewController(this, Digitals.PageBoot, null);

            var tswX60BaseClass = device as TswX60BaseClass;

            if (tswX60BaseClass != null)
            {
                tswX60BaseClass.ExtenderSystem2ReservedSigs.Use();
            }

            _restartingView = new ASubPage(this, Device.BooleanInput[Digitals.SubPageRestarting], string.Empty, TimeSpan.Zero);
            _restartingText = new UILabel(this, Serials.RestartingText)
            {
                Text = "System Restarting"
            };

            _homeButton = new UIButton(this, Digitals.HomeButton);
            _backButton = new UIButton(this, Digitals.BackButton)
            {
                VisibleJoin = Device.BooleanInput[Digitals.BackButtonVisible]
            };
            _backButton.Show();

            _homeButton.ButtonEvent += HomeButtonOnButtonEvent;

            _textEntryView = new TextFieldEntryViewController(this);

            _actionSheets[0] = new ActionSheetDefault(this);
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public ActionSheetDefault ActionSheetDefault
        {
            get { return _actionSheets[0] as ActionSheetDefault; }
        }

        public AutoUpdateReservedSigs AutoUpdate
        {
            get
            {
                var device = Device as Tswx52ButtonVoiceControl;
                return device != null ? device.ExtenderAutoUpdateReservedSigs : null;
            }
        }

        public ASubPage RestartingView
        {
            get { return _restartingView; }
        }

        public UIButton BackButton
        {
            get { return _backButton; }
        }

        public TextFieldEntryViewController TextEntryView
        {
            get { return _textEntryView; }
        }

        #endregion

        #region Methods

        protected override void OnDeviceConnect(int deviceCount)
        {
            base.OnDeviceConnect(deviceCount);

            var tswDevice = Device as Tswx52ButtonVoiceControl;
            if (tswDevice == null) return;
            tswDevice.ExtenderSystemReservedSigs.StandbyTimeout.UShortValue = 60;
            tswDevice.ExtenderSystemReservedSigs.LcdBrightness.UShortValue = ushort.MaxValue;

            var tswX60BaseClass = Device as TswX60BaseClass;

            if (tswX60BaseClass != null)
            {
                tswX60BaseClass.ExtenderHardButtonReservedSigs.DisableBrightnessAutoOn();
                tswX60BaseClass.ExtenderHardButtonReservedSigs.Brightness.UShortValue = ushort.MaxValue;
                tswX60BaseClass.ExtenderSystem2ReservedSigs.LcdLevelHigh.UShortValue = ushort.MaxValue;
                tswX60BaseClass.ExtenderSystem2ReservedSigs.LcdLevelMedium.UShortValue = ushort.MaxValue;
                tswX60BaseClass.ExtenderSystem2ReservedSigs.LcdLevelLow.UShortValue = ushort.MaxValue;
            }
        }

        protected override void UIShouldShowPrompt(UserPrompt prompt)
        {
            _currentPrompt = prompt;

            var actionSheet = _actionSheets[prompt.CustomSubPageJoin];

            _currentPrompt.StateChanged += (userPrompt, state) =>
            {
                if (userPrompt != _currentPrompt) return;

                if (state != PromptState.Shown) actionSheet.Cancel();
            };

            actionSheet.Show(prompt);
        }

        protected override void OnRoomChange(UIController uiController, RoomBase previousRoom, RoomBase newRoom)
        {
            base.OnRoomChange(uiController, previousRoom, newRoom);

            _roomNameLabel.Text = newRoom != null ? newRoom.Name : "";
        }

        protected override void OnRoomDetailsChange(RoomBase room)
        {
            base.OnRoomDetailsChange(room);

            _roomNameLabel.Text = room.Name;
            _roomContactNumberLabel.Text = room.RoomContactNumber;
        }

        protected override void SystemWillRestart(bool upgrading)
        {
            base.SystemWillRestart(upgrading);
            _restartingText.Text = upgrading ? "System Upgrading" : "System Restarting";
            _restartingView.Show();
        }

        private void HomeButtonOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            UIShouldShowHomePage(ShowHomePageEventType.NotDefined);
        }

        protected override void Initialize()
        {
#if DEBUG
            Debug.WriteInfo(GetType().Name + ".Initialize()");
#endif
            var tswDevice = Device as Tswx52ButtonVoiceControl;
            if (tswDevice == null) return;
#if DEBUG
            Debug.WriteInfo("Starting AU Check Timer for " + Device);
#endif
        }

        #endregion
    }
}