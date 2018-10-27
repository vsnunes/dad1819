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
    class TupleSpaceSMR : ITupleSpace
    {
        //Possibilidade de hashtable
        private List<Tuple> _tupleSpace;

        public TupleSpaceSMR()
        {
            _tupleSpace = new List<Tuple>();
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

        /// <summary>
        /// Takes a tuple from the tuple space.
        /// </summary>
        /// <param name="tuple">The tuple to be taken.</param>
        /// <returns></returns>
        public Tuple take(Tuple tuple)
        {         
            return null;
        }

        public void write(Tuple tuple)
        {
            _tupleSpace.Add(tuple);
        }
    }
}
