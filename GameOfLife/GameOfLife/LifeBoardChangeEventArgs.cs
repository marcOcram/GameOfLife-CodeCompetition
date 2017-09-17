using System;
using System.Collections.Generic;
using System.Text;

namespace GameOfLife
{
    public class LifeBoardChangeEventArgs : EventArgs
    {
        #region Public Constructors

        public LifeBoardChangeEventArgs(IEnumerable<Position> changes)
        {
            Changes = changes ?? throw new ArgumentNullException(nameof(changes));
        }

        #endregion Public Constructors

        #region Public Properties

        public IEnumerable<Position> Changes { get; }

        #endregion Public Properties
    }
}