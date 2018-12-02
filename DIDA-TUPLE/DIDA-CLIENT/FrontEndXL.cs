using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

        public delegate void RemoteAsyncSecondPhaseDelegate(Tuple t);

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

        public FrontEndXL(int workerId)
        {
            _workerId = workerId;

            int number_servers = this.GetView().Count();
            numServers = number_servers;
            readHandles = new AutoResetEvent[1];
            takeHandles = new AutoResetEvent[number_servers];

            readHandles[0] = new AutoResetEvent(false);

            for (int i = 0; i < number_servers; i++)
            {
                takeHandles[i] = new AutoResetEvent(false);
            }

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


            foreach (ITupleSpaceXL server in serversObj)
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
            List<string> actualView = this.GetView();
            List<ITupleSpaceXL> serversObj = new List<ITupleSpaceXL>();

            ITupleSpaceXL tupleSpace = null;
            //save remoting objects of all members of the view


            _responseTake = new List<List<Tuple>>();
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
            Tuple tup = TupleSelection(Intersection(_responseTake));

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

            foreach (ITupleSpaceXL server in serversObj)
            {
                try
                {
                    RemoteAsyncSecondPhaseDelegate RemoteDel = new RemoteAsyncSecondPhaseDelegate(server.remove);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(tuple, null, null);
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
