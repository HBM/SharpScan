// <copyright file="ConfigurationObjectsTests.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    using NUnit.Framework;

    [TestFixture]
    internal class ConfigurationObjectsTests
    {
        [Test]
        public void DeviceInstantiationWithIllegalParameters()
        {
            Assert.Throws<ArgumentException>(
                delegate
                {
                    ConfigurationDevice device = new ConfigurationDevice(null);
                },
                "Instantiation of ConfigurationDevice with null must fail");

            Assert.Throws<ArgumentException>(
                delegate
                {
                    ConfigurationDevice device = new ConfigurationDevice(string.Empty);
                },
                "Instantiation of ConfigurationDevice with empty string must fail");
        }

        [Test]
        public void ConfigurationInterfaceInstantiation()
        {
            const string InterfaceName = "eth0";
            const string Ipv4Address = "172.19.3.4";
            const string Netmask = "255.255.255.0";
            ConfigurationInterface iface = new ConfigurationInterface(InterfaceName, Ipv4Address, Netmask);

            Assert.AreEqual(InterfaceName, iface.Name, "interface name not set");
            Assert.AreEqual(Ipv4Address, iface.InternetProtocolV4.ManualAddress, "IP address not set");
            Assert.AreEqual(Netmask, iface.InternetProtocolV4.ManualNetMask, "netmask not set");

            Assert.AreEqual(ConfigurationInterface.Method.Manual, iface.ConfigurationMethod, "configuration method not set to manual");
            Assert.AreEqual("manual", iface.ConfigurationMethodString, "configuration method not \"manual\"");

            Assert.Throws<ArgumentException>(
                delegate
                {
                    iface = new ConfigurationInterface("eth0", ConfigurationInterface.Method.Manual);
                },
                "setting \"manual\" without providing ip address and netmask is not allowed");

            Assert.Throws<ArgumentException>(
                delegate
                {
                    iface = new ConfigurationInterface(null, ConfigurationInterface.Method.Dhcp);
                },
                "setting \"manual\" without providing ip address and netmask is not allowed");
        }

        [Test]
        public void ConfigurationNetSettings()
        {
            Assert.Throws<ArgumentNullException>(
                delegate
                {
                    ConfigurationNetSettings settings = new ConfigurationNetSettings(null, null);
                },
                "no exception if both gateway and configuration interface are null");
        }

        [Test]
        public void ConfigurationRequestIllegalParameters()
        {
            Assert.Throws<ArgumentNullException>(
                delegate
                {
                    ConfigurationRequest request = new ConfigurationRequest(null, "bla");
                },
                "no exception thrown if no configuration parameters set");

            Assert.Throws<ArgumentException>(
                delegate
                {
                    ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
                    ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
                    ConfigurationParams parameters = new ConfigurationParams(device, settings);
                    ConfigurationRequest request = new ConfigurationRequest(parameters, null);
                },
                "no exception thrown if no query ID given");

            Assert.Throws<ArgumentException>(
                delegate
                {
                    ConfigurationDevice device = new ConfigurationDevice("0009E5001231");
                    ConfigurationNetSettings settings = new ConfigurationNetSettings(new ConfigurationInterface("eth0", ConfigurationInterface.Method.Dhcp));
                    ConfigurationParams parameters = new ConfigurationParams(device, settings);
                    ConfigurationRequest request = new ConfigurationRequest(parameters, string.Empty);
                },
                "no exception thrown if empty query ID given");
        }
    }
}
