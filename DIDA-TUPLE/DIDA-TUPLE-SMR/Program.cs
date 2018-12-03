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

            if (args.Count() > 0)
            {
                channel = new TcpChannel(Int32.Parse(args[0]));
                name = args[1];
            }
            else
            {
                channel = new TcpChannel(8088);
                name = "DIDA-TUPLE-SMR";
            }
            
            ChannelServices.RegisterChannel(channel, false);

            TupleSpaceSMR server = new TupleSpaceSMR();
            server.MyPath = "tcp://localhost:" + args[0] + "/" + name;
            

            RemotingServices.Marshal(server, name, typeof(TupleSpaceSMR));
            
            List<string> servers = new List<string>();
            try
            {
                string[] file = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "../../../DIDA-TUPLE-SMR/serverList.txt"));
                
                foreach (string i in file)
                {
                    if ("tcp://localhost:" + args[0] + "/DIDA-TUPLE-SMR" != i)
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

            foreach (string serverPath in servers) {
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

                    if (remoteServer.areYouTheMaster("tcp://localhost:" + args[0] + "/DIDA-TUPLE-SMR"))
                    {
                        pathMaster = serverPath;
                        server.MasterPath = pathMaster;
                        break;
                    }

                }
                catch (Exception) {
                    Console.WriteLine("Failed reaching " + serverPath);
                }
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

            System.Console.WriteLine("DIDA-TUPLE-SMR Server Started!");
            System.Console.WriteLine("---------------");
            System.Console.WriteLine("<Enter> to exit...");
            System.Console.ReadLine();
        }
    }
}
