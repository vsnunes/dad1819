using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
       

        public TupleSpaceSMR()
        {
            _tupleSpace = new List<Tuple>();
            _log = new Log();
            _type = Type.NORMAL;
            _firstReplica = true;
            _backup = "";
            _myPath = "";
        }

        

        public int ItemCount()
        {
            return _tupleSpace.Count();
        }


        public Tuple read(Tuple tuple)
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

        //me no likey
        public void SetServers(List<string> servers){
            _servers = servers;
        }

        public List<Tuple> GetTuples()
        {
            return _tupleSpace;
        }
        /// <summary>
        /// Takes a tuple from the tuple space.
        /// </summary>
        /// <param name="tuple">The tuple to be taken.</param>
        /// <returns></returns>
        public Tuple take(Tuple tuple)
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
                    
                    //we are with the lock already so lets remove the element
                    else _tupleSpace.Remove(result);
                }
            }

            return result;
            
        }

        public void write(Tuple tuple)
        {
            //If any thread is waiting for read or take
            //notify them to check if this tuple match its requirements
            lock (this) { _tupleSpace.Add(tuple); Monitor.Pulse(this); }
            
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
            lock (this)
            {
                switch (request)
                {
                    case Request.OperationType.WRITE:

                        this.write(tuple);


                        break;

                    case Request.OperationType.TAKE:
                        this.take(tuple);

                        break;


                }

                //already commited changes so remove them from the log.
                _log.Remove(id);

            }

        }

        /// <summary>
        /// Get ready for a collective commit order by the MASTER.
        /// </summary>
        /// <param name="id">Identifier of request.</param>
        /// <param name="request">Type of request.</param>
        /// <param name="Tuple">A tuple to be passed to the request operation.</param>
        /// <returns></returns>
        public void prepare(int id, Request.OperationType request, Tuple tuple)
        {
            lock (this)
            {
                _log.Add(id, request, tuple, _type == Type.MASTER);
            }
        }

        public bool areYouTheMaster(string serverPath) {
            lock (this)
            {
                //First replica is my backup when i'm the master
                if (_firstReplica)
                {
                    _replic = (TupleSpaceSMR)Activator.GetObject(typeof(TupleSpaceSMR), serverPath);
                    _replic.setBackup(_myPath);
                    _firstReplica = false;
                }

            }
            return _type == Type.MASTER;
        }

        public void setNewMaster(string pathNewMaster)
        {
            _masterPath = pathNewMaster;
            Console.WriteLine("** NEW_MASTER: I was informed that the new master is at: " + _masterPath);
        }

        public void setIAmTheMaster() { _type = Type.MASTER; }

        public void setBackup(string path){

            //Backup will store the path to the master.
            lock (this)
            {
                _backup = path;
            }
            _replic = (TupleSpaceSMR)Activator.GetObject(typeof(TupleSpaceSMR), path);

            Task.Run(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    try
                    {
                        //is the master alive?
                        _replic.imAlive();
                        Console.WriteLine("My master is alive! Let me wait 10 secs!");
                    }
                    catch (Exception)
                    {
                        break;
                    }
                    Thread.Sleep(10000);
                }

                //My master has crashed!
                lock (this)
                {
                    Console.WriteLine("My master has just crashed! I'm the master now!");
                    this.setIAmTheMaster();
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
                
                
                return; //just to ensure that we stop the thread
            });
        }

        //Ping        
        public void imAlive() { }

    }
}
