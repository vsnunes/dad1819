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
        private int _minDelay;
        private int _maxDelay;
        private static bool freeze = false;
        private List<Tuple> _tupleSpace;
        private Log _log;

        //quem atualiza a view sao os servidores, quem faz get da view atual sao os workers
        private View _view;

        private LockList _lockList;

        public TupleSpaceXL()
        {
            _view = View.Instance;
            _tupleSpace = new List<Tuple>();
            _lockList = new LockList();
            MinDelay = 0;
            MaxDelay = 0;
        }


        private int generateRandomDelay() {
            if (MinDelay == 0 && MaxDelay == 0)
                return 0;
            return new Random().Next(MinDelay, MaxDelay);
        }

        public Log Log { get => _log; set => _log = value; }

        public int MinDelay { get => _minDelay; set => _minDelay = value; }
        public int MaxDelay { get => _maxDelay; set => _maxDelay = value; }

        public Tuple read(Tuple tuple)
        {
            //Checks if the server is freezed
            lock(this) {
                while (freeze)
                    Monitor.Wait(this);
            }

            Thread.Sleep(generateRandomDelay());
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
                        Monitor.Wait(_tupleSpace);
                }
            }
            Console.WriteLine("** XL READ: Just read " + result);
            return result;
        }

        public void remove(Tuple tuple)
        {
            Tuple match = null;
            lock (_tupleSpace)
            {
                foreach (Tuple t in _tupleSpace)
                {
                    if (t.Equals(tuple))
                    {
                        match = t;
                        break; //just found what i want to remove
                    }

                }

                if (match != null)
                {
                    _tupleSpace.Remove(match);
                }
            }
            Console.WriteLine("** XL REMOVE: Just removed " + tuple);
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
            //Checks if the server is freezed
            lock (this)
            {
                while (freeze)
                    Monitor.Wait(this);
            }

            Thread.Sleep(generateRandomDelay());
            List<Tuple> result = new List<Tuple>();
            int timeout = 1000;


            while (result.Count == 0)
            {
                bool lockTaken = false;
                lock (_tupleSpace)
                {
                    foreach (Tuple t in _tupleSpace)
                    {
                        if (t.Equals(tuple))
                        {
                            Monitor.TryEnter(t, timeout, ref lockTaken);
                            if (lockTaken)
                            {
                                _lockList.AddElement(workerId, t);
                                result.Add(t);
                            }
                            else
                            {
                                //this rollback just release the locks
                                Rollback(_lockList, workerId);
                                result.Clear();
                                break;
                            }

                        }
                    }
                    if (result.Count == 0) //stil has not find any match
                        Monitor.Wait(_tupleSpace);
                }
            }

            Console.WriteLine("** XL TAKE-PHASE1");
            return result;
        }

        public void write(int workerId, int requestId, Tuple tuple)
        {
            //Checks if the server is freezed
            lock (this)
            {
                while (freeze)
                    Monitor.Wait(this);
            }

            Thread.Sleep(generateRandomDelay());
            //If any thread is waiting for read or take
            //notify them to check if this tuple match its requirements
            lock (_tupleSpace)
            {
                _tupleSpace.Add(tuple);
                Monitor.PulseAll(_tupleSpace);
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

        public void Freeze()
        {
            lock (this)
            {
                freeze = true;
            }
        }

        public void Unfreeze()
        {
            lock(this) {
                freeze = false;
                Monitor.PulseAll(this);
            }
        }

        public void Crash()
        {
            System.Environment.Exit(0);
        }

        public void Status()
        {
            Console.WriteLine("My actual view:");
            Console.WriteLine(_view);
        }
    }
}