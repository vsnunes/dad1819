using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.IO;
using System.Threading;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_CLIENT
{

    public class FrontEndXL: IFrontEnd
    {
        private static Object ReadLock = new Object();


        private int _workerId;
        private int _requestId = 0;
        private static int number_servers;
        private static AutoResetEvent[] readHandles;



        /// <summary>
        /// Delegate for Reading Operations
        /// </summary>
        public delegate Tuple RemoteAsyncReadDelegate(Tuple t);

        public delegate void RemoteAsyncSecondPhaseDelegate(Tuple t);

        /// <summary>
        /// Delegate for Writes that require workerId, requestId and a tuple.
        /// </summary>
        public delegate void RemoteAsyncWriteDelegate(int workerId,int requestId, Tuple t);

        /// <summary>
        /// Delegate for Takes that require workerId, requestId and a tuple.
        /// Remeber: Take retrieves a list of potencial tuples to be removed
        /// </summary>

        public FrontEndXL(int workerId) {
            _workerId = workerId;
            
            number_servers = this.GetView().Count();

            readHandles = new AutoResetEvent[1];

            readHandles[0] = new AutoResetEvent(false);

        }

        /// <summary>
        /// First read response.
        /// It will be update on callback function
        /// </summary>
        private static Tuple _responseRead = null;

        private static List<List<Tuple>> _responseTake = null;

        public List<string> GetView()
        {
            string[] file = File.ReadAllLines("../../serverListXL.txt");
            List<string> servers = new List<string>();

            foreach (string i in file)
            {
                servers.Add(i);
            }      
            return servers;
        }

        /// <summary>
        /// Callback function to process read's server' replies
        /// <param name="IAsyncResult"A AsyncResult Delgate Object.</param>
        /// </summary>
        public static void CallbackRead(IAsyncResult ar)
        {
            RemoteAsyncReadDelegate del = (RemoteAsyncReadDelegate)((AsyncResult)ar).AsyncDelegate;
            lock (ReadLock)
            {
                //Only care about one reply, ignore others
                if (_responseRead == null)
                {
                    _responseRead = del.EndInvoke(ar);
                    readHandles[0].Set();
                }
            }
        }

        public Tuple Read(Tuple tuple)
        {

            List<string> actualView = this.GetView();
            List<ITupleSpaceXL> serversObj = new List<ITupleSpaceXL>();

            ITupleSpaceXL tupleSpace = null;

                //save remoting objects of all members of the view
                foreach (string serverPath in actualView)
                {
                    try
                    {
                        tupleSpace = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), serverPath);
                        tupleSpace.ItemCount(); //just to check availability of the server
                    }
                    catch (Exception) { tupleSpace = null; }
                    if (tupleSpace != null)
                        serversObj.Add(tupleSpace);
                }

            for (int i = 0; i < 3; i++)
            {
                new Thread(() =>
                {
                    lock(_responseRead)
                    {
                        if (_responseRead == null)
                            _responseRead = serversObj[i].read(tuple);
                    }
                    
                }).Start();
            }

                while (_responseRead == null)
                {
                    Console.WriteLine("** FRONTEND READ: Not yet receive any reply let me wait...");

                    WaitHandle.WaitAny(readHandles);
                   
                }
            
            _requestId++;
            Console.WriteLine("** FRONTEND READ: Here is a response: " + _responseRead);
            return _responseRead;
        }

        public Tuple Take(Tuple tuple)
        {
            List<string> actualView = this.GetView();
            List<ITupleSpaceXL> serversObj = new List<ITupleSpaceXL>();

            AutoResetEvent[] handlers = new AutoResetEvent[number_servers];
            Thread[] threadPool = new Thread[number_servers];

            ITupleSpaceXL tupleSpace = null;

            List<List<Tuple>> possibleTuples = new List<List<Tuple>>();
            List<Tuple>[] pt = new List<Tuple>[3];
            
            for (int i = 0; i < number_servers; i++)
            {
                handlers[i] = new AutoResetEvent(false);
            }

            foreach (string serverPath in actualView)
            {
                try
                {
                    tupleSpace = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), serverPath);
                    tupleSpace.ItemCount(); //just to check availability of the server
                }
                catch (Exception) { tupleSpace = null; }
                if (tupleSpace != null)
                    serversObj.Add(tupleSpace);
            }

            Thread task0 = new Thread(() =>
            {
                lock (possibleTuples)
                {
                    List<Tuple> l = serversObj[0].take(_workerId, _requestId, tuple);
                    pt[0] = l;
                    possibleTuples.Add(l);
                    handlers[0].Set();
                }
            });

            Thread task1 = new Thread(() =>
            {
                lock (possibleTuples)
                {
                    List<Tuple> l = serversObj[1].take(_workerId, _requestId, tuple);
                    pt[1] = l;
                    possibleTuples.Add(l);
                    handlers[1].Set();
                }
            });

            Thread task2 = new Thread(() =>
            {
                lock (possibleTuples)
                {
                    List<Tuple> l = serversObj[2].take(_workerId, _requestId, tuple);
                    pt[2] = l;
                    possibleTuples.Add(l);
                    handlers[2].Set();
                }
            });

            task0.Start();
            task1.Start();
            task2.Start();

            WaitHandle.WaitAll(handlers);

            Tuple choice = TupleSelection(Intersection(possibleTuples));

            //Remove object from the server
            for (int i = 0; i < 3; i++)
            {
                serversObj[i].remove(choice, pt[i]);
            }

            return choice;
        }
        
        //Selects a tuple do initiate the second phase of take
        private static Tuple TupleSelection(IEnumerable<Tuple> l)
        {
            if (l.Count() == 0)
                return null;
            return l.ElementAt(0);
        }

        public static IEnumerable<Tuple> Intersection(List<List<Tuple>> list) {
            //Intersection of an empty list
            if (list.Count == 0)
                return new List<Tuple>();
            else if (list.Count == 1)
                return list[0];

            IEnumerable<Tuple> intersectSet = list.ElementAt(0);

            for (int i = 1; i < list.Count(); i++) {
                intersectSet = list.ElementAt(i).Intersect(intersectSet, new TupleComparator());
            }


           return intersectSet;
        }

        public void Write(Tuple tuple)
        {
            List<string> actualView = this.GetView();
            List<ITupleSpaceXL> serversObj = new List<ITupleSpaceXL>();

            ITupleSpaceXL tupleSpace = null;
            //save remoting objects of all members of the view
            foreach (string serverPath in actualView)
            {
                try
                {
                    tupleSpace = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), serverPath);
                    tupleSpace.ItemCount(); //just to check availability of the server
                }
                catch (Exception) { tupleSpace = null; Console.WriteLine("** FRONTEND WRITE: Could not connected to server at " + serverPath);  }
                if (tupleSpace != null)
                    serversObj.Add(tupleSpace);
            }

          
            Thread task0 = new Thread(() =>
            {
                serversObj[0].write(_workerId, _requestId, tuple);
            });

            Thread task1 = new Thread(() =>
            {
                serversObj[1].write(_workerId, _requestId, tuple);
            });

            Thread task2 = new Thread(() =>
            {
                serversObj[2].write(_workerId, _requestId, tuple);
            });

            task0.Start();
            task1.Start();
            task2.Start();

            _requestId++;
            Console.WriteLine("** FRONTEND WRITE: Just write " + tuple);
        }
    }

    /// <summary>
    /// Tuple Comparator Class for Intersect method of IEnumerable objects
    /// </summary>
    class TupleComparator : IEqualityComparer<Tuple>
    {
        public bool Equals(Tuple x, Tuple y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(Tuple obj)
        {
            return obj.GetHashCode();
        }
    }
}
