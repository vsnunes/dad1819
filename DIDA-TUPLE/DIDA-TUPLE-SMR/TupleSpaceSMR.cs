﻿using System;
using System.Collections;
using System.Collections.Concurrent;
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
        private ConcurrentBag<Tuple> _tupleSpace;

        public TupleSpaceSMR()
        {
            _tupleSpace = new ConcurrentBag<Tuple>();
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

        public Tuple take(Tuple tuple)
        {
            throw new NotImplementedException();
        }

        public void write(Tuple tuple)
        {
            _tupleSpace.Add(tuple);
        }
    }
}