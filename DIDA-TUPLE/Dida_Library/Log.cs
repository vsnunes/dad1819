using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIDA_LIBRARY
{
    [Serializable]
    public class Log
    {
        public int counter = 0;
        private List<Request> _requests;

        public Log(){
            _requests = new List<Request>();
        }
    
        public void Add(int requestId, Request.OperationType operationId, Tuple tuple, bool master){
            _requests.Add(new Request(requestId, operationId, tuple));
            if (master){
                counter++;
            }
        }
        
        public void Remove(int requestId){
            for(int i = 0; i < _requests.Count(); i++){
                if (_requests.ElementAt(i).RequestId == requestId){
                    _requests.RemoveAt(i);
                }
            }
        }

    }
}
