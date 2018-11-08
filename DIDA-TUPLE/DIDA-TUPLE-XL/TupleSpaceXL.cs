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

            return result;
        }

        public void remove(Tuple tuple)
        {
            //UnLock all previews locked tuples
            /*foreach (Tuple t in _TakeMatches)
            {
                Monitor.Pulse(t);
            }*/

            //Just remove the selected tuple
            _tupleSpace.Remove(tuple);

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
            int timeout = 1000;
            

            while (result.Count == 0)
            {
                bool lockTaken = false;
                lock (this)
                {
                    foreach (Tuple t in _tupleSpace)
                    {
                        if (t.Equals(tuple))
                        {
                            Monitor.TryEnter(t, timeout, ref lockTaken);
                            if(lockTaken){
                                _lockList.AddElement(workerId,t);
                                result.Add(t);
                            }
                            else{
                                //this rollback just release the locks
                                Rollback(_lockList, workerId);
                                result.Clear();
                                break;
                            }
                            
                        }
                    }
                    if (result.Count == 0) //stil has not find any match
                        Monitor.Wait(this);
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
                Monitor.Pulse(this);
            }
            Console.WriteLine("** EXECUTE_WRITE: " + tuple);
        }


        // ================= TWO PHASE COMMIT FOR TAKE OPERATIONS =================

        /// <summary>
        /// Unlock all previous lock items.
        /// </summary>
        public void Rollback(LockList locklist, int workerId)
        {
            locklist.ReleaseAllLocks(workerId);
        }

    }
}
