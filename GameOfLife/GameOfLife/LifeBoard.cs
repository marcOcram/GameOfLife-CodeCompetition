using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace GameOfLife
{
    /// <summary>
    /// The basic implementation of a life board. The game is played on any implementation of the <see cref="LifeBoard"/> class.
    /// </summary>
    /// <seealso cref="GameOfLife.IReadOnlyLifeBoard" />
    [DataContract(Name = nameof(LifeBoard))]
    [KnownType(typeof(CuboidLifeBoard))]
    [KnownType(typeof(ToroidLifeBoard))]
    public abstract class LifeBoard : IReadOnlyLifeBoard
    {
        #region Private Fields

        /// <summary>
        /// The height of the life board.
        /// </summary>
        [DataMember(Name = "Height", IsRequired = true)]
        private readonly int _height;

        /// <summary>
        /// The width of the life board.
        /// </summary>
        [DataMember(Name = "Width", IsRequired = true)]
        private readonly int _width;

        /// <summary>
        /// The life board [width, height].
        /// </summary>
        private LifeState[,] _board;

        /// <summary>
        /// An temporary variable for de-/serializing the board because an [,] array cannot be de-/serialized.
        /// </summary>
        [DataMember(Name = "States", IsRequired = true)]
        private LifeState[][] _serializingBoard;

        #endregion Private Fields

        #region Protected Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LifeBoard"/> class.
        /// </summary>
        /// <param name="board">The board.</param>
        protected LifeBoard(LifeState[,] board)
        {
            _board = board;

            _width = _board.GetLength(0);
            _height = _board.GetLength(1);
        }

        #endregion Protected Constructors

        #region Public Properties

        /// <summary>
        /// Gets the height of the life board.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height => _height;

        /// <summary>
        /// Gets the width of the life board.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width => _width;

        #endregion Public Properties

        #region Public Indexers

        /// <summary>
        /// Gets the <see cref="LifeState"/> of the specified position.
        /// </summary>
        /// <value>
        /// The <see cref="LifeState"/>.
        /// </value>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public LifeState this[Position position] => this[position.X, position.Y];

        /// <summary>
        /// Gets the <see cref="LifeState"/> of the specified position.
        /// </summary>
        /// <value>
        /// The <see cref="LifeState"/>.
        /// </value>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public LifeState this[int x, int y] => GetLifeState(x, y);

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// Applies the changes.
        /// </summary>
        /// <param name="alivePositions">The new alive positions.</param>
        /// <param name="deadPositions">The new dead positions.</param>
        public void ApplyChanges(IEnumerable<Position> alivePositions, IEnumerable<Position> deadPositions)
        {
            foreach (Position position in alivePositions) {
                _board[position.X, position.Y] = LifeState.Alive;
            }

            foreach (Position position in deadPositions) {
                _board[position.X, position.Y] = LifeState.Dead;
            }
        }

        /// <summary>
        /// Gets the <see cref="LifeState"/> of the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// position not inside LifeBoard
        /// </exception>
        public LifeState GetLifeState(Position position)
        {
            return GetLifeState(position.X, position.Y);
        }

        /// <summary>
        /// Gets the <see cref="LifeState" /> of the specified position.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// x - LifeBoard
        /// or
        /// y - LifeBoard
        /// </exception>
        public LifeState GetLifeState(int x, int y)
        {
            if (!(0 <= x && x < Width)) {
                throw new ArgumentOutOfRangeException(nameof(x), $"Not inside {nameof(LifeBoard)}!");
            }

            if (!(0 <= y && y < Height)) {
                throw new ArgumentOutOfRangeException(nameof(y), $"Not inside {nameof(LifeBoard)}!");
            }

            return _board[x, y];
        }

        /// <summary>
        /// Gets the life states for the given positions.
        /// </summary>
        /// <param name="positions">The positions.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// position not inside LifeBoard
        /// </exception>
        public LifeState[] GetLifeStates(IReadOnlyList<Position> positions)
        {
            LifeState[] lifeStates = new LifeState[positions.Count];

            for (int index = 0; index < positions.Count; ++index) {
                lifeStates[index] = GetLifeState(positions[index]);
            }

            return lifeStates;
        }

        /// <summary>
        /// Gets the life states of the neighbors.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public abstract LifeState[] GetNeighbors(Position position);

        /// <summary>
        /// Sets the habitant at the position alive.
        /// </summary>
        /// <param name="position">The position of the habitant.</param>
        public void SetAlive(Position position)
        {
            SetAlive(position.X, position.Y);
        }

        /// <summary>
        /// Sets the habitant at the position alive.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public void SetAlive(int x, int y)
        {
            SetLifeState(x, y, LifeState.Alive);
        }

        /// <summary>
        /// Sets the habitant at the position dead.
        /// </summary>
        /// <param name="position">The position of the habitant.</param>
        public void SetDead(Position position)
        {
            SetDead(position.X, position.Y);
        }

        /// <summary>
        /// Sets the habitant at the position dead.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public void SetDead(int x, int y)
        {
            SetLifeState(x, y, LifeState.Dead);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Called when an instance has finished deserializing.
        /// </summary>
        /// <param name="streamingContext">The streaming context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext streamingContext)
        {
            _board = new LifeState[Width, Height];

            for (int x = 0; x < Width; ++x) {
                for (int y = 0; y < Height; ++y) {
                    _board[x, y] = _serializingBoard[x][y];
                }
            }

            _serializingBoard = null;
        }

        /// <summary>
        /// Called when an instance has finished serializing.
        /// </summary>
        /// <param name="streamingContext">The streaming context.</param>
        [OnSerialized]
        private void OnSerialized(StreamingContext streamingContext)
        {
            _serializingBoard = null;
        }

        /// <summary>
        /// Called when an instance starts serializing.
        /// </summary>
        /// <param name="streamingContext">The streaming context.</param>
        [OnSerializing]
        private void OnSerializing(StreamingContext streamingContext)
        {
            _serializingBoard = new LifeState[Width][];

            for (int x = 0; x < Width; ++x) {
                _serializingBoard[x] = new LifeState[Height];

                for (int y = 0; y < Height; ++y) {
                    _serializingBoard[x][y] = _board[x, y];
                }
            }
        }

        /// <summary>
        /// Sets the life state at the specified position.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="lifeState">State of the life.</param>
        private void SetLifeState(int x, int y, LifeState lifeState)
        {
            _board[x, y] = lifeState;
        }

        #endregion Private Methods
    }
}