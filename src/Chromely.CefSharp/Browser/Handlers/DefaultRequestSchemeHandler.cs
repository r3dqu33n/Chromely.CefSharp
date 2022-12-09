// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by Chromely MIT licensed and CefSharp BSD-style license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using Chromely.Core;
using Chromely.Core.Infrastructure;
using Chromely.Core.Logging;
using Chromely.Core.Network;
using Microsoft.Extensions.Logging;

namespace Chromely.CefSharp.Browser
{
    /// <summary>
    /// The CefSharp http scheme handler.
    /// </summary>
    public class DefaultRequestSchemeHandler : ResourceHandler
    {
        protected readonly IChromelyRouteProvider _routeProvider;
        protected readonly IChromelyRequestSchemeHandlerProvider _requestSchemeHandlerProvider;
        protected readonly IChromelyRequestHandler _requestHandler;
        protected readonly IChromelyDataTransferOptions _dataTransferOptions;
        protected readonly IChromelyErrorHandler _chromelyErrorHandler;
        protected IChromelyResponse _chromelyResponse;

        protected Stream _stream;
        protected string _mimeType;
        protected byte[] _responseBytes;
        protected bool _completed;
        protected int _totalBytesRead;

        public DefaultRequestSchemeHandler(IChromelyRouteProvider routeProvider,
                                           IChromelyRequestSchemeHandlerProvider requestSchemeHandlerProvider,
                                           IChromelyRequestHandler requestHandler,
                                           IChromelyDataTransferOptions dataTransferOptions,
                                           IChromelyErrorHandler chromelyErrorHandler)
        {
            _routeProvider = routeProvider;
            _requestSchemeHandlerProvider = requestSchemeHandlerProvider;
            _requestHandler = requestHandler;
            _dataTransferOptions = dataTransferOptions;
            _chromelyErrorHandler = chromelyErrorHandler;
        }

        /// <summary>
        /// The process request async.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
        {
            var scheme = _requestSchemeHandlerProvider?.GetScheme(request.Url);
            if (scheme != null && scheme.UrlSchemeType == UrlSchemeType.LocalRequest)
            {
                _stream = null;
                var uri = new Uri(request.Url);
                var path = uri.LocalPath;
                _mimeType = "application/json";

                bool isRequestAsync = _routeProvider.IsRouteAsync(path);
                if (isRequestAsync)
                {
                    ProcessRequestAsync(path);
                }
                else
                {
                    ProcessRequest(path);
                }
            }

            return CefReturnValue.ContinueAsync;

            #region Process Request

            void ProcessRequest(string path)
            {
                Task.Run(() =>
                {
                    using (callback)
                    {
                        try
                        {
                            var response = new ChromelyResponse();
                            if (string.IsNullOrEmpty(path))
                            {
                                response.ReadyState = (int)ReadyState.ResponseIsReady;
                                response.Status = (int)System.Net.HttpStatusCode.BadRequest;
                                response.StatusText = "Bad Request";

                                _chromelyResponse = response;
                            }
                            else
                            {
                                var parameters = request.Url.GetParameters();
                                var postData = request.GetPostData();

                                var jsonRequest = _dataTransferOptions.ConvertObjectToJson(postData);

                                _chromelyResponse = _requestHandler.Execute(request.Identifier.ToString(), path, parameters, postData, jsonRequest);
                                
                                string jsonData = _dataTransferOptions.ConvertResponseToJson(_chromelyResponse.Data);

                                if (jsonData is not null)
                                {
                                    var content = Encoding.UTF8.GetBytes(jsonData);
                                    _stream = new MemoryStream();
                                    _stream.Write(content, 0, content.Length);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            _stream = null;
                            Logger.Instance.Log.LogError(exception, exception.Message);

                            _chromelyResponse =
                                new ChromelyResponse
                                {
                                    Status = (int)HttpStatusCode.BadRequest,
                                    Data = "An error occured."
                                };
                        }

                        if (_stream == null)
                        {
                            callback.Cancel();
                        }
                        else
                        {
                            SetResponseInfoOnSuccess();
                            callback.Continue();
                        }
                    }
                });
            }

            #endregion

            #region Process Request Async

            void ProcessRequestAsync(string path)
            {
                Task.Run(async () =>
                {
                    using (callback)
                    {
                        try
                        {
                            var response = new ChromelyResponse();
                            if (string.IsNullOrEmpty(path))
                            {
                                response.ReadyState = (int)ReadyState.ResponseIsReady;
                                response.Status = (int)System.Net.HttpStatusCode.BadRequest;
                                response.StatusText = "Bad Request";

                                _chromelyResponse = response;
                            }
                            else
                            {
                                var parameters = request.Url.GetParameters();
                                var postData = request.GetPostData();

                                var jsonRequest = _dataTransferOptions.ConvertObjectToJson(request);


                                _chromelyResponse = await _requestHandler.ExecuteAsync(request.Identifier.ToString(), path, parameters, postData, jsonRequest);
                                string jsonData = _dataTransferOptions.ConvertResponseToJson(_chromelyResponse.Data);

                                if (jsonData is not null)
                                {
                                    var content = Encoding.UTF8.GetBytes(jsonData);
                                    _stream = new MemoryStream();
                                    _stream.Write(content, 0, content.Length);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            _stream = null;
                            Logger.Instance.Log.LogError(exception, exception.Message);

                            _chromelyResponse =
                                new ChromelyResponse
                                {
                                    Status = (int)HttpStatusCode.BadRequest,
                                    Data = "An error occured."
                                };
                        }

                        if (_stream == null)
                        {
                            callback.Cancel();
                        }
                        else
                        {
                            SetResponseInfoOnSuccess();
                            callback.Continue();
                        }
                    }
                });
            }

            #endregion
        }

        protected virtual void SetResponseInfoOnSuccess() 
        {
            //Reset the stream position to 0 so the stream can be copied into the underlying unmanaged buffer
            _stream.Position = 0;
            //Populate the response values - No longer need to implement GetResponseHeaders (unless you need to perform a redirect)
            ResponseLength = _stream.Length;
            MimeType = _mimeType;
            StatusCode = _chromelyResponse.Status;
            StatusText = _chromelyResponse.StatusText;
            Stream = _stream;
            MimeType = _mimeType;

            if (Headers != null)
            {
                Headers.Add("Cache-Control", "private");
                Headers.Add("Access-Control-Allow-Origin", "*");
                Headers.Add("Access-Control-Allow-Methods", "GET,POST");
                Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                Headers.Add("Content-Type", "application/json; charset=utf-8");
            }
        }
    }
}
