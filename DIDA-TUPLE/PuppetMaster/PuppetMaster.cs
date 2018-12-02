using System;
using DIDA_LIBRARY;
using System.Collections.Generic;
using System.IO;

namespace PUPPETMASTER
{
    public class PuppetMaster : MarshalByRefObject
    {
        public string MyPath = null;

        string[] file = File.ReadAllLines("../../pcsList.txt");
        List<IPCS> ipcs = new List<IPCS>();

        private static int XLFEcounter = 0;

        Dictionary<string, Process> processNames = new Dictionary<string, Process>();

        public PuppetMaster()
        {
            foreach (string i in file)
            {
                ipcs.Add((IPCS)Activator.GetObject(typeof(IPCS), i));
            }

        }

        public void Server(string server_id, string URL, int min_delay, int max_delay) {
            //Parse
            string urlPcs = URL.Split(':')[0] + ":" + URL.Split(':')[1] + ":10000/pcs";
            if(processNames.ContainsKey(server_id))
            {
                Console.WriteLine("Server ID already exists");
                return;
            }

            IPCS pcs = null;
            pcs = (IPCS)Activator.GetObject(typeof(IPCS), urlPcs);
            string type = pcs.Server(URL, min_delay, max_delay);
            if (type == "SMR")
                processNames.Add(server_id, new Process(URL, Process.Type.SERVER_SMR));
            else if (type == "XL")
                processNames.Add(server_id, new Process(URL, Process.Type.SERVER_XL));
        }

        public void Client(string client_id, string URL, string script_file)
        {
            if (processNames.ContainsKey(client_id))
            {
                Console.WriteLine("Client ID already exists");
                return;
            }
            //Parse
            string urlPcs = URL.Split(':')[0] + ":" + URL.Split(':')[1] + ":10000/pcs";

            IPCS pcs = null;
            pcs = (IPCS)Activator.GetObject(typeof(IPCS), urlPcs);
            string type = pcs.Client(URL, script_file, XLFEcounter);
            if (type == "SMR")
                processNames.Add(client_id, new Process(URL, Process.Type.CLIENT_SMR));
            else if (type == "XL")
            {
                processNames.Add(client_id, new Process(URL, Process.Type.CLIENT_XL));
                XLFEcounter++;
            }
        }

        public void Status() {
            foreach (KeyValuePair<string, Process> KVP in processNames)
            {
                if (KVP.Value.Type1 == Process.Type.SERVER_SMR)
                {
                    ITupleSpace smrServer = (ITupleSpace)Activator.GetObject(typeof(ITupleSpace), KVP.Value.Url);
                    smrServer.Status();
                }
                if (KVP.Value.Type1 == Process.Type.SERVER_XL)
                {
                    ITupleSpaceXL XLServer = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), KVP.Value.Url);
                    XLServer.Status();
                }


            }     
                   
        }

        public void Crash(string processName) {
            try
            {
                Process process = processNames[processName];
                if (process.Type1 == Process.Type.CLIENT_XL || process.Type1 == Process.Type.CLIENT_SMR)
                {
                    Console.WriteLine("You can't crash Clients");
                }
                else if (process.Type1 == Process.Type.SERVER_SMR)
                {
                    ITupleSpace smrServer = (ITupleSpace)Activator.GetObject(typeof(ITupleSpace), process.Url);
                    smrServer.Crash();
                }
                else if (process.Type1 == Process.Type.SERVER_XL)
                {
                    ITupleSpaceXL xlServer = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), process.Url);
                    xlServer.Crash();
                }
            }catch(System.Net.Sockets.SocketException)
            {
                Console.WriteLine("Server with process name " + processName + " has crashed.");
            }
           
        }

        public void Freeze(string processName) {
            Process process = processNames[processName];
            if(process.Type1 == Process.Type.CLIENT_XL || process.Type1 == Process.Type.CLIENT_SMR)
            {
                Console.WriteLine("Clients can't be frozen");
            }
            else if(process.Type1 == Process.Type.SERVER_SMR)
            {
                ITupleSpace smrServer = (ITupleSpace)Activator.GetObject(typeof(ITupleSpace), process.Url);
                smrServer.Freeze();
            }
            else if(process.Type1 == Process.Type.SERVER_XL)
            {
                ITupleSpaceXL xlServer = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), process.Url);
                xlServer.Freeze();
            }
            System.Console.WriteLine("processName" + processName);
        }

        public void Unfreeze(string processName)
        {
            Process process = processNames[processName];
            if (process.Type1 == Process.Type.CLIENT_XL || process.Type1 == Process.Type.CLIENT_SMR)
            {
                Console.WriteLine("Clients can't be Unfrozen");
            }
            else if (process.Type1 == Process.Type.SERVER_SMR)
            {
                ITupleSpace smrServer = (ITupleSpace)Activator.GetObject(typeof(ITupleSpace), process.Url);
                smrServer.Unfreeze();
            }
            else if (process.Type1 == Process.Type.SERVER_XL)
            {
                ITupleSpaceXL xlServer = (ITupleSpaceXL)Activator.GetObject(typeof(ITupleSpaceXL), process.Url);
                xlServer.Unfreeze();
            }
        }
    }
}
