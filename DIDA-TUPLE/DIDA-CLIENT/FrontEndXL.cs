using System;
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

    class FrontEndXL: IFrontEnd
    {
        private static Object Lock = new Object();

        private int _workerId;
        private int _requestId = 0;

        /// <summary>
        /// Delegate for Reading Operations
        /// </summary>
        public delegate Tuple RemoteAsyncDelegate(Tuple t);

        /// <summary>
        /// Delegate for Changers (aka Read and Takes) that require workerId, requestId and a tuple.
        /// </summary>
        public delegate void RemoteAsyncChangerDelegate(int workerId,int requestId, Tuple t);

        public FrontEndXL(int workerId) {
            _workerId = workerId; 
        }

        /// <summary>
        /// First read response.
        /// It will be update on callback function
        /// </summary>
        private static Tuple response = null;

        public List<string> GetView()
        {
            return View.Instance.Servers;
        }

        /// <summary>
        /// First read response.
        /// It will be update on callback function
        /// </summary>
        public static void Callback(IAsyncResult ar)
        {
            RemoteAsyncDelegate del = (RemoteAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
            if (response == null)
            {
                response = del.EndInvoke(ar);
                Monitor.Pulse(Lock);
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
                    RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(server.read);
                    AsyncCallback RemoteCallback = new AsyncCallback(Callback);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(tuple,Callback,null);
                }
                catch (Exception) { }
            }

            while(response == null)
            {
                Monitor.Wait(Lock);
            }
            _requestId++;
            return response;
        }

        public Tuple Take(Tuple tuple)
        {
            List<string> actualView = this.GetView();
            foreach (string server in actualView)
            {

            }
            _requestId++;
            return null;
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
                    RemoteAsyncChangerDelegate RemoteDel = new RemoteAsyncChangerDelegate(server.write);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(_workerId, _requestId, tuple, null, null);
                }
                catch (Exception) { }
            }
            _requestId++;
        }
    }
}
