using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class GameController
    {
        #region Private Fields

        private readonly LifeBoard _lifeBoard;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly Rules _rules;

        #endregion Private Fields

        #region Public Constructors

        public GameController(Rules rules, LifeBoard lifeBoard)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules)); ;
            _lifeBoard = lifeBoard ?? throw new ArgumentNullException(nameof(lifeBoard));
        }

        #endregion Public Constructors

        #region Public Events

        public event EventHandler<LifeBoardChangeEventArgs> LifeBoardChanged;

        #endregion Public Events

        #region Public Properties

        public uint Iteration { get; private set; }
        public IReadOnlyLifeBoard LifeBoard => _lifeBoard;

        #endregion Public Properties

        #region Public Methods

        public async Task IterateAsync()
        {
            ConcurrentBag<Position> birthPositions = new ConcurrentBag<Position>();
            ConcurrentBag<Position> deathPositions = new ConcurrentBag<Position>();

            List<Task> tasks = new List<Task>();

            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                for (uint y = 0; y < _lifeBoard.Height; ++y) {
                    for (uint x = 0; x < _lifeBoard.Width; ++x) {
                        uint x_ = x;
                        uint y_ = y;

                        //tasks.Add(Task.Run(() => {
                            var habitantPosition = new Position(x_, y_);
                            var habitantState = _lifeBoard.GetLifeState(habitantPosition);

                            if (habitantState != LifeState.NoLifePossible) {
                                LifeState[] neighbors = _lifeBoard.GetNeighbors(habitantPosition);
                                int nAliveNeighbors = neighbors.Count(n => n == LifeState.Alive);

                                if (habitantState == LifeState.Dead && _rules.Birth.Contains(nAliveNeighbors)) {
                                    birthPositions.Add(habitantPosition);
                                } else if (habitantState == LifeState.Alive && !_rules.Alive.Contains(nAliveNeighbors)) {
                                    deathPositions.Add(habitantPosition);
                                }
                            }
                        //}));
                    }
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);

                _lifeBoard.ApplyChanges(birthPositions, deathPositions);
            } finally {
                _lock.Release();
            }

            OnLifeBoardChanged(birthPositions.Union(deathPositions));

            ++Iteration;
        }

        public void ToggleLifeState(Position position)
        {
            ToggleLifeState(position.X, position.Y);
        }

        public void ToggleLifeState(uint x, uint y)
        {
            _lock.Wait();
            try {
                if (_lifeBoard[x, y] == LifeState.Alive) {
                    _lifeBoard.SetDead(x, y);
                } else {
                    _lifeBoard.SetAlive(x, y);
                }
            } finally {
                _lock.Release();
            }

            OnLifeBoardChanged(new List<Position>() {
                new Position(x, y)
            });
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void OnLifeBoardChanged(IEnumerable<Position> changes)
        {
            LifeBoardChanged?.Invoke(this, new LifeBoardChangeEventArgs(changes));
        }

        #endregion Protected Methods
    }
}