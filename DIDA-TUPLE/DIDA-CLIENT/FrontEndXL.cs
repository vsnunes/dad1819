using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
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

            _requestId++;

            return  null; //TupleSelection(Intersection(_responseTake));
        }

        //Selects a tuple do initiate the second phase of take
        private Tuple TupleSelection(List<Tuple> l)
        {
            return l.ElementAt(0);
        }

        public static IEnumerable<Tuple> Intersection(List<List<Tuple>> list) {
            if (list.Count == 1)
                return list[0];

            
            IEnumerable<Tuple> intersectSet = new List<Tuple>();
            intersectSet = list.ElementAt(0);

            for (int i = 2; i < list.Count(); i++) {
                intersectSet = list.ElementAt(i).Intersect(intersectSet);
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
}
