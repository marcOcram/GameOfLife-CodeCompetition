using GameOfLife;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeWPF
{
    /// <summary>
    /// An interface to create any kind of <see cref="LifeBoard"/>.
    /// </summary>
    internal interface ILifeBoardFactory
    {
        /// <summary>
        /// Creates a new <see cref="LifeBoard"/>.
        /// </summary>
        /// <returns></returns>
        LifeBoard Create();
    }
}
