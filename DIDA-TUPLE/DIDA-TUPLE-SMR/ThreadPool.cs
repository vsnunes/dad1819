using System;
using System.Threading;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_TUPLE_SMR
{
    class ThreadPool
    {
        private CircularBuffer<ThrWork> buf;
        private Thread [] pool;
        public ThreadPool(int threadNumber, int bufSize)
        {
            buf = new CircularBuffer<ThrWork>(bufSize);
            pool = new Thread[threadNumber];
            for (int i=0; i < threadNumber; i++)
            {
                pool[i] = new Thread(new ThreadStart(consomeExec));
                pool[i].Start();
            }
        }

        public void AssyncInvoke(ThrWork action)
        {
            buf.Produce(action);
            Console.Write("Request Started");
        }

        public void consomeExec()
        {
            while (true)
            {
                ThrWork tw = buf.Consume();
                tw();
            }
        }
    }
}
