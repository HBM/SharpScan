// <copyright file="LruCache.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ScanTests, PublicKey=" +
    "00240000048000009400000006020000" +
    "00240000525341310004000001000100" +
    "3d1f0c942caa540613d5e9c7230a1ef8" +
    "ab05266aea901184bb3bc0812b70659f" +
    "7bf79a0bbd2cfae380b8ad3b4321d73b" +
    "2a8b99c7842ab078791d484037e6a025" +
    "9a4d4dac9bc81b1731eb9238969b65db" +
    "8ece659acff236c691e1e02e797ce8c1" +
    "ecbb6bfa3664f01360b41138cb2f1c1f" +
    "5b7a0f4778b883a29cde5cd6131c3be6")]

namespace Hbm.Devices.Scan.Announcing
{
    internal class LruCache<TKey, TValue>
    {
        class Node
        {
            internal Node(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }
            internal TKey key;
            internal TValue value;
        }

        private readonly IDictionary<TKey, LinkedListNode<Node>> map;
        private readonly LinkedList<Node> list;

        private readonly int capacity;
        public int Capacity { get { return capacity; } }
        public Int32 Count { get { return map.Count; } }

        internal LruCache(int capacity)
        {
            map = new Dictionary<TKey, LinkedListNode<Node>>(capacity);
            list = new LinkedList<Node>();
            this.capacity = capacity;
        }

        internal void Add(TKey key, TValue value)
        {
            if (map.Count >= capacity)
            {
                LinkedListNode<Node> nodeToRemove = list.Last;
                list.RemoveLast();
                Node node = nodeToRemove.Value;
                map.Remove(node.key);
            }
            Node newNode = new Node(key, value);
            LinkedListNode<Node> listNode = new LinkedListNode<Node>(newNode);
            map.Add(key, listNode);
            list.AddFirst(listNode);
        }

        internal bool TryGetValue(TKey key, out TValue value)
        {
            LinkedListNode<Node> node;
            if (map.TryGetValue(key, out node))
            {
                list.Remove(node);
                list.AddFirst(node);
                value = node.Value.value;
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
            if (map.TryGetValue(key, out node))
            {
                map.Remove(key);
                list.Remove(node);
            }
        }
    }
}
