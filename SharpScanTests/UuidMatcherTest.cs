// <copyright file="UuidMatcherTest.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    class UuidMatcherTest
    {
        private Announce filteredAnnounce;
        private Announce announce;
        private FakeMessageReceiver fmr;
        private string[] uuids = { "fred", "0009E500123A", "blah" };
        private UuidMatcher matcher;
        AnnounceFilter filter;

        [SetUp]
        public void setup()
        {
            matcher = new UuidMatcher(uuids);
            filteredAnnounce = null;
            fmr = new FakeMessageReceiver();
            AnnounceDeserializer parser = new AnnounceDeserializer();
            fmr.HandleMessage += parser.HandleEvent;
            filter = new AnnounceFilter(matcher);
            parser.HandleMessage += filter.HandleEvent;
            parser.HandleMessage += this.HandleAnnounceEvent;
            filter.HandleMessage += this.HandleFilteredEvent;
        }

        [Test]
        public void CheckUuidsTest()
        {
            Assert.AreEqual(uuids, matcher.GetFilterStrings());
            Assert.AreEqual(matcher, filter.Matcher);
        }

        [Test]
        public void NoFamilyTypesTest()
        {
            Assert.Throws<ArgumentNullException>(() => new UuidMatcher(null));
        }

        [Test]
        public void FilterCorrectMessageTest()
        {
            fmr.EmitSingleCorrectMessage();
            Assert.NotNull(filteredAnnounce, "Didn't got an Announce object");
        }

        [Test]
        public void FilterCorrectMessageNoMatchTest()
        {
            fmr.EmitCorrectMessageUuid123b();
            Assert.NotNull(announce, "No announce parsed");
            Assert.Null(filteredAnnounce, "Got an Announce object despite non-matching UUID");
        }

        [Test]
        public void FilterMissingUuidTest()
        {
            fmr.EmitMissingDeviceUuidMessage();
            Assert.Null(filteredAnnounce, "Got an Announce object despite existant UUID");
        }

        private void HandleFilteredEvent(object sender, AnnounceEventArgs args)
        {
            filteredAnnounce = args.Announce;
        }

        private void HandleAnnounceEvent(object sender, AnnounceEventArgs args)
        {
            announce = args.Announce;
        }
    }
}
