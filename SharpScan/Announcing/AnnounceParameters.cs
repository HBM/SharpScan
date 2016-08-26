// <copyright file="AnnounceParameters.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    public class AnnounceParameters
    {
#pragma warning disable 0649
        [DataMember(Name = "expiration")]
        private int expiration;

        [DataMember(Name = "apiVersion")]

        private string apiVersion;

        [DataMember(Name = "device")]
        private AnnouncedDevice device;

        [DataMember(Name = "router")]
        private Router router;

        [DataMember(Name = "services")]
        private IList<ServiceEntry> services;

        [DataMember(Name = "netSettings")]
        private NetSettings netSettings;
#pragma warning restore 0649

        public int Expiration
        {
            get
            {
                return this.expiration;
            }
        }

        public string ApiVersion
        {
            get
            {
                return this.apiVersion;
            }
        }

        public AnnouncedDevice Device
        {
            get
            {
                return this.device;
            }
        }

        public Router Router
        {
            get
            {
                return this.router;
            }
        }

        public IList<ServiceEntry> Services
        {
            get
            {
                if (this.services != null)
                {
                    return new ReadOnlyCollection<ServiceEntry>(this.services);
                }
                else
                {
                    return new List<ServiceEntry>();
                }
            }
        }

        public NetSettings NetSettings
        {
            get
            {
                return this.netSettings;
            }
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext c)
        {
            expiration = (expiration == 0) ? 20 : expiration;
        }
    }
}
