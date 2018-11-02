using System;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_TUPLE_SMR
{
    delegate Tuple ThrWork(Tuple tuple);
    delegate void VoidThrWork(Tuple tuple);

    class Server
    {
        private TupleSpaceSMR tupleSpace;
        private ThreadPool tpool;
        private ThrWork work;
        public Server()
        {
            tupleSpace = new TupleSpaceSMR();
            tpool = new ThreadPool(5, 10);
            work = null;
        }

        public Tuple Read(Tuple tuple)
        {
            Tuple returnTuple = tpool.AssyncInvoke(new ThrWork(tupleSpace.read, new[] { tuple }));
            return tuple;
        }

        public void Add(Tuple tuple)
        {
            //Tuple returnTuple = tpool.AssyncInvoke(new VoidThrWork(tupleSpace.write));
        }

        public Tuple Take(Tuple tuple)
        {
            Tuple returnTuple = tpool.AssyncInvoke(new ThrWork(tupleSpace.take));
            return tuple;
        }
    }
}
