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
using System.Linq;
using Crestron.SimplSharp.Reflection;
using JaneStreetAV.UI;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Models;
using UX.Lib2.WebApp.Templates;
using UX.Lib2.WebScripting2;

namespace JaneStreetAV.WebApp
{
    public class AppDashboardContentHandler : BaseRequestHandler
    {
        #region Fields
        #endregion

        #region Constructors

        public AppDashboardContentHandler(SystemBase system, Request request, bool loginRequired)
            : base(system, request, loginRequired)
        {
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties
        #endregion

        #region Methods

        public void Get()
        {
            if (RequireLogin(false)) return;

            try
            {
                var pageName = Request.PathArguments["page"];
                var assembly = Assembly.GetExecutingAssembly();
                var template = new TemplateEngine(assembly, string.Format("WebApp.Templates.{0}.html", pageName),
                    null, LoggedIn);

                switch (pageName)
                {
                    case "softwareupdates":
                        SoftwareUpdate.SoftwareUpdate.GetAppUpdates();

                        template.Context["update_server_address"] = SoftwareUpdate.SoftwareUpdate.ServerAddress;
                        template.Context["currentVersion"] = Assembly.GetExecutingAssembly().GetName().Version;

                        template.Context["updateAvailable"] = SoftwareUpdate.SoftwareUpdate.UpdateAvailable;
                        template.Context["latestUpdate"] = SoftwareUpdate.SoftwareUpdate.LatestUpdate;
                        template.Context["otherUpdatesAvailable"] =
                            SoftwareUpdate.SoftwareUpdate.Updates.Any(
                                update => update != SoftwareUpdate.SoftwareUpdate.LatestUpdate);
                        template.Context["otherUpdates"] =
                            SoftwareUpdate.SoftwareUpdate.Updates.Where(
                                update => update != SoftwareUpdate.SoftwareUpdate.LatestUpdate);
                        template.Context["rollBacksAvailable"] = SoftwareUpdate.SoftwareUpdate.RollBacks.Any();
                        template.Context["rollBacks"] = SoftwareUpdate.SoftwareUpdate.RollBacks;
                        template.Context["debugBuildAvailable"] = SoftwareUpdate.SoftwareUpdate.DebugBuilds.Any();
                        template.Context["debugBuilds"] = SoftwareUpdate.SoftwareUpdate.DebugBuilds;
                        break;
                    case"xpanel":
                        uint ipId = 0;
                        try
                        {
                            var roomId = uint.Parse(Request.QueryString["room"]);
                            var controllers =
                                System.UIControllers.Where(
                                    ui => ui is RoomUIController && ui.DefaultRoom != null && ui.DefaultRoom.Id == roomId).ToArray();
                            if (controllers.Any())
                            {
                                ipId = controllers.First().Device.ID;
                            }
                        }
                        catch
                        {
                            CloudLog.Error("Error getting ipId for Dashboard XPanel");
                        }

                        template.Context.Add("xpanel_port", 41794);
                        template.Context.Add("xpanel_ipid", ipId.ToString("X2"));
                        break;
                    default:
                        HandleNotFound();
                        return;
                }

                WriteResponse(template.Render(), true);
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        #endregion
    }
}