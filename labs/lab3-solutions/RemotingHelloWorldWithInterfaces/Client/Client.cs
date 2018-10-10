using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace RemotingHelloWorld {
	
	class Client {
		
		static void Main() {
			TcpChannel channel = new TcpChannel();
			ChannelServices.RegisterChannel(channel,true);

            IHello obj = (IHello)Activator.GetObject(
                typeof(IHello),
				"tcp://localhost:8086/HelloService");
			if (obj == null) {
				System.Console.WriteLine("Could not locate server");
			} else {
        Console.WriteLine(obj.Hello());
			}
			Console.ReadLine();
		}
	}
}