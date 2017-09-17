using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace GameOfLifeWPF.Controls
{
    internal class ColorInfo
    {
        #region Public Constructors

        public ColorInfo(string name, Color color)
        {
            Color = color;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        #endregion Public Constructors

        #region Public Properties

        public Color Color { get; }

        public string Name { get; }

        #endregion Public Properties
    }
}