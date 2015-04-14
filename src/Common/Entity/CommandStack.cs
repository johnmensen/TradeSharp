using System.Collections.Generic;

namespace Entity
{
    /// <summary>
    /// стек, аналогичный тому, что используется для операций Undo - Redo
    /// </summary>
    public class Stack<T>
    {
        private readonly List<T> list = new List<T>();
        public int Position
        {
            get;
            private set;
        }

        public void Push(T obj)
        {
            if (list.Count == 0)
            {
                list.Add(obj);
                Position = 0;
                return;
            }
            if (Position < (list.Count - 1))
                list.RemoveRange(Position + 1, list.Count - Position - 1);
            list.Add(obj);
            Position = list.Count - 1;
            return;
        }

        public T StepBack()
        {
            if (Position == 0) return default(T);
            Position--;
            return list[Position];
        }

        public T StepForward()
        {
            if (Position >= (list.Count - 1)) return default(T);
            Position++;
            return list[Position];
        }

        public void Clear()
        {
            list.Clear();
            Position = 0;
        }
    }
}
