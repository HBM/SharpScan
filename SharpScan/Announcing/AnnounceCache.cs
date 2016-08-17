// <copyright file="AnnounceCache.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

    internal class AnnounceCache
    {
        public const int DefaultCacheSize = 10000;

        private readonly LruCache<string, Announce> parsedMessages;
        private readonly LruCache<string, string> lastDeviceAnnounceString;

        internal AnnounceCache() : this(DefaultCacheSize)
        {
        }

        internal AnnounceCache(int cacheSize)
        {
            this.parsedMessages = new LruCache<string, Announce>(cacheSize);
            this.lastDeviceAnnounceString = new LruCache<string, string>(cacheSize);
        }

        internal Announce Get(string announceString)
        {
            Announce announce;
            this.parsedMessages.TryGetValue(announceString, out announce);
            return announce;
        }

        internal void Add(string announceString, Announce announce)
        {
            string path = announce.Path;
            string lastAnnounceString;
            if (this.lastDeviceAnnounceString.TryGetValue(path, out lastAnnounceString))
            {
                this.parsedMessages.Remove(lastAnnounceString);
                this.lastDeviceAnnounceString.Remove(path);
            }

            this.parsedMessages.Add(announceString, announce);
            this.lastDeviceAnnounceString.Add(path, announceString);
        }

        internal int Size()
        {
            return this.parsedMessages.Count;
        }

        internal int LastAnnounceSize()
        {
            return this.lastDeviceAnnounceString.Count;
        }
    }
}
