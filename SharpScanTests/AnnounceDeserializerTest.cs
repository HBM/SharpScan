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
    using System.Globalization;
    using Announcing;
    using NUnit.Framework;

    [TestFixture]
    public class AnnounceDeserializerTest
    {
        private Announce announce;
        private FakeMessageReceiver fmr;
        private AnnounceDeserializer parser;

        [SetUp]
        public void Setup()
        {
            this.announce = null;
            this.fmr = new FakeMessageReceiver();
            this.parser = new AnnounceDeserializer();
            this.fmr.HandleMessage += this.parser.HandleEvent;
            this.parser.HandleMessage += this.HandleEvent;
        }

        [Test]
        public void ParseCorrectMessage()
        {
            this.fmr.EmitSingleCorrectMessage();
            Assert.NotNull(this.announce, "No Announce object after correct message");
        }

        [Test]
        public void ParseInvalidJsonMessage()
        {
            this.fmr.EmitInvalidJsonMessage();
            Assert.Null(this.announce, "Got Announce object after invalid message");
        }

        [Test]
        public void ParseNotAnnounceMessage()
        {
            this.fmr.EmitNotAnnounceMessage();
            Assert.Null(this.announce, "Got Announce object from message with method that's not an announce");
        }

        [Test]
        public void ParseNoJsonRpcMessage()
        {
            this.fmr.EmitNoJsonRpcMessage();
            Assert.Null(this.announce, "Got Announce object from message with method that's not JsonRpc");
        }

        [Test]
        public void ParseEmptyMessage()
        {
            this.fmr.EmitEmptyString();
            Assert.Null(this.announce, "Got Announce object after empty message");
        }

        [Test]
        public void ParseNullMessage()
        {
            this.fmr.EmitNullMessage();
            Assert.Null(this.announce, "Got Announce object after null");
        }

        [Test]
        public void ParseMissingDeviceMessage()
        {
            this.fmr.EmitMissingDeviceMessage();
            Assert.Null(this.announce, "Got Announce from message without device");
        }

        [Test]
        public void ParseEmptyDeviceUuidMessage()
        {
            this.fmr.EmitEmptyDeviceUuidMessage();
            Assert.Null(this.announce, "Got Announce from message with empty UUID");
        }

        [Test]
        public void ParseMissingParamsMessage()
        {
            this.fmr.EmitMissingParamsMessage();
            Assert.Null(this.announce, "Got Announce from message without params");
        }

        [Test]
        public void ParseNoInterfaceNameMessage()
        {
            this.fmr.EmitNoInterfaceNameMessage();
            Assert.Null(this.announce, "Got Announce from message without interface name");
        }

        [Test]
        public void ParseEmptyInterfaceNameMessage()
        {
            this.fmr.EmitEmptyInterfaceNameMessage();
            Assert.Null(this.announce, "Got Announce from message with empty interface name");
        }

        [Test]
        public void ParseNoInterfaceMessage()
        {
            this.fmr.EmitNoInterfaceMessage();
            Assert.Null(this.announce, "Got Announce from message without interface");
        }

        [Test]
        public void ParseNoNetSettingsMessage()
        {
            this.fmr.EmitNoNetSettingsMessage();
            Assert.Null(this.announce, "Got Announce from message without network settings");
        }

        [Test]
        public void ParseMissingRouterUuidMessage()
        {
            this.fmr.EmitMissingRouterUuidMessage();
            Assert.Null(this.announce, "Got Announce from message without router UUID");
        }

        [Test]
        public void ParseEmtpyRouterUuidMessage()
        {
            this.fmr.EmitEmptyRouterUuidMessage();
            Assert.Null(this.announce, "Got Announce from message with empty router UUID");
        }

        [Test]
        public void ParseMissingTypeMessage()
        {
            this.fmr.EmitMissingMethodMessage();
            Assert.Null(this.announce, "Got Announce message from JSON with no method");
        }

        [Test]
        public void ParseMissingVersionMessage()
        {
            this.fmr.EmitNoVersionMessage();
            Assert.Null(this.announce, "Got announce from message with no version");
        }

        [Test]
        public void ParseWrongVersionMessage()
        {
            this.fmr.EmitWrongVersionMessage();
            Assert.Null(this.announce, "Got announce from message with wrong version");
        }

        [Test]
        public void ParseMissingExpireMessage()
        {
            this.fmr.EmitMessageNoExpire();
            Assert.AreEqual(int.Parse(ScanConstants.defaultExpirationInSeconds, CultureInfo.InvariantCulture), this.announce.Parameters.Expiration, "expiration not set to a meaningful value");
        }

        [Test]
        public void ParseNegativeExpireMessage()
        {
            this.fmr.EmitMessageNegativeExpire();
            Assert.Null(this.announce, "Got announce form message with negative expiration");
        }

        [Test]
        public void ParseMissingSevicesMessage()
        {
            this.fmr.EmitMessageNoServices();
            Assert.NotNull(this.announce.Parameters.Services, "Service list must not be null");
            Assert.AreEqual(0, this.announce.Parameters.Services.Count, "Service list not empty");
        }

        [Test]
        public void ParseNoIPv4Addreses()
        {
            this.fmr.EmitNoIPv4Message();
            Assert.NotNull(this.announce.Parameters.NetSettings.Interface.InternetProtocolV4, "IPv4 list must not be null");
            Assert.AreEqual(0, this.announce.Parameters.NetSettings.Interface.InternetProtocolV4.Count, "IPv4 list not empty");
        }

        [Test]
        public void ParseNoIPv6Addreses()
        {
            this.fmr.EmitNoIPv6Message();
            Assert.NotNull(this.announce.Parameters.NetSettings.Interface.InternetProtocolV6, "IPv6 list must not be null");
            Assert.AreEqual(0, this.announce.Parameters.NetSettings.Interface.InternetProtocolV6.Count, "IPv6 list not empty");
        }

        [Test]
        public void AnnounceEqualsTest()
        {
            this.fmr.EmitSingleCorrectMessage();
            Announce a = this.announce;
            this.announce = null;
            this.fmr.EmitSingleCorrectMessage();
            Assert.NotNull(this.announce, "No Announce object after correct message");
            Assert.True(a.Equals(this.announce), "Same announces are not equal");
            Assert.False(a.Equals(null), "announce equals null");
        }

        [Test]
        public void HandleEventParameterChecking()
        {
            Assert.DoesNotThrow(delegate { this.parser.HandleEvent(null, null); }, "HandleEvent doesn't handle errors correctly", "null");
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
            this.fmr.SendMessage(json);
            Assert.NotNull(this.announce, "Did not got an announce!");
            Assert.AreEqual(jsonRpcVersion, this.announce.Version, "JsonRpc version not correct");
            Assert.AreEqual(method, this.announce.Method, "Method not correct");
            Assert.AreEqual(apiVersion, this.announce.Parameters.ApiVersion, "API version notn correct");
            Assert.AreEqual(label, this.announce.Parameters.Device.Label, "Device label not correct");
            Assert.AreEqual(family, this.announce.Parameters.Device.FamilyType, "Device Family not correct");
            Assert.AreEqual(firmwareVersion, this.announce.Parameters.Device.FirmwareVersion, "Device firmware version not correct");
            Assert.AreEqual(hardwareId, this.announce.Parameters.Device.HardwareId, "Device hardware ID not correct");
            Assert.AreEqual(name, this.announce.Parameters.Device.Name, "Device name not correct");
            Assert.AreEqual(type, this.announce.Parameters.Device.DeviceType, "Device type not correct");
            Assert.AreEqual(uuid, this.announce.Parameters.Device.Uuid, "Device uuid not correct");
            Assert.AreEqual(isRouter, this.announce.Parameters.Device.IsRouter, "Device isRouter not correct");
            Assert.AreEqual(expiration, this.announce.Parameters.Expiration, "Expiration not correct");
            Assert.AreEqual(defaultGwIPv4, this.announce.Parameters.NetSettings.DefaultGateway.InternetProtocolV4Address, "Default gateway IPv4 address not correct");
            Assert.AreEqual(defaultGwIPv6, this.announce.Parameters.NetSettings.DefaultGateway.InternetProtocolV6Address, "Default gateway IPv6 address not correct");
            Assert.AreEqual(configurationMethod, this.announce.Parameters.NetSettings.Interface.ConfigurationMethod, "Configuration method not correct");
            Assert.AreEqual(ipv4Address, this.announce.Parameters.NetSettings.Interface.InternetProtocolV4[0].Address, "IPv4 address not correct");
            Assert.AreEqual(ipv4Netmask, this.announce.Parameters.NetSettings.Interface.InternetProtocolV4[0].NetMask, "IPv4 netmask not correct");
            Assert.AreEqual(ipv6Address, this.announce.Parameters.NetSettings.Interface.InternetProtocolV6[0].Address, "IPv6 address not correct");
            Assert.AreEqual(ipv6Prefix, this.announce.Parameters.NetSettings.Interface.InternetProtocolV6[0].Prefix, "IPv6 prefix not correct");
            Assert.AreEqual(networkName, this.announce.Parameters.NetSettings.Interface.Name, "Interface name not correct");
            Assert.AreEqual(description, this.announce.Parameters.NetSettings.Interface.Description, "Description not correct");
            Assert.AreEqual(networkType, this.announce.Parameters.NetSettings.Interface.InterfaceType, "Interface type not correct");
            Assert.AreEqual(routerUuid, this.announce.Parameters.Router.Uuid, "Router uuid not correct");
            IList<ServiceEntry> entries = this.announce.Parameters.Services;
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
            this.announce = args.Announce;
        }
    }
}
