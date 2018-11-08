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
    public class TupleSpaceXL : MarshalByRefObject, ITupleSpaceXL, IEnlistmentNotification
    {
        private List<Tuple> _tupleSpace;
        private Log _log;

        //quem atualiza a view sao os servidores, quem faz get da view atual sao os workers
        private View _view;

        private List<Tuple> _TakeMatches;

        public TupleSpaceXL()
        {
            _view = View.Instance;
            _tupleSpace = new List<Tuple>();
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
            foreach (Tuple t in _TakeMatches)
            {
                Monitor.Pulse(t);
            }

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

            while (result.Count == 0)
            {
                lock (this)
                {
                    foreach (Tuple t in _tupleSpace)
                    {
                        if (t.Equals(tuple))
                        {
                            result.Add(tuple);
                            Monitor.Enter(tuple);
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
        /// Remove the selected tuple from the intersection.
        /// </summary>
        /// <param name="enlistment"></param>
        public void Commit(Enlistment enlistment)
        {
            throw new NotImplementedException();
        }

        public void InDoubt(Enlistment enlistment)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Prepares the take operation by locking only the items.
        /// </summary>
        /// <param name="preparingEnlistment"></param>
        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unlock all previous lock items.
        /// </summary>
        /// <param name="enlistment"></param>
        public void Rollback(Enlistment enlistment)
        {
            throw new NotImplementedException();
        }

    }
}
