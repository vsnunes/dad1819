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

        private int _workerId;
        private int _requestId = 0;

        private static AutoResetEvent[] readHandles;

        private static AutoResetEvent[] takeHandles;

        private static Int32 takeCounter = 0;

        /// <summary>
        /// Delegate for Reading Operations
        /// </summary>
        public delegate Tuple RemoteAsyncReadDelegate(Tuple t);

        public delegate void RemoteAsyncSecondPhaseDelegate(Tuple t, int workerId);

        /// <summary>
        /// Delegate for Writes that require workerId, requestId and a tuple.
        /// </summary>
        public delegate void RemoteAsyncWriteDelegate(int workerId, int requestId, Tuple t);

        /// <summary>
        /// Delegate for Takes that require workerId, requestId and a tuple.
        /// Remeber: Take retrieves a list of potencial tuples to be removed
        /// </summary>
        public delegate List<Tuple> RemoteAsyncTakeDelegate(int workerId, int requestId, Tuple t);

        public static int numServers = 0;

        private View _view = null;

        public FrontEndXL(int workerId)
        {
            _workerId = workerId;

        }

        /// <summary>
        /// First read response.
        /// It will be update on callback function
        /// </summary>
        private static Tuple _responseRead = null;

        private static List<List<Tuple>> _responseTake = null;

        public View GetView()
        {
            if(_view != null)
            {
                return _view;
            }

            string[] file = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "../../../config/serverListXL.txt"));

            View mostRecentView = null;
            View possibleView = null;

            foreach (string i in file)
            {

                try
                {
                    ITupleSpaceXL viewGetter = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), i);
                    possibleView = viewGetter.GetActualView();
                    if(mostRecentView == null || mostRecentView.Version < possibleView.Version)
                    {
                        mostRecentView = possibleView;
                    }
                }
                catch (System.Net.Sockets.SocketException)
                {
                    Console.WriteLine("Cannot get View from " + i);
                    continue;
                }
                _view = mostRecentView;
                return _view;
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

        /// <summary>
        /// Test all servers in the current view and request view changes if discover some possible server crash
        /// Cases: Catch on exception
        /// </summary>
        public List<ITupleSpaceXL> getServersFromView()
        {
            this.GetView();
            List<string> possibleCrashed = null;
            List<ITupleSpaceXL> liveServers = null;
            ITupleSpaceXL server = null;
            do
            {
                possibleCrashed = new List<string>();
                server = null;
                liveServers = new List<ITupleSpaceXL>();

                List<string> actualServersOnView = _view.Servers.ToList();

            
                //Test if the server on this current view are still alive.
                foreach (string serverPath in actualServersOnView)
                {
                    try
                    {
                        server = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), serverPath);
                        View possibleNewView = server.GetActualView();
                        if (_view.Version < possibleNewView.Version)
                            _view = possibleNewView;
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        possibleCrashed.Add(serverPath);
                        continue;
                    }
                    liveServers.Add(server);

                }

                if (possibleCrashed.Count > 0)
                {
                    try
                    {
                        _view = liveServers[0].RemoveFromView(possibleCrashed);
                    } catch(System.Net.Sockets.SocketException) {
                        continue; //When the server just crashed when we are about to remove the crashes
                    }
                }
            } while (possibleCrashed.Count > 0);
            
            //When there are no possible crashed servers just return the liveServers
            return liveServers;


 
        }


        public Tuple Read(Tuple tuple)
        {
            readHandles = new AutoResetEvent[1];

            readHandles[0] = new AutoResetEvent(false);

            foreach (ITupleSpaceXL server in this.getServersFromView())
            {
                try
                {
                    RemoteAsyncReadDelegate RemoteDel = new RemoteAsyncReadDelegate(server.read);
                    AsyncCallback RemoteCallback = new AsyncCallback(CallbackRead);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(tuple, CallbackRead, null);
                }
                catch (Exception) { Console.WriteLine("** FRONTEND READ: Cannot invoke read on server!"); }
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
            Console.WriteLine("Vou tentar take: " + tuple);
            View actualView = this.GetView();
            List<ITupleSpaceXL> serversObj = new List<ITupleSpaceXL>();

            ITupleSpaceXL tupleSpace = null;

            int number_servers = this.GetView().Count();
            numServers = number_servers;
            takeHandles = new AutoResetEvent[number_servers];

            for (int i = 0; i < number_servers; i++)
            {
                takeHandles[i] = new AutoResetEvent(false);
            }

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
                    catch (Exception) { tupleSpace = null; }
                    if (tupleSpace != null)
                        serversObj.Add(tupleSpace);
                }

                foreach (ITupleSpaceXL server in serversObj)
                {
                    try
                    {
                        RemoteAsyncTakeDelegate RemoteDel = new RemoteAsyncTakeDelegate(server.take);
                        AsyncCallback RemoteCallback = new AsyncCallback(CallbackTake);
                        IAsyncResult RemAr = RemoteDel.BeginInvoke(_workerId, _requestId, tuple, CallbackTake, null);
                    }
                    catch (Exception) { }
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
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(tuple, _workerId, null, null);
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

            ITupleSpaceXL tupleSpace = null;
            //save remoting objects of all members of the view
            foreach (string serverPath in actualView.Servers)
            {
                try
                {
                    tupleSpace = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), serverPath);
                    tupleSpace.ItemCount(); //just to check availability of the server
                }
                catch (Exception) { tupleSpace = null; Console.WriteLine("** FRONTEND WRITE: Could not connected to server at " + serverPath); }
                if (tupleSpace != null)
                    serversObj.Add(tupleSpace);
            }

            foreach (ITupleSpaceXL server in serversObj)
            {
                try
                {
                    RemoteAsyncWriteDelegate RemoteDel = new RemoteAsyncWriteDelegate(server.write);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(_workerId, _requestId, tuple, null, null);
                }
                catch (Exception) { Console.WriteLine("** FRONTEND WRITE: Could not call write on server"); }
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
