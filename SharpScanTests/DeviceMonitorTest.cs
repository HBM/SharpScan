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
    internal class DeviceMonitorTest
    {
        private bool newDevice;
        private bool updateDevice;
        private bool removeDevice;
        private Announce announce;
        private Announce oldAnnounce;

        private FakeMessageReceiver fmr;
        private DeviceMonitor monitor;

        [SetUp]
        public void Setup()
        {
            this.newDevice = false;
            this.updateDevice = false;
            this.removeDevice = false;
            this.announce = null;
            this.oldAnnounce = null;

            this.fmr = new FakeMessageReceiver();
            AnnounceDeserializer parser = new AnnounceDeserializer();
            this.fmr.HandleMessage += parser.HandleEvent;
            this.monitor = new DeviceMonitor();
            parser.HandleMessage += this.monitor.HandleEvent;
            this.monitor.HandleNewDevice += this.HandleNewDeviceEvent;
            this.monitor.HandleRemoveDevice += this.HandleRemoveDeviceEvent;
            this.monitor.HandleUpdateDevice += this.HandleUpdateDeviceEvent;
        }

        [Test]
        public void NewDeviceEventTest()
        {
            this.fmr.EmitSingleCorrectMessage();
            Assert.True(this.newDevice && !this.updateDevice && !this.removeDevice, "No new device event fired");
            Assert.NotNull(this.announce, "No announce in event");
        }

        [Test]
        public void UpdateDeviceEventTest()
        {
            this.fmr.EmitSingleCorrectMessage();
            Assert.True(this.newDevice && !this.updateDevice && !this.removeDevice, "No new device event fired");
            Assert.NotNull(this.announce, "No announce in event");
            this.newDevice = false;

            this.fmr.EmitSingleCorrectMessageDifferentServices();
            Assert.True(!this.newDevice && this.updateDevice && !this.removeDevice, "No update device event fired");
            Assert.NotNull(this.announce, "No new announce in event");
            Assert.NotNull(this.oldAnnounce, "No old announce in event");
            this.newDevice = false;
            this.removeDevice = false;
            this.updateDevice = false;

            this.fmr.EmitSingleCorrectMessageDifferentServices();
            Assert.True(!this.newDevice && !this.updateDevice && !this.removeDevice, "Update event fired twice");
        }

        [Test]
        public void RemoveDeviceEventTest()
        {
            this.fmr.EmitSingleCorrectMessageShortExpire();
            Thread.Sleep(3000);
            Assert.True(this.newDevice && !this.updateDevice && this.removeDevice, "No remove event fired");
            Assert.NotNull(this.announce, "No announce in event");
        }

        [Test]
        public void StopWithoutRunningTimerTest()
        {
            Assert.False(this.monitor.IsClosed(), "DeviceMonitor monitor");
            this.monitor.Close();
            Assert.True(this.monitor.IsClosed(), "DeviceMonitor not closed");
        }

        [Test]
        public void StopWithRunningTimerTest()
        {
            Assert.False(this.monitor.IsClosed(), "DeviceMonitor monitor");
            this.fmr.EmitSingleCorrectMessage();
            this.monitor.Close();
            Assert.True(this.monitor.IsClosed(), "DeviceMonitor not closed");
        }

        [Test]
        public void MessageAfterCloseTest()
        {
            Assert.False(this.monitor.IsClosed(), "DeviceMonitor monitor");
            this.monitor.Close();
            Assert.True(this.monitor.IsClosed(), "DeviceMonitor not closed");
            this.fmr.EmitSingleCorrectMessage();
            Assert.True(!this.newDevice && !this.updateDevice && !this.removeDevice, "Got event after closing monitor");
        }

        [Test]
        public void HandleEventParameterChecking()
        {
            Assert.DoesNotThrow(
                delegate 
                {
                    this.monitor.HandleNewDevice -= this.HandleNewDeviceEvent;
                    this.monitor.HandleUpdateDevice -= this.HandleUpdateDeviceEvent;
                    this.monitor.HandleRemoveDevice -= this.HandleRemoveDeviceEvent;
                    this.fmr.EmitSingleCorrectMessageShortExpire();
                    this.fmr.EmitSingleCorrectMessageDifferentServicesShortExpire();
                    Thread.Sleep(3000);
                },
                "monitor throw exception without attached HandleRemoveDeviceEvent",
                "null");

            Assert.True(!this.newDevice && !this.updateDevice && !this.removeDevice, "Events fired without event handler attached");
            Assert.Null(this.announce, "Announce object not null");
        }

        [Test]
        public void HandleEventWithNoArgs()
        {
            Assert.DoesNotThrow(
                delegate
                {
                    this.monitor.HandleEvent(null, null);
                },
                "monitor throw exception when HandleEvent called wiht null args",
                "null");

            Assert.True(!this.newDevice && !this.updateDevice && !this.removeDevice, "Events fired without event handler attached");
            Assert.Null(this.announce, "Announce object not null");
        }

        public void HandleNewDeviceEvent(object sender, NewDeviceEventArgs args)
        {
            this.newDevice = true;
            this.announce = args.Announce;
        }

        public void HandleRemoveDeviceEvent(object sender, RemoveDeviceEventArgs args)
        {
            this.removeDevice = true;
            this.announce = args.Announce;
        }

        public void HandleUpdateDeviceEvent(object sender, UpdateDeviceEventArgs args)
        {
            this.updateDevice = true;
            this.announce = args.NewAnnounce;
            this.oldAnnounce = args.OldAnnounce;
        }
    }
}
