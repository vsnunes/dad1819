using System;
using System.Collections.Generic;

namespace DIDA_LIBRARY
{
    /// <summary>
    /// An interface for describing Client/Worker basic operations. 
    /// </summary>
    public interface IFrontEnd
    {
        /// <summary>
        /// Returns a List containing all paths to servers that beloging to this view.
        /// </summary>
        /// <returns></returns>
        List<String> GetView();
        /// <summary>
        /// Performs a async read on all servers in the current view.
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns>The tuple that match the argument.</returns>
        Tuple Read(Tuple tuple);
        /// <summary>
        /// Performs a take on all servers in the current view.
        /// </summary>
        /// <param name="tuple">The tuple to be taken.</param>
        /// <returns>The tuple which was selected to be removed.</returns>
        Tuple Take(Tuple tuple);
        /// <summary>
        /// Performs a write on all servers in the current view.
        /// </summary>
        /// <param name="tuple">The tuple to be written.</param>
        void Write(Tuple tuple);

        void Freeze();

        void Unfreeze();

        void Crash();
    }
}
