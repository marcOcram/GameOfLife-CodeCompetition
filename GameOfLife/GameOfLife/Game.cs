using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace GameOfLife
{
    /// <summary>
    /// Plays the game on an <see cref="LifeBoard"/> by following the given rules and updating the board.
    /// </summary>
    [DataContract(Name = nameof(Game))]
    public class Game
    {
        #region Private Fields

        /// <summary>
        /// De-/Serializes an instance of this class.
        /// </summary>
        private static readonly Lazy<DataContractSerializer> _dataContractSerializer = new Lazy<DataContractSerializer>(() => new DataContractSerializer(typeof(Game)));

        /// <summary>
        /// The life board which is used to play the game of life.
        /// </summary>
        [DataMember(Name = "Board")]
        private readonly LifeBoard _lifeBoard;

        /// <summary>
        /// A locking mechanism to synchronize the access on the board.
        /// </summary>
        private SemaphoreSlim _lock;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="rules">The rules which should be used.</param>
        /// <param name="lifeBoard">The life board on which the game is played.</param>
        /// <exception cref="ArgumentNullException">
        /// rules
        /// or
        /// lifeBoard
        /// </exception>
        public Game(Rules rules, LifeBoard lifeBoard)
        {
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _lifeBoard = lifeBoard ?? throw new ArgumentNullException(nameof(lifeBoard));

            Initialize();
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        /// Occurs when the <see cref="LifeBoard"/> has changed.
        /// </summary>
        public event EventHandler<LifeBoardChangeEventArgs> LifeBoardChanged;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets the current iteration of the board.
        /// </summary>
        /// <value>
        /// The iteration.
        /// </value>
        [DataMember(Name = "Iteration")]
        public int Iteration { get; private set; }

        /// <summary>
        /// Gets the life board on which the game is played.
        /// </summary>
        /// <value>
        /// The life board.
        /// </value>
        public IReadOnlyLifeBoard LifeBoard => _lifeBoard;

        /// <summary>
        /// Gets the rules for the game of life.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        [DataMember(Name = "Rules")]
        public Rules Rules { get; private set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Creates a new instance of the <see cref="Game"/> class by loading a saved instance from any stream.
        /// </summary>
        /// <param name="source">The stream where the save game can be loaded from.</param>
        /// <returns></returns>
        public static Task<Game> Load(Stream source)
        {
            return Task.Run(() => {
                using (Stream deflateStream = new DeflateStream(source, CompressionMode.Decompress)) {
                    using (XmlReader xmlReader = XmlReader.Create(deflateStream)) {
                        return (Game)_dataContractSerializer.Value.ReadObject(xmlReader);
                    }
                }
            });
        }

        /// <summary>
        /// Changes the rule for the current game of life. Is only applied when the <paramref name="ruleDescription"/> is valid.
        /// </summary>
        /// <param name="ruleDescription">The rule description.</param>
        public void ChangeRule(string ruleDescription)
        {
            _lock.Wait();
            try {
                if (Rules.IsValid(ruleDescription)) {
                    Rules = new Rules(ruleDescription);
                }
            } finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Changes to the next iteration of the game only when the operation is not canceled!
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel the pending iteration.</param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException">When the cancellation token is cancelled.</exception>
        public async Task IterateAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<Task> tasks = new List<Task>();

            int nLogicalProcessors = Environment.ProcessorCount;

            int split = (int)Math.Ceiling(1.0 * _lifeBoard.Height / nLogicalProcessors);

            List<Position>[] taskBirthPositions = new List<Position>[nLogicalProcessors];
            List<Position>[] taskDeathPositions = new List<Position>[nLogicalProcessors];

            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                for (int i = 0; i < nLogicalProcessors; ++i) {
                    int currentProcessor = i;

                    taskBirthPositions[currentProcessor] = new List<Position>();
                    taskDeathPositions[currentProcessor] = new List<Position>();

                    tasks.Add(Task.Run(() => {
                        int start = currentProcessor * split;
                        int end = Math.Min((currentProcessor + 1) * split, _lifeBoard.Height);

                        for (int y = start; y < end; ++y) {
                            for (int x = 0; x < _lifeBoard.Width; ++x) {
                                cancellationToken.ThrowIfCancellationRequested();

                                var habitantPosition = new Position(x, y);
                                var habitantState = _lifeBoard.GetLifeState(habitantPosition);

                                if (habitantState != LifeState.NoLifePossible) {
                                    LifeState[] neighbors = _lifeBoard.GetNeighbors(habitantPosition);
                                    int nAliveNeighbors = neighbors.Count(n => n == LifeState.Alive);

                                    if (habitantState == LifeState.Dead && Rules.Birth.Contains(nAliveNeighbors)) {
                                        taskBirthPositions[currentProcessor].Add(habitantPosition);
                                    } else if (habitantState == LifeState.Alive && !Rules.Alive.Contains(nAliveNeighbors)) {
                                        taskDeathPositions[currentProcessor].Add(habitantPosition);
                                    }
                                }
                            }
                        }
                    }));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            } finally {
                _lock.Release();
            }

            ++Iteration;

            var flatBirthPositions = taskBirthPositions.SelectMany(x => x);
            var flatDeathPositions = taskDeathPositions.SelectMany(x => x);

            _lifeBoard.ApplyChanges(flatBirthPositions, flatDeathPositions);

            RaiseLifeBoardChanged(flatBirthPositions.Union(flatDeathPositions));
        }

        /// <summary>
        /// Saves the current <see cref="Game"/> instance into the given stream.
        /// </summary>
        /// <param name="target">The target stream.</param>
        /// <returns></returns>
        public async Task SaveAsync(Stream target)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                await Task.Run(() => {
                    using (Stream deflateStream = new DeflateStream(target, CompressionLevel.Optimal)) {
                        using (XmlWriter xmlWriter = XmlWriter.Create(deflateStream)) {
                            _dataContractSerializer.Value.WriteObject(xmlWriter, this);
                        }
                    }
                }).ConfigureAwait(false);
            } finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Toggles the life state at the given position.
        /// </summary>
        /// <param name="position">The position.</param>
        public void ToggleLifeState(Position position)
        {
            ToggleLifeState(position.X, position.Y);
        }

        /// <summary>
        /// Toggles the life state at the given position.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public void ToggleLifeState(int x, int y)
        {
            if (_lifeBoard[x, y] != LifeState.NoLifePossible) {
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

                RaiseLifeBoardChanged(new List<Position>() {
                    new Position(x, y)
                });
            }
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Raises the life board changed event.
        /// </summary>
        /// <param name="changes">The changes.</param>
        protected virtual void RaiseLifeBoardChanged(IEnumerable<Position> changes)
        {
            LifeBoardChanged?.Invoke(this, new LifeBoardChangeEventArgs(changes));
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            _lock = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Called when an instance has finished deserializing.
        /// </summary>
        /// <param name="streamingContext">The streaming context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext streamingContext)
        {
            Initialize();
        }

        #endregion Private Methods
    }
}