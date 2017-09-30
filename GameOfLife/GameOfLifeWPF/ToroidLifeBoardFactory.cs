using GameOfLife;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLifeWPF
{
    /// <summary>
    /// Factory to create a <see cref="ToroidLifeBoard"/>.
    /// </summary>
    /// <seealso cref="GameOfLifeWPF.ILifeBoardFactory" />
    internal class ToroidLifeBoardFactory : ILifeBoardFactory
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the height of the toroid.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height { get; set; } = 10;

        /// <summary>
        /// Gets or sets the width of the toroid.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width { get; set; } = 10;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Creates a new instance of the <see cref="ToroidLifeBoard" /> class.
        /// </summary>
        /// <returns></returns>
        public LifeBoard Create()
        {
            return ToroidLifeBoard.Create(Width, Height);
        }

        #endregion Public Methods
    }
}