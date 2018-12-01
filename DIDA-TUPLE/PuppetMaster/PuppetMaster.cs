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

        Dictionary<String, Process> processNames = new Dictionary<string, Process>();

        public PuppetMaster()
        {
            foreach (string i in file)
            {
                ipcs.Add((IPCS)Activator.GetObject(typeof(IPCS), i));
            }

        }

        public void Server(string server_id, string URL, int min_delay, int max_delay) {
            //Parse
            string urlPcs = URL.Split(':')[0] + ":" + URL.Split(':')[1] + ":/10000/pcs";

            IPCS pcs = null;
            pcs = (IPCS)Activator.GetObject(typeof(IPCS), urlPcs);
            string type = pcs.Server(URL, min_delay, max_delay);
            if (type == "SMR")
                processNames.Add(server_id, new Process(URL, Process.Type.SERVER_SMR));
            else if (type == "XL")
                processNames.Add(server_id, new Process(URL, Process.Type.SERVER_XL));
        }

        public void Client(string client_id, string URL, string script_file) {
            //Parse
            string urlPcs = URL.Split(':')[0] + ":" + URL.Split(':')[1] + ":/10000/pcs";

            IPCS pcs = null;
            pcs = (IPCS)Activator.GetObject(typeof(IPCS), urlPcs);
            string type = pcs.Client(URL, script_file);
            if (type == "SMR")
                processNames.Add(client_id, new Process(URL, Process.Type.CLIENT_SMR));
            else if (type == "XL")
                processNames.Add(client_id, new Process(URL, Process.Type.CLIENT_XL));
        }

        public void Status() {
            foreach (IPCS pcs in ipcs)
                Console.WriteLine(pcs.Status());
        }

        public void Crash(string processName) {
            Process process = processNames[processName];
            if (process.Type1 == Process.Type.CLIENT_XL || process.Type1 == Process.Type.CLIENT_SMR)
            {
                IFrontEnd client = (IFrontEnd)Activator.GetObject(typeof(IFrontEnd), process.Url);
                client.Crash();
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
            //System.Console.WriteLine("processName" + processName);
        }

        public void Freeze(string processName) {
            Process process = processNames[processName];
            if(process.Type1 == Process.Type.CLIENT_XL || process.Type1 == Process.Type.CLIENT_SMR)
            {
                IFrontEnd client = (IFrontEnd)Activator.GetObject(typeof(IFrontEnd), process.Url);
                client.Freeze();
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
                IFrontEnd client = (IFrontEnd)Activator.GetObject(typeof(IFrontEnd), process.Url);
                client.Unfreeze();
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
            System.Console.WriteLine("processName" + processName);
        }
    }
}
