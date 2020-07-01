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
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using JaneStreetAV.Models;
using JaneStreetAV.Models.Config;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Logging;

namespace JaneStreetAV
{
    public class ControlSystem : CrestronControlSystem
    {
        private readonly ASystem _system;

        public ControlSystem()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 100;
                var assembly = Assembly.GetExecutingAssembly();
                Tools.PrintLibInfo(assembly);

                #region Logging
#if DEBUG
                CloudLog.SystemLogLevel = LoggingLevel.Ok;
                CloudLog.Level = LoggingLevel.Ok;
#else
                CloudLog.SystemLogLevel = LoggingLevel.Info;
                CloudLog.Level = LoggingLevel.Info;
#endif
                CloudLog.RegisterConsoleService();

                #endregion

                #region System Config

                var config = ConfigManager.Config;

                if (config.SystemType == SystemType.NotConfigured)
                {
                    CloudLog.Warn("Config cannot generate automatically based on IP, please setup config manually!");
                }

                #endregion

                #region Load System

                switch (ConfigManager.Config.SystemType)
                {
                    case SystemType.Classroom:
                        _system = new ClassroomSystem(this);
                        break;
                    case SystemType.Auditorium:
                        _system = new AuditoriumSystem(this);
                        break;
                    case SystemType.RoomMonitoring:
                        _system = new RoomMonitoringSystem(this);
                        break;
                    case SystemType.FireAlarm:
                        _system = new FireAlarmMonitorSystem(this);
                        break;
                   default:
                        _system = new MeetingSpaceSystem(this);
                        break;
                }

                #endregion
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Error in ControlSystem.ctor()");
            }
        }

        public override void InitializeSystem()
        {
            try
            {
                _system.Initialize();
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Error in ControlSystem.InitializeSystem()");
            }
        }
    }
}