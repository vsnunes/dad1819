using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIDA_LIBRARY
{
    /// <summary>
    /// An interface for describing basic Tuple Spaces operations.
    /// </summary>
    public interface ITupleSpace
    {
        /// <summary>
        /// Reads a tuple from the tuple space.
        /// </summary>
        /// <param name="tuple">A tuple to match the read.</param>
        /// <returns>A taple matching the param.</returns>
        Tuple read(Tuple tuple);

        /// <summary>
        /// Writes a new tuple to the tuple space.
        /// </summary>
        /// <param name="tuple">A tupple to be written.</param>
        void write(Tuple tuple);

        /// <summary>
        /// Reads and takes a tuple from the tuple space.
        /// </summary>
        /// <param name="tuple">A tuple to take.</param>
        /// <returns>A taple matching the param.</returns>
        Tuple take(Tuple tuple);
    }
}
