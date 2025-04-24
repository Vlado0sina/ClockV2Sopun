using System;

namespace PriorityQueue
{
    public class SortedArrayPriorityQueue<T> : PriorityQueue<T>
    {
        private readonly PriorityItem<T>[] storage;
        private readonly int capacity;
        private int tailIndex;

        public SortedArrayPriorityQueue(int size)
        {
            storage = new PriorityItem<T>[size];
            capacity = size;
            tailIndex = -1;
        }

        public T Head()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("Queue is empty");
            }
            return storage[0].Item;
        }

        public void Add(T item, int priority)
        {
            if (tailIndex >= capacity - 1)
            {
                throw new InvalidOperationException("Queue is full");
            }

            int i = tailIndex;
            while (i >= 0 && storage[i] != null && storage[i].Priority < priority)
            {
                storage[i + 1] = storage[i];
                i--;
            }
            storage[i + 1] = new PriorityItem<T>(item, priority);
            tailIndex++;
        }

        public void Remove(int index)
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("Queue is empty");
            }

            if (index < 0 || index > tailIndex)
            {
                throw new InvalidOperationException("Index is out of range");
            }

            for (int i = index; i < tailIndex; i++)
            {
                storage[i] = storage[i + 1];
            }
            storage[tailIndex] = null;
            tailIndex--;
        }

        public bool IsEmpty()
        {
            return tailIndex < 0;
        }
        public int size
        {
            get { return tailIndex + 1; } 
        }

        public T getAt( int index)
        {
            if (index < 0 || index > tailIndex)
            {
                throw new ArgumentOutOfRangeException("Index is out of range");
            }
            if (storage[index] == null)
            {
                throw new ArgumentOutOfRangeException("Item at the requested index does not exist");
            }
            return storage[index].Item;
        }

        public override string ToString()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("No items to display");
            }

            string result = "[";
            for (int i = 0; i <= tailIndex; i++)
            {
                if (i > 0)
                {
                    result += ", ";
                }
                result += storage[i].Item.ToString();
            }
            result += "]";
            return result;
        }
    }
}
