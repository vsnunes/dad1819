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

            if (args.Count() > 0)
            {
                channel = new TcpChannel(Int32.Parse(args[0]));
            }
            else
            {
                channel = new TcpChannel(8088);
            }
            
            ChannelServices.RegisterChannel(channel, false);

            TupleSpaceSMR server = new TupleSpaceSMR();
            server.MyPath = "tcp://localhost:" + args[0] + "/DIDA-TUPLE-SMR";

            RemotingServices.Marshal(server, "DIDA-TUPLE-SMR", typeof(TupleSpaceSMR));

            List<string> servers = new List<string>();
            try
            {
                string[] file = File.ReadAllLines("../../serverList.txt");
                
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

            foreach (string serverPath in servers) {
                try
                {
                    ITotalOrder remoteServer = (ITotalOrder)Activator.GetObject(typeof(ITotalOrder), serverPath);
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
