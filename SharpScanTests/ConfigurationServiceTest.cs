// <copyright file="ConfigurationServiceTest.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    using System.Runtime.Serialization.Json;
    using NUnit.Framework;
    using System.IO;
    using System.Text;
    using System.Runtime.Serialization;

    [TestFixture]
    internal class ConfigurationServiceTest : IMulticastSender, IConfigurationCallback
    {
        private bool closed = false;
        private ResponseDeserializer parser = new ResponseDeserializer();
        private DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(ConfigurationRequest));
        private bool gotTimeout;
        private bool gotSuccessResponse;
        private bool gotErrorResponse;

        [SetUp]
        public void setup()
        {
            gotTimeout = false;
            gotSuccessResponse = false;
            gotErrorResponse = false;
        }

        [Test]
        public void ServiceInstantiationAndClose()
        {
            Assert.DoesNotThrow(delegate 
            {
                ConfigurationMessageReceiver receiver = new ConfigurationMessageReceiver();
                receiver.HandleMessage += parser.HandleEvent;
                ConfigurationService service = new ConfigurationService(parser, this);
                service.Close();
            }, "instantiation and closing of ConfigurationService threw exception", "null");
        }

        [Test]
        public void SendConfigurationTest()
        {
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 100);
            Assert.True(gotSuccessResponse && !gotErrorResponse && !gotTimeout, "got timeout or error for correct configuration response");
        }

        public void SendMessage(string json)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                try
                {
                    ConfigurationRequest request = (ConfigurationRequest)this.deserializer.ReadObject(ms);
                    string response = "{\"jsonrpc\":\"2.0\",\"id\":\"" + request.QueryId + "\",\"result\":true}";
                    MulticastMessageEventArgs args = new MulticastMessageEventArgs();
                    args.JsonString = response;
                    parser.HandleEvent(null, args);
                }
                catch (SerializationException)
                {
                    return;
                }
            }
        }

        public void Close()
        {
            closed = true;
        }

        public bool IsClosed()
        {
            return closed;
        }

        public void OnSuccess(JsonRpcResponse response)
        {
            gotSuccessResponse = true;
        }

        public void OnError(JsonRpcResponse response)
        {
            gotErrorResponse = true;   
        }

        public void OnTimeout()
        {
            gotTimeout = true;
        }
    }
}
