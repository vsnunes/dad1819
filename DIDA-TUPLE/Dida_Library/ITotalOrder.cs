using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIDA_LIBRARY
{
    /// <summary>
    /// Interface for describing Total Order operations
    /// </summary>
    public interface ITotalOrder
    {
        
        void prepare(int id, Request.OperationType request, Tuple tuple);

        void commit(int id, Request.OperationType request, Tuple tuple);

        bool areYouTheMaster();

    }
}
