﻿//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Net;
using System.Threading;
using Emul8.Logging;

namespace Emul8.Robot
{
    internal class HttpServer : IDisposable
    {
        public HttpServer(XmlRpcServer processor)
        {
            listener = new HttpListener();
            xmlRpcServer = processor;
            listenerThread = new Thread(Runner)
            {
                Name = "Robot Framework listener thread",
                IsBackground = true
            };
        }

        public void Start(int port)
        {
            Logger.Log(LogLevel.Info, "Robot Framework remote server is listening on port {0}", port);
            listener.Prefixes.Add(string.Format("http://*:{0}/", port));
            listenerThread.Start();
        }

        public void Run(int port)
        {
            Start(port);
            listenerThread.Join();
        }

        public void Shutdown()
        {
            quit = true;
            if(Thread.CurrentThread != listenerThread)
            {
                listener.Close();
                listenerThread.Join();
            }
        }

        public void Dispose()
        {
            xmlRpcServer.Dispose();
        }

        private void Runner()
        {
            listener.Start();
            while(!quit)
            {
                var context = listener.GetContext();
                xmlRpcServer.ProcessRequest(context);
            }
            Logger.Log(LogLevel.Info, "Robot Framework listener thread stopped");
        }

        private bool quit;
        private readonly HttpListener listener;
        private readonly Thread listenerThread;
        private readonly XmlRpcServer xmlRpcServer;
    }
}

