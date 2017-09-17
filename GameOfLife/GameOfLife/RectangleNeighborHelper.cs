using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    internal static class RectangleNeighborHelper
    {
        #region Private Methods

        private static Position GetEastNeighborPosition(Position position, uint maxWidth)
        {
            uint horizontalIndex = position.X + 1 < maxWidth ? position.X + 1 : 0;
            uint verticalIndex = position.Y;

            return new Position(horizontalIndex, verticalIndex);
        }

        public static IEnumerable<Position> GetNeighborPositions(Position position, uint maxWidth, uint maxHeight)
        {
            Position westNeighborPosition = GetWestNeighborPosition(position, maxWidth);
            Position northNeighborPosition = GetNorthNeighborPosition(position, maxHeight);
            Position eastNeighborPosition = GetEastNeighborPosition(position, maxWidth);
            Position southNeighborPosition = GetSouthNeighborPosition(position, maxHeight);
            Position northWestNeighborPosition = new Position(westNeighborPosition.X, northNeighborPosition.Y);
            Position northEastNeighborPosition = new Position(eastNeighborPosition.X, northNeighborPosition.Y);
            Position southEastNeighborPosition = new Position(eastNeighborPosition.X, southNeighborPosition.Y);
            Position southWestNeighborPosition = new Position(westNeighborPosition.X, southNeighborPosition.Y);

            return new List<Position>()
            {
                westNeighborPosition, northWestNeighborPosition, northNeighborPosition, northEastNeighborPosition,
                eastNeighborPosition, southEastNeighborPosition, southNeighborPosition, southWestNeighborPosition
            };
        }

        private static Position GetNorthNeighborPosition(Position position, uint maxHeight)
        {
            uint horizontalIndex = position.X;
            uint verticalIndex = position.Y == 0 ? maxHeight - 1 : position.Y - 1;

            return new Position(horizontalIndex, verticalIndex);
        }

        private static Position GetSouthNeighborPosition(Position position, uint maxHeight)
        {
            uint horizontalIndex = position.X;
            uint verticalIndex = position.Y + 1 < maxHeight ? position.Y + 1 : 0;

            return new Position(horizontalIndex, verticalIndex);
        }

        private static Position GetWestNeighborPosition(Position position, uint maxWidth)
        {
            uint horizontalIndex = position.X == 0 ? maxWidth - 1 : position.X - 1;
            uint verticalIndex = position.Y;

            return new Position(horizontalIndex, verticalIndex);
        }

        #endregion Private Methods
    }
}