using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    /// <summary>
    /// Helper class to get the neighbors in any direction: N, NE, E, SE, S, SW, W, NW
    /// </summary>
    internal static class NeighborHelper
    {
        #region Public Methods

        /// <summary>
        /// Gets the neighbor positions of an habitant.
        /// </summary>
        /// <param name="position">The position of the habitant.</param>
        /// <param name="maxWidth">The maximum width to determine if the neighbor is on the other side.</param>
        /// <param name="maxHeight">The maximum height to determine if the neighbor is on the other side.</param>
        /// <returns></returns>
        public static Position[] GetNeighborPositions(Position position, int maxWidth, int maxHeight)
        {
            Position westNeighborPosition = GetWestNeighborPosition(position, maxWidth);
            Position northNeighborPosition = GetNorthNeighborPosition(position, maxHeight);
            Position eastNeighborPosition = GetEastNeighborPosition(position, maxWidth);
            Position southNeighborPosition = GetSouthNeighborPosition(position, maxHeight);
            Position northWestNeighborPosition = new Position(westNeighborPosition.X, northNeighborPosition.Y);
            Position northEastNeighborPosition = new Position(eastNeighborPosition.X, northNeighborPosition.Y);
            Position southEastNeighborPosition = new Position(eastNeighborPosition.X, southNeighborPosition.Y);
            Position southWestNeighborPosition = new Position(westNeighborPosition.X, southNeighborPosition.Y);

            return new Position[] {
                westNeighborPosition, northWestNeighborPosition, northNeighborPosition, northEastNeighborPosition,
                eastNeighborPosition, southEastNeighborPosition, southNeighborPosition, southWestNeighborPosition
            };
        }

        #endregion Public Methods

        #region Private Methods

        private static Position GetEastNeighborPosition(Position position, int maxWidth)
        {
            int horizontalIndex = position.X + 1 < maxWidth ? position.X + 1 : 0;
            int verticalIndex = position.Y;

            return new Position(horizontalIndex, verticalIndex);
        }

        private static Position GetNorthNeighborPosition(Position position, int maxHeight)
        {
            int horizontalIndex = position.X;
            int verticalIndex = position.Y == 0 ? maxHeight - 1 : position.Y - 1;

            return new Position(horizontalIndex, verticalIndex);
        }

        private static Position GetSouthNeighborPosition(Position position, int maxHeight)
        {
            int horizontalIndex = position.X;
            int verticalIndex = position.Y + 1 < maxHeight ? position.Y + 1 : 0;

            return new Position(horizontalIndex, verticalIndex);
        }

        private static Position GetWestNeighborPosition(Position position, int maxWidth)
        {
            int horizontalIndex = position.X == 0 ? maxWidth - 1 : position.X - 1;
            int verticalIndex = position.Y;

            return new Position(horizontalIndex, verticalIndex);
        }

        #endregion Private Methods
    }
}