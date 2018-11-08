using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        private ImmutableList<Tuple> _tupleSpace;
        private Log _log;

        //quem atualiza a view sao os servidores, quem faz get da view atual sao os workers
        private View _view;

        private ImmutableList<Tuple> _TakeMatches;

        public TupleSpaceXL()
        {
            _view = View.Instance;
            _tupleSpace = ImmutableList.Create<Tuple>();
        }

        public Log Log { get => _log; set => _log = value; }

        public int ItemCount()
        {
            return _tupleSpace.Count();
        }

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

        /// <summary>
        /// Takes a tuple from the tuple space.
        /// </summary>
        /// <param name="tuple">The tuple to be taken.</param>
        /// <returns></returns>
        public ImmutableList<Tuple> take(int workerId, int requestId, Tuple tuple)
        {
            //Same as read but retrieves the list and lock the items.
            Tuple match = null;
            _TakeMatches = ImmutableList.Create<Tuple>();


            foreach (Tuple t in _tupleSpace)
            {
                if (t.Equals(tuple))
                {
                    //Lock all selected tuples 'cause i don't know what tuple is going to be removed
                    Monitor.Enter(t);
                    if (_tupleSpace.Contains(t) == false) continue;
                    _TakeMatches.Add(t);
                }
            }

            return _TakeMatches;
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

    }
}
