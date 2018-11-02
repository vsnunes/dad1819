using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_TUPLE_SMR
{
    class TotalOrder : ITotalOrder
    {
        
        private Log _log;
        private TupleSpaceSMR _tupleSpace;

        /// <summary>
        /// Type of the server
        /// </summary>
        public enum Type {NORMAL, MASTER };

        private Type _type;

        public TotalOrder(TupleSpaceSMR tupleSpace) {
            _log = new Log();
            _tupleSpace = tupleSpace;
            _type = Type.NORMAL;
        }

        public void commit(int id, Request.OperationType request, Tuple tuple)
        {
            lock (this)
            {
                switch (request)
                {
                    case Request.OperationType.WRITE:

                        _tupleSpace.write(tuple);


                        break;

                    case Request.OperationType.TAKE:
                        _tupleSpace.take(tuple);

                        break;

                    
                }

                //already commited changes so remove them from the log.
                _log.Remove(id);

            }
            
        }

        public void prepare(int id, Request.OperationType request, Tuple tuple)
        {
            lock (this)
            {
                _log.Add(id, request, tuple, _type == Type.MASTER);
            }
        }
    }
}
