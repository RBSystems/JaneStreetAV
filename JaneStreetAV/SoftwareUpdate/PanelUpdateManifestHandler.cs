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
using System.Text.RegularExpressions;
using Crestron.SimplSharp.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UX.Lib2.Models;
using UX.Lib2.WebApp;
using UX.Lib2.WebScripting2;

namespace JaneStreetAV.SoftwareUpdate
{
    public class PanelUpdateManifestHandler : ApiHandler
    {
        #region Fields
        #endregion

        #region Constructors

        public PanelUpdateManifestHandler(SystemBase system, Request request, bool loginRequired)
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

        public override void Get()
        {
            try
            {
                var model = Request.QueryString["model"];
                if (string.IsNullOrEmpty(model))
                {
                    HandleError(400, "Bad Request", "No or invalid model of panel specified");
                    return;
                }

                model = model.ToUpper();

                var version = Request.QueryString["version"];
                if (string.IsNullOrEmpty(version))
                {
                    HandleError(400, "Bad Request", "No or invalid version of file specified");
                    return;
                }

                var fileInfo = SoftwareUpdate.GetPanelUpdates()
                    .FirstOrDefault(
                        f => f["version"].Value<string>() == version && f["model"].Value<string>() == model);

                if (fileInfo == null)
                {
                    HandleError(404, "Not Found", "Update server could not find any files suitable");
                    return;
                }

                Request.Response.Header.ContentType = "application/json";
                Request.Response.ContentSource = ContentSource.ContentString;
                var files = new List<object>
                {
                    new
                    {
                        @fileUrl = fileInfo["file_url"].Value<string>(),
                        @fileHashUrl = fileInfo["hash_file_url"].Value<string>(),
                        @fileType = "project"
                    }
                };

                if (Regex.IsMatch(model, @"TSW-\d{1,2}60"))
                {
                    files.Add(new
                    {
                        @fileUrl =
                            string.Format("http://{0}/updatefiles/tsw-xx60_2.001.0040.001.puf",
                                SoftwareUpdate.ServerAddress),
                        @fileHashUrl =
                            string.Format("http://{0}/updatefiles/tsw-xx60_2.001.0040.001.puf.hash",
                                SoftwareUpdate.ServerAddress),
                        @fileType = "firmware"
                    });
                }

                var data = new[]
                {
                    new
                    {
                        @deviceHostname = "*",
                        @deviceModel = model,
                        @filesToUpdate = files
                    }
                };
                Request.Response.ContentString = JToken.FromObject(data).ToString(Formatting.Indented);
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        #endregion
    }
}