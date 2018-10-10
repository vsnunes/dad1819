using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace RemotingHelloWorld {

    class Server {

        static void Main(string[] args) {
            // change usingSingleton to false to use Marshal activation
            bool usingSingleton = false;
            HelloService myRem = null;

            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, true);

            if (usingSingleton) {
                RemotingConfiguration.RegisterWellKnownServiceType(
                  typeof(HelloService),
                  "HelloService",
                  WellKnownObjectMode.Singleton);
            } else {
                myRem = new HelloService();
                RemotingServices.Marshal(myRem, "HelloService");
            }
            System.Console.WriteLine("<enter> to exit...");
            System.Console.ReadLine();
        }
    }
}