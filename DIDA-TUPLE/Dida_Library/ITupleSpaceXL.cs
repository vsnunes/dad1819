﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIDA_LIBRARY
{
    /// <summary>
    /// An interface for describing basic Tuple Spaces Xu and Liskov operations.
    /// </summary>
    public interface ITupleSpaceXL
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
        /// <param name="workerId">The client/worker identification.</param>
        /// <param name="requestId">The request identification of this operation.</param>
        /// <param name="tuple">A tupple to be written.</param>
        void write(int workerId, int requestId, Tuple tuple);

        /// <summary>
        /// Reads and takes a tuple from the tuple space.
        /// </summary>
        /// <param name="workerId">The client/worker identification.</param>
        /// <param name="requestId">The request identification of this operation.</param>
        /// <param name="tuple">A tuple to take.</param>
        /// <returns>A taple matching the param.</returns>
        Tuple take(int workerId, int requestId, Tuple tuple);

        /// <summary>
        /// Gets the number of itens in the tupple space.
        /// </summary>
        /// <returns></returns>
        int ItemCount();
    }
}