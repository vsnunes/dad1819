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
    public class View
    {

        private List<String> _servers;
        private static View instance = null;
        private static readonly object padlock = new object();

        private View()
        {
            _servers = new List<String>();
        }

        public static View Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new View();
                    }
                    return instance;
                }
            }
        }

        public List<string> Servers { get => _servers;}

        public void Add(String str)
        {
            Servers.Add(str);
        }

        public void Remove(String str)
        {
            Servers.Remove(str);
        }

    }
}
