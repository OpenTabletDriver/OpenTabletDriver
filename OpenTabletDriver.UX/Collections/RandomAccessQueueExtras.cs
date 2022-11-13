using System;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.UX.Collections;

namespace OpenTabletDriver.UX.Collections
{
    public partial class RandomAccessQueue<T>
    {
        public T this[int index]
        {
            get
            {
                // Following trick can reduce the range check by one
                if ((uint)index >= (uint)this._size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, SR.ArgumentOutOfRange_IndexMustBeLess);
                }
                return _array[arrayIndex(index)];
            }
            set
            {
                if ((uint)index >= (uint)_size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, SR.ArgumentOutOfRange_IndexMustBeLess);
                }

                _array[arrayIndex(index)] = value;

            }
        }

        public int IndexOf(T item)
        {
            if (_size == 0)
            {
                return -1;
            }

            if (_head < _tail)
            {
                return Array.IndexOf(_array, item, _head, _size) - _head;
            }

            // We've wrapped around. Check both partitions, the least recently enqueued first.
            int firstIndex = Array.IndexOf(_array, item, _head, _array.Length - _head);

            if (firstIndex >= 0)
            {
                return firstIndex - _head;
            }
            return Array.IndexOf(_array, item, 0, _tail) + _array.Length - _head;
        }

        private int arrayIndex(int index)
        {
            int capacity = _array.Length;

            // _index represents the 0-based index into the queue, however the queue
            // doesn't have to start from 0 and it may not even be stored contiguously in memory.

            int arrayIndex = _head + index; // this is the actual index into the queue's backing array
            if (arrayIndex >= capacity)
            {
                // NOTE: Originally we were using the modulo operator here, however
                // on Intel processors it has a very high instruction latency which
                // was slowing down the loop quite a bit.
                // Replacing it with simple comparison/subtraction operations sped up
                // the average foreach loop by 2x.

                arrayIndex -= capacity; // wrap around if needed
            }
            return arrayIndex;
        }

        internal class SR
        {
            internal static readonly string Arg_RankMultiDimNotSupported = "Only single dimensional arrays are supported for the requested action.";
            internal static readonly string ArgumentOutOfRange_IndexMustBeLessOrEqual = "Index was out of range. Must be non-negative and less than or equal to the size of the collection.";
            internal static readonly string Argument_InvalidOffLen = "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.";
            internal static readonly string Arg_NonZeroLowerBound = "The lower bound of target array must be zero.";
            internal static readonly string Argument_InvalidArrayType = "Target array type is not compatible with the type of items in the collection.";
            internal static readonly string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
            internal static readonly string InvalidOperation_EnumFailedVersion = "Collection was modified; enumeration operation may not execute.";
            internal static readonly string InvalidOperation_EnumNotStarted = "Enumeration has not started. Call MoveNext.";
            internal static readonly string InvalidOperation_EnumEnded = "Enumeration already finished.";
            internal static readonly string ArgumentOutOfRange_IndexMustBeLess = "Index was out of range. Must be non-negative and less than the size of the collection.";
            internal static readonly string InvalidOperation_EmptyQueue = "Queue empty.";
        }

    }
}


