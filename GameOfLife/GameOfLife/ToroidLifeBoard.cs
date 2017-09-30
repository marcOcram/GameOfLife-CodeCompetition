using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    /// <summary>
    /// An implementation of a toroid life board.
    /// </summary>
    [DataContract(Name = nameof(ToroidLifeBoard))]
    [KnownType(typeof(ToroidLifeBoard))]
    public sealed class ToroidLifeBoard : LifeBoard
    {
        /// <summary>
        /// This cache holds the positions for every livable position on the board
        /// </summary>
        private Position[,][] _neighborCache;

        #region Private Fields

        #endregion Private Fields

        #region Private Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ToroidLifeBoard"/> class.
        /// </summary>
        /// <param name="width">The width of the toroid.</param>
        /// <param name="height">The height of the toroid.</param>
        /// <param name="alivePositions">The alive positions.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// width - Cannot be lower 1!
        /// or
        /// width - Cannot be lower 1!
        /// </exception>
        private ToroidLifeBoard(int width, int height, IEnumerable<Position> alivePositions) :
            base(new LifeState[width, height])
        {
            if (width < 1) {
                throw new ArgumentOutOfRangeException(nameof(width), "Cannot be lower 1!");
            }

            if (height < 1) {
                throw new ArgumentOutOfRangeException(nameof(width), "Cannot be lower 1!");
            }

            ApplyChanges(alivePositions, new List<Position>());

            _neighborCache = CreateNeighborCache();
        }

        #endregion Private Constructors

        #region Public Methods

        /// <summary>
        /// Creates a new instance of the <see cref="ToroidLifeBoard"/> class.
        /// </summary>
        /// <param name="width">The width of the toroid.</param>
        /// <param name="height">The height of the toroid.</param>
        /// <returns></returns>
        public static ToroidLifeBoard Create(int width, int height)
        {
            return Create(width, height, new List<Position>());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ToroidLifeBoard"/> class.
        /// </summary>
        /// <param name="width">The width of the toroid.</param>
        /// <param name="height">The height of the toroid.</param>
        /// <param name="alivePositions">The positions where the habitants are alive at the beginning of the game.</param>
        /// <returns></returns>
        public static ToroidLifeBoard Create(int width, int height, IEnumerable<Position> alivePositions)
        {
            return new ToroidLifeBoard(width, height, alivePositions);
        }

        /// <summary>
        /// Gets the life states of the neighbors.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public override LifeState[] GetNeighbors(Position position)
        {
            if (!(0 <= position.X && position.X < Width)) {
                throw new ArgumentOutOfRangeException(nameof(position), $"Position not inside {nameof(LifeBoard)}!");
            }

            if (!(0 <= position.Y && position.Y < Height)) {
                throw new ArgumentOutOfRangeException(nameof(position), $"Position not inside {nameof(LifeBoard)}!");
            }

            return GetLifeStates(_neighborCache[position.X, position.Y]);
        }

        #endregion Public Methods

        /// <summary>
        /// Creates the neighbor cache.
        /// </summary>
        /// <returns></returns>
        private Position[,][] CreateNeighborCache()
        {
            Position[,][] positions = new Position[Width, Height][];

            for (int y = 0; y < Height; ++y) {
                for (int x = 0; x < Width; ++x) {
                    Position currentPosition = new Position(x, y);
                    positions[x, y] = NeighborHelper.GetNeighborPositions(currentPosition, Width, Height);
                }
            }

            return positions;
        }

        /// <summary>
        /// Called when an instance has finished deserializing.
        /// </summary>
        /// <param name="streamingContext">The streaming context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext streamingContext)
        {
            _neighborCache = CreateNeighborCache();
        }
    }
}