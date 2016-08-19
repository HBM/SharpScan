// <copyright file="LRUCacheTest.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

namespace Hbm.Devices.Scan.Announcing
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    internal class LRUCacheTest
    {
        LruCache<string, string> cache;

        [SetUp]
        public void setup()
        {
            cache = new LruCache<string, string>(3);
        }

        [Test]
        public void DropTest()
        {
            cache.Add("key1", "val1");
            cache.Add("key2", "val2");
            cache.Add("key3", "val3");
            cache.Add("key4", "val4");

            string value;
            Assert.False(cache.TryGetValue("key1", out value), "Eldest element still in cache");
            Assert.True(cache.TryGetValue("key2", out value), "New element not in cache");
            Assert.True(cache.TryGetValue("key3", out value), "New element not in cache");
            Assert.True(cache.TryGetValue("key4", out value), "New element not in cache");
        }

        [Test]
        public void CountTest()
        {
            Int32 capacity = 10;
            cache = new LruCache<string, string>(capacity);
            Assert.AreEqual(cache.Capacity, capacity);
        }
    }
}
