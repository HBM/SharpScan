﻿// <copyright file="AnnounceCache.cs" company="Hottinger Baldwin Messtechnik GmbH">
//
// CS Scan, a library for scanning and configuring HBM devices.
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

namespace Hbm.Devices.Scan.Announcing
{
    internal class AnnounceCache
    {
        public const Int32 DefaultCacheSize = 10000;

        private readonly LruCache<string, Announce> parsedMessages;
        private readonly LruCache<string, string> lastDeviceAnnounceString;

        internal AnnounceCache() : this(DefaultCacheSize) { }
        internal AnnounceCache(Int32 cacheSize)
        {
            parsedMessages = new LruCache<string, Announce>(cacheSize);
            lastDeviceAnnounceString = new LruCache<string, string>(cacheSize);
        }

        internal Announce Get(string announceString)
        {
            Announce announce;
            parsedMessages.TryGetValue(announceString, out announce);
            return announce;
        }

        internal void Add(string announceString, Announce announce)
        {
            string path = announce.Path;
            string lastAnnounceString;
            if (lastDeviceAnnounceString.TryGetValue(path, out lastAnnounceString))
            {
                parsedMessages.Remove(lastAnnounceString);
                lastDeviceAnnounceString.Remove(path);
            }
            parsedMessages.Add(announceString, announce);
            lastDeviceAnnounceString.Add(path, announceString);
        }

        internal int Size()
        {
            return parsedMessages.Count;
        }

        internal int LastAnnounceSize()
        {
            return lastDeviceAnnounceString.Count;
        }
    }
}
