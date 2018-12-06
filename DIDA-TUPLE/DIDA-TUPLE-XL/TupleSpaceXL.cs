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

        //quem atualiza a view sao os servidores, quem faz get da view atual sao os workers
        private View _view;

        private static LockList _lockList;

        private List<string> serverList = new List<string>();

        private string myPath;

        public bool onUpdate = false;

        public static readonly Object onUpdateLock = new Object();

        public TupleSpaceXL(String path)
        {
            _view = new View();
            _tupleSpace = new List<Tuple>();
            _lockList = new LockList();
            MinDelay = 0;
            MaxDelay = 0;

            myPath = path;
            bool obtainedView = false;


            try
            {
                string[] file = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "../../../config/serverListXL.txt"));

                foreach (string i in file)
                {
                    if (i != path)
                    {

                        ServerList.Add(i);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                System.Console.WriteLine("Server List not Found");
                System.Console.WriteLine("Aborting...");
                System.Environment.Exit(1);
            }

            List<List<Tuple>> allTupleSpaces = new List<List<Tuple>>();
            foreach (string s in ServerList)
            {
                try
                {
                    Console.WriteLine("Trying to get view acesss: " + s);
                    TupleSpaceXL otherServer = (TupleSpaceXL)Activator.GetObject(typeof(TupleSpaceXL), s);
                    View currentView = otherServer.AddView(myPath);
                    allTupleSpaces.Add(otherServer.GetTupleSpace());
                    if(_view.Version < currentView.Version)
                        _view = currentView;

                    obtainedView = true;

                }
                catch (System.Net.Sockets.SocketException)
                {
                    Console.WriteLine(s + " is not alive");
                }
            }

            _tupleSpace = Intersection(allTupleSpaces).ToList();
            
            foreach(string s in _view.Servers)
            {
                if(s != myPath)
                {
                    TupleSpaceXL otherServer = (TupleSpaceXL)Activator.GetObject(typeof(TupleSpaceXL), s);
                    otherServer.SetTupleSpace(_tupleSpace);
                    otherServer.SetOnUpdate(false);
                }
            }
            this.SetOnUpdate(false);

            //If no one replies to the view then i create my own view
            if (obtainedView == false)
            {
                _view = new View();
                _view.Add(myPath);
            }
        }

        public void SetOnUpdate(bool v)
        {
            lock (onUpdateLock)
            {
                onUpdate = v;

                if (onUpdate == true)
                {
                    Monitor.PulseAll(onUpdateLock);
                }
            }

        }

        public void SetTupleSpace(List<Tuple> tupleSpace)
        {
            _tupleSpace = tupleSpace;
        }

        private int generateRandomDelay()
        {
            if (MinDelay == 0 && MaxDelay == 0)
                return 0;
            return new Random().Next(MinDelay, MaxDelay);
        }

        public int MinDelay { get => _minDelay; set => _minDelay = value; }
        public int MaxDelay { get => _maxDelay; set => _maxDelay = value; }
        public List<string> ServerList { get => serverList; set => serverList = value; }
        public string MyPath { get => myPath; set => myPath = value; }

        public Tuple read(Tuple tuple)
        {
            //Checks if the server is freezed
            lock (this)
            {
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

        public void remove(Tuple tuple, int workerId)
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
        public List<Tuple> take(int workerId, int requestId, Tuple tuple, View view)
        {
            //Checks if the server is freezed
            lock (this)
            {
                while (freeze)
                    Monitor.Wait(this);
            }

            if (_view.Version > view.Version)
            {
                lock (onUpdateLock)
                {
                    while (onUpdate == true)
                    {
                        Monitor.Wait(onUpdateLock);
                    }
                }
                throw new ViewChangeException();
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

        public void write(int workerId, int requestId, Tuple tuple, View view)
        {
            //Checks if the server is freezed
            lock (this)
            {
                while (freeze)
                    Monitor.Wait(this);
            }

            if (_view.Version > view.Version)
            {
                lock (onUpdateLock)
                {
                    while (onUpdate == true)
                    {
                        Monitor.Wait(onUpdateLock);
                    }
                }
                throw new ViewChangeException();
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
            lock (this)
            {
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
            List<string> notalive = new List<string>();
            List<string> servers = _view.Servers.ToList();
            foreach (string i in servers)
            {
                try
                {
                    TupleSpaceXL server = (TupleSpaceXL)Activator.GetObject(typeof(TupleSpaceXL), i);
                    server.ItemCount();
                    Console.WriteLine(i);
                }
                catch (Exception)
                {
                    notalive.Add(i);
                    Remove(i);
                }

            }

            Console.WriteLine("Not alive servers:");
            foreach (string server in notalive)
            {
                Console.WriteLine(server);
            }
        }

        public View AddView(string url)
        {
            _view.Add(url);
            lock(onUpdateLock)
                onUpdate = true;
            return _view;

        }

        public View Remove(string url)
        {
            this.RemoveFromView(url);
            foreach (string server in _view.Servers)
            {
                if (server != myPath)
                {
                    TupleSpaceXL serverObj = (TupleSpaceXL)Activator.GetObject(typeof(TupleSpaceXL), server);
                    serverObj.RemoveFromView(url);

                }
            }

            return _view;
        }

        public View RemoveFromView(string url)
        {
            if (_view.Servers.Contains(url))
            {
                _view.Remove(url);
            }
            return _view;
        }

        public List<Tuple> GetTupleSpace()
        {
            return _tupleSpace;
        }

        public View GetActualView()
        {
            return _view;
        }

        public IEnumerable<Tuple> Intersection(List<List<Tuple>> list)
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