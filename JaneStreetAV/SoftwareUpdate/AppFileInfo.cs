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
using Crestron.SimplSharp.Reflection;
using Newtonsoft.Json;
using UX.Lib2;

namespace JaneStreetAV.SoftwareUpdate
{
    public class AppFileInfo
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime LastModified { get; set; }

        public string RelativeDate
        {
            get { return (DateTime.Now - LastModified).ToPrettyTimeAgo(); }
        }

        [JsonIgnore]
        public string Date
        {
            get { return LastModified.ToString("F"); }
        }

        [JsonProperty(PropertyName = "local_directory")]
        public string LocalDirectory { get; set; }

        [JsonProperty(PropertyName = "local_path")]
        public string LocalPath { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "extension")]
        public string Extension { get; set; }

        [JsonProperty(PropertyName = "size")]
        public long Size { get; set; }

        [JsonIgnore]
        public string SizePretty {
            get { return Tools.PrettyByteSize(Size, 1); }
        }

        [JsonProperty(PropertyName = "app_name")]
        public string ApplicationName { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "is_debug")]
        public bool IsDebug { get; set; }

        [JsonProperty(PropertyName = "release_notes_url")]
        public string ReleaseNotesUrl { get; set; }

        [JsonIgnore]
        public bool PatternMatches { get; private set; }

        [JsonIgnore]
        public bool VersionIsNewerThanRunningVersion
        {
            get
            {
                var runningVersion = Assembly.GetExecutingAssembly().GetName().Version;
                var thisVersion = new Version(Version);

                return thisVersion > runningVersion;
            }
        }

        [JsonIgnore]
        public bool VersionIsOlderThanRunningVersion
        {
            get
            {
                var runningVersion = Assembly.GetExecutingAssembly().GetName().Version;
                var thisVersion = new Version(Version);

                return thisVersion < runningVersion;
            }
        }
    }
}