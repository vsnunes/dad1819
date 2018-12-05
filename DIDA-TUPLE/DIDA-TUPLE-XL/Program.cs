using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using DIDA_LIBRARY;

namespace DIDA_TUPLE_XL
{
    class Program
    {
        static void Main(string[] args)
        {

            TcpChannel channel;

            if (args.Count() > 0)
            {
                channel = new TcpChannel(Int32.Parse(args[0].Split(':')[2].Split('/')[0]));
            }
            else
            {
                channel = new TcpChannel(8088);
            }

            bool logUpdated = false;

            TupleSpaceXL server = new TupleSpaceXL(args[0]);

            //Set min delay and max delay
            if (args.Length == 3)
            {
                server.MinDelay = Int32.Parse(args[1]);
                server.MaxDelay = Int32.Parse(args[2]);
            }

            try
            {
                string[] file = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "../../../config/serverListXL.txt"));

                foreach (string i in file)
                {
                    if (i != args[0])
                    {

                        server.ServerList.Add(i);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                System.Console.WriteLine("Server List not Found");
                System.Console.WriteLine("Aborting...");
                System.Environment.Exit(1);
            }

            ChannelServices.RegisterChannel(channel, false);

            //if requests received, they are delayed until log recover and master finding complete
            server.Freeze();
            RemotingServices.Marshal(server, args[0].Split('/')[3], typeof(TupleSpaceXL));

            foreach (string serverPath in server.ServerList)
            {
                try
                {
                    ITupleSpaceXL remoteServer = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), serverPath);

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
                    
                }
                catch (Exception)
                {
                    Console.WriteLine(" attempt -> Failed reaching " + serverPath);
                }

            }

            server.Unfreeze();

            System.Console.WriteLine(args[0].Split('/')[3] + ": DIDA-TUPLE-XL Server Started!");
            System.Console.WriteLine("---------------");
            System.Console.WriteLine("<Enter> to exit...");
            System.Console.ReadLine();
            
        }
    }
}
