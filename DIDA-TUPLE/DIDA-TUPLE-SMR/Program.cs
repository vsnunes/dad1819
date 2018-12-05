using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using DIDA_LIBRARY;

namespace DIDA_TUPLE_SMR
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpChannel channel;
            string name;
            int id;

            if (args.Count() > 0)
            {
                channel = new TcpChannel(Int32.Parse(args[0].Split(':')[2].Split('/')[0]));
                name = args[0].Split('/')[3];
                id = Int32.Parse(args[1]);
            }
            else
            {
                channel = new TcpChannel(8088);
                name = "DIDA-TUPLE-SMR";
                id = 1;
            }
            
            ChannelServices.RegisterChannel(channel, false);

            TupleSpaceSMR server = new TupleSpaceSMR();
            server.MyPath = args[0];
            server.ServerId = id;

            //if requests received, they are delayed until log recover and master finding complete
            server.SoftFreeze();
            RemotingServices.Marshal(server, name, typeof(TupleSpaceSMR));
            
            List<string> servers = new List<string>();
            try
            {
                string[] file = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "../../../config/serverListSMR.txt"));
                
                foreach (string i in file)
                {
                    //Just ignore my path when caching server's URL
                    if (args[0] != i)
                    {
                        servers.Add(i);
                    }
                }
                server.SetServers(servers);
            }
            catch (FileNotFoundException)
            {
                System.Console.WriteLine("Server List not Found");
                System.Console.WriteLine("Aborting...");
                System.Environment.Exit(1);
            }

            string pathMaster = "";
            bool logUpdated = false;

            Console.WriteLine(server.Log);

            bool alreadyDiscoverMaster = false;
            for (int i = 0; i < server.ServerId; i++)
            {
                foreach (string serverPath in servers)
                {
                    try
                    {
                        ITotalOrder remoteServer = (ITotalOrder)Activator.GetObject(typeof(ITotalOrder), serverPath);

                        if (logUpdated == false)
                        {
                            //when the server start running fetch from one server the log
                            //so i can sync my tuple space
                            server.Log = remoteServer.fetchLog();
                            Console.WriteLine(server.Log);
                            //execute by order operation in that log
                            server.executeLog();

                            logUpdated = true;

                        }
                        //ask if this replic is the master and give them my path
                        if (remoteServer.areYouTheMaster(args[0]))
                        {
                            pathMaster = serverPath;
                            server.MasterPath = pathMaster;
                            //All replicas ping the master
                            server.setBackup(pathMaster);
                            alreadyDiscoverMaster = true;
                            break;
                        }


                    }
                    catch (Exception)
                    {
                        Console.WriteLine((i + 1) + " attempt -> Failed reaching " + serverPath);
                    }
                    
                }
                if (alreadyDiscoverMaster)
                    break;
            }

            //no master exists! so i am the master now!
            if (pathMaster == "") {
                server.setIAmTheMaster();
                server.MasterPath = server.MyPath;
                Console.WriteLine("** STATUS: I'm the master now!");
            }
            else {
                Console.WriteLine("** STATUS: The master is at " + pathMaster);
            }

            //log was updated and master search is over, so i can receive client requests
            server.SoftUnFreeze();

            System.Console.WriteLine("DIDA-TUPLE-SMR Server Started!");
            System.Console.WriteLine("---------------");
            System.Console.WriteLine("<Enter> to exit...");
            System.Console.ReadLine();
        }
    }
}
