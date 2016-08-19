// <copyright file="DeviceMonitorTest.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    using System.Threading;
    using Announcing;
    using NUnit.Framework;

    [TestFixture]
    class DeviceMonitorTest
    {
        private bool newDevice;
        private bool updateDevice;
        private bool removeDevice;
        private Announce announce;
        private Announce oldAnnounce;

        private FakeMessageReceiver fmr;
        private DeviceMonitor monitor;

        [SetUp]
        public void setup()
        {
            newDevice = false;
            updateDevice = false;
            removeDevice = false;
            announce = null;
            oldAnnounce = null;

            fmr = new FakeMessageReceiver();
            AnnounceDeserializer parser = new AnnounceDeserializer();
            fmr.HandleMessage += parser.HandleEvent;
            monitor = new DeviceMonitor();
            parser.HandleMessage += monitor.HandleEvent;
            monitor.HandleNewDevice += this.HandleNewDeviceEvent;
            monitor.HandleRemoveDevice += this.HandleRemoveDeviceEvent;
            monitor.HandleUpdateDevice += this.HandleUpdateDeviceEvent;
        }

        [Test]
        public void NewDeviceEventTest()
        {
            fmr.EmitSingleCorrectMessage();
            Assert.True(newDevice && !updateDevice && !removeDevice, "No new device event fired");
            Assert.NotNull(announce, "No announce in event");
        }

        [Test]
        public void UpdateDeviceEventTest()
        {
            fmr.EmitSingleCorrectMessage();
            Assert.True(newDevice && !updateDevice && !removeDevice, "No new device event fired");
            Assert.NotNull(announce, "No announce in event");
            newDevice = false;

            fmr.EmitSingleCorrectMessageDifferentServices();
            Assert.True(!newDevice && updateDevice && !removeDevice, "No update device event fired");
            Assert.NotNull(announce, "No new announce in event");
            Assert.NotNull(oldAnnounce, "No old announce in event");
            newDevice = false;
            removeDevice = false;
            updateDevice = false;

            fmr.EmitSingleCorrectMessageDifferentServices();
            Assert.True(!newDevice && !updateDevice && !removeDevice, "Update event fired twice");
        }

        [Test]
        public void RemoveDeviceEventTest()
        {
            fmr.EmitSingleCorrectMessageShortExpire();
            Thread.Sleep(3000);
            Assert.True(newDevice && !updateDevice && removeDevice, "No remove event fired");
            Assert.NotNull(announce, "No announce in event");
        }

        [Test]
        public void StopWithoutRunningTimerTest()
        {
            Assert.False(monitor.IsClosed(), "DeviceMonitor monitor");
            monitor.Close();
            Assert.True(monitor.IsClosed(), "DeviceMonitor not closed");
        }

        [Test]
        public void StopWithRunningTimerTest()
        {
            Assert.False(monitor.IsClosed(), "DeviceMonitor monitor");
            fmr.EmitSingleCorrectMessage();
            monitor.Close();
            Assert.True(monitor.IsClosed(), "DeviceMonitor not closed");
        }

        [Test]
        public void MessageAfterCloseTest()
        {
            Assert.False(monitor.IsClosed(), "DeviceMonitor monitor");
            monitor.Close();
            Assert.True(monitor.IsClosed(), "DeviceMonitor not closed");
            fmr.EmitSingleCorrectMessage();
            Assert.True(!newDevice && !updateDevice && !removeDevice, "Got event after closing monitor");
        }

        public void HandleNewDeviceEvent(object sender, NewDeviceEventArgs args)
        {
            newDevice = true;
            announce = args.Announce;
        }

        public void HandleRemoveDeviceEvent(object sender, RemoveDeviceEventArgs args)
        {
            removeDevice = true;
            announce = args.Announce;
        }

        public void HandleUpdateDeviceEvent(object sender, UpdateDeviceEventArgs args)
        {
            updateDevice = true;
            announce = args.NewAnnounce;
            oldAnnounce = args.OldAnnounce;
        }
    }
}
