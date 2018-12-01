using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUPPETMASTER
{
    [Serializable]
    class Process
    {
        private string _url;
        private Type _type;

        public string Url { get => _url; set => _url = value; }
        internal Type Type1 { get => _type; set => _type = value; }

        public enum Type { CLIENT_SMR, SERVER_SMR, CLIENT_XL, SERVER_XL };

        public Process(string url, Type type)
        {
            Url = url;
            Type1 = type;
        }

    }
}
