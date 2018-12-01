using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_TUPLE_XL
{
    public class TupleSpaceXL : MarshalByRefObject, ITupleSpaceXL
    {
        private List<Tuple> _tupleSpace;
        private Log _log;

        private static object Lock = new object();

        //quem atualiza a view sao os servidores, quem faz get da view atual sao os workers
        private View _view;

        private LockList _lockList;

        public TupleSpaceXL()
        {
            _view = View.Instance;
            _tupleSpace = new List<Tuple>();
            _lockList = new LockList();

            //vitor: Hardcoded worker configuration!
            _lockList.AddWorker(1);
            _lockList.AddWorker(2);
            _lockList.AddWorker(3);
        }

        public Log Log { get => _log; set => _log = value; }

        public Tuple read(Tuple tuple)
        {
            Tuple result = null;

            while (result == null)
            {
                lock (_tupleSpace)
                {
                    foreach (Tuple t in _tupleSpace)
                    {
                        if (t.Equals(tuple))
                        {
                            result = t;
                            break; //just found one so no need to continue searching
                        }
                    }
                    if (result == null) //stil has not find any match
                        Monitor.Wait(_tupleSpace, new Random().Next(1, 50));
                }
            }
            Console.WriteLine("** XL READ: Just read " + result);
            return result;
        }

        public void remove(int workerId, Tuple choice)
        {
            Console.WriteLine("** START TAKE PHASE2 OF: " + choice);
            
            lock (_tupleSpace)
            {
                _lockList.ReleaseAllLocks(workerId);
                _tupleSpace.Remove(choice);
            }

            Console.WriteLine("** FINISHED TAKE PHASE2 OF: " + choice);
        }

        public int ItemCount()
        {
            return _tupleSpace.Count();
        }

        /// <summary>
        /// Takes a tuple from the tuple space.
        /// </summary>
        /// <param name="tuple">The tuple to be taken.</param>
        /// <returns></returns>
        public List<Tuple> take(int workerId, int requestId, Tuple tuple)
        {
            Console.WriteLine("** STARTING TAKE PHASE1 OF: " + tuple);
            List<Tuple> matchingTuples;

            
                do
                {
                    lock (Lock)
                    {
                        matchingTuples = match(workerId, tuple);
                        if (matchingTuples == null || matchingTuples.Count() == 0)
                            Monitor.Wait(Lock, new Random().Next(1, 50));
                    }
                } while (matchingTuples == null || matchingTuples.Count() == 0);
                

            Console.WriteLine("** FINISHED TAKE PHASE1 OF: " + tuple);
            return matchingTuples;
        }

        public List<Tuple> match(int workerId, Tuple t)
        {
            List<Tuple> M = new List<Tuple>();

            //vitor: nasty code ahead!

            lock (_tupleSpace)
            {
                foreach (Tuple tuple in _tupleSpace)
                {
                    if (tuple.Equals(t))
                    {
                        if (_lockList.CheckTupleLock(tuple))
                        {
                            _lockList.ReleaseAllLocks(workerId);
                            return null; //failure
                        }
                        else
                        {
                            _lockList.AddElement(workerId, tuple);
                            M.Add(tuple);
                        }
                    }
                }
            }
            return M;
        }

        public void write(int workerId, int requestId, Tuple tuple)
        {
            //If any thread is waiting for read or take
            //notify them to check if this tuple match its requirements

            lock (Lock)
            {
                lock (_tupleSpace)
                {
                    _tupleSpace.Add(tuple);
                }
                Monitor.PulseAll(Lock);
            }

            Console.WriteLine("** XL WRITE: " + tuple);
        }


        // ================= TWO PHASE COMMIT FOR TAKE OPERATIONS =================

        /// <summary>
        /// Unlock all previous lock items.
        /// </summary>
        public void Rollback(LockList locklist, int workerId)
        {
            locklist.ReleaseAllLocks(workerId);
            Console.WriteLine("** XL ROLLBACK: Rollback this workerId " + workerId);
        }

    }
}
