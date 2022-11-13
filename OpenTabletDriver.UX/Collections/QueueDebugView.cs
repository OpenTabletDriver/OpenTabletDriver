// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace OpenTabletDriver.UX.Collections
{
    internal sealed class RandomAccessQueueDebugView<T>
    {
        private readonly RandomAccessQueue<T> _queue;

        public RandomAccessQueueDebugView(RandomAccessQueue<T> queue)
        {
            ArgumentNullException.ThrowIfNull(queue);

            _queue = queue;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                return _queue.ToArray();
            }
        }
    }
}
