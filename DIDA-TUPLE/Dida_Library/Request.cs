using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIDA_LIBRARY
{
    [Serializable]
    public class Request
    {
        public enum OperationType { WRITE, READ, TAKE };
        private int _requestId;
        private OperationType _operationId;
        private Tuple _tuple;

        public Request(int requestId, OperationType operationId, Tuple tuple){
            _requestId = requestId;
            _operationId = operationId;
            _tuple = tuple;
        }

        public int RequestId { get => _requestId;  }
        public OperationType OperationId { get => _operationId; }
        public Tuple Tuple { get => _tuple; }
    }
}
