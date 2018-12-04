using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIDA_LIBRARY
{
    /// <summary>
    /// Class for describing View sets.
    /// A view is a dynamic representation of alive servers.
    /// </summary>

    [Serializable]
    public class View
    {

        private List<String> _servers;

        private static readonly object padlock = new object();

        private int version = 0;

        public View()
        {
            _servers = new List<String>();
        }

        public List<string> Servers { get => _servers;}

        public int Version { get => version; }

        public void Add(String str)
        {
            Servers.Add(str);
        }

        public void Remove(String str)
        {
            Servers.Remove(str);
        }

        public void IncrementVersion()
        {
            version++;
        }

        public int Count()
        {
            return _servers.Count;
        }

        public override string ToString()
        {
            string tostring = "";
            foreach (string i in _servers)
            {
                tostring += i + "\n";
            }
            return tostring;
        }

    }
}
