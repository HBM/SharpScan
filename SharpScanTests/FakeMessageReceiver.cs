// <copyright file="FakeMessageReceiver.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    using System;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;

    class FakeMessageReceiver
    {
        public FakeMessageReceiver()
        {
            ScanInterfaces scanInterfaces = new ScanInterfaces();
            IList<NetworkInterface> ifaces = scanInterfaces.NetworkInterfaces;
            if (ifaces.Count <= 0)
            {
                throw new IndexOutOfRangeException("No network interfaces");
            }
            incomingInterface = ifaces[0];
        }

        private NetworkInterface incomingInterface;
        public NetworkInterface IncomingInterface { get { return incomingInterface; } }
        public event EventHandler<MulticastMessageEventArgs> HandleMessage;

        public void EmitSingleCorrectMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.CorrectMessage);
        }

        public void EmitCorrectMessageUuid123b()
        {
            SendMessage(SharpScanTests.FakeMessages.CorrectMessage_UUID0009E500123B);
        }

        public void EmitSingleCorrectMessageDifferentDevice()
        {
            SendMessage(SharpScanTests.FakeMessages.CorrectMessageDifferentDevice);
        }

        public void EmitSingleCorrectMessageDifferentServices()
        {
            SendMessage(SharpScanTests.FakeMessages.CorretMessageDifferentServices);
        }

        public void EmitSingleCorrectMessageDifferentServicesShortExpire()
        {
            SendMessage(SharpScanTests.FakeMessages.CorretMessageDifferentServicesShortExpire);
        }

        public void EmitSingleCorrectMessageShortExpire()
        {
            SendMessage(SharpScanTests.FakeMessages.CorrectMessageShortExpire);
        }

        public void EmitInvalidJsonMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.InvalidJsonMessage);
        }

        public void EmitNotAnnounceMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.NotAnnounceMessage);
        }

        public void EmitEmptyString()
        {
            SendMessage("");
        }

        public void EmitNullMessage()
        {
            SendMessage(null);
        }

        public void EmitMissingDeviceMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.MissingDeviceMessage);
        }

        public void EmitEmptyDeviceUuidMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.EmptyDeviceUuidMessage);
        }

        public void EmitMissingDeviceUuidMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.MissingDeviceUuidMessage);
        }

        public void EmitMissingParamsMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.MissingParamsMessage);
        }

        public void EmitNoInterfaceNameMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.NoInterfaceNameMessage);
        }

        public void EmitEmptyInterfaceNameMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.EmptyInterfaceNameMessage);
        }

        public void EmitNoInterfaceMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.NoInterfaceMessage);
        }

        public void EmitNoNetSettingsMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.NoNetSettingsMessage);
        }

        public void EmitMissingRouterUuidMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.MissingRouterUuidMessage);
        }

        public void EmitEmptyRouterUuidMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.EmptyRouterUuidMessage);
        }

        public void EmitMissingMethodMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.MissingMethodMessage);
        }

        public void EmitNoVersionMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.NoVersionMessage);
        }

        public void EmitWrongVersionMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.WrongVersionMessage);
        }

        public void EmitMessageNoExpire()
        {
            SendMessage(SharpScanTests.FakeMessages.CorrectMessageNoExpire);
        }

        public void EmitMessageNegativeExpire()
        {
            SendMessage(SharpScanTests.FakeMessages.CorrectMessageNegativeExpire);
        }

        public void EmitMessageNoServices()
        {
            SendMessage(SharpScanTests.FakeMessages.CorrectMessageNoServices);
        }

        public void EmitMissingFamilytypeMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.MissingFamilytypeMessage);
        }

        public void EmitNoJsonRpcMessage()
        {
            SendMessage(SharpScanTests.FakeMessages.NoJsonRpc);
        }

        public void EmitNoIPv4Message()
        {
            SendMessage(SharpScanTests.FakeMessages.CorrectMessageNoIPv4);
        }

        public void EmitNoIPv6Message()
        {
            SendMessage(SharpScanTests.FakeMessages.CorrectMessageNoIPv6);
        }

        public void SendMessage(string json)
        {
            MulticastMessageEventArgs ev = new MulticastMessageEventArgs();
            ev.JsonString = json;
            ev.IncomingInterface = incomingInterface;
            this.HandleMessage(this, ev);
        }
    }
}
