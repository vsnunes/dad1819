using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Collections.Generic;

using ChatRemotingInterfaces;

namespace Chat
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Server
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			TcpChannel channel = new TcpChannel(8086);
			ChannelServices.RegisterChannel(channel,false);
			RemotingConfiguration.RegisterWellKnownServiceType(
				typeof(ChatServerServices), "ChatServer", 
				WellKnownObjectMode.Singleton);
			System.Console.WriteLine("Press <enter> to terminate chat server...");
			System.Console.ReadLine();
		}
	}
	
	class ChatServerServices : MarshalByRefObject, IChatServer {
        List<IChatClient> clients;
		List<string> messages;

		ChatServerServices() {
            clients = new List<IChatClient>();
            messages = new List<string>();
		}


        public List<string> RegisterClient(string NewClientName) {
			Console.WriteLine("New client listening at " + "tcp://localhost:" + NewClientName + "/ChatClient");
			IChatClient newClient = 
				(IChatClient) Activator.GetObject(
                       typeof(IChatClient), "tcp://localhost:" + NewClientName + "/ChatClient");
			clients.Add(newClient);
			return messages;
		}

		/// <summary>
		/// o que impede aqui de que se houver muitas mensagens para redifundir, a thread difusora seja
		/// interrompida/intercalada com a thread Remoting que executa este metodo que altera a lista
		/// e que por sua vez tb ira lancar outra thread que envia a mensagem seguinte.
		/// Possibilidade de corrida em que a nova mensagem ja foi incorporada enquanto estao a ser enviadas ainda
		/// as copias da anterior mas, como mensagensCount-1 é reavaliado, a mensagem nova vai duas vezes e a
		/// anterior perdeu-se (para os ultimos clientes).
		/// 
		/// A razao de ser da thread adicional é para que possa ser possivel responder 
		/// a invocacao RMI do cliente, terminando-a, antes de fazer uma invocacao remota no mesmo cliente.
		/// O Remoting nao permite a invocacao de metodos remotos dum processo quando esse mesmo processo tb está 
		/// bloqueado  numa outro invocacao remota
		/// 
		/// Solucao possivel para a corrida. Fixar o valor de count copiando para uma variavel 
		/// e evitar a sua reavaliacao. Manter uma veriavel inteira que exlicitamente regista a ultima mensagem difundida
		/// 
		/// </summary>
		/// <param name="mensagem"></param>
		public void SendMsg(string mensagem){
			messages.Add(mensagem);
			ThreadStart ts = new ThreadStart(this.BroadcastMessage);
			Thread t = new Thread(ts);
			t.Start();
		}
		private void BroadcastMessage() {
            string MsgToBcast;
            lock (this) {
                MsgToBcast = messages[messages.Count - 1];
            }
			for (int i = 0; i < clients.Count ; i++) {
				try {
                    ((IChatClient)clients[i]).MsgToClient(MsgToBcast);}
				catch (Exception e) {
                    Console.WriteLine("Failed sending message to client. Removing client. " + e.Message);
					clients.RemoveAt(i);
				}
			}
		}
	}
}
