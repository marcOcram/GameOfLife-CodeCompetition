using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    public class RectangularLifeBoard : LifeBoard
    {
        #region Private Fields

        private readonly uint _height;

        private readonly uint _width;

        #endregion Private Fields

        #region Private Constructors

        private RectangularLifeBoard(uint width, uint height, IEnumerable<Position> alivePositions) :
            base(new LifeState[width, height])
        {
            _width = width;
            _height = height;

            ApplyChanges(alivePositions, new List<Position>());
        }

        #endregion Private Constructors

        #region Public Methods

        public static RectangularLifeBoard Create(uint width, uint height)
        {
            return Create(width, height, new List<Position>());
        }

        public static RectangularLifeBoard Create(uint width, uint height, IEnumerable<Position> alivePositions)
        {
            return new RectangularLifeBoard(width, height, alivePositions);
        }

        public override IReadOnlyDictionary<Position, LifeState> GetNeighbors(Position position)
        {
            if (!(0 <= position.X && position.X < _width)) throw new ArgumentOutOfRangeException(nameof(position), $"Position not inside {nameof(LifeBoard)}!");
            if (!(0 <= position.Y && position.Y < _height)) throw new ArgumentOutOfRangeException(nameof(position), $"Position not inside {nameof(LifeBoard)}!");

            IEnumerable<Position> neighborPositions = RectangleNeighborHelper.GetNeighborPositions(position, TotalWidth, TotalHeight);

            return GetLifeStates(neighborPositions);
        }

        #endregion Public Methods
    }
}