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

        enum responseType
        {
            responseSuccess,
            noResponse,
            responseError,
            responseNoQueryId,
            responseNoErrorNoResult,
            responseIllegalJson,
            responseEmptyJson
        };

        private bool closed = false;
        private ResponseDeserializer parser = new ResponseDeserializer();
        private DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(ConfigurationRequest));
        private bool gotTimeout;
        private bool gotSuccessResponse;
        private bool gotErrorResponse;
        private responseType response;

        [SetUp]
        public void setup()
        {
            gotTimeout = false;
            gotSuccessResponse = false;
            gotErrorResponse = false;
            response = responseType.responseSuccess;
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
        public void SendConfigurationTestDhcp()
        {
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 1000);
            Assert.True(gotSuccessResponse && !gotErrorResponse && !gotTimeout, "got timeout or error for correct configuration response");
        }

        [Test]
        public void SendConfigurationTestManual()
        {
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", "172.19.3.4", "255.255.0.0"));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 1000);
            Assert.True(gotSuccessResponse && !gotErrorResponse && !gotTimeout, "got timeout or error for correct configuration response");
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
            Assert.True(gotSuccessResponse && !gotErrorResponse && !gotTimeout, "got timeout or error for correct configuration response");
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
            Assert.True(gotSuccessResponse && !gotErrorResponse && !gotTimeout, "got timeout or error for correct configuration response");
        }

        [Test]
        public void SendConfigurationTestNoIpGateway()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                DefaultGateway gateway = new DefaultGateway();
                ConfigurationNetSettings settings = new ConfigurationNetSettings(gateway);
            }, "no exception if neither IPv4 nor IPv6 address set");
        }

        [Test]
        public void IllegalParamatersTest()
        {
            ConfigurationService service = new ConfigurationService(parser, this);

            Assert.Throws<ArgumentException>(delegate
            {
                service.SendConfiguration(null, "bla", this, 1000);
            }, "no exception thrown if no configuration parameter given");

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            Assert.Throws<ArgumentException>(delegate
            {
                service.SendConfiguration(parameters, null, this, 1000);
            }, "no exception thrown if no query ID given");

            Assert.Throws<ArgumentException>(delegate
            {
                service.SendConfiguration(parameters, "foo", null, 1000);
            }, "no exception thrown if no callbacks given");

            Assert.Throws<ArgumentException>(delegate
            {
                service.SendConfiguration(parameters, "foo", this, -10);
            }, "no exception thrown if negative timeout given");
        }

        [Test]
        public void SendConfigurationTimeout()
        {
            response = responseType.noResponse;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 10);
            Thread.Sleep(100);
            Assert.True(!gotSuccessResponse && !gotErrorResponse && gotTimeout, "got no timeout if no response was sent");
        }

        [Test]
        public void SendConfigurationNoQueryIdResponse()
        {
            response = responseType.responseNoQueryId;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 10);
            Thread.Sleep(100);
            Assert.True(!gotSuccessResponse && !gotErrorResponse && gotTimeout, "got no timeout if response with no query ID was sent");
        }

        [Test]
        public void SendConfigurationIllegalResponse()
        {
            response = responseType.responseIllegalJson;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 10);
            Thread.Sleep(100);
            Assert.True(!gotSuccessResponse && !gotErrorResponse && gotTimeout, "got no timeout if illegal response was sent");
        }

        [Test]
        public void SendConfigurationNullResponse()
        {
            response = responseType.responseEmptyJson;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 10);
            Thread.Sleep(100);
            Assert.True(!gotSuccessResponse && !gotErrorResponse && gotTimeout, "got no timeout if null response was sent");
        }

        [Test]
        public void SendConfigurationNoErrorNoResultResponse()
        {
            response = responseType.responseNoErrorNoResult;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 10);
            Thread.Sleep(100);
            Assert.True(!gotSuccessResponse && !gotErrorResponse && gotTimeout, "got no timeout if response with no result/error was sent");
        }

        [Test]
        public void SendConfigurationErrorResponse()
        {
            response = responseType.responseError;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 1000);
            Assert.True(!gotSuccessResponse && gotErrorResponse && !gotTimeout, "got no error callback if error response was sent");
        }

        [Test]
        public void CloseWhileSendingConfiguration()
        {
            response = responseType.noResponse;
            ConfigurationService service = new ConfigurationService(this.parser, this);

            ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
            ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
            ConfigurationParams parameters = new ConfigurationParams(device, settings);
            service.SendConfiguration(parameters, this, 1000);
            service.Close();
            Assert.True(!gotSuccessResponse && !gotErrorResponse && !gotTimeout, "got callbacks after closing the service");
        }

        public void SendMessage(string json)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                switch (response)
                {
                    default:
                    case responseType.responseSuccess:
                        {
                            ConfigurationRequest request = (ConfigurationRequest)this.deserializer.ReadObject(ms);
                            string responseString = "{\"jsonrpc\":\"2.0\",\"id\":\"" + request.QueryId + "\",\"result\":true}";
                            MulticastMessageEventArgs args = new MulticastMessageEventArgs();
                            args.JsonString = responseString;
                            parser.HandleEvent(null, args);
                        }
                        break;

                    case responseType.noResponse:
                        break;

                    case responseType.responseError:
                        {
                            ConfigurationRequest request = (ConfigurationRequest)this.deserializer.ReadObject(ms);
                            string responseString = "{\"jsonrpc\":\"2.0\",\"id\":\"" + request.QueryId + "\",\"error\":{\"code\": -32602, \"data\": \"error\", \"message\": \"Invalid method parameters\"}}";
                            MulticastMessageEventArgs args = new MulticastMessageEventArgs();
                            args.JsonString = responseString;
                            parser.HandleEvent(null, args);
                        }
                        break;

                    case responseType.responseNoQueryId:
                        {
                            ConfigurationRequest request = (ConfigurationRequest)this.deserializer.ReadObject(ms);
                            string responseString = "{\"jsonrpc\":\"2.0\",\"error\":{\"code\": -32602, \"message\": \"Invalid method parameters\"}}";
                            MulticastMessageEventArgs args = new MulticastMessageEventArgs();
                            args.JsonString = responseString;
                            parser.HandleEvent(null, args);
                        }
                        break;

                    case responseType.responseNoErrorNoResult:
                        {
                            ConfigurationRequest request = (ConfigurationRequest)this.deserializer.ReadObject(ms);
                            string responseString = "{\"jsonrpc\":\"2.0\",\"id\":\"" + request.QueryId + "\"}";
                            MulticastMessageEventArgs args = new MulticastMessageEventArgs();
                            args.JsonString = responseString;
                            parser.HandleEvent(null, args);
                        }
                        break;

                    case responseType.responseIllegalJson:
                        {
                            ConfigurationRequest request = (ConfigurationRequest)this.deserializer.ReadObject(ms);
                            MulticastMessageEventArgs args = new MulticastMessageEventArgs();
                            args.JsonString = @"{""jsonrpc""2.0"",""id"":""" + request.QueryId + @"""}";
                            parser.HandleEvent(null, args);
                        }
                        break;
                    case responseType.responseEmptyJson:
                        {
                            ConfigurationRequest request = (ConfigurationRequest)this.deserializer.ReadObject(ms);
                            MulticastMessageEventArgs args = new MulticastMessageEventArgs();
                            args.JsonString = null;
                            parser.HandleEvent(null, args);
                        }
                        break;
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
