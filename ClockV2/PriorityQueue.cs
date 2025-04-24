namespace PriorityQueue
{
    public interface PriorityQueue<T>
    {
        void Add(T item, int priority);

        T Head();

        void Remove(int index);

        bool IsEmpty();

        string ToString();
    }
}
