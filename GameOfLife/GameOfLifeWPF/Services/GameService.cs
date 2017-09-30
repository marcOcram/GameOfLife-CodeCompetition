using GameOfLife;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeWPF.Services
{
    /// <summary>
    /// This service is used for everything which is related to a <see cref="Game"/>.
    /// </summary>
    internal class GameService
    {
        #region Private Fields

        /// <summary>
        /// The lazy loaded example file paths.
        /// </summary>
        private readonly Lazy<IEnumerable<string>> _examples = new Lazy<IEnumerable<string>>(GetExamples);
        private Game _currentGame;

        #endregion Private Fields

        #region Public Events

        /// <summary>
        /// Occurs when the <see cref="CurrentGame"/> changed.
        /// </summary>
        public event EventHandler CurrentGameChanged;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets the current game. Is <see cref="null"/> if there's no game opened.
        /// </summary>
        /// <value>
        /// The current game.
        /// </value>
        public Game CurrentGame {
            get { return _currentGame; }
            private set {
                if (_currentGame != value) {
                    _currentGame = value;
                    RaiseCurrentGameChanged();
                }
            }
        }

        /// <summary>
        /// Gets the file paths to the examples.
        /// </summary>
        /// <value>
        /// The examples.
        /// </value>
        public IEnumerable<string> Examples => _examples.Value;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Loads a game from a file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns></returns>
        public async Task LoadAsync(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath)) {
                CurrentGame = await Game.Load(fileStream).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Saves current game to a file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">There's no game to save!</exception>
        public async Task SaveAsync(string filePath)
        {
            if (CurrentGame == null) {
                throw new InvalidOperationException("There's no game to save!");
            }

            using (FileStream fileStream = File.Create(filePath)) {
                await CurrentGame.SaveAsync(fileStream).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Starts a game with the given rules on the given life board.
        /// </summary>
        /// <param name="rules">The rules.</param>
        /// <param name="lifeBoard">The life board.</param>
        public void Start(Rules rules, LifeBoard lifeBoard)
        {
            CurrentGame = new Game(rules, lifeBoard);
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Raises the <see cref="CurrentGameChanged"/> event.
        /// </summary>
        protected virtual void RaiseCurrentGameChanged()
        {
            CurrentGameChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Gets the file paths to the examples by examining the "Examples" folder where the executable is located.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string> GetExamples()
        {
            string exampleFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Examples");

            if (Directory.Exists(exampleFolder)) {
                return Directory.GetFiles(exampleFolder);
            }

            return new List<string>();
        }

        #endregion Private Methods
    }
}