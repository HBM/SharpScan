// <copyright file="ConfigurationService.cs" company="Hottinger Baldwin Messtechnik GmbH">
//
// SharpScan, a library for scanning and configuring HBM devices.
//
// The MIT License (MIT)
//
// Copyright (C) Hottinger Baldwin Messtechnik GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// </copyright>

namespace Hbm.Devices.Scan.Configure
{
    using System;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;
    using System.Timers;

    public class ConfigurationService
    {
        private ConfigurationSerializer serializer;
        private IMulticastSender sender;
        private ResponseDeserializer parser;
        private IDictionary<string, ConfigQuery> awaitingResponses;

        public ConfigurationService(ICollection<NetworkInterface> adapters, ResponseDeserializer parser)
        {
            this.awaitingResponses = new Dictionary<string, ConfigQuery>();
            this.serializer = new ConfigurationSerializer();
            this.sender = new ConfigurationMulticastSender(adapters);
            this.parser = parser;
            parser.HandleMessage += this.HandleEvent;
        }

        public void SendConfiguration(ConfigurationParams parameters, IConfigurationCallback callbacks, double timeoutMs)
        {
            Guid queryId = System.Guid.NewGuid();
            this.SendConfiguration(parameters, queryId.ToString(), callbacks, timeoutMs);
        }

        public void SendConfiguration(ConfigurationParams parameters, string queryId, IConfigurationCallback callbacks, double timeoutMs)
        {
            if (parameters == null)
            {
                throw new ArgumentException("no parameters given");
            }

            if (callbacks == null)
            {
                throw new ArgumentException("no callbacks given");
            }

            if (timeoutMs <= 0)
            {
                throw new ArgumentException("timeout must be greater the 0");
            }

            if (string.IsNullOrEmpty(queryId))
            {
                throw new ArgumentException("no query id given");
            }

            ConfigurationRequest request = new ConfigurationRequest(parameters, queryId);
            ConfigurationTimer timer = new ConfigurationTimer(timeoutMs, request);
            ConfigQuery query = new ConfigQuery(request, callbacks, timer);
            lock (this.awaitingResponses)
            {
                this.awaitingResponses.Add(queryId, query);
                timer.AutoReset = false;
                timer.Elapsed += new ElapsedEventHandler(this.OnTimedEvent);
                timer.Start();
            }

            this.sender.SendMessage(this.serializer.Serialize(request));
        }

        public void Close()
        {
            this.sender.Close();
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            ConfigurationTimer timer = (ConfigurationTimer)sender;
            lock (this.awaitingResponses)
            {
                ConfigQuery query;
                if (this.awaitingResponses.TryGetValue(timer.Request.QueryId, out query))
                {
                    this.awaitingResponses.Remove(timer.Request.QueryId);
                    query.Callbacks.OnTimeout();
                }
            }
        }

        private void HandleEvent(object sender, JsonRpcResponseEventArgs args)
        {
            JsonRpcResponse response = args.Response;
            if ((response.Result != null) || (response.Error != null))
            {
                string queryId = response.Id;
                if (!string.IsNullOrEmpty(queryId))
                {
                    lock (this.awaitingResponses)
                    {
                        ConfigQuery query;
                        if (this.awaitingResponses.TryGetValue(queryId, out query))
                        {
                            this.awaitingResponses.Remove(queryId);
                            query.Timer.Stop();
                        }

                        if (response.Error != null)
                        {
                            query.Callbacks.OnError(response);
                        }
                        else if (response.Result != null)
                        {
                            query.Callbacks.OnSuccess(response);
                        }
                    }
                }
            }
        }

        internal class ConfigQuery
        {
            internal ConfigQuery(ConfigurationRequest request, IConfigurationCallback callbacks, ConfigurationTimer timer)
            {
                this.Request = request;
                this.Callbacks = callbacks;
                this.Timer = timer;
            }

            internal ConfigurationRequest Request { get; set; }

            internal IConfigurationCallback Callbacks { get; set; }

            internal ConfigurationTimer Timer { get; set; }
        }

        internal class ConfigurationTimer : Timer
        {
            internal ConfigurationTimer(double expire, ConfigurationRequest request)
                : base(expire)
            {
                this.Request = request;
            }

            internal ConfigurationRequest Request { get; set; }
        }
    }
}
