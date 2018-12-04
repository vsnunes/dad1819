using System;
using System.Collections.Generic;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;
using System.IO;


namespace DIDA_CLIENT
{
    class FrontEndSMR : IFrontEnd
    {
        string[] file = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "../../../config/serverListSMR.txt"));
        List<string> servers = new List<string>();

        public FrontEndSMR()
        {
            foreach (string i in file)
            {
                servers.Add(i);
            }
        }

        public List<string> GetView()
        {
            List<string> view = new List<string>();
            ITupleSpace tupleSpace = null;
            
            foreach (string serverPath in servers)
            {
                try
                {
                    tupleSpace = (ITupleSpace)Activator.GetObject(typeof(ITupleSpace), serverPath);
                    tupleSpace.ItemCount();
                }
                catch (Exception) { tupleSpace = null; }
                if (tupleSpace != null)
                    view.Add(serverPath);
            }
            return view;
        }

        public Tuple Read(Tuple tuple)
        {
            ITupleSpace tupleSpace = null;

            foreach (string i in this.GetView())
            {
                try
                {
                    tupleSpace = (ITupleSpace)Activator.GetObject(typeof(ITupleSpace), i);
                    tupleSpace.ItemCount();
                    Tuple response = null;
                    response = tupleSpace.read(tuple);
                    if (response != null)
                        return response;
                }catch(Exception){ Console.WriteLine("Server with address: " + i + "has crashed"); }
            }
            return null;
        }

        public Tuple Take(Tuple tuple)
        {

            ITupleSpace tupleSpace = null;

            foreach (string i in this.GetView())
            {
                try
                {
                    tupleSpace = (ITupleSpace)Activator.GetObject(typeof(ITupleSpace), i);
                    tupleSpace.ItemCount();
                    Tuple response = null;
                    response = tupleSpace.take(tuple);
                    if (response != null)
                        return response;
                }catch(Exception){ Console.WriteLine("Server with address: " + i + "has crashed"); }
            }
            return null; 
        }

        public void Write(Tuple tuple)
        {
            ITupleSpace tupleSpace = null;

            foreach (string i in this.GetView())
            {
                try
                {
                    tupleSpace = (ITupleSpace)Activator.GetObject(typeof(ITupleSpace), i);
                    tupleSpace.ItemCount();
                    tupleSpace.write(tuple);
                }catch(Exception) { Console.WriteLine("Server with address: " + i + "has crashed"); }
            }
        }

        View IFrontEnd.GetView()
        {
            throw new NotImplementedException();
        }
    }
}
