// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System.Diagnostics;
using System.Threading.Tasks;
using Chromely.Core;
using Chromely.Core.Infrastructure;
using Chromely.Core.Network;

namespace Chromely.CefSharp.Browser
{
    public class DefaultBoundObject 
    {
        protected readonly IChromelyRouteProvider _routeProvider;
        protected readonly IChromelyRequestHandler _requestHandler;
        protected readonly IChromelyDataTransferOptions _dataTransferOptions;

        //protected readonly IChromelyRequestTaskRunner _requestTaskRunner;
        //protected readonly IChromelyCommandTaskRunner _commandTaskRunner;
        //protected readonly IChromelySerializerUtil _serializerUtil;

        public DefaultBoundObject(IChromelyRouteProvider routeProvider,
                                  IChromelyRequestHandler requestHandler,
                                  IChromelyDataTransferOptions dataTransferOptions)
                                  //IChromelyRequestTaskRunner requestTaskRunner, 
                                  //IChromelyCommandTaskRunner commandTaskRunner,
                                  //IChromelySerializerUtil serializerUtil)
        {
            _routeProvider = routeProvider;
            _requestHandler = requestHandler;
            _dataTransferOptions = dataTransferOptions;
            // _requestTaskRunner = requestTaskRunner;
            //_commandTaskRunner = commandTaskRunner;
            //_serializerUtil = serializerUtil;
        }

        public string Execute(string path, object parameters = null, object postData = null)
        {
            IChromelyResponse response = null;
            bool isRequestAsync = _routeProvider.IsRouteAsync(path);
            if (isRequestAsync)
            {
                var task = Task.Run(async () =>
                {
                    return await _requestHandler.ExecuteAsync("request.Identifier.ToString()", path, parameters.ToDictionary<object>(), postData, null);
                });
                //var task = Task.Run(async () => { return await _requestTaskRunner.RunAsync(path, parameters.ToDictionary(), postData); });
                task.Wait();
                response = task.Result;
            }
            else
            {
                response = _requestHandler.Execute("request.Identifier.ToString()", path, parameters.ToDictionary<object>(), postData, null);
                //response = _requestTaskRunner.Run(path, parameters.ToDictionary(), postData);
            }

            return response == null ? string.Empty : _dataTransferOptions.ConvertResponseToJson(response);
        }

        public void Command(string url)
        {
            Debugger.Break();
        //    _commandTaskRunner.Run(url);
        }
    }
}
