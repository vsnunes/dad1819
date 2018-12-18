using System;


namespace DIDA_LIBRARY
{
    /// <summary>
    /// A class for describing basic Tuple Space Requests/Operations.
    /// </summary>
    [Serializable]
    public class Request
    {
        /// <summary>
        /// Types of Operations on a Tuple Space.
        /// Remeber the DIDA-TUPLE paper: A tuple space must be readable, writable and deletable.
        /// </summary>
        public enum OperationType { WRITE, READ, TAKE };

        /// <summary>
        /// The request identifier.
        /// </summary>
        private int _requestId;
        private OperationType _operationId;

        /// <summary>
        /// The tuple associated with this request.
        /// </summary>
        private Tuple _tuple;

        public Request(int requestId, OperationType operationId, Tuple tuple){
            _requestId = requestId;
            _operationId = operationId;
            _tuple = tuple;
        }

        public int RequestId { get => _requestId;  }
        public OperationType OperationId { get => _operationId; }
        public Tuple Tuple { get => _tuple; }

        /// <summary>
        /// String representation of a request
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string repr = "";
            switch(_operationId){

                case OperationType.WRITE:
                    repr += "WRITE";
                    break;
                case OperationType.READ:
                    repr += "READ";
                    break;
                case OperationType.TAKE:
                    repr += "TAKE";
                    break;
            }
            repr += " -> " + _requestId + " -> " + _tuple + "\n";
            return repr;
        }
    }
}
