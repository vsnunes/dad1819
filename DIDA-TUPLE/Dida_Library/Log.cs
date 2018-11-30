using System;
using System.Collections.Generic;
using System.Linq;

namespace DIDA_LIBRARY
{
    [Serializable]
    public class Log
    {
        private int _counter = 0;
        private List<Request> _requests;

        public Log(){
            _requests = new List<Request>();
        }

        public int Counter { get => _counter; }

        public List<Request> Requests { get => _requests; }

        public void Increment(){
            _counter++;
        }

        public void Add(int requestId, Request.OperationType operationId, Tuple tuple, bool master){
            Requests.Add(new Request(requestId, operationId, tuple));
        }
        
        public void Remove(int requestId){
            for(int i = 0; i < Requests.Count(); i++){
                if (Requests.ElementAt(i).RequestId == requestId){
                    Requests.RemoveAt(i);
                }
            }
        }

        public override string ToString()
        {
            string repr = "BEGIN LOG \n";
            foreach (Request request in _requests){
                repr += request.ToString();
            }
            repr += "END LOG\n";
            return repr;
        } 


    }
}
