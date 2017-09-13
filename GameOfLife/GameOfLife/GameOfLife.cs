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
            _lifeBoard = lifeBoard ?? throw new ArgumentNullException(nameof(lifeBoard)); ;

            Habitants = _lifeBoard.GetLifeStates();
        }

        #endregion Public Constructors

        #region Public Properties

        public IReadOnlyDictionary<Position, LifeState> Habitants { get; private set; }

        public uint Height => _lifeBoard.TotalHeight;
        public uint Iteration { get; private set; }
        public uint Width => _lifeBoard.TotalWidth;

        #endregion Public Properties

        #region Public Methods

        public void Iterate()
        {
            ConcurrentDictionary<Position, LifeState> changes = new ConcurrentDictionary<Position, LifeState>();

            Parallel.ForEach(_lifeBoard.GetLifeStates().Where(h => h.Value != LifeState.NoLifePossible), habitant => {
                LifeState[] neighbors = _lifeBoard.GetNeighbors(habitant.Key).Values.ToArray();
                int nAliveNeighbors = neighbors.Count(n => n == LifeState.Alive);

                if (habitant.Value == LifeState.Dead && _rules.Birth.Contains(nAliveNeighbors)) {
                    changes.AddOrUpdate(habitant.Key, LifeState.Alive, (key, oldValue) => LifeState.Alive);
                } else if (habitant.Value == LifeState.Alive && !_rules.Alive.Contains(nAliveNeighbors)) {
                    changes.AddOrUpdate(habitant.Key, LifeState.Dead, (key, oldValue) => LifeState.Dead);
                }
            });

            _lifeBoard.ApplyChanges(changes.Where(c => c.Value == LifeState.Alive).Select(c => c.Key), changes.Where(c => c.Value == LifeState.Dead).Select(c => c.Key));

            Habitants = _lifeBoard.GetLifeStates();

            ++Iteration;
        }

        #endregion Public Methods
    }
}