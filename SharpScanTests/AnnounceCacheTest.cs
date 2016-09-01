// <copyright file="AnnounceCacheTest.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    using Hbm.Devices.Scan.Announcing;
    using NUnit.Framework;

    internal class AnnounceCacheTest
    {
        private FakeMessageReceiver fmr;
        private AnnounceDeserializer parser;

        [SetUp]
        public void Setup()
        {
            this.fmr = new FakeMessageReceiver();
            this.parser = new AnnounceDeserializer();
            this.fmr.HandleMessage += this.parser.HandleEvent;
            this.parser.HandleMessage += this.HandleEvent;
        }

        [Test]
        public void AddDeviceToCacheTest()
        {
            this.fmr.EmitSingleCorrectMessage();
            Assert.AreEqual(this.parser.GetCache().Size(), 1, "Entries in cache != 1");
            Assert.AreEqual(this.parser.GetCache().LastAnnounceSize(), 1, "Paths in cache != 1");
            Assert.NotNull(this.parser.GetCache().Get(SharpScanTests.FakeMessages.CorrectMessage), "JSON message not in cache");

            this.fmr.EmitSingleCorrectMessageDifferentDevice();
            Assert.AreEqual(this.parser.GetCache().Size(), 2, "Entries in cache != 2");
            Assert.AreEqual(this.parser.GetCache().LastAnnounceSize(), 2, "Paths in cache != 2");
            Assert.NotNull(this.parser.GetCache().Get(SharpScanTests.FakeMessages.CorrectMessage), "JSON message not in cache");
            Assert.NotNull(this.parser.GetCache().Get(SharpScanTests.FakeMessages.CorrectMessageDifferentDevice), "JSON message not in cache");
        }

        [Test]
        public void GetFromCacheTest()
        {
            this.fmr.EmitSingleCorrectMessage();
            Assert.NotNull(this.parser.GetCache().Get(SharpScanTests.FakeMessages.CorrectMessage), "Correct message not in cache");
        }

        [Test]
        public void DontAddTwiceTest()
        {
            this.fmr.EmitSingleCorrectMessage();
            this.fmr.EmitSingleCorrectMessage();
            Assert.AreEqual(this.parser.GetCache().Size(), 1, "Entries in cache != 1");
            Assert.AreEqual(this.parser.GetCache().LastAnnounceSize(), 1, "Paths in cache != 1");
        }

        [Test]
        public void UpdateDeviceEntry()
        {
            this.fmr.EmitSingleCorrectMessage();
            this.fmr.EmitSingleCorrectMessageDifferentServices();
            Assert.AreEqual(this.parser.GetCache().Size(), 1, "Entries in cache != 1");
            Assert.AreEqual(this.parser.GetCache().LastAnnounceSize(), 1, "Paths in cache != 1");
            Assert.Null(this.parser.GetCache().Get(SharpScanTests.FakeMessages.CorrectMessage), "Original message still in cache");
            Assert.NotNull(this.parser.GetCache().Get(SharpScanTests.FakeMessages.CorretMessageDifferentServices), "New message not in cache");
        }

        [Test]
        public void CreateCacheDefaultSize()
        {
            AnnounceCache cache = new AnnounceCache();
            Assert.AreEqual(cache.Capacity(), AnnounceCache.DefaultCacheSize, "announce cache size is not default");
        }

        private void HandleEvent(object sender, AnnounceEventArgs args)
        {
        }
    }
}
