using System;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

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

            ChannelServices.RegisterChannel(channel, false);

            TupleSpaceXL server = new TupleSpaceXL(args[0]);

            //Set min delay and max delay
            if (args.Length == 3)
            {
                server.MinDelay = Int32.Parse(args[1]);
                server.MaxDelay = Int32.Parse(args[2]);
            }

            RemotingServices.Marshal(server, args[0].Split('/')[3], typeof(TupleSpaceXL));

            System.Console.WriteLine(args[0].Split('/')[3] + ": DIDA-TUPLE-XL Server Started!");
            System.Console.WriteLine("---------------");
            System.Console.WriteLine("# of tuples: " + server.ItemCount());
            System.Console.WriteLine("---------------");
            System.Console.WriteLine("<Enter> to exit...");
            System.Console.ReadLine();
            
        }
    }
}
