// <copyright file="NetInterface.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

namespace Hbm.Devices.Scan.Announcing
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [DataContractAttribute]
    public class NetInterface
    {
#pragma warning disable 0649
        [DataMember(Name = "name")]
        private string name;

        [DataMember(Name = "type")]
        private string interfaceType;

        [DataMember(Name = "description")]
        private string description;

        [DataMember(Name = "configurationMethod")]
        private string configurationMethod;

        [DataMember(Name = "ipv4")]
        private IList<InternetProtocolV4Address> ipv4;

        [DataMember(Name = "ipv6")]
        private IList<InternetProtocolV6Address> ipv6;
#pragma warning restore 0649

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public string InterfaceType
        {
            get
            {
                return this.interfaceType;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
        }

        public string ConfigurationMethod
        {
            get
            {
                return this.configurationMethod;
            }
        }

        public IList<InternetProtocolV4Address> InternetProtocolV4
        {
            get
            {
                if (this.ipv4 != null)
                {
                    return new ReadOnlyCollection<InternetProtocolV4Address>(this.ipv4);
                }
                else
                {
                    return new Collection<InternetProtocolV4Address>();
                }
            }
        }

        public IList<InternetProtocolV6Address> InternetProtocolV6
        {
            get
            {
                if (this.ipv6 != null)
                {
                    return new ReadOnlyCollection<InternetProtocolV6Address>(this.ipv6);
                }
                else
                {
                    return new Collection<InternetProtocolV6Address>();
                }
            }
        }
    }
}
