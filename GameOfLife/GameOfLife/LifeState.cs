using System;
using System.Collections.Generic;
using System.Text;

namespace GameOfLife
{
    /// <summary>
    /// The possible states an habitant can have on a life board.
    /// </summary>
    public enum LifeState
    {
        /// <summary>
        /// No life possible
        /// </summary>
        NoLifePossible = -1,

        /// <summary>
        /// The habitant is dead
        /// </summary>
        Dead = 0,

        /// <summary>
        /// The habitant is alive
        /// </summary>
        Alive = 1
    }
}