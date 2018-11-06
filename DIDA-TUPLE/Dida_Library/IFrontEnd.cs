using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIDA_LIBRARY
{
    /// <summary>
    /// An interface for describing Client/Worker basic operations. 
    /// </summary>
    public interface IFrontEnd
    {
        /// <summary>
        /// Returns a 
        /// </summary>
        /// <returns></returns>
        List<String> GetView();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        Tuple Read(Tuple tuple);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        Tuple Take(Tuple tuple);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tuple"></param>
        void Write(Tuple tuple);
    }
}
