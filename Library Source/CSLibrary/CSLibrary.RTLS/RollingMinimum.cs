using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.RTLS
{
    class RollingMinimum
    {
        public int Capacity = 0;
        Queue<int> que = new Queue<int>();

        public RollingMinimum(int size)
        {
            que = new Queue<int>(size);
            Capacity = size;
        }

        public int Minimum
        {
            get
            {
                int minValue = int.MaxValue;
                foreach (int i in que)
                {
                    if (i < minValue)
                    {
                        minValue = i;
                    }
                }
                return minValue;
            }
        }

        public void Add(int value)
        {
            if (que.Count >= Capacity)
            {
                que.Dequeue();
            }
            que.Enqueue(value);
        }
    }
}
