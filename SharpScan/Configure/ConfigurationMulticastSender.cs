// <copyright file="ConfigurationMulticastSender.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;

    public class ConfigurationMulticastSender : IMulticastSender, IDisposable
    {
        private static readonly string ConfigureAddress = Hbm.Devices.Scan.ScanConstants.configureAddress;
        private static readonly int ConfigurePort = int.Parse(Hbm.Devices.Scan.ScanConstants.configurePort, CultureInfo.InvariantCulture);

        private List<NetworkInterface> interfaces;
        private Socket socket;
        private bool closed;

        public ConfigurationMulticastSender(ICollection<NetworkInterface> interfaces)
        {
            if (interfaces == null)
            {
                throw new ArgumentException("no collection of Interfaces given");
            }

            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress ip = IPAddress.Parse(ConfigureAddress);
            this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip));
            this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 1);
            this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, 0);

            IPEndPoint ipep = new IPEndPoint(ip, ConfigurePort);
            this.socket.Connect(ipep);

            this.interfaces = new List<NetworkInterface>();
            this.interfaces.AddRange(interfaces);
            this.closed = false;
        }

        public void Close()
        {
            if (!this.closed)
            {
                IPAddress ip = IPAddress.Parse(ConfigureAddress);
                this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(ip));
                this.socket.Close();
                this.closed = true;
            }
        }

        public bool IsClosed()
        {
            return this.closed;
        }

        public void SendMessage(string json)
        {
            foreach (NetworkInterface adapter in this.interfaces)
            {
                if (adapter.GetIPProperties().MulticastAddresses.Count <= 0)
                {
                    continue; // most of VPN adapters will be skipped
                }

                if (!adapter.SupportsMulticast)
                {
                    continue; // multicast is meaningless for this type of connection
                }

                if (OperationalStatus.Up != adapter.OperationalStatus)
                {
                    continue; // this adapter is off or not connected
                }

                IPv4InterfaceProperties p = adapter.GetIPProperties().GetIPv4Properties();
                if (null == p)
                {
                    continue; // IPv4 is not configured on this adapter
                }

                this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, (int)IPAddress.HostToNetworkOrder(p.Index));
                this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, 0);
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                this.socket.Send(buffer, buffer.Length, SocketFlags.None);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.socket.Close();
            }
        }
    }
}
