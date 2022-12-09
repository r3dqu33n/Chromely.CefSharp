using CefSharp;
using CefSharp.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chromely.CefSharp.Extensions
{
    internal static class WebBrowserExtensions
    {
        /// <summary>
        /// See <see cref="IChromiumWebBrowserBase.LoadUrlAsync(string)"/> for details
        /// </summary>
        /// <param name="chromiumWebBrowser">ChromiumWebBrowser instance (cannot be null)</param>
        /// <summary>
        /// Load the <paramref name="url"/> in the main frame of the browser
        /// </summary>
        /// <param name="url">url to load</param>
        /// <returns>See <see cref="IChromiumWebBrowserBase.LoadUrlAsync(string)"/> for details</returns>
        public static Task<LoadUrlAsyncResponse> LoadUrlAsyncO(this IChromiumWebBrowserBase chromiumWebBrowser, string url)
        {
            ThrowExceptionIfChromiumWebBrowserDisposed(chromiumWebBrowser);

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var tcs = new TaskCompletionSource<LoadUrlAsyncResponse>();

            EventHandler<LoadErrorEventArgs> loadErrorHandler = null;
            EventHandler<LoadingStateChangedEventArgs> loadingStateChangeHandler = null;

            loadErrorHandler = (sender, args) =>
            {
                //Actions that trigger a download will raise an aborted error.
                //Generally speaking Aborted is safe to ignore
                if (args.ErrorCode == CefErrorCode.Aborted)
                {
                    return;
                }

                //If LoadError was called then we'll remove both our handlers
                //as we won't need to capture LoadingStateChanged, we know there
                //was an error
                chromiumWebBrowser.LoadError -= loadErrorHandler;
                chromiumWebBrowser.LoadingStateChanged -= loadingStateChangeHandler;

                //Ensure our continuation is executed on the ThreadPool
                //For the .Net Core implementation we could use
                //TaskCreationOptions.RunContinuationsAsynchronously
                tcs.TrySetResultAsync(new LoadUrlAsyncResponse(args.ErrorCode, -1));
            };

            loadingStateChangeHandler = (sender, args) =>
            {
                //Wait for IsLoading = false
                if (!args.IsLoading)
                {
                    //If LoadingStateChanged was called then we'll remove both our handlers
                    //as LoadError won't be called, our site has loaded with a valid HttpStatusCode
                    //HttpStatusCodes can still be for example 404, this is considered a successful request,
                    //the server responded, it just didn't have the page you were after.
                    chromiumWebBrowser.LoadError -= loadErrorHandler;
                    chromiumWebBrowser.LoadingStateChanged -= loadingStateChangeHandler;

                    var host = args.Browser.GetHost();

                    var navEntry = host?.GetVisibleNavigationEntry();

                    int statusCode = navEntry?.HttpStatusCode ?? -1;

                    //By default 0 is some sort of error, we map that to -1
                    //so that it's clearer that something failed.
                    if (statusCode == 0)
                    {
                        statusCode = -1;
                    }

                    //Ensure our continuation is executed on the ThreadPool
                    //For the .Net Core implementation we could use
                    //TaskCreationOptions.RunContinuationsAsynchronously
                    tcs.TrySetResultAsync(new LoadUrlAsyncResponse(statusCode == -1 ? CefErrorCode.Failed : CefErrorCode.None, statusCode));
                }
            };

            chromiumWebBrowser.LoadError += loadErrorHandler;
            chromiumWebBrowser.LoadingStateChanged += loadingStateChangeHandler;

            chromiumWebBrowser.LoadUrl(url);

            return tcs.Task;
        }

        private static void ThrowExceptionIfChromiumWebBrowserDisposed(IChromiumWebBrowserBase chromiumWebBrowser)
        {
            if (chromiumWebBrowser.IsDisposed) throw new ObjectDisposedException(nameof(chromiumWebBrowser));
        }
    }
}
