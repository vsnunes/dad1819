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

        private static object ReadLock = new object();
        private static object TakeLock = new object();

        //quem atualiza a view sao os servidores, quem faz get da view atual sao os workers
        private View _view;

        private LockList _lockList;

        public TupleSpaceXL()
        {
            _view = View.Instance;
            _tupleSpace = new List<Tuple>();
            _lockList = new LockList();
        }

        public Log Log { get => _log; set => _log = value; }

        public Tuple read(Tuple tuple)
        {
            Tuple result = null;

            while (result == null)
            {
                lock (this)
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
                        Monitor.Wait(this);
                }
            }
            Console.WriteLine("** XL READ: Just read " + result);
            return result;
        }

        public void remove(Tuple choice, List<Tuple> lockedTuples)
        {
            Tuple match = null;
            lock (_tupleSpace)
            {
                foreach (Tuple t in _tupleSpace)
                {
                    if (t.Equals(choice))
                    {
                        match = t;
                        break; //just found what i want to remove
                    }

                }

                if (match != null)
                {
                    lockedTuples.Remove(match);
                    _tupleSpace.Remove(match);
                    foreach (Tuple t in lockedTuples)
                    {
                        t.Locker = false;
                    }
                }
            }
            Console.WriteLine("** XL REMOVE: Just removed " + choice);
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
            List<Tuple> result = new List<Tuple>();

            while (result.Count == 0)
            {
                lock (this)
                {
                    foreach (Tuple t in _tupleSpace)
                    {
                        lock (t)
                        {
                            if (t.Locker == false)
                            {
                                t.Locker = true;
                                if (t.Equals(tuple))
                                {
                                    result.Add(t);
                                }
                                else
                                {
                                    t.Locker = false;
                                }
                            }

                        }
                    }
                    if (result.Count == 0)
                    {
                        Monitor.Wait(this);
                    }
                    
                }
            }
            return result;
        }

        public void write(int workerId, int requestId, Tuple tuple)
        {
            //If any thread is waiting for read or take
            //notify them to check if this tuple match its requirements
            lock (this)
            {
                _tupleSpace.Add(tuple);
                Monitor.PulseAll(this);
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
