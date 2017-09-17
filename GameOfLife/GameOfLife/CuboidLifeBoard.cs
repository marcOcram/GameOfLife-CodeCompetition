using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class CuboidLifeBoard : LifeBoard
    {
        #region Private Fields

        private readonly uint _cuboidDepth;
        private readonly uint _cuboidHeight;
        private readonly Position[,][] _neighborCache;
        private readonly uint _cuboidWidth;

        #endregion Private Fields

        #region Private Constructors

        private CuboidLifeBoard(uint width, uint height, uint depth, LifeState[,] lifeBoard) :
            base(lifeBoard)
        {
            _cuboidWidth = width;
            _cuboidHeight = height;
            _cuboidDepth = depth;

            _neighborCache = CreateNeighborCache();
        }

        #endregion Private Constructors

        #region Public Methods

        public static CuboidLifeBoard Create(uint width, uint height, uint depth, IEnumerable<Position> alivePositions)
        {
            if (width < 1) throw new ArgumentOutOfRangeException(nameof(width), "Cannot be lower 1!");
            if (height < 1) throw new ArgumentOutOfRangeException(nameof(height), "Cannot be lower 1!");
            if (depth < 1) throw new ArgumentOutOfRangeException(nameof(height), "Cannot be lower 1!");

            uint lifeBoardWidth = 2 * width + 2 * depth;
            uint lifeBoardHeight = 2 * depth + height;

            LifeState[,] lifeBoard = new LifeState[lifeBoardWidth, lifeBoardHeight];
            for (uint hIndex = 0; hIndex < lifeBoardWidth; ++hIndex) {
                for (uint vIndex = 0; vIndex < lifeBoardHeight; ++vIndex) {
                    if (!IsLifePossible(hIndex, vIndex, width, height, depth)) {
                        lifeBoard[hIndex, vIndex] = LifeState.NoLifePossible;
                    }
                }
            }

            foreach (var alivePosition in alivePositions) {
                if (lifeBoard[alivePosition.X, alivePosition.Y] != LifeState.NoLifePossible) {
                    lifeBoard[alivePosition.X, alivePosition.Y] = LifeState.Alive;
                }
            }

            return new CuboidLifeBoard(width, height, depth, lifeBoard);
        }

        public override LifeState[] GetNeighbors(Position position)
        {
            if (!IsLifePossible(position.X, position.Y, _cuboidWidth, _cuboidHeight, _cuboidDepth)) throw new ArgumentOutOfRangeException(nameof(position), "Life is for position impossible");

            return GetLifeStates(_neighborCache[position.X, position.Y]);
        }

        #endregion Public Methods

        #region Private Methods

        private static bool IsLifePossible(uint x, uint y, uint width, uint height, uint depth)
        {
            return (depth <= x && x < (depth + width)) || (depth <= y && y < (depth + height));
        }

        private ILookup<Position, Position> CreateEdgeMapping()
        {
            /*
             * A normal dice has six sides according to the image. To get the neighbors of the edges we use a lookup table.
             *
             * Le => Left Side
             * Fr => Front Side
             * Ri => Right Side
             * Ba => Back Side
             * To => Top Side
             * Bo => Bottom Side
             *
             *          ┌─────┐
             *          │     │
             *          │  To │
             *          │     │
             * ┌────────┼─────┼────────┬─────┐
             * │   Le   │     │   Ri   │     │
             * │        │ Fr  │        │ Ba  │
             * └────────┼─────┼────────┴─────┘
             *          │     │
             *          │ Bo  │
             *          │     │
             *          └─────┘
             *
             * We have 14 different edges where the neighbors can't be calculated like with a normal rectangle.
             * For the calculation of the neighbors the lifeboard is enlarged by 2 in every direction and the
             * dice is moved one down and 1 to the right. This results in calculating the neighbors with virtual
             * coordinates instead of the real coordinates.
             *
             *
             *        0 1 2 3 4 5 6 7 8 9 A B C D E F    <-- real x-coordinates
             *      0 1 2 3 4 5 6 7 8 9 A B C D E F G H  <-- virtual x-coordinates
             *
             *   0              B B B
             * 0 1            1 █ █ █ 2
             * 1 2            1 █ █ █ 2
             * 2 3            1 █ █ █ 2
             * 3 4            1 █ █ █ 2
             * 4 5    5 5 5 5 \ █ █ █ / 6 6 6 6 9 9 9
             * 5 6  D █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ E
             * 6 7  D █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ E
             * 7 8  D █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ E
             * 8 9  D █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ █ E
             * 9 A    7 7 7 7 / █ █ █ \ 8 8 8 8 A A A
             * A B            3 █ █ █ 4
             * B C            3 █ █ █ 4
             * C D            3 █ █ █ 4
             * D E            3 █ █ █ 4
             * | F              C C C
             * | |
             * | └─ virtual y-coordinates
             * └─── real y-coordinates
             */

            ConcurrentBag<Tuple<Position, Position>> edgeMappings = new ConcurrentBag<Tuple<Position, Position>>();
            
            for (uint i = 0; i < _cuboidDepth; ++i) { 
                // 1
                Position firstEdgePosition = new Position(_cuboidDepth, i + 1);
                edgeMappings.Add(Tuple.Create(firstEdgePosition, new Position(i + 1, _cuboidDepth + 1)));
                // 2
                Position secondEdgePosition = new Position(_cuboidDepth + _cuboidWidth + 1, i + 1);
                edgeMappings.Add(Tuple.Create(secondEdgePosition, new Position(2 * _cuboidDepth + _cuboidWidth - i, _cuboidDepth + 1)));
                // 3
                Position thirdEdgePosition = new Position(_cuboidDepth, _cuboidDepth + _cuboidHeight + i + 1);
                edgeMappings.Add(Tuple.Create(thirdEdgePosition, new Position(_cuboidDepth - i, _cuboidDepth + _cuboidHeight)));
                // 4
                Position fourthEdgePosition = new Position(_cuboidDepth + _cuboidWidth + 1, _cuboidDepth + _cuboidHeight + i + 1);
                edgeMappings.Add(Tuple.Create(fourthEdgePosition, new Position(_cuboidDepth + _cuboidWidth + 1 + i, _cuboidDepth + _cuboidHeight)));
                // 5
                Position fifthEdgePosition = new Position(i + 1, _cuboidDepth);
                edgeMappings.Add(Tuple.Create(fifthEdgePosition, new Position(_cuboidDepth + 1, i + 1)));
                // 6
                Position sixthEdgePosition = new Position(_cuboidDepth + _cuboidWidth + i + 1, _cuboidDepth);
                edgeMappings.Add(Tuple.Create(sixthEdgePosition, new Position(_cuboidDepth + _cuboidWidth, _cuboidDepth - i)));
                // 7
                Position seventhEdgePosition = new Position(i + 1, _cuboidDepth + _cuboidHeight + 1);
                edgeMappings.Add(Tuple.Create(seventhEdgePosition, new Position(_cuboidDepth + 1, 2 * _cuboidDepth + _cuboidHeight - i)));
                // 8
                Position eigthEdgePosition = new Position(_cuboidDepth + _cuboidWidth + 1 + i, _cuboidDepth + _cuboidHeight + 1);
                edgeMappings.Add(Tuple.Create(eigthEdgePosition, new Position(_cuboidDepth + _cuboidWidth, _cuboidDepth + _cuboidHeight + 1 + i)));
            }

            for (uint i = 0; i < _cuboidWidth; ++i) {
                // 9
                Position ninthEdgePosition = new Position(2 * _cuboidDepth + _cuboidWidth + i + 1, _cuboidDepth);
                edgeMappings.Add(Tuple.Create(ninthEdgePosition, new Position(_cuboidDepth + _cuboidWidth - i, 1)));
                // A
                Position tenthEdgePosition = new Position(2 * _cuboidDepth + _cuboidWidth + i + 1, _cuboidDepth + _cuboidHeight + 1);
                edgeMappings.Add(Tuple.Create(tenthEdgePosition, new Position(_cuboidDepth + _cuboidWidth - i, 2 * _cuboidDepth + _cuboidHeight)));
                // B
                Position eleventhEdgePosition = new Position(_cuboidDepth + i + 1, 0);
                edgeMappings.Add(Tuple.Create(eleventhEdgePosition, new Position(2 * _cuboidDepth + 2 * _cuboidWidth - i, _cuboidDepth + 1)));
                // C
                Position twelthEdgePosition = new Position(_cuboidDepth + i + 1, 2 * _cuboidDepth + _cuboidHeight + 1);
                edgeMappings.Add(Tuple.Create(twelthEdgePosition, new Position(2 * _cuboidDepth + 2 * _cuboidWidth - i, _cuboidDepth + _cuboidHeight)));
            }

            for (uint i = 0; i < _cuboidHeight; ++i) {
                // D
                Position thirteenthEdgePosition = new Position(0, _cuboidDepth + i + 1);
                edgeMappings.Add(Tuple.Create(thirteenthEdgePosition, new Position(2 * _cuboidDepth + 2 * _cuboidWidth, _cuboidDepth + i + 1)));
                // E
                Position fourteenthEdgePosition = new Position(2 * _cuboidDepth + 2 * _cuboidWidth + 1, _cuboidDepth + i + 1);
                edgeMappings.Add(Tuple.Create(fourteenthEdgePosition, new Position(1, _cuboidDepth + i + 1)));
            }

            return edgeMappings.ToLookup(g => g.Item1, g => g.Item2);
        }

        private Position[,][] CreateNeighborCache()
        {
            var edgeMapping = CreateEdgeMapping();

            Position[,][] positions = new Position[Width, Height][]; // TODO: Don't waste space! new Position[2 * (_width * _depth + _width * _height + _height * _depth)][];

            for (uint y = 0; y < Height; ++y) {
                for (uint x = 0; x < Width; ++x) {
                    if (IsLifePossible(x, y)) {
                        // by adding a height of two and and moving the Y position one to the bottom, we can simulate an empty row above and below the cuboid field.
                        // by adding a width of two and and moving the X position one to the right, we can simulate an empty column to the left and right of the cuboid field.
                        Position virtualPosition = new Position(x + 1, y + 1);
                        IEnumerable<Position> virtualNeighborPositions = RectangleNeighborHelper.GetNeighborPositions(virtualPosition, Width + 2, Height + 2);

                        List<Position> realNeighborPositions = new List<Position>();
                        foreach (var virtualNeighborPosition in virtualNeighborPositions) {
                            //if (virtualNeighborPosition != virtualPosition) {
                                if (edgeMapping.Contains(virtualNeighborPosition)) {
                                    realNeighborPositions.AddRange(edgeMapping[virtualNeighborPosition].Where(p => p != virtualPosition).Select(vp => new Position(vp.X - 1, vp.Y - 1)));
                                } else if (virtualNeighborPosition.X != 0 && virtualNeighborPosition.X != (2 * _cuboidDepth + 2 * _cuboidWidth) + 1 && virtualNeighborPosition.Y != 0 && virtualNeighborPosition.Y != (2 * _cuboidDepth + _cuboidHeight) + 1) {
                                    realNeighborPositions.Add(new Position(virtualNeighborPosition.X - 1, virtualNeighborPosition.Y - 1));
                                }
                            //}
                        }

                        positions[x, y] = realNeighborPositions.Distinct().ToArray();
                    }
                }
            }

            return positions;
        }

        private bool IsLifePossible(uint x, uint y)
        {
            return IsLifePossible(x, y, _cuboidWidth, _cuboidHeight, _cuboidDepth);
        }

        #endregion Private Methods
    }
}