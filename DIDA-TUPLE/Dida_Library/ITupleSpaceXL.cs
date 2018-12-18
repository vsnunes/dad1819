using System.Collections.Generic;

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
        bool write(int workerId, int requestId, Tuple tuple, View view);

        /// <summary>
        /// Reads and takes a tuple from the tuple space.
        /// </summary>
        /// <param name="workerId">The client/worker identification.</param>
        /// <param name="requestId">The request identification of this operation.</param>
        /// <param name="tuple">A tuple to take.</param>
        /// <returns>A list of taples matching the param.</returns>
        List<Tuple> take(int workerId, int requestId, Tuple tuple, View view);

        /// <summary>
        /// Gets the number of itens in the tupple space.
        /// </summary>
        /// <returns></returns>
        int ItemCount();

        /// <summary>
        /// Executes second phase of take
        /// </summary>
        /// <param name="tuple"></param>
        void remove(Tuple tuple, int workerId);

        /// <summary>
        /// Removes a URL (machine) from my view
        /// </summary>
        /// <param name="url">The URL of the machine to be removed.</param>
        /// <returns>The new view whose URL (machine) no longer belongs.</returns>
        View Remove(string url);

        /// <summary>
        /// Returns my current view
        /// </summary>
        /// <returns>View structure</returns>
        View GetActualView();

        /// <summary>
        /// Display the status of the server.
        /// It contains the alive node and the dead.
        /// </summary>
        void Status();
        
        void Freeze();

        void Unfreeze();

        void Crash();

        /// <summary>
        /// Forces the server to check liveliness of all members of the view and
        /// remove those who are not alive.
        /// </summary>
        void checkView();

    }
}
