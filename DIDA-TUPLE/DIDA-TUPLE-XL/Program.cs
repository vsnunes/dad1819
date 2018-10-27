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
            TcpChannel channel = new TcpChannel(8087);
            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(TupleSpaceXL),
                "DIDA-TUPLE-XL",
                WellKnownObjectMode.Singleton);

            System.Console.WriteLine("DIDA-TUPLE-XL Server Started!");
            System.Console.WriteLine("---------------");
            System.Console.WriteLine("<Enter> to exit...");
            System.Console.ReadLine();
        }
}
}
