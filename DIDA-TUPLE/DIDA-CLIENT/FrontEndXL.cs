using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_CLIENT
{

    public class FrontEndXL: IFrontEnd
    {
        private static Object ReadLock = new Object();

        private static Object TakeLock = new Object();


        private int _workerId;
        private int _requestId = 0;

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
        public delegate List<Tuple> RemoteAsyncTakeDelegate(int workerId, int requestId, Tuple t);

        public FrontEndXL(int workerId) {
            _workerId = workerId; 
        }

        /// <summary>
        /// First read response.
        /// It will be update on callback function
        /// </summary>
        private static Tuple _responseRead = null;

        private static List<List<Tuple>> _responseTake = null;

        public List<string> GetView()
        {
            return View.Instance.Servers;
        }

        /// <summary>
        /// Callback function to process read's server' replies
        /// <param name="IAsyncResult"A AsyncResult Delgate Object.</param>
        /// </summary>
        public static void CallbackRead(IAsyncResult ar)
        {
            RemoteAsyncReadDelegate del = (RemoteAsyncReadDelegate)((AsyncResult)ar).AsyncDelegate;
            if (_responseRead == null)
            {
                _responseRead = del.EndInvoke(ar);
                Monitor.Pulse(ReadLock);
            }
        }

        /// <summary>
        /// Callback function to process take's server' replies
        /// <param name="ar"A AsyncResult Delgate Object.</param>
        /// </summary>
        public static void CallbackTake(IAsyncResult ar)
        {
            RemoteAsyncTakeDelegate del = (RemoteAsyncTakeDelegate)((AsyncResult)ar).AsyncDelegate;
            lock(TakeLock) {
                _responseTake.Add(del.EndInvoke(ar));
                Monitor.Pulse(TakeLock);
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

            foreach(ITupleSpaceXL server in serversObj)
            {
                try
                {
                    RemoteAsyncReadDelegate RemoteDel = new RemoteAsyncReadDelegate(server.read);
                    AsyncCallback RemoteCallback = new AsyncCallback(CallbackRead);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(tuple, CallbackRead, null);
                }
                catch (Exception) { }
            }

            while(_responseRead == null)
            {
                Monitor.Wait(ReadLock);
            }
            _requestId++;
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
            while (_responseTake.Count() != serversObj.Count())
            {
                Monitor.Wait(TakeLock);
            }

            Tuple tup = TupleSelection(Intersection(_responseTake));
            Remove(tup);
            _requestId++;
            return  null; 
        }

        public void Remove(Tuple tuple){
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
        }

        //Selects a tuple do initiate the second phase of take
        private static Tuple TupleSelection(IEnumerable<Tuple> l)
        {
            return l.ElementAt(0);
        }

        public static IEnumerable<Tuple> Intersection(List<List<Tuple>> list) {
            //Intersection of an empty list
            if (list.Count == 0)
                return new List<Tuple>();
            else if (list.Count == 1)
                return list[0];

            IEnumerable<Tuple> intersectSet = list.ElementAt(0);
            Console.WriteLine(intersectSet.ElementAt(0));

            for (int i = 1; i < list.Count(); i++) {
                intersectSet = list.ElementAt(i).Intersect(intersectSet, new TupleComparator());
                Console.WriteLine(intersectSet.Count());
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
                catch (Exception) { tupleSpace = null; }
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
                catch (Exception) { }
            }
            _requestId++;
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
