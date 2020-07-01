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
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Reflection;
using UX.Lib2.Models;
using UX.Lib2.WebScripting2;

namespace JaneStreetAV.WebApp.Files
{
    public class StaticFileHandler : FileHandlerBase
    {
        public StaticFileHandler(SystemBase system, Request request, bool loginRequired)
            : base(system, request, loginRequired)
        {
        }

        protected override string RootFilePath
        {
            get { return GetType().Namespace; }
        }

        protected override Stream GetResourceStream(Assembly assembly, string fileName)
        {
            SetCacheTime(TimeSpan.FromHours(1));
            return base.GetResourceStream(Assembly.GetExecutingAssembly(), fileName);
        }
    }
}