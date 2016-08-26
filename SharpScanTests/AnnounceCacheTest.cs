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

using Hbm.Devices.Scan.Announcing;

namespace Hbm.Devices.Scan
{
    using NUnit.Framework;

    class AnnounceCacheTest
    {
        private FakeMessageReceiver fmr;
        private AnnounceDeserializer parser;

        [SetUp]
        public void Setup()
        {
            fmr = new FakeMessageReceiver();
            parser = new AnnounceDeserializer();
            fmr.HandleMessage += parser.HandleEvent;
            parser.HandleMessage += this.HandleEvent;
        }

        [Test]
        public void AddDeviceToCacheTest()
        {
            fmr.EmitSingleCorrectMessage();
            Assert.AreEqual(parser.GetCache().Size(), 1, "Entries in cache != 1");
            Assert.AreEqual(parser.GetCache().LastAnnounceSize(), 1, "Paths in cache != 1");
            Assert.NotNull(parser.GetCache().Get(SharpScanTests.FakeMessages.CorrectMessage), "JSON message not in cache");

            fmr.EmitSingleCorrectMessageDifferentDevice();
            Assert.AreEqual(parser.GetCache().Size(), 2, "Entries in cache != 2");
            Assert.AreEqual(parser.GetCache().LastAnnounceSize(), 2, "Paths in cache != 2");
            Assert.NotNull(parser.GetCache().Get(SharpScanTests.FakeMessages.CorrectMessage), "JSON message not in cache");
            Assert.NotNull(parser.GetCache().Get(SharpScanTests.FakeMessages.CorrectMessageDifferentDevice), "JSON message not in cache");
        }

        [Test]
        public void GetFromCacheTest()
        {
            fmr.EmitSingleCorrectMessage();
            Assert.NotNull(parser.GetCache().Get(SharpScanTests.FakeMessages.CorrectMessage), "Correct message not in cache");
        }

        [Test]
        public void DontAddTwiceTest()
        {
            fmr.EmitSingleCorrectMessage();
            fmr.EmitSingleCorrectMessage();
            Assert.AreEqual(parser.GetCache().Size(), 1, "Entries in cache != 1");
            Assert.AreEqual(parser.GetCache().LastAnnounceSize(), 1, "Paths in cache != 1");
        }

        [Test]
        public void UpdateDeviceEntry()
        {
            fmr.EmitSingleCorrectMessage();
            fmr.EmitSingleCorrectMessageDifferentServices();
            Assert.AreEqual(parser.GetCache().Size(), 1, "Entries in cache != 1");
            Assert.AreEqual(parser.GetCache().LastAnnounceSize(), 1, "Paths in cache != 1");
            Assert.Null(parser.GetCache().Get(SharpScanTests.FakeMessages.CorrectMessage), "Original message still in cache");
            Assert.NotNull(parser.GetCache().Get(SharpScanTests.FakeMessages.CorretMessageDifferentServices), "New message not in cache");
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
