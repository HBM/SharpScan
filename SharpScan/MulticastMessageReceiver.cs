// <copyright file="MulticastMessageReceiver.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

[assembly: CLSCompliant(true)]

namespace Hbm.Devices.Scan
{
    public class MulticastMessageReceiver
    {
        private readonly Socket socket;
        private readonly IPAddress multicastIP;
        private readonly byte[] receiveBuffer;

        private readonly Dictionary<int, NetworkInterface> interfaceMap;
        private readonly List<IPAddress> multicastInterfaces;
        private readonly MulticastMessageEventArgs eventArgs;

        private EndPoint endPoint;

        public MulticastMessageReceiver(string multicastIP, int port)
            : this(IPAddress.Parse(multicastIP), port)
        {
        }

        public MulticastMessageReceiver(IPAddress address, int port)
        {
            this.multicastIP = address;
            this.interfaceMap = new Dictionary<int, NetworkInterface>();
            this.multicastInterfaces = new List<IPAddress>();

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.eventArgs = new MulticastMessageEventArgs();
            try
            {
                s.ReceiveBufferSize = 128000;
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
                s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, 0);
                s.Bind(ipep);

                this.socket = s;
                this.receiveBuffer = new byte[65536];
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                this.endPoint = (EndPoint)sender;

                NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(this.AddressChangedCallback);

                this.AddMulticastMembership();

                s.BeginReceiveMessageFrom(
                    this.receiveBuffer,
                    0,
                    this.receiveBuffer.Length,
                    SocketFlags.None,
                    ref this.endPoint,
                    new AsyncCallback(this.MessageComplete),
                    null);
            }
            catch
            {
                s.Dispose();
                throw;
            }
        }

        ~MulticastMessageReceiver()
        {
            NetworkChange.NetworkAddressChanged -= this.AddressChangedCallback;
            if (this.socket != null)
            {
                this.socket.Close();
            }
        }

        public event EventHandler<MulticastMessageEventArgs> HandleMessage;

        private static NetworkInterface GetInterface(int index)
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                try
                {
                    IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                    IPv4InterfaceProperties ip4 = adapterProperties.GetIPv4Properties();
                    if ((ip4 != null) && (ip4.Index == index))
                    {
                        return adapter;
                    }

                    IPv6InterfaceProperties ip6 = adapterProperties.GetIPv6Properties();
                    if ((ip6 != null) && (ip6.Index == index))
                    {
                        return adapter;
                    }
                }
                catch (NetworkInformationException e)
                {
                    if (e.NativeErrorCode != (int)System.Net.Sockets.SocketError.ProtocolNotSupported)
                    {
                        throw;
                    }
                }
            }

            return null;
        }

        private void AddressChangedCallback(object sender, EventArgs e)
        {
            this.DropMulticastMembership();
            this.AddMulticastMembership();
        }

        private void DropMulticastMembership()
        {
            lock (this.multicastInterfaces)
            {
                foreach (IPAddress address in this.multicastInterfaces)
                {
                    this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(this.multicastIP, address));
                }

                this.multicastInterfaces.Clear();
                this.interfaceMap.Clear();
            }
        }

        private void AddMulticastMembership()
        {
            lock (this.multicastInterfaces)
            {
                this.multicastInterfaces.Clear();
                this.interfaceMap.Clear();

                NetworkInterface[] ifaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in ifaces)
                {
                    if (ni.SupportsMulticast &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        (ni.OperationalStatus == OperationalStatus.Up || ni.OperationalStatus == OperationalStatus.Unknown))
                    {
                        UnicastIPAddressInformationCollection addresses = ni.GetIPProperties().UnicastAddresses;
                        foreach (UnicastIPAddressInformation address in addresses)
                        {
                            IPAddress addr = address.Address;
                            if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(this.multicastIP, addr));
                                this.multicastInterfaces.Add(addr);
                            }
                        }
                    }
                }
            }
        }

        private void MessageComplete(IAsyncResult result)
        {
            var flags = SocketFlags.None;
            IPPacketInformation packetInformation;
            try
            {
                int received = this.socket.EndReceiveMessageFrom(result, ref flags, ref this.endPoint, out packetInformation);
                if (received > 0)
                {
                    NetworkInterface incomingIF = this.LookupIncomingInterface(packetInformation.Interface);
                    string message = System.Text.Encoding.ASCII.GetString(this.receiveBuffer, 0, received);

                    if (this.HandleMessage != null)
                    {
                        this.eventArgs.IncomingInterface = incomingIF;
                        this.eventArgs.JsonString = message;
                        this.HandleMessage(this, this.eventArgs);
                    }
                }

                this.socket.BeginReceiveMessageFrom(
                    this.receiveBuffer,
                    0,
                    this.receiveBuffer.Length,
                    SocketFlags.None,
                    ref this.endPoint,
                    new AsyncCallback(this.MessageComplete),
                    null);
            }
            catch (ObjectDisposedException)
            {
                // Deliberatly ignored. This exception shows that the socket was closed.
                return;
            }
        }

        private NetworkInterface LookupIncomingInterface(int index)
        {
            NetworkInterface iface = null;
            if (this.interfaceMap.ContainsKey(index))
            {
                iface = this.interfaceMap[index];
            }
            else
            {
                iface = GetInterface(index);
                if (iface != null)
                {
                    this.interfaceMap[index] = iface;
                }
            }

            return iface;
        }
    }
}
