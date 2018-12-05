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

    public class FrontEndXL : IFrontEnd
    {
        private static Object ReadLock = new Object();

        private static Object TakeLock = new Object();

        private View _view = null;

        private int _workerId;
        private int _requestId = 0;

        private static AutoResetEvent[] readHandles;

        private static AutoResetEvent[] takeHandles;

        private static Int32 takeCounter = 0;

        /// <summary>
        /// Delegate for Reading Operations
        /// </summary>
        public delegate Tuple RemoteAsyncReadDelegate(Tuple t);

        public delegate void RemoteAsyncSecondPhaseDelegate(Tuple t, int workerId, bool writeOnLog=false);

        /// <summary>
        /// Delegate for Writes that require workerId, requestId and a tuple.
        /// </summary>
        public delegate void RemoteAsyncWriteDelegate(int workerId, int requestId, Tuple t, bool writeOnLog = false);

        /// <summary>
        /// Delegate for Takes that require workerId, requestId and a tuple.
        /// Remeber: Take retrieves a list of potencial tuples to be removed
        /// </summary>
        public delegate List<Tuple> RemoteAsyncTakeDelegate(int workerId, int requestId, Tuple t);

        public static int numServers = 0;

        public FrontEndXL(int workerId)
        {
            _workerId = workerId;
            _view = this.GetView();

            int number_servers = this.GetView().Count();
            readHandles = new AutoResetEvent[1];
            

        }

        /// <summary>
        /// First read response.
        /// It will be update on callback function
        /// </summary>
        
        private static Tuple _responseRead = null;

        private static List<List<Tuple>> _responseTake = null;

        public View GetView()
        {
            List<ITupleSpaceXL> serversObj = new List<ITupleSpaceXL>();
            List<string> toRemove = new List<string>();

            if(_view != null)
            {
                foreach(string i in _view.Servers)
                {
                    try
                    {
                        ITupleSpaceXL viewGetter = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), i);
                        View tempView = viewGetter.GetActualView();
                        
                        if (tempView.Version > _view.Version)
                        {
                            _view = tempView;
                        }

                        serversObj.Add(viewGetter);
                        break;
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        toRemove.Add(i);                        
                    }
                }
                if(serversObj.Count > 0)
                {
                    foreach(string s in toRemove)
                    {
                        serversObj[0].Remove(s);
                        _view.Remove(s);
                    }
                    
                }
                return _view;
            }

            string[] file = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "../../../config/serverListXL.txt"));
            List<string> servers = new List<string>();

            foreach (string i in file)
            {

                try
                {
                    ITupleSpaceXL viewGetter = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), i);
                    return viewGetter.GetActualView();
                }
                catch (System.Net.Sockets.SocketException)
                {
                    Console.WriteLine("Cannot get View from " + i);
                    continue;
                }
            }
            Console.WriteLine("No server is available. Press <Enter> to exit...");
            Console.ReadLine();
            Environment.Exit(0);
            return null;
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

        /// <summary>
        /// Callback function to process take's server' replies
        /// <param name="ar"A AsyncResult Delgate Object.</param>
        /// </summary>
        public static void CallbackTake(IAsyncResult ar)
        {
            RemoteAsyncTakeDelegate del = (RemoteAsyncTakeDelegate)((AsyncResult)ar).AsyncDelegate;
            lock (TakeLock)
            {
                _responseTake.Add(del.EndInvoke(ar));
                takeHandles[takeCounter++].Set();
                if (takeCounter == numServers)
                    takeCounter = 0;
            }

        }

        public Tuple Read(Tuple tuple)
        {
            readHandles = new AutoResetEvent[1];
            readHandles[0] = new AutoResetEvent(false);

            View actualView = this.GetView();
            List<ITupleSpaceXL> serversObj = new List<ITupleSpaceXL>();
            List<string> toRemove = new List<string>();

            ITupleSpaceXL tupleSpace = null;

            _responseRead = null;

            //save remoting objects of all members of the view
            foreach (string serverPath in actualView.Servers)
            {
                try
                {
                    tupleSpace = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), serverPath);
                    tupleSpace.ItemCount(); //just to check availability of the server
                }
                catch (System.Net.Sockets.SocketException) {
                    tupleSpace = null;
                    toRemove.Add(serverPath);
                }
                if (tupleSpace != null)
                    serversObj.Add(tupleSpace);
            }

            if (serversObj.Count > 0)
            {
                foreach (string crashed in toRemove)
                {
                    //only one can crash so serverObj is obviously online
                    _view = serversObj[0].Remove(crashed);
                }
            }
            else {
                Console.WriteLine("All servers are dead! Exiting...");
                Environment.Exit(1);
            }


            foreach (ITupleSpaceXL server in serversObj)
            {
                try
                {
                    RemoteAsyncReadDelegate RemoteDel = new RemoteAsyncReadDelegate(server.read);
                    AsyncCallback RemoteCallback = new AsyncCallback(CallbackRead);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(tuple, CallbackRead, null);
                }
                catch (System.Net.Sockets.SocketException) { Console.WriteLine("** FRONTEND READ: Cannot invoke read on server!"); }
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
            View actualView = this.GetView();
            int numberServers = actualView.Servers.Count;
            numServers = numberServers;
            takeHandles = new AutoResetEvent[numberServers];

            for (int i = 0; i < actualView.Servers.Count; i++)
            {
                takeHandles[i] = new AutoResetEvent(false);
            }

            Console.WriteLine("Vou tentar take: " + tuple);
            List<ITupleSpaceXL> serversObj = new List<ITupleSpaceXL>();

            List<string> toRemove = new List<string>();

            ITupleSpaceXL tupleSpace = null;
            //save remoting objects of all members of the view
            Tuple tup = null;
            
            while (tup == null)
            {
                _responseTake = new List<List<Tuple>>();
                foreach (string serverPath in actualView.Servers)
                {
                    try
                    {
                        tupleSpace = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), serverPath);
                        tupleSpace.ItemCount(); //just to check availability of the server
                    }
                    catch (System.Net.Sockets.SocketException) {
                        tupleSpace = null;
                        toRemove.Add(serverPath);
                    }
                    if (tupleSpace != null)
                        serversObj.Add(tupleSpace);
                }

                if (serversObj.Count > 0)
                {
                    foreach (string crashed in toRemove)
                    {
                        //only one can crash so serverObj is obviously online
                        _view = serversObj[0].Remove(crashed);
                    }
                }
                else
                {
                    Console.WriteLine("All servers are dead! Exiting...");
                    Environment.Exit(1);
                }

                foreach (ITupleSpaceXL server in serversObj)
                {
                    try
                    {
                        RemoteAsyncTakeDelegate RemoteDel = new RemoteAsyncTakeDelegate(server.take);
                        AsyncCallback RemoteCallback = new AsyncCallback(CallbackTake);
                        IAsyncResult RemAr = RemoteDel.BeginInvoke(_workerId, _requestId, tuple, CallbackTake, null);
                    }
                    catch (System.Net.Sockets.SocketException) {
                        lock (TakeLock)
                        {
                            takeHandles[takeCounter++].Set();
                            if (takeCounter == numServers)
                                takeCounter = 0;
                        }
                    }
                }

                //miguel: this only works in perfect case
                //TODO: One machine belonging to the view has just crashed

                WaitHandle.WaitAll(takeHandles);


                //Performs the intersection of all responses and decide using TupleSelection
                tup = TupleSelection(Intersection(_responseTake));
                if(tup == null)
                {
                    Remove(null);
                }

            }

            //Tuple tup is now selected lets remove
            if (tup != null)
                Remove(tup);
            _requestId++;

            if (tup != null)
                Console.WriteLine("** FRONTEND TAKE: Here is a response: " + tup);
            else
                Console.WriteLine("** FRONTEND TAKE: Here is a NULL response");
            return tup;
        }

        public void Remove(Tuple tuple)
        {
            Console.WriteLine("Remove o tuplo: " + tuple);
            View actualView = this.GetView();
            List<ITupleSpaceXL> serversObj = new List<ITupleSpaceXL>();

            ITupleSpaceXL tupleSpace = null;
            //save remoting objects of all members of the view
            foreach (string serverPath in actualView.Servers)
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

            foreach (ITupleSpaceXL server in serversObj)
            {
                try
                {
                    RemoteAsyncSecondPhaseDelegate RemoteDel = new RemoteAsyncSecondPhaseDelegate(server.remove);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(tuple, _workerId, true, null, null);
                }
                catch (Exception) { }
            }
            Console.WriteLine("** FRONTEND REMOVE: Just removed " + tuple);
        }

        //Selects a tuple do initiate the second phase of take
        private static Tuple TupleSelection(IEnumerable<Tuple> l)
        {
            if (l.Count() == 0)
                return null;
            return l.ElementAt(0);
        }

        public static IEnumerable<Tuple> Intersection(List<List<Tuple>> list)
        {
            //Intersection of an empty list
            if (list.Count == 0)
                return new List<Tuple>();
            else if (list.Count == 1)
                return list[0];

            IEnumerable<Tuple> intersectSet = list.ElementAt(0);

            for (int i = 1; i < list.Count(); i++)
            {
                intersectSet = list.ElementAt(i).Intersect(intersectSet, new TupleComparator());
            }


            return intersectSet;
        }

        public void Write(Tuple tuple)
        {
            View actualView = this.GetView();
            List<ITupleSpaceXL> serversObj = new List<ITupleSpaceXL>();

            List<string> toRemove = new List<string>();

            ITupleSpaceXL tupleSpace = null;
            //save remoting objects of all members of the view
            foreach (string serverPath in actualView.Servers)
            {
                try
                {
                    tupleSpace = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), serverPath);
                    tupleSpace.ItemCount(); //just to check availability of the server
                }
                catch (System.Net.Sockets.SocketException)
                {
                    tupleSpace = null;
                    Console.WriteLine("** FRONTEND WRITE: Could not connected to server at " + serverPath);
                    toRemove.Add(serverPath);
                }
                if (tupleSpace != null)
                    serversObj.Add(tupleSpace);
            }

            if (serversObj.Count > 0)
            {

                foreach (string crashed in toRemove)
                {
                   _view = serversObj[0].Remove(crashed);
                }
                
            }
            else {
                Console.WriteLine("All servers are dead! Exiting...");
                Environment.Exit(1);
            }


            foreach (ITupleSpaceXL server in serversObj)
            {
                try
                {
                    RemoteAsyncWriteDelegate RemoteDel = new RemoteAsyncWriteDelegate(server.write);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(_workerId, _requestId, tuple, true, null, null);
                }
                catch (System.Net.Sockets.SocketException) {
                    Console.WriteLine("** FRONTEND WRITE: Could not call write on server");

                }
            }
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
