using System;

namespace PriorityQueue
{
    class HeapPriorityQueue<T>
    {
        private PriorityItem<T>[] heap;//Array to store heap elements
        public int size; //Current size of heap 
        private int capacity;//Max of capacity od heap

        //Constructor
        public HeapPriorityQueue(int capacity = 10)
        {
            this.capacity = capacity;
            heap = new PriorityItem<T>[capacity];
            size = 0;
        }

        //Adds new item
        public void Add(T item, int priority) 
        {
            if (size == capacity)
            {
                ResizeHeap();//Double the heap size if it's full
            }

            //Insert  new item at the next available position
            heap[size] = new PriorityItem<T>(item, priority);
            HeapifyUp(size);//Restore heap property
            size++;
        }

        //Return highest-priority
        public T Head() 
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("Queue is empty");
            }

            return heap[0].Item;//Root of the heap has highest-priority
        }

        public void Remove(int index)
        {
            if (index < 0 || index>= size)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range");
            }

            //T item = heap[0].Item;//Root 
            heap[index] = heap[size - 1];//Move last item to root
            size--;
            HeapifyDown(index);//Restore heap property
            HeapifyUp(index);
            //return item;

        }

        //Check if the priority queue  is empty
        public bool IsEmpty() 
        {
            return size == 0;
        }

        //Double the heap capacity when full
        public void ResizeHeap() 
        {
            capacity *= 2;
            var newHeap = new PriorityItem<T>[capacity];
            Array.Copy(heap, newHeap, size);
            heap = newHeap;
        }

        //Moves the item at index "i" up to restore heap property
        private void HeapifyUp(int i) 
        {
            while (i > 0)
            {
                int parentIndex = (i - 1) / 2;
                //Stop if the current node is in cirrect position
                if (heap[i].Priority >= heap[parentIndex].Priority)
                {
                    break;
                }

                //Swap child with parent
                Swap(i, parentIndex);
                i = parentIndex; //Move up the heap
            }
        }

        private void HeapifyDown(int i) 
        {
            while (true) 
            {
                int leftChild = 2 * i + 1;
                int rightChild = 2 * i + 2;
                int largest = i;

                //Find the largest value amounf parent and children
                if (leftChild < size && heap[leftChild].Priority > heap[largest].Priority)
                {
                    largest = leftChild;
                }

                if (rightChild < size && heap[rightChild].Priority > heap[largest].Priority)
                {
                    largest = rightChild;
                }

                //Stop if heap property restored 
                if(largest == i)
                {
                    break ;
                }

                //Swap and movinf down
                Swap(i, largest);
                i = largest;
            }

        }


        private void Swap(int i, int j) 
        {
            //Swap two elements in the heap array
            var temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }

        public T getAt(int index)
        {
            if (index < 0 || index >= size)
            {
                throw new ArgumentOutOfRangeException("Index is out of range");
            }
            return heap[index].Item;
        }

    }

}
