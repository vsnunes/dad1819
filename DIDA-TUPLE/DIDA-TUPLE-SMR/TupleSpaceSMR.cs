using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_TUPLE_SMR
{
    public class TupleSpaceSMR : MarshalByRefObject, ITupleSpace, ITotalOrder
    {
        //Possibilidade de hashtable
        private List<Tuple> _tupleSpace;

        /** Unique identifier of the server **/
        private int _serverId;

        private object PMLock = new object();
        private object VCLock = new object();

        private static bool freeze = false;

        private View _view;

        /// <summary>
        /// List of pending operations that requires coordination between all replics.
        /// </summary>
        private Log _log;

        /// <summary>
        /// Type of the server.
        /// NORMAL -> Server replic
        /// MASTER -> Leader of changes in replic's group
        /// By default a new server is a NORMAL server.
        /// </summary>
        public enum Type { NORMAL, MASTER };

        private string _masterPath;

        private List<string> _servers;

        private Type _type;

        /// <summary>
        /// Check the first areYouTheMaster to select a candidate to backup me (the MASTER)
        /// </summary>
        private bool _firstReplica;

        /// <summary>
        /// Path for the backup server.
        /// </summary>
        private string _backup;

        private TupleSpaceSMR _replic;

        private string _myPath;

        public string MyPath { get => _myPath; set => _myPath = value; }

        public string MasterPath {set => _masterPath = value; }
        public Log Log { get => _log; set => _log = value; }
        public int ServerId { get => _serverId; set => _serverId = value; }

        public TupleSpaceSMR()
        {
            _tupleSpace = new List<Tuple>();
            _log = new Log();
            _type = Type.NORMAL;
            _firstReplica = true;
            _backup = "";
            _myPath = "";
            _servers = new List<string>();
            //_view = View.Instance;
            //_view.Add(MyPath);
        }

        

        public int ItemCount()
        {
            return _tupleSpace.Count();
        }


        public Tuple read(Tuple tuple)
        {
            //Checks if the server is freezed
            lock (PMLock)
            {
                while (freeze)
                    Monitor.Wait(PMLock);
            }

            if (_type != Type.MASTER)
            {
                return null;
            }
            else
            {
                Tuple result = null;

                while (result == null)
                {
                    lock (this)
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
                            Monitor.Wait(this);
                    }
                }

                return result;
            }
        }

        //me no likey
        public void SetServers(List<string> servers){
            _servers = servers;
        }

        public List<Tuple> GetTuples()
        {
            return _tupleSpace;
        }
        
        /// <summary>
        /// Returns my log containing all operations performed
        /// </summary>
        /// <returns></returns>
        public Log fetchLog(){
            return _log;
        }

        public int GetID() {
            return _serverId;
        }

        public void executeLog(){
            List<Request> _requests = new List<Request>();
            _requests = _log.Requests;

            foreach(Request request in _requests){
                if(request.OperationId == Request.OperationType.WRITE)
                {
                    executeWrite(request.Tuple, false);
                }
                else if(request.OperationId == Request.OperationType.TAKE)
                {
                    executeTake(request.Tuple, false);
                }
            }
        }

        /// <summary>
        /// Takes a tuple from the tuple space.
        /// </summary>
        /// <param name="tuple">The tuple to be taken.</param>
        /// <returns></returns>
        public Tuple take(Tuple tuple)
        {
            //Checks if the server is freezed
            lock (PMLock)
            {
                while (freeze)
                    Monitor.Wait(PMLock);
            }

            if (_type != Type.MASTER)
            {
                return null;
            }
            else
            {
               
                //When all servers are ready perfome the commit and
                //everyone will execute the prepared instruction at the same order.
                foreach (string path in _servers)
                {
                    try
                    {
                        TupleSpaceSMR replic = (TupleSpaceSMR)Activator.GetObject(typeof(TupleSpaceSMR), path);
                        replic.commit(_log.Counter, Request.OperationType.TAKE, tuple);
                        Console.WriteLine("** TAKE: Successfuly commited server at " + path);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("** TAKE: Failed to commit server at " + path);
                    }
                }

                //performs the take on master
                return this.executeTake(tuple);
            }


        }

        private Tuple executeTake(Tuple tuple, bool writeOnLog=true){
            Tuple result = null;
            while (result == null)
            {
                lock (this)
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
                        Monitor.Wait(this);
                         
                    //we are with the lock already so lets remove the element
                    else _tupleSpace.Remove(result);

                    //if flag is true then write on current replic log
                    
                }
            }
            if (writeOnLog)
            {
                lock (this)
                {
                    _log.Add(_log.Counter, Request.OperationType.TAKE, tuple, _type == Type.MASTER);
                    _log.Increment();
                }
            }
            
            Console.WriteLine("** EXECUTE_TAKE: " + tuple);
            return result;
            
        }

        public void write(Tuple tuple)
        {
            //Checks if the server is freezed
            lock (PMLock)
            {
                while (freeze)
                    Monitor.Wait(PMLock);
            }

            if (_type != Type.MASTER)
            {
                return;
            }
            else
            {
                
                //When all servers are ready perfome the commit and
                //everyone will execute the prepared instruction at the same order.
                foreach (string path in _servers)
                {
                    try
                    {
                        TupleSpaceSMR replic = (TupleSpaceSMR)Activator.GetObject(typeof(TupleSpaceSMR), path);
                        replic.commit(_log.Counter, Request.OperationType.WRITE, tuple);
                        Console.WriteLine("** WRITE: Successfuly commited server at " + path);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("** WRITE: Failed to commit server at " + path);
                    }
                }

                //performs the write on master
                this.executeWrite(tuple);
            }
        }

        private void executeWrite(Tuple tuple, bool writeOnLog=true)
        {
            //If any thread is waiting for read or take
            //notify them to check if this tuple match its requirements
            lock (this) {
                _tupleSpace.Add(tuple);
                if (writeOnLog)
                {
                    _log.Add(_log.Counter, Request.OperationType.WRITE, tuple, _type == Type.MASTER);
                    _log.Increment();
                }
               
                Monitor.Pulse(this); }
            Console.WriteLine("** EXECUTE_WRITE: " + tuple);
        }

        /// <summary>
        /// Performes a collective commit order by the MASTER.
        /// </summary>
        /// <param name="id">Identifier of request.</param>
        /// <param name="request">Type of request.</param>
        /// <param name="Tuple">A tuple to be passed to the request operation.</param>
        /// <returns></returns>
        public void commit(int id, Request.OperationType request, Tuple tuple)
        {

                switch (request)
                {
                    case Request.OperationType.WRITE:

                        this.executeWrite(tuple);


                        break;

                    case Request.OperationType.TAKE:
                        this.executeTake(tuple);

                        break;

                        
                }
        }

        public bool areYouTheMaster(string serverPath) {
            //Checks if the server is freezed
            lock (PMLock)
            {
                while (freeze)
                    Monitor.Wait(PMLock);
            }

            
            return _type == Type.MASTER;
        }

        public void setNewMaster(string pathNewMaster)
        {
            //Checks if the server is freezed
            lock (PMLock)
            {
                while (freeze)
                    Monitor.Wait(PMLock);
            }
            _masterPath = pathNewMaster;
            Console.WriteLine("** NEW_MASTER: I was informed that the new master is at: " + _masterPath);
            //New master so new machine to ping!
            setBackup(_masterPath);
        }

        public void setIAmTheMaster() { _type = Type.MASTER; }

        public void setBackup(string path) {

            //Backup will store the path to the master.
            lock (this)
            {
                _backup = path;
            }
            _replic = (TupleSpaceSMR)Activator.GetObject(typeof(TupleSpaceSMR), path);

            Task.Run(() =>
            {
            Thread.CurrentThread.IsBackground = true;
            int timeout = new Random().Next(3000, 10000);
            while (true)
            {
                lock (PMLock)
                {
                    while (freeze)
                        Monitor.Wait(PMLock);
                }

                try
                {
                    //is the master alive?
                    _replic.imAlive();
                    Console.WriteLine("My master is alive! Let me wait " + timeout / 1000 + " seconds");
                }
                catch (Exception)
                {
                    break;
                }
                Thread.Sleep(timeout);
            }

            int minID = ServerId;

            //Asks all servers for replying its IDs
            foreach (string serverPath in _servers) {
                TupleSpaceSMR server = (TupleSpaceSMR)Activator.GetObject(typeof(TupleSpaceSMR), serverPath);
                try
                {
                    int serverId = server.GetID();
                    if (serverId < minID)
                       minID = serverId;
                }
                catch (Exception)
                {
                    Console.WriteLine("** Replic at: " + serverPath + " not responding to my GetID command! Ignoring...");
                }
            }

                //I'm the master when my ID is the smaller
                if (minID == ServerId)
                {

                    //My master has crashed!
                    lock (this)
                    {
                        Console.WriteLine("My master has just crashed! Let me freeze everything.");

                        // ====== FREEZE PROCCESS ======
                        foreach (string serverPath in _servers)
                        {
                            try
                            {
                                TupleSpaceSMR server = (TupleSpaceSMR)Activator.GetObject(typeof(TupleSpaceSMR), serverPath);
                                //Freeze all servers before setting i am the master to ensure no one is take my lidership
                                //10 is the timeout, if i failed all replicas wait at least 10 seconds before unfreeze
                                server.Freeze(10);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("** Replic at: " + serverPath + " not responding to my freeze command! Ignoring...");
                            }
                        }

                        this.setIAmTheMaster();

                        // ====== UNFREEZE PROCCESS ======
                        foreach (string serverPath in _servers)
                        {
                            try
                            {
                                TupleSpaceSMR server = (TupleSpaceSMR)Activator.GetObject(typeof(TupleSpaceSMR), serverPath);
                                //Unfreeze all servers
                                server.Unfreeze(10);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("** Replic at: " + serverPath + " not responding to my unfreeze command! Ignoring...");
                            }
                        }

                    }
                    //But we need to update alive servers because they don't know that his
                    //master is not available!
                    foreach (string serverPath in _servers)
                    {
                        try
                        {
                            TupleSpaceSMR server = (TupleSpaceSMR)Activator.GetObject(typeof(TupleSpaceSMR), serverPath);
                            //I'm the master so inform others
                            server.setNewMaster(_myPath);
                            Console.WriteLine("** NEW_MASTER_UPDATE: Successfuly informed " + serverPath + " of who is the new master!");
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("** NEW_MASTER_UPDATE: Tried to inform " + serverPath + " but its dead!");
                        }
                    }

                }
                return; //just to ensure that we stop the thread
            });
        
            Console.WriteLine("** SETBACKUP: Is about to finish!");
        }

        //Ping        
        public void imAlive() {
            //Checks if the server is freezed
            lock (PMLock)
            {
                while (freeze)
                    Monitor.Wait(PMLock);
            }
        }

        public void Freeze()
        {
            lock(PMLock) {
                freeze = true;
            }
        }

        public void Freeze(int seconds)
        {
            lock (VCLock)
            {
                freeze = true;
            }

            Task.Run(() =>
            {
                Thread.Sleep(seconds * 1000);
                lock(VCLock)
                    Monitor.PulseAll(VCLock);
            });
        }

        public void Unfreeze()
        {
            lock (PMLock)
            {
                freeze = false;
                Monitor.PulseAll(PMLock);
            }
        }

        public void Unfreeze(int seconds)
        {
            lock (VCLock)
            {
                freeze = false;
                Monitor.PulseAll(VCLock);
            }
        }

        public void Crash()
        {
            System.Environment.Exit(0);
        }

        public void Status()
        {
            Console.WriteLine("My actual view:");
            foreach (string i in _servers)
            {
                Console.WriteLine(i);
            }
        }
    }
}
