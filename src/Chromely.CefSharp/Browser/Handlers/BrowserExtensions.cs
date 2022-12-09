// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using Chromely.Core.Network;
using System.Collections.Generic;
using System.Linq;

namespace Chromely.CefSharp.Browser
{
    public static class BrowserExtensions
    {
        public static bool IsUrlRegisteredLocalRequestScheme(this List<UrlScheme> urlSchemes, string url)
        {
            if (urlSchemes == null ||
                 !urlSchemes.Any() ||
                 string.IsNullOrWhiteSpace(url))
                return false;

            return urlSchemes.Any((x => x.IsUrlOfSameScheme(url) && (x.UrlSchemeType == UrlSchemeType.LocalRequest)));
        }
    }
}
