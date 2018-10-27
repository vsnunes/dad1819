using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_TUPLE_SMR
{
    public class TupleSpaceSMR : MarshalByRefObject, ITupleSpace
    {
        //Possibilidade de hashtable
        private List<Tuple> _tupleSpace; 

        public TupleSpaceSMR()
        {
            _tupleSpace = new List<Tuple>();
        }

        public int ItemCount()
        {
            return _tupleSpace.Count();
        }

        public Tuple read(Tuple tuple)
        {
            foreach(Tuple pos in _tupleSpace)
            {
                if (pos.Equals(tuple))
                {
                    return pos;
                }
            }
            return null; //temos de incluir o hold para uma queue
        }
        public List<Tuple> GetTuples()
        {
            return _tupleSpace;
        }
        /// <summary>
        /// Takes a tuple from the tuple space.
        /// </summary>
        /// <param name="tuple">The tuple to be taken.</param>
        /// <returns></returns>
        public Tuple take(Tuple tuple)
        {
            Tuple match = null;

            foreach (Tuple t in _tupleSpace)
            {
                if (t.Equals(tuple))
                {
                    match = t;
                    break; //just found one so no need to continue searching
                }
            }

            if (match != null)
            {
                lock (this) { _tupleSpace.Remove(match); }
                return match;
            }

            //not found 
            return null; 
        }

        public void write(Tuple tuple)
        {
            lock (this) { _tupleSpace.Add(tuple); }
        }
    }
}
