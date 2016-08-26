// <copyright file="AnnounceDeserializerTest.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

namespace Hbm.Devices.Scan
{
    using System.Collections.Generic;
    using Announcing;
    using NUnit.Framework;

    [TestFixture]
    public class AnnounceDeserializerTest
    {
        private Announce announce;
        private FakeMessageReceiver fmr;

        [SetUp]
        public void setup()
        {
            announce = null;
            fmr = new FakeMessageReceiver();
            AnnounceDeserializer parser = new AnnounceDeserializer();
            fmr.HandleMessage += parser.HandleEvent;
            parser.HandleMessage += this.HandleEvent;
        }

        [Test]
        public void ParseCorrectMessage()
        {
            fmr.EmitSingleCorrectMessage();
            Assert.NotNull(announce, "No Announce object after correct message");
        }

        [Test]
        public void ParseInvalidJsonMessage()
        {
            fmr.EmitInvalidJsonMessage();
            Assert.Null(announce, "Got Announce object after invalid message");
        }

        [Test]
        public void ParseNotAnnounceMessage()
        {
            fmr.EmitNotAnnounceMessage();
            Assert.Null(announce, "Got Announce object from message with method that's not an announce");
        }

        [Test]
        public void ParseNoJsonRpcMessage()
        {
            fmr.EmitNoJsonRpcMessage();
            Assert.Null(announce, "Got Announce object from message with method that's not JsonRpc");
        }

        [Test]
        public void ParseEmptyMessage()
        {
            fmr.EmitEmptyString();
            Assert.Null(announce, "Got Announce object after empty message");
        }

        [Test]
        public void ParseNullMessage()
        {
            fmr.EmitNullMessage();
            Assert.Null(announce, "Got Announce object after null");
        }

        [Test]
        public void ParseMissingDeviceMessage()
        {
            fmr.EmitMissingDeviceMessage();
            Assert.Null(announce, "Got Announce from message without device");
        }

        [Test]
        public void ParseEmptyDeviceUuidMessage()
        {
            fmr.EmitEmptyDeviceUuidMessage();
            Assert.Null(announce, "Got Announce from message with empty UUID");
        }

        [Test]
        public void ParseMissingParamsMessage()
        {
            fmr.EmitMissingParamsMessage();
            Assert.Null(announce, "Got Announce from message without params");
        }

        [Test]
        public void ParseNoInterfaceNameMessage()
        {
            fmr.EmitNoInterfaceNameMessage();
            Assert.Null(announce, "Got Announce from message without interface name");
        }

        [Test]
        public void ParseEmptyInterfaceNameMessage()
        {
            fmr.EmitEmptyInterfaceNameMessage();
            Assert.Null(announce, "Got Announce from message with empty interface name");
        }

        [Test]
        public void ParseNoInterfaceMessage()
        {
            fmr.EmitNoInterfaceMessage();
            Assert.Null(announce, "Got Announce from message without interface");
        }

        [Test]
        public void ParseNoNetSettingsMessage()
        {
            fmr.EmitNoNetSettingsMessage();
            Assert.Null(announce, "Got Announce from message without network settings");
        }

        [Test]
        public void ParseMissingRouterUuidMessage()
        {
            fmr.EmitMissingRouterUuidMessage();
            Assert.Null(announce, "Got Announce from message without router UUID");
        }

        [Test]
        public void ParseEmtpyRouterUuidMessage()
        {
            fmr.EmitEmptyRouterUuidMessage();
            Assert.Null(announce, "Got Announce from message with empty router UUID");
        }

        [Test]
        public void ParseMissingTypeMessage()
        {
            fmr.EmitMissingMethodMessage();
            Assert.Null(announce, "Got Announce message from JSON with no method");
        }

        [Test]
        public void ParseMissingVersionMessage()
        {
            fmr.EmitNoVersionMessage();
            Assert.Null(announce, "Got announce from message with no version");
        }

        [Test]
        public void ParseWrongVersionMessage()
        {
            fmr.EmitWrongVersionMessage();
            Assert.Null(announce, "Got announce from message with wrong version");
        }

        [Test]
        public void AnnounceEqualsTest()
        {
            fmr.EmitSingleCorrectMessage();
            Announce a = announce;
            announce = null;
            fmr.EmitSingleCorrectMessage();
            Assert.NotNull(announce, "No Announce object after correct message");
            Assert.True(a.Equals(announce), "Same announces are not equal");
            Assert.False(a.Equals(null), "announce equals null");
        }

        [Test]
        public void TestGetters()
        {
            string jsonRpcVersion = "2.0";
            string method = "announce";
            string apiVersion = "1.0";
            string label = "MX1615-BR";
            string family = "QuantumX";
            string firmwareVersion = "4.1.1.18610.1";
            string hardwareId = "MX1615_R3D";
            string name = "MX1615-Hein";
            string type = "MX1615";
            string uuid = "0009E500123A";
            bool isRouter = true;
            int expiration = 15;
            string defaultGwIPv4 = "172.19.169.254";
            string defaultGwIPv6 = "fdfb:84a3:9d2d:0:222:4dff:feaa:4c1e";
            string configurationMethod = "dhcp";
            string description = "ethernet backplane side";
            string ipv4Address = "172.19.192.57";
            string ipv4Netmask = "255.255.0.0";
            string ipv6Address = "fe80::209:e5ff:fe00:123a";
            int ipv6Prefix = 64;
            string networkName = "eth0";
            string networkType = "ethernet";
            string routerUuid = "0009E50013E9";
            string serviceDaqType = "daqStream";
            int serviceDaqPort = 7411;
            string serviceSshType = "ssh";
            int serviceSshPort = 22;
            string json =
                "{\"jsonrpc\":\"" + jsonRpcVersion + "\",\"method\":\"" + method +
                "\",\"params\":{\"apiVersion\":\"" + apiVersion + "\", " +
                "\"device\":{\"isRouter\":" + isRouter.ToString().ToLower() + ", \"label\":\"" + label + "\", \"familyType\":\"" + family +
                "\",\"firmwareVersion\":\"" + firmwareVersion + "\",\"hardwareId\":\"" + hardwareId + "\"," +
                "\"name\":\"" + name + "\",\"type\":\"" + type + "\",\"uuid\":\"" + uuid + "\"}," +
                "\"expiration\":" + expiration + "," +
                "\"netSettings\":{\"defaultGateway\":{\"ipv4Address\":\"" + defaultGwIPv4 + "\", \"ipv6Address\":\"" + defaultGwIPv6 + "\"}," +
                "\"interface\":{\"configurationMethod\":\"" + configurationMethod + "\",\"description\":\"" + description + "\"," +
                "\"ipv4\":[{\"address\":\"" + ipv4Address + "\",\"netmask\":\"" + ipv4Netmask + "\"}]," +
                "\"ipv6\":[{\"address\":\"" + ipv6Address + "\",\"prefix\":" + ipv6Prefix + "}]," +
                "\"name\":\"" + networkName + "\",\"type\":\"" + networkType + "\"}},\"router\":{\"uuid\":\"" + routerUuid + "\"}," +
                "\"services\":[{\"port\":" + serviceDaqPort + ",\"type\":\"" + serviceDaqType + "\"}," +
                "{\"port\":" + serviceSshPort + ",\"type\":\"" + serviceSshType + "\"}]}}";
            fmr.SendMessage(json);
            Assert.NotNull(announce, "Did not got an announce!");
            Assert.AreEqual(announce.Version, jsonRpcVersion, "JsonRpc version not correct");
            Assert.AreEqual(announce.Method, method, "Method not correct");
            Assert.AreEqual(announce.Parameters.ApiVersion, apiVersion, "API version notn correct");
            Assert.AreEqual(announce.Parameters.Device.Label, label, "Device label not correct");
            Assert.AreEqual(announce.Parameters.Device.FamilyType, family, "Device Family not correct");
            Assert.AreEqual(announce.Parameters.Device.FirmwareVersion, firmwareVersion, "Device firmware version not correct");
            Assert.AreEqual(announce.Parameters.Device.HardwareId, hardwareId, "Device hardware ID not correct");
            Assert.AreEqual(announce.Parameters.Device.Name, name, "Device name not correct");
            Assert.AreEqual(announce.Parameters.Device.DeviceType, type, "Device type not correct");
            Assert.AreEqual(announce.Parameters.Device.Uuid, uuid, "Device uuid not correct");
            Assert.AreEqual(announce.Parameters.Device.IsRouter, isRouter, "Device isRouter not correct");
            Assert.AreEqual(announce.Parameters.Expiration, expiration, "Expiration not correct");
            Assert.AreEqual(announce.Parameters.NetSettings.DefaultGateway.InternetProtocolV4Address, defaultGwIPv4, "Default gateway IPv4 address not correct");
            Assert.AreEqual(announce.Parameters.NetSettings.DefaultGateway.InternetProtocolV6Address, defaultGwIPv6, "Default gateway IPv6 address not correct");
            Assert.AreEqual(announce.Parameters.NetSettings.Interface.ConfigurationMethod, configurationMethod, "Configuration method not correct");
            Assert.AreEqual(announce.Parameters.NetSettings.Interface.InternetProtocolV4[0].Address, ipv4Address, "IPv4 address not correct");
            Assert.AreEqual(announce.Parameters.NetSettings.Interface.InternetProtocolV4[0].NetMask, ipv4Netmask, "IPv4 netmask not correct");
            Assert.AreEqual(announce.Parameters.NetSettings.Interface.InternetProtocolV6[0].Address, ipv6Address, "IPv6 address not correct");
            Assert.AreEqual(announce.Parameters.NetSettings.Interface.InternetProtocolV6[0].Prefix, ipv6Prefix, "IPv6 prefix not correct");
            Assert.AreEqual(announce.Parameters.NetSettings.Interface.Name, networkName, "Interface name not correct");
            Assert.AreEqual(announce.Parameters.NetSettings.Interface.Description, description, "Description not correct");
            Assert.AreEqual(announce.Parameters.NetSettings.Interface.InterfaceType, networkType, "Interface type not correct");
            Assert.AreEqual(announce.Parameters.Router.Uuid, routerUuid, "Router uuid not correct");
            IList<ServiceEntry> entries = announce.Parameters.Services;
            foreach (ServiceEntry entry in entries)
            {
                string serviceType = entry.ServiceType;
                Assert.True(serviceType.Equals(serviceDaqType) || serviceType.Equals(serviceSshType), "Service type not correct");
                if (serviceType.Equals(serviceSshType))
                {
                    Assert.AreEqual(entry.Port, serviceSshPort, "ssh port not correct");
                }
                else
                {
                    Assert.AreEqual(entry.Port, serviceDaqPort, "Daq port not correct");
                }
            }
        }

        private void HandleEvent(object sender, AnnounceEventArgs args)
        {
            announce = args.Announce;
        }
    }
}
