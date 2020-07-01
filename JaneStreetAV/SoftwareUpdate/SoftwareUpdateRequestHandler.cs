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
using Crestron.SimplSharpPro.CrestronThread;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SSMono.Net.Http;
using UX.Lib2.Models;
using UX.Lib2.WebScripting2;

namespace JaneStreetAV.SoftwareUpdate
{
    public class SoftwareUpdateRequestHandler : BaseRequestHandler
    {
        #region Fields

        private static readonly HttpClient HttpClient = new HttpClient();
        
        #endregion

        #region Constructors

        public SoftwareUpdateRequestHandler(SystemBase system, Request request, bool loginRequired)
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
            try
            {
                if (Request.PathArguments.ContainsKey("method") && Request.PathArguments["method"] == "progress")
                {
                    var reply = new
                    {
                        @result = SoftwareUpdate.Status.ToString(),
                        @progress = SoftwareUpdate.Progress
                    };

                    Request.Response.Header.ContentType = "application/json";
                    Request.Response.ContentString = JToken.FromObject(reply).ToString(Formatting.Indented);
                    //Request.Response.FinalizeHeader();
                }
                else if (Request.PathArguments.ContainsKey("method") && Request.PathArguments["method"] == "available")
                {
                    var reply = new
                    {
                        @update_available = SoftwareUpdate.UpdateAvailable
                    };

                    Request.Response.Header.ContentType = "application/json";
                    Request.Response.ContentString = JToken.FromObject(reply).ToString(Formatting.Indented);
                    //Request.Response.FinalizeHeader();
                }
                else
                {
                    HandleNotFound();
                }
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        public void Post()
        {
            try
            {
                var json = JToken.Parse(Request.ContentString);
                var url = json["url"].Value<string>();

                var reply = new
                {
                    @result = SoftwareUpdate.Status.ToString(),
                    @progress = SoftwareUpdate.Progress,
                    @progress_url = Request.Path + "/progress"
                };

                Request.Response.Header.ContentType = "application/json";
                Request.Response.ContentString = JToken.FromObject(reply).ToString(Formatting.Indented);
                //Request.Response.FinalizeHeader();
                
                new Thread(specific =>
                {
                    if (SoftwareUpdate.Download(url) == 0)
                    {
                        SoftwareUpdate.OnUpdateShouldLoad();
                    }
                    return null;
                }, null)
                {
                    Priority = Thread.eThreadPriority.LowestPriority
                };
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        #endregion
    }
}