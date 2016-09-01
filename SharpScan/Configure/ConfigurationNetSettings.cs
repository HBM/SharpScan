// <copyright file="ConfigurationNetSettings.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    using Announcing;

    [DataContractAttribute]
    public class ConfigurationNetSettings
    {
        public ConfigurationNetSettings(ConfigurationInterface configurationInterface)
        {
            if (configurationInterface == null)
            {
                throw new ArgumentNullException("configurationInterface");
            }

            this.ConfigurationInterface = configurationInterface;
        }

        public ConfigurationNetSettings(DefaultGateway gateway)
        {
            if (gateway == null)
            {
                throw new ArgumentNullException("gateway");
            }

            if (string.IsNullOrEmpty(gateway.InternetProtocolV4Address) && string.IsNullOrEmpty(gateway.InternetProtocolV6Address))
            {
                throw new ArgumentException("gateway");
            }

            this.DefaultGateway = gateway;
        }

        public ConfigurationNetSettings(ConfigurationInterface configurationInterface, DefaultGateway gateway)
        {
            if (configurationInterface == null) 
            {
                throw new ArgumentNullException("configurationInterface");
            }

            if (gateway == null)
            {
                throw new ArgumentNullException("gateway");
            }

            if (gateway != null)
            {
                if (string.IsNullOrEmpty(gateway.InternetProtocolV4Address) && string.IsNullOrEmpty(gateway.InternetProtocolV6Address))
                {
                    throw new ArgumentException("gateway");
                }
            }

            this.ConfigurationInterface = configurationInterface;
            this.DefaultGateway = gateway;
        }

        [DataMember(Name = "interface")]
        public ConfigurationInterface ConfigurationInterface { get; set; }

        [DataMember(Name = "defaultGateway", EmitDefaultValue = false)]
        public DefaultGateway DefaultGateway { get; set; }
    }
}
