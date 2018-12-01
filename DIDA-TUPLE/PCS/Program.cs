using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace PCS
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
                channel = new TcpChannel(10000);

                ChannelServices.RegisterChannel(channel, false);

            }
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(PCS),
                "ProcessCreationService",
                WellKnownObjectMode.Singleton);


            System.Console.WriteLine("Process Creation Service Started!");
            System.Console.WriteLine("---------------");
            System.Console.WriteLine("<Enter> to exit...");
            System.Console.ReadLine();

        }
    }
}
