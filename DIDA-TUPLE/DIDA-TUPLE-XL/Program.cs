using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                channel = new TcpChannel(Int32.Parse(args[0]));
            }
            else
            {
                channel = new TcpChannel(8088);

                ChannelServices.RegisterChannel(channel, false);

                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof(TupleSpaceXL),
                    "DIDA-TUPLE-XL",
                    WellKnownObjectMode.Singleton);
            }

            RemotingConfiguration.RegisterWellKnownServiceType(
                     typeof(TupleSpaceXL),
                     args[1],
                     WellKnownObjectMode.Singleton);



                System.Console.WriteLine(args[1] + ": DIDA-TUPLE-XL Server Started!");
                System.Console.WriteLine("---------------");
                System.Console.WriteLine("<Enter> to exit...");
                System.Console.ReadLine();
            
        }
    }
}
