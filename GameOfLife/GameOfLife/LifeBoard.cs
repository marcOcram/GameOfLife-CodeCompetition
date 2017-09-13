using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameOfLife
{
    public abstract class LifeBoard
    {
        #region Protected Fields
        
        protected LifeState[,] _board;
        private readonly ManualResetEventSlim _updating = new ManualResetEventSlim(true);

        #endregion Protected Fields

        #region Protected Constructors

        protected LifeBoard(LifeState[,] board)
        {
            _board = board;

            TotalWidth = (uint)_board.GetLength(0);
            TotalHeight = (uint)_board.GetLength(1);
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

        public virtual void ApplyChanges(IEnumerable<Position> alivePositions, IEnumerable<Position> deadPositions)
        {
            var t = alivePositions.ToDictionary(p => p, p => LifeState.Alive).Union(deadPositions.ToDictionary(p => p, p => LifeState.Dead));
            
            _updating.Wait();
            _updating.Reset();

            Parallel.ForEach(t, c => _board[c.Key.X, c.Key.Y] = c.Value);

            _updating.Set();
        }

        public virtual LifeState GetLifeState(Position position)
        {
            if (!(0 <= position.X && position.X < TotalWidth)) throw new ArgumentOutOfRangeException(nameof(position), $"Position not inside {nameof(LifeBoard)}!");
            if (!(0 <= position.Y && position.Y < TotalHeight)) throw new ArgumentOutOfRangeException(nameof(position), $"Position not inside {nameof(LifeBoard)}!");

            _updating?.Wait();
            
            return _board[position.X, position.Y];
        }

        public virtual IReadOnlyDictionary<Position, LifeState> GetLifeStates()
        {
            _updating?.Wait();

            Dictionary<Position, LifeState> lifeStates = new Dictionary<Position, LifeState>();
            
            for (uint vIndex = 0; vIndex < TotalHeight; ++vIndex) {
                for (uint hIndex = 0; hIndex < TotalWidth; ++hIndex) {
                    lifeStates[new Position(hIndex, vIndex)] = _board[hIndex, vIndex];
                }
            }

            return lifeStates;
        }

        public abstract IReadOnlyDictionary<Position, LifeState> GetNeighbors(Position position);

        #endregion Public Methods

        #region Protected Methods

        protected IReadOnlyDictionary<Position, LifeState> GetLifeStates(IEnumerable<Position> positions)
        {
            _updating?.Wait();

            Dictionary<Position, LifeState> lifeStates = new Dictionary<Position, LifeState>();
            
            foreach (Position position in positions) {
                uint x = position.X;
                uint y = position.Y;
                lifeStates.Add(position, _board[x, y]);
            }

            return lifeStates;
        }

        #endregion Protected Methods

        #region Private Methods

        //private void SetAlive(IEnumerable<Position> positions)
        //{
        //    lock (_syncObject) {
        //        foreach (Position position in positions) {
        //            SetAlive(position);
        //        }
        //    }
        //}

        //private void SetAlive(Position position)
        //{
        //    SetLifeState(position, LifeState.Alive);
        //}

        //private void SetDead(IEnumerable<Position> positions)
        //{
        //    lock (_syncObject) {
        //        foreach (Position position in positions) {
        //            SetDead(position);
        //        }
        //    }
        //}

        //private void SetDead(Position position)
        //{
        //    SetLifeState(position, LifeState.Dead);
        //}

        //private void SetLifeState(Position position, LifeState lifeState)
        //{
        //    lock (_syncObject) {
        //        if (_board[position.X, position.Y] != LifeState.NoLifePossible) {
        //            _board[position.X, position.Y] = lifeState;
        //        }
        //    }
        //}

        #endregion Private Methods
    }
}