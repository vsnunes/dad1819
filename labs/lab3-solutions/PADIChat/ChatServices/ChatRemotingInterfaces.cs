using System;
using System.Collections.Generic;

namespace ChatRemotingInterfaces
{
	public interface IChatServer {
        List<string> RegisterClient(string NewClientPort);
		 void SendMsg(string message);
	}

	public interface IChatClient {
		void MsgToClient(string message);
	}
}
