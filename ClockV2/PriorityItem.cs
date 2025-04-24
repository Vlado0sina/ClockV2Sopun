using System.Configuration;

namespace PriorityQueue
{
    public class PriorityItem<T>
    {
        public T Item { get; }
        public int Priority { get; }

        public PriorityItem(T item, int priority)
        {
            Item = item;
            Priority = priority;
        }

        public override string ToString()
        {
            return $"({Item}, {Priority})";
        }
    }

    //Represents a node in a linked list for a priority queue
    public class Node<T>
    {
        //Gets or sets the priority item is sorted in this node
        public PriorityItem<T> PriorityItem { get; set; }
        //Gets or sets the reference to the next node in the list
        public Node<T> Next { get; set; }

        //Constructor
        public Node(PriorityItem<T> priorityItem)
        {
            PriorityItem = priorityItem;
            Next = null;
        }
    }
}
