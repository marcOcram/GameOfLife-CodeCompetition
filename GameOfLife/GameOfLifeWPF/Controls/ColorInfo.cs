using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace GameOfLifeWPF.Controls
{
    /// <summary>
    /// Contains the name and color of a color.
    /// </summary>
    internal class ColorInfo
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the color.</param>
        /// <param name="color">The color.</param>
        /// <exception cref="ArgumentNullException">name</exception>
        public ColorInfo(string name, Color color)
        {
            Color = color;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public Color Color { get; }

        /// <summary>
        /// Gets the name of the color.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        #endregion Public Properties
    }
}