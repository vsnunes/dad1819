﻿using System.Collections.Generic;
using System.Threading;

namespace DIDA_LIBRARY
{
    /// <summary>
    /// A Class for describing List of Locked Tuples Given an Index
    /// </summary>
    public class LockList
    {
        /// <summary>
        /// Dictionary containing the client front end ID as the key
        /// and a List of Locked Tuples by that client as the value.
        /// </summary>
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
                
                //Cleans the entire list
                _lockList[workerId].Clear();
            }
            _lockList.Remove(workerId);
        }

        /// <summary>
        /// Given a tuple checks if the tuple is locked
        /// </summary>
        /// <param name="tuple">The tuple to be checked</param>
        /// <returns>True if the tuple is locked, false otherwise.</returns>
        public bool CheckTupleLock(Tuple tuple)
        {
            bool result = false;
            lock(_lockList)
            {
                foreach (int workerId in _lockList.Keys)
                {
                    foreach(Tuple t in _lockList[workerId])
                    {
                        if (t.Equals(tuple))
                            result = true;
                    }
                    if (result == true)
                        break;
                }
            }
            return result;
        }
    }
}
