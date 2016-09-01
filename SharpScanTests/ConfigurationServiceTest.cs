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
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading;

    using Announcing;
    using NUnit.Framework;

    [TestFixture]
    internal class ConfigurationServiceTest : IMulticastSender, IConfigurationCallback
    {
        private bool closed = false;
        private ResponseDeserializer parser = new ResponseDeserializer();
        private DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(ConfigurationRequest));
        private bool gotTimeout;
        private bool gotSuccessResponse;
        private bool gotErrorResponse;
        private ResponseType response;

        internal enum ResponseType
        {
            responseSuccess,
            noResponse,
            responseError,
            responseNoQueryId,
            responseNoErrorNoResult,
            responseIllegalJson,
            responseEmptyJson
        }

        [SetUp]
        public void Setup()
        {
            this.gotTimeout = false;
            this.gotSuccessResponse = false;
            this.gotErrorResponse = false;
            this.response = ResponseType.responseSuccess;
        }

        [Test]
        public void ServiceInstantiationAndClose()
        {
            Assert.DoesNotThrow(
                delegate 
                {
                    ConfigurationMessageReceiver receiver = new ConfigurationMessageReceiver();
                    receiver.HandleMessage += this.parser.HandleEvent;
                    ConfigurationService service = new ConfigurationService(this.parser, this);
                    service.Close();
                },
                "instantiation and closing of ConfigurationService threw exception",
                "null");
        }

        [Test]
        public void SendConfigurationTestDhcp()
        {
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 1000);
            Assert.True(this.gotSuccessResponse && !this.gotErrorResponse && !this.gotTimeout, "got timeout or error for correct configuration response");
        }

        [Test]
        public void SendConfigurationTestManual()
        {
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", "172.19.3.4", "255.255.0.0"));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 1000);
            Assert.True(this.gotSuccessResponse && !this.gotErrorResponse && !this.gotTimeout, "got timeout or error for correct configuration response");
        }

        [Test]
        public void SendConfigurationTestIPv4Gateway()
        {
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            DefaultGateway gateway = new DefaultGateway();
            gateway.InternetProtocolV4Address = "172.19.3.4";
            ConfigurationNetSettings settings = new ConfigurationNetSettings(gateway);
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 1000);
            Assert.True(this.gotSuccessResponse && !this.gotErrorResponse && !this.gotTimeout, "got timeout or error for correct configuration response");
        }

        [Test]
        public void SendConfigurationTestIPv6Gateway()
        {
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            DefaultGateway gateway = new DefaultGateway();
            gateway.InternetProtocolV6Address = "2001:db8:85a3::8a2e:370:7334";
            ConfigurationNetSettings settings = new ConfigurationNetSettings(gateway);
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 1000);
            Assert.True(this.gotSuccessResponse && !this.gotErrorResponse && !this.gotTimeout, "got timeout or error for correct configuration response");
        }

        [Test]
        public void SendConfigurationTestNoIpGateway()
        {
            Assert.Throws<ArgumentException>(
                delegate
                {
                    DefaultGateway gateway = new DefaultGateway();
                    ConfigurationNetSettings settings = new ConfigurationNetSettings(gateway);
                },
                "no exception if neither IPv4 nor IPv6 address set");
        }

        [Test]
        public void IllegalParamatersTest()
        {
            ConfigurationService service = new ConfigurationService(this.parser, this);

            Assert.Throws<ArgumentException>(
                delegate
                {
                    service.SendConfiguration(null, "bla", this, 1000);
                },
                "no exception thrown if no configuration parameter given");

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            Assert.Throws<ArgumentException>(
                delegate
                {
                    service.SendConfiguration(parameters, null, this, 1000);
                },
                "no exception thrown if no query ID given");

            Assert.Throws<ArgumentException>(
                delegate
                {
                    service.SendConfiguration(parameters, "foo", null, 1000);
                },
                "no exception thrown if no callbacks given");

            Assert.Throws<ArgumentException>(
                delegate
                {
                    service.SendConfiguration(parameters, "foo", this, -10);
                },
                "no exception thrown if negative timeout given");
        }

        [Test]
        public void SendConfigurationTimeout()
        {
            this.response = ResponseType.noResponse;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 10);
            Thread.Sleep(100);
            Assert.True(!this.gotSuccessResponse && !this.gotErrorResponse && this.gotTimeout, "got no timeout if no response was sent");
        }

        [Test]
        public void SendConfigurationNoQueryIdResponse()
        {
            this.response = ResponseType.responseNoQueryId;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 10);
            Thread.Sleep(100);
            Assert.True(!this.gotSuccessResponse && !this.gotErrorResponse && this.gotTimeout, "got no timeout if response with no query ID was sent");
        }

        [Test]
        public void SendConfigurationIllegalResponse()
        {
            this.response = ResponseType.responseIllegalJson;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 10);
            Thread.Sleep(100);
            Assert.True(!this.gotSuccessResponse && !this.gotErrorResponse && this.gotTimeout, "got no timeout if illegal response was sent");
        }

        [Test]
        public void SendConfigurationNullResponse()
        {
            this.response = ResponseType.responseEmptyJson;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 10);
            Thread.Sleep(100);
            Assert.True(!this.gotSuccessResponse && !this.gotErrorResponse && this.gotTimeout, "got no timeout if null response was sent");
        }

        [Test]
        public void SendConfigurationNoErrorNoResultResponse()
        {
            this.response = ResponseType.responseNoErrorNoResult;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 10);
            Thread.Sleep(100);
            Assert.True(!this.gotSuccessResponse && !this.gotErrorResponse && this.gotTimeout, "got no timeout if response with no result/error was sent");
        }

        [Test]
        public void SendConfigurationErrorResponse()
        {
            this.response = ResponseType.responseError;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 1000);
            Assert.True(!this.gotSuccessResponse && this.gotErrorResponse && !this.gotTimeout, "got no error callback if error response was sent");
        }

        [Test]
        public void CloseWhileSendingConfiguration()
        {
            this.response = ResponseType.noResponse;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 1000);
            service.Close();
            Assert.True(!this.gotSuccessResponse && !this.gotErrorResponse && !this.gotTimeout, "got callbacks after closing the service");
        }

        public void SendMessage(string json)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                switch (this.response)
                {
                    default:
                    case ResponseType.responseSuccess:
                        {
                            ConfigurationRequest request = (ConfigurationRequest)this.deserializer.ReadObject(ms);
                            string responseString = "{\"jsonrpc\":\"2.0\",\"id\":\"" + request.QueryId + "\",\"result\":true}";
                            MulticastMessageEventArgs args = new MulticastMessageEventArgs();
                            args.JsonString = responseString;
                            this.parser.HandleEvent(null, args);
                        }

                        break;

                    case ResponseType.noResponse:
                        break;

                    case ResponseType.responseError:
                        {
                            ConfigurationRequest request = (ConfigurationRequest)this.deserializer.ReadObject(ms);
                            string responseString = "{\"jsonrpc\":\"2.0\",\"id\":\"" + request.QueryId + "\",\"error\":{\"code\": -32602, \"data\": \"error\", \"message\": \"Invalid method parameters\"}}";
                            MulticastMessageEventArgs args = new MulticastMessageEventArgs();
                            args.JsonString = responseString;
                            this.parser.HandleEvent(null, args);
                        }

                        break;

                    case ResponseType.responseNoQueryId:
                        {
                            ConfigurationRequest request = (ConfigurationRequest)this.deserializer.ReadObject(ms);
                            string responseString = "{\"jsonrpc\":\"2.0\",\"error\":{\"code\": -32602, \"message\": \"Invalid method parameters\"}}";
                            MulticastMessageEventArgs args = new MulticastMessageEventArgs();
                            args.JsonString = responseString;
                            this.parser.HandleEvent(null, args);
                        }

                        break;

                    case ResponseType.responseNoErrorNoResult:
                        {
                            ConfigurationRequest request = (ConfigurationRequest)this.deserializer.ReadObject(ms);
                            string responseString = "{\"jsonrpc\":\"2.0\",\"id\":\"" + request.QueryId + "\"}";
                            MulticastMessageEventArgs args = new MulticastMessageEventArgs();
                            args.JsonString = responseString;
                            this.parser.HandleEvent(null, args);
                        }

                        break;

                    case ResponseType.responseIllegalJson:
                        {
                            ConfigurationRequest request = (ConfigurationRequest)this.deserializer.ReadObject(ms);
                            MulticastMessageEventArgs args = new MulticastMessageEventArgs();
                            args.JsonString = @"{""jsonrpc""2.0"",""id"":""" + request.QueryId + @"""}";
                            this.parser.HandleEvent(null, args);
                        }

                        break;
                    case ResponseType.responseEmptyJson:
                        {
                            ConfigurationRequest request = (ConfigurationRequest)this.deserializer.ReadObject(ms);
                            MulticastMessageEventArgs args = new MulticastMessageEventArgs();
                            args.JsonString = null;
                            this.parser.HandleEvent(null, args);
                        }

                        break;
                }
            }
        }

        public void Close()
        {
            this.closed = true;
        }

        public bool IsClosed()
        {
            return this.closed;
        }

        public void OnSuccess(JsonRpcResponse response)
        {
            this.gotSuccessResponse = true;
        }

        public void OnError(JsonRpcResponse response)
        {
            this.gotErrorResponse = true;   
        }

        public void OnTimeout()
        {
            this.gotTimeout = true;
        }
    }
}
