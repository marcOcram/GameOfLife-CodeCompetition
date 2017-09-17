using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameOfLife
{
    [DebuggerDisplay("({X}, {Y})")]
    public struct Position
    {
        #region Public Constructors

        public Position(uint x, uint y)
        {
            Y = y;
            X = x;
        }

        #endregion Public Constructors

        #region Public Properties

        public uint X { get; }

        public uint Y { get; }

        #endregion Public Properties

        #region Public Methods

        public static bool operator !=(Position p1, Position p2)
        {
            return !p1.Equals(p2);
        }

        public static bool operator ==(Position p1, Position p2)
        {
            return p1.Equals(p2);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        #endregion Public Methods
    }
}