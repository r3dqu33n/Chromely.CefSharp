﻿// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using CefSharp;
using Chromely.Core;
using Chromely.Core.Configuration;
using Chromely.Core.Infrastructure;
using Chromely.Core.Network;

namespace Chromely.CefSharp.Browser
{

    internal interface IDefaultCustomHandler
    {
    }

    internal interface IDefaultResourceCustomHandler
    {
    }


    internal interface IDefaultAssemblyResourceCustomHandler
    {
    }

    internal interface IDefaultRequestCustomHandler
    {
    }

    internal interface IDefaultExernalRequestCustomHandler
    {
    }


    /*
     * JavaScript Binding Object
     */
    internal sealed class ChromelyBoundObjectHandler : IChromelyJsBindingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChromelyBoundObjectHandler"/> class.
        /// </summary>
        public ChromelyBoundObjectHandler(IChromelyRouteProvider routeProvider, 
                                            IChromelyRequestHandler requestHandler,
                                            IChromelyDataTransferOptions dataTransferOptions)
        {
            Key = JsObjectBinding.DEFAULTNAME;
            ObjectName = JsObjectBinding.DEFAULTNAME;
            BoundObject = new DefaultBoundObject(routeProvider, requestHandler, dataTransferOptions);
            BindingOptions = null;
        }

        public string Key { get; }
        public string ObjectName { get; set; }
        public object BoundObject { get; set; }
        public object BindingOptions { get; set; }
    }

    /*
     * Resource/Request Scheme handlers
     */
    internal sealed class ChromelyResourceSchemeHandlerFactory : DefaultResourceSchemeHandlerFactory, IDefaultResourceCustomHandler
    {
        public ChromelyResourceSchemeHandlerFactory(IChromelyErrorHandler chromelyErrorHandler) : base(chromelyErrorHandler)
        {
        }
    }

    internal sealed class ChromelyAssemblyResourceSchemeHandlerFactory : DefaultAssemblyResourceSchemeHandlerFactory, IDefaultAssemblyResourceCustomHandler
    {
        public ChromelyAssemblyResourceSchemeHandlerFactory(IChromelyRequestSchemeHandlerProvider requestSchemeHandlerProvider, IChromelyErrorHandler chromelyErrorHandler) : base(requestSchemeHandlerProvider, chromelyErrorHandler)
        {
        }
    }

    internal sealed class ChromelyRequestSchemeHandler : DefaultRequestSchemeHandler, IDefaultRequestCustomHandler
    {
        public ChromelyRequestSchemeHandler(IChromelyRouteProvider routeProvider, 
                                            IChromelyRequestSchemeHandlerProvider requestSchemeHandlerProvider,
                                            IChromelyRequestHandler requestHandler,
                                            IChromelyDataTransferOptions dataTransferOptions,
                                            IChromelyErrorHandler errorHandler)
            : base(routeProvider, requestSchemeHandlerProvider, requestHandler, dataTransferOptions, errorHandler)
        {
        }
    }

    internal sealed class ChromelyExternalRequestSchemeHandler: DefaultExternalRequestSchemeHandler, IDefaultExernalRequestCustomHandler
    {
    }

    /*
     * Custom handlers
     */
    internal sealed class ChromelyContextMenuHandler : DefaultContextMenuHandler, IDefaultCustomHandler
    {
        public ChromelyContextMenuHandler(IChromelyConfiguration config) : base(config)
        {
        }
    }

    internal sealed class ChromelyDownloadHandler : DefaultDownloadHandler, IDefaultCustomHandler
    {
    }

    internal sealed class ChromelyDragHandler : DefaultDragHandler, IDefaultCustomHandler
    {
        public ChromelyDragHandler(IChromelyConfiguration config) : base(config)
        {
        }
    }

    internal sealed class ChromelyLifeSpanHandler : DefaultLifeSpanHandler, IDefaultCustomHandler
    {
        public ChromelyLifeSpanHandler(IChromelyConfiguration config/*, IChromelyCommandTaskRunner commandTaskRunner*/)
            :base(config)//, commandTaskRunner)
        {
        }
    }

    internal sealed class ChromelyRequestHandler : DefaultRequestHandler, IDefaultCustomHandler
    {
        public ChromelyRequestHandler(IChromelyConfiguration config, IChromelyRequestSchemeHandlerProvider requestSchemeHandlerProvider, IResourceRequestHandler resourceRequestHandler)
                : base(config, requestSchemeHandlerProvider, resourceRequestHandler)
        {
        }
    }
  }
