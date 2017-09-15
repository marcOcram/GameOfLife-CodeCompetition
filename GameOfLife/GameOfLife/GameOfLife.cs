using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class GameOfLife
    {
        #region Private Fields

        private readonly LifeBoard _lifeBoard;
        private readonly Rules _rules;

        #endregion Private Fields

        #region Public Constructors

        public GameOfLife(Rules rules, LifeBoard lifeBoard)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules)); ;
            _lifeBoard = lifeBoard ?? throw new ArgumentNullException(nameof(lifeBoard));

            NonHabitablePositions = _lifeBoard.ToPositionDictionary(_lifeBoard.GetLifeStates().Where(l => l == LifeState.NoLifePossible).ToArray()).Keys;

            UpdateHabitablePositions();
        }

        #endregion Public Constructors

        #region Public Properties

        public IEnumerable<Position> NonHabitablePositions { get; }

        public IReadOnlyDictionary<Position, LifeState> Habitants { get; private set; }

        public uint Height => _lifeBoard.TotalHeight;

        public uint Iteration { get; private set; }

        public uint Width => _lifeBoard.TotalWidth;

        #endregion Public Properties

        #region Public Methods

        public void Iterate()
        {
            ConcurrentBag<Position> birthPositions = new ConcurrentBag<Position>();
            ConcurrentBag<Position> deathPositions = new ConcurrentBag<Position>();

            Parallel.ForEach(Habitants.Where(h => h.Value != LifeState.NoLifePossible), new ParallelOptions() { MaxDegreeOfParallelism = 4 }, habitant => {
                LifeState[] neighbors = _lifeBoard.GetNeighbors(habitant.Key);
                int nAliveNeighbors = neighbors.Count(n => n == LifeState.Alive);

                if (habitant.Value == LifeState.Dead && _rules.Birth.Contains(nAliveNeighbors)) {
                    birthPositions.Add(habitant.Key);
                } else if (habitant.Value == LifeState.Alive && !_rules.Alive.Contains(nAliveNeighbors)) {
                    deathPositions.Add(habitant.Key);
                }
            });

            _lifeBoard.ApplyChanges(birthPositions, deathPositions);

            UpdateHabitablePositions();

            ++Iteration;
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateHabitablePositions()
        {
            Habitants = _lifeBoard.ToPositionDictionary(_lifeBoard.GetLifeStates());
        }

        #endregion Private Methods
    }
}