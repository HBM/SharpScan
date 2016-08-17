// <copyright file="Announce.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;

namespace Hbm.Devices.Scan.Announcing
{
    [DataContractAttribute]
    public class Announce : JsonRpc
    {
        [DataMember(Name = "params")]
        public AnnounceParameters Parameters { get; private set; }
        internal string Path { get; set; }
        internal NetworkInterface IncomingInterface { get; set; }

        private Announce()
            : base("announce")
        {
        }

        internal void identifyCommunicationPath(NetworkInterface incomingIF)
        {
            Path = this.Parameters.Device.Uuid + incomingIF.Id + this.Parameters.NetSettings.Interface.Name;
            if (this.Parameters.Router != null)
            {
                Path = Path + this.Parameters.Router.Uuid;
            }
            IncomingInterface = incomingIF;
        }

        public bool Equals(Announce other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return string.Compare(JsonString, other.JsonString, StringComparison.Ordinal) == 0;
        }
    }

    [DataContractAttribute]
    public class AnnounceParameters
    {
        [DataMember(Name = "apiVersion")]
#pragma warning disable 0649
        private string apiVersion;
        public string ApiVersion { get { return apiVersion; } }

        [DataMember(Name = "device")]
        private AnnouncedDevice device;
        public AnnouncedDevice Device { get { return device; } }

        [DataMember(Name = "router")]
        private Router router;
        public Router Router { get { return router; } }

        [DataMember(Name = "services")]
        private IList<ServiceEntry> services;
        public IList<ServiceEntry> Services
        {
            get
            {
                if (services != null)
                {
                    return new ReadOnlyCollection<ServiceEntry>(services);
                }
                else
                {
                    return new List<ServiceEntry>();
                }
            }
        }

        [DataMember(Name = "expiration")]
        private readonly int expiration = 20;
        public int Expiration { get { return expiration; } }

        [DataMember(Name = "netSettings")]
        private NetSettings netSettings;
        public NetSettings NetSettings { get { return netSettings; } }
    }

    [DataContractAttribute]
    public class AnnouncedDevice
    {
        [DataMember(Name = "uuid")]
        private string uuid;
        public string Uuid { get { return uuid; } }

        [DataMember(Name = "name")]
        private string name;
        public string Name { get { return name; } }

        [DataMember(Name = "type")]
        private string deviceType;
        public string DeviceType { get { return deviceType; } }

        [DataMember(Name = "label")]
        private string label;
        public string Label { get { return label; } }

        [DataMember(Name = "familyType")]
        private string familyType;
        public string FamilyType { get { return familyType; } }

        [DataMember(Name = "hardwareId")]
        private string hardwareId;
        public string HardwareId { get { return hardwareId; } }

        [DataMember(Name = "firmwareVersion")]
        private string firmwareVersion;
        public string FirmwareVersion { get { return firmwareVersion; } }

        [DataMember(Name = "isRouter")]
        private bool isRouter;
        public bool IsRouter { get { return isRouter; } }
    }

    [DataContractAttribute]
    public class Router
    {
        [DataMember(Name = "uuid")]
        private string uuid;
        public string Uuid { get { return uuid; } }
    }

    [DataContractAttribute]
    public class ServiceEntry
    {
        public const string ServiceDataAcquisition = "daqStream";
        public const string ServiceHbmProtocol = "hbmProtocol";
        public const string ServiceHttp = "http";
        public const string ServiceJetDaemon = "jetd";
        public const string ServiceJetWebSockets = "jetws";
        public const string ServiceSecureShell = "ssh";

        [DataMember(Name = "type")]
        private string serviceType;
        public string ServiceType { get { return serviceType; } }

        [DataMember(Name = "port")]
        private int port;
        public int Port { get { return port; } }
    }

    [DataContractAttribute]
    public class NetSettings
    {
        [DataMember(Name = "interface")]
        private NetInterface iface;
        public NetInterface Interface { get { return iface; } }

        [DataMember(Name = "defaultGateway")]
        private DefaultGateway defaultGateway;
        public DefaultGateway DefaultGateway { get { return defaultGateway; } }
    }

    [DataContractAttribute]
    public class NetInterface
    {
        [DataMember(Name = "name")]
        private string name;
        public string Name { get { return name; } }

        [DataMember(Name = "type")]
        private string interfaceType;
        public string InterfaceType { get { return interfaceType; } }

        [DataMember(Name = "description")]
        private string description;
        public string Description { get { return description; } }

        [DataMember(Name = "configurationMethod")]
        private string configurationMethod;
        public string ConfigurationMethod { get { return configurationMethod; } }

        [DataMember(Name = "ipv4")]
        private IList<InternetProtocolV4Address> ipv4;
        public IList<InternetProtocolV4Address> InternetProtocolV4
        {
            get
            {
                if (ipv6 != null)
                {
                    return new ReadOnlyCollection<InternetProtocolV4Address>(ipv4);
                }
                else
                {
                    return new Collection<InternetProtocolV4Address>();
                }
            }
        }

        [DataMember(Name = "ipv6")]
        private IList<InternetProtocolV6Address> ipv6;
        public IList<InternetProtocolV6Address> InternetProtocolV6
        {
            get
            {
                if (ipv6 != null)
                {
                    return new ReadOnlyCollection<InternetProtocolV6Address>(ipv6);
                }
                else
                {
                    return new Collection<InternetProtocolV6Address>();
                }
            }
        }
    }

    [DataContractAttribute]
    public class InternetProtocolV4Address
    {
        [DataMember(Name = "address")]
        private string address;
        public string Address { get { return address; } }

        [DataMember(Name = "netmask")]
        private string netMask;
        public string NetMask { get { return netMask; } }
    }

    [DataContractAttribute]
    public class InternetProtocolV6Address
    {
        [DataMember(Name = "address")]
        private string address;
        public string Address { get { return address; } }

        [DataMember(Name = "prefix")]
        private int prefix;
        public int Prefix { get { return prefix; } }
    }
#pragma warning restore 0649
}
