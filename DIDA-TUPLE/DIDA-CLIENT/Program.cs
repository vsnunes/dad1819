using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_CLIENT
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> servers = new List<string>();

            List<Tuple> responses = new List<Tuple>();

            string[] file = File.ReadAllLines("../../serverList.txt");

            foreach (string i in file)
            {
                servers.Add(i);
            }

            List<ITupleSpace> serversObj = new List<ITupleSpace>();


            foreach (string serverPath in servers)
            {
                try
                {
                    serversObj.Add((ITupleSpace)Activator.GetObject(typeof(ITupleSpace), serverPath));
                    
                }
                catch (Exception) { }
            }
                
            if(serversObj.Count() == 0)
            {
                Console.WriteLine("No servers available!");
                System.Environment.Exit(1);
            }


            List<Object> fields = new List<Object>();
            fields.Add("cat");
            fields.Add("white");
            Tuple t = new Tuple(fields);

            if (args.Count() == 0)
            {
                Console.WriteLine("Client will read now ...");
                foreach(ITupleSpace its in serversObj)
                {
                    responses.Add(its.read(t));
                }
               
            }
            else if (args.Count() == 1)
            {
                Console.WriteLine("Client will take now ...");
                foreach (ITupleSpace its in serversObj)
                {
                    responses.Add(its.take(t));
                }
            }

            else
            {
                Console.WriteLine("Client will write now ...");
                foreach (ITupleSpace its in serversObj)
                {
                    its.write(t);
                }
                
            }

            Console.ReadLine();
        }
    }
}
