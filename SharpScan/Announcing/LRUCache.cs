// <copyright file="LruCache.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SharpScanTests")]

namespace Hbm.Devices.Scan.Announcing
{
    internal class LruCache<TKey, TValue>
    {
        private readonly IDictionary<TKey, LinkedListNode<Node>> map;

        private readonly LinkedList<Node> list;

        private readonly int capacity;

        internal LruCache(int capacity)
        {
            this.map = new Dictionary<TKey, LinkedListNode<Node>>(capacity);
            this.list = new LinkedList<Node>();
            this.capacity = capacity;
        }

        public int Capacity
        {
            get
            {
                return this.capacity;
            }
        }

        public int Count
        {
            get
            {
                return this.map.Count;
            }
        }

        internal void Add(TKey key, TValue value)
        {
            if (this.map.Count >= this.capacity)
            {
                LinkedListNode<Node> nodeToRemove = this.list.Last;
                this.list.RemoveLast();
                Node node = nodeToRemove.Value;
                this.map.Remove(node.Key);
            }

            Node newNode = new Node(key, value);
            LinkedListNode<Node> listNode = new LinkedListNode<Node>(newNode);
            this.map.Add(key, listNode);
            this.list.AddFirst(listNode);
        }

        internal bool TryGetValue(TKey key, out TValue value)
        {
            LinkedListNode<Node> node;
            if (this.map.TryGetValue(key, out node))
            {
                this.list.Remove(node);
                this.list.AddFirst(node);
                value = node.Value.Value;
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        internal void Remove(TKey key)
        {
            LinkedListNode<Node> node;
            if (this.map.TryGetValue(key, out node))
            {
                this.map.Remove(key);
                this.list.Remove(node);
            }
        }

        internal class Node
        {
            private TKey key;

            private TValue value;

            internal Node(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }

            public TKey Key
            {
                get
                {
                    return this.key;
                }
            }

            public TValue Value
            {
                get
                {
                    return this.value;
                }
            }
        }
    }
}
