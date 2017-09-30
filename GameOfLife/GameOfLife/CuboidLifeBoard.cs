using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GameOfLife
{
    /// <summary>
    /// An implementation of a cuboid life board.
    /// </summary>
    /// <seealso cref="GameOfLife.LifeBoard" />
    [DataContract(Name = nameof(CuboidLifeBoard))]
    [KnownType(typeof(CuboidLifeBoard))]
    public sealed class CuboidLifeBoard : LifeBoard
    {
        #region Private Fields

        /// <summary>
        /// This cache holds the positions for every livable position on the board
        /// </summary>
        private Position[,][] _neighborCache;

        #endregion Private Fields

        #region Private Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CuboidLifeBoard"/> class.
        /// </summary>
        /// <param name="width">The width of the cuboid.</param>
        /// <param name="height">The height of the cuboid.</param>
        /// <param name="depth">The depth of the cuboid.</param>
        /// <param name="lifeBoard">The life board on which the game is played.</param>
        private CuboidLifeBoard(int width, int height, int depth, LifeState[,] lifeBoard) :
            base(lifeBoard)
        {
            CuboidWidth = width;
            CuboidHeight = height;
            CuboidDepth = depth;

            _neighborCache = CreateNeighborCache();
        }

        #endregion Private Constructors

        #region Public Properties

        /// <summary>
        /// Gets the cuboid depth.
        /// </summary>
        /// <value>
        /// The cuboid depth.
        /// </value>
        [DataMember(Name = "CuboidDepth")]
        public int CuboidDepth { get; private set; }

        /// <summary>
        /// Gets the height of the cuboid.
        /// </summary>
        /// <value>
        /// The height of the cuboid.
        /// </value>
        [DataMember(Name = "CuboidHeight")]
        public int CuboidHeight { get; private set; }

        /// <summary>
        /// Gets the width of the cuboid.
        /// </summary>
        /// <value>
        /// The width of the cuboid.
        /// </value>
        [DataMember(Name = "CuboidWidth")]
        public int CuboidWidth { get; private set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Creates a new instance of the <see cref="CuboidLifeBoard"/> class.
        /// </summary>
        /// <param name="width">The width of the cuboid.</param>
        /// <param name="height">The height of the cuboid.</param>
        /// <param name="depth">The depth of the cuboid.</param>
        /// <param name="alivePositions">The positions where the habitants are alive at the beginning of the game.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// width - Cannot be lower 1!
        /// or
        /// height - Cannot be lower 1!
        /// or
        /// height - Cannot be lower 1!
        /// </exception>
        public static CuboidLifeBoard Create(int width, int height, int depth, IEnumerable<Position> alivePositions)
        {
            if (width < 1) {
                throw new ArgumentOutOfRangeException(nameof(width), "Cannot be lower 1!");
            }

            if (height < 1) {
                throw new ArgumentOutOfRangeException(nameof(height), "Cannot be lower 1!");
            }

            if (depth < 1) {
                throw new ArgumentOutOfRangeException(nameof(height), "Cannot be lower 1!");
            }

            int lifeBoardWidth = (2 * width) + (2 * depth);
            int lifeBoardHeight = (2 * depth) + height;

            LifeState[,] lifeBoard = new LifeState[lifeBoardWidth, lifeBoardHeight];
            for (int hIndex = 0; hIndex < lifeBoardWidth; ++hIndex) {
                for (int vIndex = 0; vIndex < lifeBoardHeight; ++vIndex) {
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

        /// <summary>
        /// Gets the life states of the neighbors.
        /// </summary>
        /// <param name="position">The position of the habitant.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">position - Life is for position impossible</exception>
        public override LifeState[] GetNeighbors(Position position)
        {
            if (!IsLifePossible(position.X, position.Y)) {
                throw new ArgumentOutOfRangeException(nameof(position), "Life is for position impossible");
            }

            return GetLifeStates(_neighborCache[position.X, position.Y]);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Determines whether life is possible at the specified position of the life board.
        /// </summary>
        /// <param name="x">The x-position of the life board.</param>
        /// <param name="y">The y-position of the life board.</param>
        /// <param name="width">The width of the cuboid.</param>
        /// <param name="height">The height of the cuboid.</param>
        /// <param name="depth">The depth of the cuboid.</param>
        /// <returns>
        ///   <c>true</c> if life is possible at the specified position of the life board; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsLifePossible(int x, int y, int width, int height, int depth)
        {
            return (depth <= x && x < (depth + width)) || (depth <= y && y < (depth + height));
        }

        /// <summary>
        /// Creates the edge mapping. This is used for getting the neighbors of a habitant.
        /// </summary>
        /// <returns></returns>
        private ILookup<Position, Position> CreateEdgeMapping()
        {
            /*
             * A normal cuboid has six sides according to the image. To get the neighbors of the edges we use a lookup table.
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
             * We have 14 different edges (1 to E) where the neighbors can't be calculated like with a normal rectangle.
             * For the calculation of the neighbors the life board is enlarged by 2 in every direction and the
             * cuboid is moved one down and 1 to the right. This results in calculating the neighbors with virtual
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

            for (int i = 0; i < CuboidDepth; ++i) {
                // 1
                Position firstEdgePosition = new Position(CuboidDepth, i + 1);
                edgeMappings.Add(Tuple.Create(firstEdgePosition, new Position(i + 1, CuboidDepth + 1)));
                // 2
                Position secondEdgePosition = new Position(CuboidDepth + CuboidWidth + 1, i + 1);
                edgeMappings.Add(Tuple.Create(secondEdgePosition, new Position((2 * CuboidDepth) + CuboidWidth - i, CuboidDepth + 1)));
                // 3
                Position thirdEdgePosition = new Position(CuboidDepth, CuboidDepth + CuboidHeight + i + 1);
                edgeMappings.Add(Tuple.Create(thirdEdgePosition, new Position(CuboidDepth - i, CuboidDepth + CuboidHeight)));
                // 4
                Position fourthEdgePosition = new Position(CuboidDepth + CuboidWidth + 1, CuboidDepth + CuboidHeight + i + 1);
                edgeMappings.Add(Tuple.Create(fourthEdgePosition, new Position(CuboidDepth + CuboidWidth + 1 + i, CuboidDepth + CuboidHeight)));
                // 5
                Position fifthEdgePosition = new Position(i + 1, CuboidDepth);
                edgeMappings.Add(Tuple.Create(fifthEdgePosition, new Position(CuboidDepth + 1, i + 1)));
                // 6
                Position sixthEdgePosition = new Position(CuboidDepth + CuboidWidth + i + 1, CuboidDepth);
                edgeMappings.Add(Tuple.Create(sixthEdgePosition, new Position(CuboidDepth + CuboidWidth, CuboidDepth - i)));
                // 7
                Position seventhEdgePosition = new Position(i + 1, CuboidDepth + CuboidHeight + 1);
                edgeMappings.Add(Tuple.Create(seventhEdgePosition, new Position(CuboidDepth + 1, (2 * CuboidDepth) + CuboidHeight - i)));
                // 8
                Position eigthEdgePosition = new Position(CuboidDepth + CuboidWidth + 1 + i, CuboidDepth + CuboidHeight + 1);
                edgeMappings.Add(Tuple.Create(eigthEdgePosition, new Position(CuboidDepth + CuboidWidth, CuboidDepth + CuboidHeight + 1 + i)));
            }

            for (int i = 0; i < CuboidWidth; ++i) {
                // 9
                Position ninthEdgePosition = new Position((2 * CuboidDepth) + CuboidWidth + i + 1, CuboidDepth);
                edgeMappings.Add(Tuple.Create(ninthEdgePosition, new Position(CuboidDepth + CuboidWidth - i, 1)));
                // A
                Position tenthEdgePosition = new Position((2 * CuboidDepth) + CuboidWidth + i + 1, CuboidDepth + CuboidHeight + 1);
                edgeMappings.Add(Tuple.Create(tenthEdgePosition, new Position(CuboidDepth + CuboidWidth - i, (2 * CuboidDepth) + CuboidHeight)));
                // B
                Position eleventhEdgePosition = new Position(CuboidDepth + i + 1, 0);
                edgeMappings.Add(Tuple.Create(eleventhEdgePosition, new Position((2 * CuboidDepth) + (2 * CuboidWidth) - i, CuboidDepth + 1)));
                // C
                Position twelthEdgePosition = new Position(CuboidDepth + i + 1, (2 * CuboidDepth) + CuboidHeight + 1);
                edgeMappings.Add(Tuple.Create(twelthEdgePosition, new Position((2 * CuboidDepth) + (2 * CuboidWidth) - i, CuboidDepth + CuboidHeight)));
            }

            for (int i = 0; i < CuboidHeight; ++i) {
                // D
                Position thirteenthEdgePosition = new Position(0, CuboidDepth + i + 1);
                edgeMappings.Add(Tuple.Create(thirteenthEdgePosition, new Position((2 * CuboidDepth) + (2 * CuboidWidth), CuboidDepth + i + 1)));
                // E
                Position fourteenthEdgePosition = new Position((2 * CuboidDepth) + (2 * CuboidWidth) + 1, CuboidDepth + i + 1);
                edgeMappings.Add(Tuple.Create(fourteenthEdgePosition, new Position(1, CuboidDepth + i + 1)));
            }

            return edgeMappings.ToLookup(g => g.Item1, g => g.Item2);
        }

        /// <summary>
        /// Creates the neighbor cache. The result contains the neighbors for every livable position.
        /// </summary>
        /// <returns>The neighbors for every livable position.</returns>
        private Position[,][] CreateNeighborCache()
        {
            var edgeMapping = CreateEdgeMapping();

            Position[,][] positions = new Position[Width, Height][]; // TODO: Don't waste space! new Position[2 * (_width * _depth + _width * _height + _height * _depth)][];

            for (int y = 0; y < Height; ++y) {
                for (int x = 0; x < Width; ++x) {
                    if (IsLifePossible(x, y)) {
                        // by adding a height of two and and moving the Y position one to the bottom, we can simulate an empty row above and below the cuboid field.
                        // by adding a width of two and and moving the X position one to the right, we can simulate an empty column to the left and right of the cuboid field.
                        Position virtualPosition = new Position(x + 1, y + 1);
                        IEnumerable<Position> virtualNeighborPositions = NeighborHelper.GetNeighborPositions(virtualPosition, Width + 2, Height + 2);

                        List<Position> realNeighborPositions = new List<Position>();
                        foreach (var virtualNeighborPosition in virtualNeighborPositions) {
                            if (edgeMapping.Contains(virtualNeighborPosition)) {
                                realNeighborPositions.AddRange(edgeMapping[virtualNeighborPosition].Where(p => p.X != virtualPosition.X || p.Y != virtualPosition.Y).Select(vp => new Position(vp.X - 1, vp.Y - 1)));
                            } else if (virtualNeighborPosition.X != 0 && virtualNeighborPosition.X != ((2 * CuboidDepth) + (2 * CuboidWidth)) + 1 && virtualNeighborPosition.Y != 0 && virtualNeighborPosition.Y != ((2 * CuboidDepth) + CuboidHeight) + 1) {
                                realNeighborPositions.Add(new Position(virtualNeighborPosition.X - 1, virtualNeighborPosition.Y - 1));
                            }
                        }

                        positions[x, y] = realNeighborPositions.Distinct().ToArray();
                    }
                }
            }

            return positions;
        }

        /// <summary>
        /// Determines whether life is possible at the specified position.
        /// </summary>
        /// <param name="x">The x-position on the life board.</param>
        /// <param name="y">The y-position on the life board.</param>
        /// <returns>
        ///   <c>true</c> if life possible is at the specified position; otherwise, <c>false</c>.
        /// </returns>
        private bool IsLifePossible(int x, int y)
        {
            return IsLifePossible(x, y, CuboidWidth, CuboidHeight, CuboidDepth);
        }

        /// <summary>
        /// Called when an instance of this class is deserialized.
        /// </summary>
        /// <param name="streamingContext">The streaming context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext streamingContext)
        {
            _neighborCache = CreateNeighborCache();
        }

        #endregion Private Methods
    }
}