// <copyright file="InterfaceMatcherTest.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    using Announcing;
    using Announcing.Filter;
    using NUnit.Framework;

    [TestFixture]
    internal class InterfaceMatcherTest
    {
        private Announce filteredAnnounce;
        private Announce announce;
        private FakeMessageReceiver fmr;
        private InterfaceMatcher matcher;
        private AnnounceFilter filter;

        [SetUp]
        public void Setup()
        {
            ScanInterfaces scanInterfaces = new ScanInterfaces();
            this.matcher = new InterfaceMatcher(scanInterfaces.NetworkInterfaces);
            this.filteredAnnounce = null;
            this.fmr = new FakeMessageReceiver();
            AnnounceDeserializer parser = new AnnounceDeserializer();
            this.fmr.HandleMessage += parser.HandleEvent;
            this.filter = new AnnounceFilter(this.matcher);
            parser.HandleMessage += this.filter.HandleEvent;
            parser.HandleMessage += this.HandleAnnounceEvent;
            this.filter.HandleMessage += this.HandleFilteredEvent;
        }

        [Test]
        public void FilterCorrectMessageTest()
        {
            this.fmr.EmitSingleCorrectMessage();
            Assert.NotNull(this.filteredAnnounce, "Didn't got an Announce object");
        }

        [Test]
        public void WrongArgumentsTest()
        {
            // coverity[var_deref_model]
            Assert.Throws<ArgumentNullException>(() => new InterfaceMatcher(null));

            // coverity[var_deref_model]
            Assert.Throws<ArgumentNullException>(() => this.matcher.AddInterface(null));

            // coverity[var_deref_model]
            Assert.Throws<ArgumentNullException>(() => this.matcher.RemoveInterface(null));
        }

        [Test]
        public void FilterWrongInterfaceTest()
        {
            this.matcher.RemoveInterface(this.fmr.IncomingInterface);
            this.fmr.EmitSingleCorrectMessage();
            Assert.NotNull(this.announce, "No announce parsed");
            Assert.Null(this.filteredAnnounce, "Got Announce object despite wrong incoming interface");

            this.matcher.AddInterface(this.fmr.IncomingInterface);
            this.fmr.EmitSingleCorrectMessage();
            Assert.NotNull(this.filteredAnnounce, "Didn't got an Announce object");
        }

        [Test]
        public void HandleEventWithAnnounceNull()
        {
            AnnounceEventArgs args = new AnnounceEventArgs();
            args.Announce = null;
            filter.HandleEvent(null, args);
            Assert.Null(filteredAnnounce, "got an Announce object");
        }

        private void HandleFilteredEvent(object sender, AnnounceEventArgs args)
        {
            this.filteredAnnounce = args.Announce;
        }

        private void HandleAnnounceEvent(object sender, AnnounceEventArgs args)
        {
            this.announce = args.Announce;
        }
    }
}
