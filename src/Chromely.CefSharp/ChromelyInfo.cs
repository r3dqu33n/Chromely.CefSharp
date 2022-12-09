// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using Chromely.Core;
using Chromely.Core.Network;
using System;
using System.Collections.Generic;

namespace Chromely.CefSharp
{
    public class ChromelyInfo : IChromelyInfo
    {
        public IChromelyResponse GetInfo(string requestId)
        {
            var response = new ChromelyResponse(requestId)
            {
                ReadyState = (int)ReadyState.ResponseIsReady,
                Status = (int)System.Net.HttpStatusCode.OK,
                StatusText = "OK",
                Data = GetInfo()
            };

            return response;
        }

        public IDictionary<string, string> GetInfo()
        {
            var bitness = Environment.Is64BitProcess ? "x64" : "x86";

            var chromeVersion = $"Chromium: {Cef.ChromiumVersion}, CEF: {Cef.CefVersion}, CefSharp: {Cef.CefSharpVersion}, Environment: {bitness}";

            var infoItemDic = new Dictionary<string, string>
            {
                {
                    "divObjective",
                    "To build HTML5 desktop apps using embedded Chromium without WinForm or WPF. Uses Windows, Linux and MacOS native GUI API. It can be extended to use WinForm or WPF. Main form of communication with Chromium rendering process is via CEF Message Router, Ajax HTTP/XHR requests using custom schemes and domains."
                },
                {
                    "divPlatform",
                    "Cross-platform - Windows, Linux, MacOS. Built on CefGlue, CefSharp, NET Standard 2.0, .NET Core 3.0, .NET Framework 4.61 and above."
                },
                { "divVersion", chromeVersion }
            };

            return infoItemDic;
        }
    }
}
