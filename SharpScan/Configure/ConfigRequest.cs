// <copyright file="ConfigRequest.cs" company="Hottinger Baldwin Messtechnik GmbH">
//
// CS Scan, a library for scanning and configuring HBM devices.
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

using Hbm.Devices.Scan.Announcing;
using System.Threading;

namespace Hbm.Devices.Scan.Configure
{
    public class ConfigRequest : JsonRpc
    {
        public int Id { get; set; }
        internal ConfigParams ConfigParameters { get; set; }

        private static int idCounter;

        public ConfigRequest(string uuid, ConfigNetSettings configNetSettings)
            : base("configure")
        {
            Id = Interlocked.Increment(ref idCounter);
            ConfigParameters = new ConfigParams(uuid, configNetSettings);
        }

    }

    internal class ConfigParams
    {
        public Device device { get; set; }
        public ConfigNetSettings netSettings { get; set; }
        public int ttl { get; set; }

        public ConfigParams(string uuid, ConfigNetSettings cns)
        {
            device = new Device(uuid);
            netSettings = cns;
            ttl = 1;
        }
    }

    public class ConfigNetSettings
    {
        public ConfigInterface ConfigurationInterface { get; set; }
        public DefaultGateway DefaultGateway { get; set; }

        public ConfigNetSettings(ConfigInterface configurationInterface) : this(configurationInterface, null) { }
        public ConfigNetSettings(DefaultGateway gateway) : this(null, gateway) { }
        public ConfigNetSettings(ConfigInterface configurationInterface, DefaultGateway gateway)
        {
            this.ConfigurationInterface = configurationInterface;
            DefaultGateway = gateway;
        }
    }

    public class ConfigInterface
    {
        public string Name { get; set; }
        public ManualInternetProtocolV4Address InternetProtocolV4 { get; set; }
        public string ConfigurationMethod { get; set; }

        private ConfigInterface(string iface)
        {
            Name = iface;
        }

        public ConfigInterface(string configurationInterface, string address, string mask)
            : this(configurationInterface)
        {
            InternetProtocolV4 = new ManualInternetProtocolV4Address();
            InternetProtocolV4.ManualAddress = address;
            InternetProtocolV4.ManualNetMask = mask;
        }

        public ConfigInterface(string configurationInterface, string mode)
            : this(configurationInterface)
        {
            ConfigurationMethod = mode;
        }

        public ConfigInterface(string configurationInterface, string address, string mask, string mode)
            : this(configurationInterface, address, mask)
        {
            ConfigurationMethod = mode;
        }
    }

    public class ManualInternetProtocolV4Address
    {
        public string ManualAddress { get; set; }
        public string ManualNetMask { get; set; }
    }
}
