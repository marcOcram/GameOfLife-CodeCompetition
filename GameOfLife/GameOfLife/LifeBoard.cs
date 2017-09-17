using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GameOfLife
{
    public abstract class LifeBoard : IReadOnlyLifeBoard
    {
        #region Private Fields

        private readonly LifeState[,] _board;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        #endregion Private Fields

        #region Protected Constructors

        protected LifeBoard(LifeState[,] board)
        {
            _board = board;

            Width = (uint)_board.GetLength(0);
            Height = (uint)_board.GetLength(1);
        }

        #endregion Protected Constructors

        #region Public Properties

        public uint Height { get; }
        public uint Width { get; }

        #endregion Public Properties

        #region Public Indexers

        public LifeState this[Position position] => this[position.X, position.Y];

        public LifeState this[uint x, uint y] => GetLifeState(x, y);

        #endregion Public Indexers

        #region Public Methods

        public void ApplyChanges(IEnumerable<Position> alivePositions, IEnumerable<Position> deadPositions)
        {
            _lock.EnterWriteLock();
            try {
                foreach (Position position in alivePositions) {
                    _board[position.X, position.Y] = LifeState.Alive;
                }

                foreach (Position position in deadPositions) {
                    _board[position.X, position.Y] = LifeState.Dead;
                }
            } finally {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the state of life for a position.
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

        public LifeState GetLifeState(uint x, uint y)
        {
            if (!(0 <= x && x < Width)) throw new ArgumentOutOfRangeException(nameof(x), $"Not inside {nameof(LifeBoard)}!");
            if (!(0 <= y && y < Height)) throw new ArgumentOutOfRangeException(nameof(y), $"Not inside {nameof(LifeBoard)}!");

            _lock.EnterReadLock();
            try {
                return _board[x, y];
            } finally {
                _lock.ExitReadLock();
            }
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

            _lock.EnterReadLock();
            try {
                for (int index = 0; index < positions.Count; ++index) {
                    lifeStates[index] = GetLifeState(positions[index]);
                }
            } finally {
                _lock.ExitReadLock();
            }

            return lifeStates;
        }

        /// <summary>
        /// Gets the life states of the neighbors.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public abstract LifeState[] GetNeighbors(Position position);

        public void SetAlive(Position position)
        {
            SetAlive(position.X, position.Y);
        }

        public void SetAlive(uint x, uint y)
        {
            SetLifeState(x, y, LifeState.Alive);
        }

        public void SetDead(Position position)
        {
            SetDead(position.X, position.Y);
        }

        public void SetDead(uint x, uint y)
        {
            SetLifeState(x, y, LifeState.Dead);
        }

        #endregion Public Methods

        #region Private Methods

        private void SetLifeState(uint x, uint y, LifeState lifeState)
        {
            _lock.EnterWriteLock();
            try {
                _board[x, y] = lifeState;
            } finally {
                _lock.ExitWriteLock();
            }
        }

        #endregion Private Methods
    }
}