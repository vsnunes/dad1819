using System;
using System.Collections.Generic;
using System.Linq;

namespace DIDA_LIBRARY
{
    /// <summary>
    /// A class for describing the log of operations.
    /// The Log is a record of already executed operations.
    /// </summary>
    [Serializable]
    public class Log
    {
        /// <summary>
        /// The number of operations on the log.
        /// </summary>
        private int _counter = 0;

        /// <summary>
        /// The list of requests that are in the log.
        /// </summary>
        private List<Request> _requests;

        public Log(){
            _requests = new List<Request>();
        }

        public int Counter { get => _counter; }

        public List<Request> Requests { get => _requests; }

        public void Increment(){
            _counter++;
        }

        /// <summary>
        /// Adds an already executed operation to the log.
        /// </summary>
        /// <param name="requestId">The request ID</param>
        /// <param name="operationId">The operation Type !Bad name!</param>
        /// <param name="tuple">The tuple associated with the operation</param>
        /// <param name="master">A boolean value to inform if the operation was taking place on a master node. !Not used!</param>
        public void Add(int requestId, Request.OperationType operationId, Tuple tuple, bool master){
            Requests.Add(new Request(requestId, operationId, tuple));
        }

        /// <summary>
        /// Adds an already executed operation to the log.
        /// </summary>
        /// <param name="requestId">The request ID</param>
        /// <param name="operationId">The operation Type !Bad name!</param>
        /// <param name="tuple">The tuple associated with the operation</param>
        public void Add(int requestId, Request.OperationType operationId, Tuple tuple)
        {
            Requests.Add(new Request(requestId, operationId, tuple));
        }

        /// <summary>
        /// Removes a request from the log.
        /// </summary>
        /// <param name="requestId">The request ID to be removed.</param>
        public void Remove(int requestId){
            for(int i = 0; i < Requests.Count(); i++){
                if (Requests.ElementAt(i).RequestId == requestId){
                    Requests.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// String representation of the log.
        /// </summary>
        /// <returns></returns>
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
