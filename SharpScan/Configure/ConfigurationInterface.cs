// <copyright file="ConfigurationInterface.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    using System.Runtime.Serialization;

    [DataContractAttribute]
    public class ConfigurationInterface
    {
        public ConfigurationInterface(string configurationInterface, string address, string mask)
            : this(configurationInterface)
        {
            this.InternetProtocolV4 = new ManualInternetProtocolV4Address();
            this.InternetProtocolV4.ManualAddress = address;
            this.InternetProtocolV4.ManualNetMask = mask;
        }

        public ConfigurationInterface(string configurationInterface, Method mode)
            : this(configurationInterface)
        {
            this.ConfigurationMethod = mode;
        }

        public ConfigurationInterface(string configurationInterface, string address, string mask, Method mode)
            : this(configurationInterface, address, mask)
        {
            this.ConfigurationMethod = mode;
        }

        private ConfigurationInterface(string iface)
        {
            this.Name = iface;
        }

        public enum Method
        {
            Manual,
            Dhcp
        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "ipv4", EmitDefaultValue = false)]
        public ManualInternetProtocolV4Address InternetProtocolV4 { get; set; }

        public Method ConfigurationMethod { get; set; }

        [DataMember(Name = "configurationMethod")]
        public string ConfigurationMethodString
        {
            get
            {
                if (ConfigurationMethod == Method.Dhcp)
                {
                    return "dhcp";
                }

                if (ConfigurationMethod == Method.Manual)
                {
                    return "manual";
                }

                throw new Exception("illegal configuration method");
            }

            set
            {
                if ("manual".Equals(value))
                {
                    ConfigurationMethod = Method.Manual;
                }
                else if ("dhcp".Equals(value))
                {
                    ConfigurationMethod = Method.Dhcp;
                }
                else
                {
                    throw new ArgumentException("no valid configuration method given");
                }
            }
        }
    }
}
