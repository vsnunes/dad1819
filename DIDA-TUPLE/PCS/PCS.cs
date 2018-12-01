using System;
using DIDA_LIBRARY;

namespace PCS
{
    class PCS : MarshalByRefObject, IPCS
    {
        public enum ServerType { XL, SMR };
        private ServerType _type;

        public PCS(string Type)
        {
            if (Type == "XL")
                _type = ServerType.XL;
            else if (Type == "SMR")
                _type = ServerType.SMR;
        }

        public string Server(int min_delay, int max_delay)
        {
            System.Console.WriteLine("Criei Server");
            if (_type == ServerType.SMR)
                return "SMR";
            else if (_type == ServerType.XL)
                return "XL";
            return null;
        }

        public string Client(string script_file)
        {
            System.Console.WriteLine("Criei Cliente");
            if (_type == ServerType.SMR)
                return "SMR";
            else if (_type == ServerType.XL)
                return "XL";
            return null;
        }

        public string Status()
        {
            return "STAAAAAAAAAAAAAAAAAAAAAAAAAAAATUS";
        }

        public void Crash(string processName) { }

        public void Freeze(string processName) { }

        public void Unfreeze(string processName) { }

    }
}
