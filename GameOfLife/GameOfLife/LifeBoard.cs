using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public abstract class LifeBoard
    {
        #region Private Fields

        private readonly LifeState[,] _board;
        private readonly ConcurrentBag<Position> _habitablePositions = new ConcurrentBag<Position>();

        #endregion Private Fields

        #region Protected Constructors

        protected LifeBoard(LifeState[,] board)
        {
            _board = board;

            TotalWidth = (uint)_board.GetLength(0);

            TotalHeight = (uint)_board.GetLength(1);

            Parallel.ForEach(Range(0, TotalHeight), y => {
                for (uint x = 0; x < TotalWidth; ++x) {
                    if (_board[x, y] != LifeState.NoLifePossible) {
                        _habitablePositions.Add(new Position(x, y));
                    }
                }
            });
        }

        #endregion Protected Constructors

        #region Public Properties

        public uint TotalHeight { get; }
        public uint TotalWidth { get; }

        #endregion Public Properties

        #region Public Indexers

        public LifeState this[Position position] => GetLifeState(position);

        #endregion Public Indexers

        #region Public Methods

        public void ApplyChanges(IEnumerable<Position> alivePositions, IEnumerable<Position> deadPositions)
        {
            var t = alivePositions.ToDictionary(p => p, p => LifeState.Alive).Union(deadPositions.ToDictionary(p => p, p => LifeState.Dead));

            Parallel.ForEach(t, c => _board[c.Key.X, c.Key.Y] = c.Value);
        }

        ///// <summary>
        ///// Gets the habitable life states.
        ///// The position can be calculated via the index or by utilizing <see cref="ToPositionDictionary{T}(T[])"/>
        ///// <para>
        ///// uint x = (uint)index % <see cref="TotalWidth" />
        ///// </para>
        ///// <para>
        ///// uint y = (uint)index / <see cref="TotalHeight" />
        ///// </para>
        ///// </summary>
        ///// <returns></returns>
        //public LifeState[] GetHabitableLifeStates()
        //{
        //    LifeState[] lifeStates = new LifeState[_habitablePositions.Count];

        //    Parallel.ForEach(_habitablePositions, habitablePosition => {
        //        lifeStates[habitablePosition.Y * TotalWidth + habitablePosition.X] = _board[habitablePosition.X, habitablePosition.Y];
        //    });

        //    return lifeStates;
        //}

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
            if (!(0 <= position.X && position.X < TotalWidth)) throw new ArgumentOutOfRangeException(nameof(position), $"Position not inside {nameof(LifeBoard)}!");
            if (!(0 <= position.Y && position.Y < TotalHeight)) throw new ArgumentOutOfRangeException(nameof(position), $"Position not inside {nameof(LifeBoard)}!");

            return _board[position.X, position.Y];
        }

        /// <summary>
        /// Gets the life states including non habitable positions.
        /// </summary>
        /// <returns></returns>
        public LifeState[] GetLifeStates()
        {
            LifeState[] lifeStates = new LifeState[TotalWidth * TotalHeight];

            Parallel.ForEach(Range(0, TotalHeight), y => {
                for (uint x = 0; x < TotalWidth; ++x) {
                    lifeStates[y * TotalWidth + x] = _board[x, y];
                }
            });

            return lifeStates;
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

            Parallel.For(0, positions.Count, index => {
                lifeStates[index] = GetLifeState(positions[index]);
            });

            return lifeStates;
        }

        /// <summary>
        /// Gets the life states of the neighbors.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public abstract LifeState[] GetNeighbors(Position position);

        /// <summary>
        /// Converts a position to an index.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public uint ToIndex(Position position)
        {
            return position.Y * TotalWidth + position.X;
        }

        /// <summary>
        /// Converts an index to position.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public Position ToPosition(uint index)
        {
            return new Position(index % TotalWidth, index / TotalWidth);
        }

        /// <summary>
        /// Converts an enumeration to an <see cref="IReadOnlyDictionary{Position, T}"/> where the index is used for calculating the position.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public IReadOnlyDictionary<Position, T> ToPositionDictionary<T>(IEnumerable<T> values)
        {
            return values.Select((value, index) => new { index, value }).ToDictionary(i => ToPosition((uint)i.index), i => i.value);
        }

        #endregion Public Methods

        #region Protected Methods

        protected static IEnumerable<uint> Range(uint fromInclusive, uint toExclusive)
        {
            for (uint i = fromInclusive; i < toExclusive; ++i) {
                yield return i;
            }
        }

        #endregion Protected Methods
    }
}