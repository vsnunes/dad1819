using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;
using System.IO;

namespace DIDA_TUPLE_XL
{
    /// <summary>
    /// Tuple Space approach using Xu and Liskov implementation
    /// </summary>
    public class TupleSpaceXL : MarshalByRefObject, ITupleSpaceXL
    {
        /// <summary>
        /// DEBUG PM: Min Delay allowed
        /// </summary>
        private int _minDelay;

        /// <summary>
        /// DEBUG PM: Max Delay allowed
        /// </summary>
        private int _maxDelay;

        /// <summary>
        /// Is the server freeze?
        /// </summary>
        private static bool freeze = false;

        /// <summary>
        /// The Tuple space complex structure :)
        /// </summary>
        private List<Tuple> _tupleSpace;

        //quem atualiza a view sao os servidores, quem faz get da view atual sao os workers
        private View _view;

        /// <summary>
        /// A list containing which tuples are locked and what is the client ID that locked them.
        /// </summary>
        private static LockList _lockList;

        /// <summary>
        /// A list of all possible servers (including the alive and dead ones).
        /// </summary>
        private List<string> serverList = new List<string>();

        /// <summary>
        /// My current path
        /// </summary>
        private string myPath;

        /// <summary>
        /// The status of update process.
        /// Remember that the Update Process is ON when a new server joins the view.
        /// </summary>
        public bool onUpdate = false;

        /// <summary>
        /// A Lock that prevents client from receiving the new view when are in the "middle" of a write/take operation.
        /// </summary>
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
            List<string> serversCopy = _view.Servers.ToList();

            foreach(string s in serversCopy)
            {
                if(s != myPath)
                {
                    try
                    {
                        TupleSpaceXL otherServer = (TupleSpaceXL)Activator.GetObject(typeof(TupleSpaceXL), s);
                        otherServer.SetTupleSpace(_tupleSpace);
                        otherServer.SetOnUpdate(false);
                    } catch (System.Net.Sockets.SocketException)
                    {
                        Console.WriteLine("** SETTING TS: Server at: " + s + " is dead but is in my view!");
                        _view.Remove(s);
                    }
                }
            }
            //Already have the new tuple space. Update process is now complete. Inform the clients of the new view.
            this.SetOnUpdate(false);

            //If no one replies to the view then i create my own view
            if (obtainedView == false)
            {
                _view = new View();
                _view.Add(myPath);
            }
        }

        /// <summary>
        /// Sets On Update Process.
        /// This happens when a new server joins the view.
        /// This causes the client to be paused waiting for the new server recovering process
        /// </summary>
        /// <param name="v"></param>
        public void SetOnUpdate(bool v)
        {
            lock (onUpdateLock)
            {
                onUpdate = v;

                if (onUpdate == false)
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

        /// <summary>
        /// Removes a tuple from the tuple space. This is take PHASE 2.
        /// </summary>
        /// <param name="tuple">The tuple to be removed</param>
        /// <param name="workerId">The client front end ID.</param>
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

        /// <summary>
        /// Returns the number of tuples in the tuple space.
        /// </summary>
        /// <returns></returns>
        public int ItemCount()
        {
            return _tupleSpace.Count();
        }

        /// <summary>
        /// Takes a tuple from the tuple space. This is only the TAKE PHASE 1
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
                return null;
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

        public bool write(int workerId, int requestId, Tuple tuple, View view)
        {
            //Checks if the server is freezed
            lock (this)
            {
                while (freeze)
                    Monitor.Wait(this);
            }

            Console.WriteLine("MINHA VIEW NO WRITE: " + _view);

            if (_view.Version > view.Version)
            {
                lock (onUpdateLock)
                {
                    while (onUpdate == true)
                    {
                        Monitor.Wait(onUpdateLock);
                    }
                }
                return false;
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
            return true;
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

        /// <summary>
        /// Forces the server to check liveliness of all members of the view and remove
        /// those that are dead.
        /// </summary>
        public void checkView()
        {
            List<string> servers = _view.Servers.ToList();
            foreach (string i in servers)
            {
                if(i!= myPath)
                {
                    try
                    {
                        TupleSpaceXL server = (TupleSpaceXL)Activator.GetObject(typeof(TupleSpaceXL), i);
                        server.ItemCount();
                        Console.WriteLine(i);
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        Remove(i);
                    }
                }
            }
        }

        /// <summary>
        /// Displays the server alive nodes and dead ones.
        /// </summary>
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

        /// <summary>
        /// Adds a new URL (machine) to the view.
        /// </summary>
        /// <param name="url">The URL of the machine to be added.</param>
        /// <returns>The new view containing the new machine.</returns>
        public View AddView(string url)
        {
            _view.Add(url);
            lock(onUpdateLock)
                onUpdate = true;
            return _view;

        }

        /// <summary>
        /// Removes a server from the view and propagate those changes to other members of the view
        /// </summary>
        /// <param name="url">The URL (machine) to be removed from the View</param>
        /// <returns>The new view without the machine removed.</returns>
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

        /// <summary>
        /// Only removes the URL (machine) from our view BUT DO NOT PROPAGATE those changes to other members.
        /// </summary>
        /// <param name="url">The URL (machine) to be removed from the View</param>
        /// <returns>The new view without the machine removed</returns>
        public View RemoveFromView(string url)
        {
            if (_view.Servers.Contains(url))
            {
                _view.Remove(url);
            }
            return _view;
        }

        /// <summary>
        /// Returns the tuple space
        /// </summary>
        /// <returns>A complex structure of a tuple space</returns>
        public List<Tuple> GetTupleSpace()
        {
            return _tupleSpace;
        }

        /// <summary>
        /// Returns the current view of the server
        /// </summary>
        /// <returns>A view structure representing current view.</returns>
        public View GetActualView()
        {
            lock (onUpdateLock)
            {
                while (onUpdate)
                {
                    Monitor.Wait(onUpdateLock);
                }
            }
            return _view;
        }

        /// <summary>
        /// Given a set of Tuples returns a intersection of those tuples.
        /// </summary>
        /// <param name="list">A List of list of tuples to intersect</param>
        /// <returns>A list of tuples corresponding to the intersection of tuples.</returns>
        public IEnumerable<Tuple> Intersection(List<List<Tuple>> list)
        {
            if (list.Count == 0)
                return new List<Tuple>();
            else if (list.Count == 1)
                return list.ElementAt(0);

            List<Tuple> firstList = list.ElementAt(0);
            List<Tuple> intersectSet = new List<Tuple>();
            bool notIn;

            foreach (Tuple tuple in firstList)
            {
                notIn = false;

                for (int i = 0; i < list.Count; i++)
                {
                    if (list.ElementAt(i).Contains(tuple) == false)
                    {
                        notIn = true;
                        break;
                    }
                }
                if (notIn == false)
                {
                    intersectSet.Add(tuple);

                    for (int i = 1; i < list.Count; i++)
                    {
                        list.ElementAt(i).Remove(tuple);
                    }
                }
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