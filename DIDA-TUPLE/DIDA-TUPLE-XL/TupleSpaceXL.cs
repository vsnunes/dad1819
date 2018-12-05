using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;
using System.IO;

namespace DIDA_TUPLE_XL
{
    public class TupleSpaceXL : MarshalByRefObject, ITupleSpaceXL
    {
        private int _minDelay;
        private int _maxDelay;
        private static bool freeze = false;
        private List<Tuple> _tupleSpace;
        private Log _log;

        //quem atualiza a view sao os servidores, quem faz get da view atual sao os workers
        private View _view;

        private static LockList _lockList;

        private List<string> serverList = new List<string>();

        private string myPath;

        public TupleSpaceXL(String path)
        {
            _view = new View();
            _tupleSpace = new List<Tuple>();
            _lockList = new LockList();
            MinDelay = 0;
            MaxDelay = 0;

            myPath = path;
            bool obtainedView = false;
           

            foreach (string s in ServerList)
            {
                try
                {
                    Console.WriteLine("Trying to get view acesss: " + s);
                    TupleSpaceXL otherServer = (TupleSpaceXL)Activator.GetObject(typeof(TupleSpaceXL), s);
                    View currentView = otherServer.AddView(myPath);
                    SetView(currentView);
                    obtainedView = true;
                    break;
                }
                catch (System.Net.Sockets.SocketException) {
                    Console.WriteLine(s + " is not alive");
                    continue;
                }
            }

            //If no one replies to the view then i create my own view
            if(obtainedView == false)
            {
                _view = new View();
                _view.Add(myPath);
            }
        }


        private int generateRandomDelay() {
            if (MinDelay == 0 && MaxDelay == 0)
                return 0;
            return new Random().Next(MinDelay, MaxDelay);
        }

        public Log Log { get => _log; set => _log = value; }

        public int MinDelay { get => _minDelay; set => _minDelay = value; }
        public int MaxDelay { get => _maxDelay; set => _maxDelay = value; }
        public List<string> ServerList { get => serverList; set => serverList = value; }
        public string MyPath { get => myPath; set => myPath = value; }

        public Tuple read(Tuple tuple)
        {
            //Checks if the server is freezed
            lock(this) {
                while (freeze)
                    Monitor.Wait(this);
            }

            Thread.Sleep(generateRandomDelay());
            Tuple result = null;

            while (result == null)
            {
                lock (_tupleSpace)
                {
                    foreach (Tuple t in _tupleSpace)
                    {
                        if (t.Equals(tuple))
                        {
                            result = t;
                            break; //just found one so no need to continue searching
                        }
                    }
                    if (result == null) //stil has not find any match
                        Monitor.Wait(_tupleSpace);
                }
            }
            Console.WriteLine("** XL READ: Just read " + result);
            return result;
        }

        public void remove(Tuple tuple, int workerId, bool writeOnLog = true)
        {
            if (tuple == null)
            {
                _lockList.ReleaseAllLocks(workerId);
            }
            else
            {
                Tuple match = null;
                lock (_tupleSpace)
                {
                    foreach (Tuple t in _tupleSpace)
                    {
                        if (t.Equals(tuple))
                        {
                            match = t;
                            break; //just found what i want to remove
                        }

                    }

                    if (match != null)
                    {
                        _lockList.ReleaseAllLocks(workerId);
                        _tupleSpace.Remove(match);
                        if (writeOnLog)
                        {
                            lock (_log)
                            {
                                _log.Add(_log.Counter, Request.OperationType.TAKE, tuple);
                                _log.Increment();
                            }
                        }
                    }
                }
                Console.WriteLine("** XL REMOVE: Just removed " + tuple);
            }
           
        }

        public int ItemCount()
        {
            return _tupleSpace.Count();
        }

        /// <summary>
        /// Takes a tuple from the tuple space.
        /// </summary>
        /// <param name="tuple">The tuple to be taken.</param>
        /// <returns></returns>
        public List<Tuple> take(int workerId, int requestId, Tuple tuple)
        {
            //Checks if the server is freezed
            lock (this)
            {
                while (freeze)
                    Monitor.Wait(this);
            }

            Thread.Sleep(generateRandomDelay());
            List<Tuple> result = new List<Tuple>();
            int timeout = new Random().Next(100, 500);


            while (result.Count == 0)
            {
                lock (_tupleSpace)
                {

                    foreach (Tuple t in _tupleSpace)
                    {
                        bool lockTaken = false;

                        if (t.Equals(tuple))
                        {
                            Monitor.TryEnter(t, timeout, ref lockTaken);
                            if (lockTaken)
                            {
                                _lockList.AddElement(workerId, t);
                                result.Add(t);
                            }
                        }
                    }
                    if (result.Count == 0) //stil has not find any match
                        Monitor.Wait(_tupleSpace);
                }
            }

            Console.WriteLine("** XL TAKE-PHASE1");
            return result;
        }

        public void write(int workerId, int requestId, Tuple tuple, bool writeOnLog=true)
        {
            //Checks if the server is freezed
            lock (this)
            {
                while (freeze)
                    Monitor.Wait(this);
            }

            Thread.Sleep(generateRandomDelay());
            //If any thread is waiting for read or take
            //notify them to check if this tuple match its requirements
            lock (_tupleSpace)
            {
                _tupleSpace.Add(tuple);
                Monitor.PulseAll(_tupleSpace);
            }
            Console.WriteLine("** XL WRITE: " + tuple);

            if (writeOnLog)
            {
                lock (_log)
                {
                    _log.Add(_log.Counter, Request.OperationType.TAKE, tuple);
                    _log.Increment();
                }
            }
        }


        // ================= TWO PHASE COMMIT FOR TAKE OPERATIONS =================

        public void Freeze()
        {
            lock (this)
            {
                freeze = true;
            }
        }

        public void Unfreeze()
        {
            lock(this) {
                freeze = false;
                Monitor.PulseAll(this);
            }
        }

        public void Crash()
        {
            System.Environment.Exit(0);
        }

        public void Status()
        {
            Console.WriteLine("My actual view:");
            Console.WriteLine(_view);
        }

        public View AddView(string url)
        {
            _view.Add(url);
            //RM para propagar a mudança
            _view.IncrementVersion();
            return _view;
        }

        public View RemoveFromView(string url)
        {
            _view.Remove(url);
            //RM para propagar a mudança
            _view.IncrementVersion();
            return _view;
        }

        public View GetActualView()
        {
            return _view;
        }
       
        public void SetView(View view)
        {
            _view = view;
        }

        public Log fetchLog()
        {
            return _log;
        }

        public void executeLog() {
            List<Request> _requests = new List<Request>();
            _requests = _log.Requests;
            
            foreach (Request request in _requests)
            {
                if (request.OperationId == Request.OperationType.WRITE)
                {
                    write(0, 0, request.Tuple, false);
                }
                else if (request.OperationId == Request.OperationType.TAKE)
                {
                    write(0, 0, request.Tuple, false);
                }
            }
        }
    }
}