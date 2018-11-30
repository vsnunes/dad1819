using System.Collections.Generic;
using System.Threading;

namespace DIDA_LIBRARY
{
    /// <summary>
    /// A Class for describing List of Locked Tuples Given an Index
    /// </summary>
    public class LockList
    {
        private Dictionary<int, List<Tuple>> _lockList;

        public LockList()
        {
            _lockList = new Dictionary<int, List<Tuple>>();
        }

        /// <summary>
        /// Adds a new List to a new worker
        /// </summary>
        /// <param name="workerId">The ID of the new worker.</param>
        public void AddWorker(int workerId)
        {
            _lockList.Add(workerId, new List<Tuple>());
        }

        /// <summary>
        /// Adds a tuple to the lock list according to the workerId
        /// </summary>
        /// <param name="workerId">The ID of the Worker.</param>
        /// <param name="tuple">The tuple to be added.</param>
        public void AddElement(int workerId, Tuple tuple)
        {
            //If element does not exists then create it
            if (_lockList.ContainsKey(workerId) == false)
                AddWorker(workerId);

            lock (_lockList[workerId])
            {
                _lockList[workerId].Add(tuple);
            }           
        }

        /// <summary>
        /// Releases all locks for a current workerId
        /// </summary>
        /// <param name="workerId">A ID of the worker to release all locked tuples.</param>
        public void ReleaseAllLocks(int workerId)
        {
            if (_lockList.ContainsKey(workerId) == false)
                return;

            lock (_lockList[workerId])
            {
                foreach (Tuple tuple in _lockList[workerId])
                {
                    Monitor.Exit(tuple);
                }

                //Cleans the entire list
                _lockList[workerId].Clear();

            }
        }
    }
}
