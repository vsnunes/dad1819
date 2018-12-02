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

        List<ITupleSpaceXL> serversObj;

        List<string> actualView;

        public FrontEndXL(int workerId) {
            _workerId = workerId;
            
            number_servers = this.GetView().Count();
            actualView = this.GetView();
            getServersProxy();
        }

        //Just for testing purposes
        public FrontEndXL(int workerId, bool debugMode)
        {
            _workerId = workerId;

            number_servers = this.GetView().Count();
            actualView = this.GetView();
            
        }

        public void getServersProxy()
        {
            serversObj = new List<ITupleSpaceXL>();
            ITupleSpaceXL tupleSpace = null;
            //save remoting objects of all members of the view
            foreach (string serverPath in actualView)
            {
                try
                {
                    tupleSpace = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), serverPath);
                    tupleSpace.ItemCount(); //just to check availability of the server
                }
                catch (Exception) { }
                if (tupleSpace != null)
                    serversObj.Add(tupleSpace);
            }
        }

        public List<string> GetView()
        {
            string[] file = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "../../../DIDA-CLIENT/serverListXL.txt"));
            List<string> servers = new List<string>();

            foreach (string i in file)
            {
                servers.Add(i);
            }      
            return servers;
        }


        public Tuple Read(Tuple tuple)
        {
            AutoResetEvent[] readHandles;
            readHandles = new AutoResetEvent[1];
            readHandles[0] = new AutoResetEvent(false);

            Tuple _responseRead = null;

            Thread task0 = new Thread(() =>
            {

                while (_responseRead == null)
                {
                    Tuple possible = serversObj[0].read(tuple);
                    if (possible != null)
                    {
                        lock (ReadLock)
                        {
                            _responseRead = possible;
                            readHandles[0].Set();
                        }
                    }
                }
            });


            Thread task1 = new Thread(() =>
            {
                
                while (_responseRead == null)
                {
                    Tuple possible = serversObj[1].read(tuple);
                    if(possible != null)
                    {
                        lock (ReadLock)
                        {
                            _responseRead = possible;
                            readHandles[0].Set();
                        }
                    }
                }
            });

            Thread task2 = new Thread(() =>
            {

                while (_responseRead == null)
                {
                    Tuple possible = serversObj[2].read(tuple);
                    if (possible != null)
                    {
                        lock (ReadLock)
                        {
                            _responseRead = possible;
                            readHandles[0].Set();
                        }
                    }
                }
            });

            task0.Start();
            task1.Start();
            task2.Start();

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
            AutoResetEvent[] handlers = new AutoResetEvent[3];
            Thread[] threadPool = new Thread[3];

            List<List<Tuple>> possibleTuples = new List<List<Tuple>>();
            List<Tuple>[] pt = new List<Tuple>[3];
            
            for (int i = 0; i < 3; i++)
            {
                handlers[i] = new AutoResetEvent(false);
            }

            

            Thread task0 = new Thread(() =>
            {
                
                    List<Tuple> l = serversObj[0].take(_workerId, _requestId, tuple);
                    pt[0] = l;
                lock (possibleTuples)
                {
                    possibleTuples.Add(l);
                }
                    handlers[0].Set();
                
            });

            Thread task1 = new Thread(() =>
            {
                
                 List<Tuple> l = serversObj[1].take(_workerId, _requestId, tuple);
                 pt[1] = l;
                lock (possibleTuples)
                {
                   possibleTuples.Add(l);
                }
                handlers[1].Set();
                
            });

            Thread task2 = new Thread(() =>
            {
                    List<Tuple> l = serversObj[2].take(_workerId, _requestId, tuple);
                    pt[2] = l;
                lock (possibleTuples)
                {
                    possibleTuples.Add(l);
                }
                handlers[2].Set();
            });

            task0.Start();
            task1.Start();
            task2.Start();

            WaitHandle.WaitAll(handlers);

            handlers = new AutoResetEvent[3];
            handlers[0] = new AutoResetEvent(false);
            handlers[1] = new AutoResetEvent(false);
            handlers[2] = new AutoResetEvent(false);

            Tuple choice = TupleSelection(Intersection(possibleTuples));

           task0 = new Thread(() =>
            {
                serversObj[0].remove(_workerId, choice);
                handlers[0].Set();
            });

            task1 = new Thread(() =>
            {
                serversObj[1].remove(_workerId, choice);
                handlers[1].Set();
            });

            task2 = new Thread(() =>
            {
                serversObj[2].remove(_workerId, choice);
                handlers[2].Set();
            });

            task0.Start();
            task1.Start();
            task2.Start();

            WaitHandle.WaitAll(handlers);

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
            AutoResetEvent[] handlers = new AutoResetEvent[3];
            handlers[0] = new AutoResetEvent(false);
            handlers[1] = new AutoResetEvent(false);
            handlers[2] = new AutoResetEvent(false);

            Thread task0 = new Thread(() =>
            {
                serversObj[0].write(_workerId, _requestId, tuple);
                handlers[0].Set();
            });

            Thread task1 = new Thread(() =>
            {
                serversObj[1].write(_workerId, _requestId, tuple);
                handlers[1].Set();
            });

            Thread task2 = new Thread(() =>
            {
                serversObj[2].write(_workerId, _requestId, tuple);
                handlers[2].Set();
            });

            task0.Start();
            task1.Start();
            task2.Start();

            WaitHandle.WaitAll(handlers);

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
