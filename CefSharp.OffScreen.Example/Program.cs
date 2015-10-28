﻿// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using CefSharp.Example;

namespace CefSharp.OffScreen.Example
{
    public class Program
    {
        private const string TestUrl = "https://www.google.com/";

        public static void Main(string[] args)
        {
            Console.WriteLine("This example application will load {0}, take a screenshot, and save it to your desktop.", TestUrl);
            Console.WriteLine("You may see a lot of Chromium debugging output, please wait...");
            Console.WriteLine();

            // You need to replace this with your own call to Cef.Initialize();
            CefExample.Init(true, multiThreadedMessageLoop:true);

            MainAsync("cachePath1", 1.0);
            //Demo showing Zoom Level of 3.0
            //Using seperate request contexts allows the urls from the same domain to have independent zoom levels
            //otherwise they would be the same - default behaviour of Chromium
            //MainAsync("cachePath2", 3.0);

            // We have to wait for something, otherwise the process will exit too soon.
            Console.ReadKey();

            // Clean up Chromium objects.  You need to call this in your application otherwise
            // you will get a crash when closing.
            Cef.Shutdown();
        }

        private static async void MainAsync(string cachePath, double zoomLevel)
        {
            var injectedTime = new DateTime(991, 7, 23);
            using (var browser = new ChromiumWebBrowser("http://crawlbin.com/", null, null, false))
            {
                browser.RegisterJsObject("testTime", injectedTime);
                browser.CreateBrowser(IntPtr.Zero);
                await LoadPageAsync(browser);

                // KO, Message error 'Frame 1 is no longer available, most likely the Frame has been Disposed.'
                JavascriptResponse response = await browser.EvaluateScriptAsync(@"(function() { return testTime.year + '/' + testTime.month + '/' + testTime.day; })();");
                var result = response.Success ? (response.Result ?? "null") : response.Message;
                
                Console.WriteLine(result);
                Debug.Assert("991/7/23".Equals(result), "The evaluation result does not match the expected value.");
            }
        }

        public static Task LoadPageAsync(IWebBrowser browser, string address = null)
        {
            var tcs = new TaskCompletionSource<bool>();

            EventHandler<LoadingStateChangedEventArgs> handler = null;
            handler = (sender, args) =>
            {
                //Wait for while page to finish loading not just the first frame
                if (!args.IsLoading)
                {
                    browser.LoadingStateChanged -= handler;
                    tcs.TrySetResult(true);
                }
            };

            browser.LoadingStateChanged += handler;

            if (!string.IsNullOrEmpty(address))
            {
                browser.Load(address);
            }
            return tcs.Task;
        }

    }
}
