using GameOfLifeWPF.Mvvm;
using GameOfLifeWPF.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace GameOfLifeWPF.ViewModel
{
    /// <summary>
    /// This is the view model which is responsible for the menu system.
    /// </summary>
    /// <seealso cref="GameOfLifeWPF.Mvvm.PropertyChangedBase" />
    internal class ShellViewModel : PropertyChangedBase
    {
        #region Private Fields

        private readonly SimpleCommand _exitCommand;
        private readonly GameService _gameService;
        private readonly GameViewModel _gameViewModel;
        private readonly InteractivityService _interactivityService;
        private readonly SimpleCommand<string> _loadCommand;
        private readonly CreateViewModel _mainViewModel;
        private readonly SimpleCommand _newCommand;
        private readonly SimpleCommand _saveAsCommand;
        private readonly SimpleCommand _saveCommand;
        private readonly Dispatcher _uiDispatcher;
        private object _activeViewModel;
        private string _currentGameFilePath;

        #endregion Private Fields

        #region Public Constructors

        public ShellViewModel(GameService gameService, InteractivityService interactivityService, Dispatcher uiDispatcher)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _interactivityService = interactivityService ?? throw new ArgumentNullException(nameof(interactivityService));
            _uiDispatcher = uiDispatcher ?? throw new ArgumentNullException(nameof(uiDispatcher));

            _mainViewModel = new CreateViewModel(_gameService, interactivityService, _uiDispatcher);
            _gameViewModel = new GameViewModel(_gameService, _uiDispatcher);

            _newCommand = new SimpleCommand(Create, CanCreate);
            _saveCommand = new SimpleCommand(Save, CanSave);
            _saveAsCommand = new SimpleCommand(SaveAs, CanSaveAs);
            _loadCommand = new SimpleCommand<string>(Load, CanLoad);
            _exitCommand = new SimpleCommand(Exit);

            _mainViewModel.Started += (s, e) => {
                SetCurrentGameFilePath(null);
                UpdateActiveViewModel();
            };
            _mainViewModel.Canceled += (s, e) => UpdateActiveViewModel();

            _gameViewModel.PropertyChanged += (s, e) => {
                _newCommand.RaiseCanExecuteChanged();
                _loadCommand.RaiseCanExecuteChanged();
                _saveCommand.RaiseCanExecuteChanged();
                _saveAsCommand.RaiseCanExecuteChanged();
            };

            _gameService.CurrentGameChanged += (s, e) => {
                // the service isn't expected to run the events on the main thread
                _uiDispatcher.Invoke(() => {
                    _saveCommand.RaiseCanExecuteChanged();
                    UpdateActiveViewModel();
                });
            };

            ActiveViewModel = _mainViewModel;
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        /// Occurs when the user requests to exit the application.
        /// </summary>
        public event EventHandler ExitRequest;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets or sets the active view model.
        /// </summary>
        /// <value>
        /// The active view model.
        /// </value>
        public object ActiveViewModel {
            get { return _activeViewModel; }
            set { SetField(ref _activeViewModel, value); }
        }

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <value>
        /// The examples.
        /// </value>
        public IEnumerable<SaveGame> Examples => _gameService.Examples.Select(filePath => new SaveGame() { FilePath = filePath }).OrderBy(sg => sg.Name);

        /// <summary>
        /// Gets the command to request to exit the application.
        /// </summary>
        /// <value>
        /// The exit command.
        /// </value>
        public ICommand ExitCommand => _exitCommand;

        /// <summary>
        /// Gets the command to load a game.
        /// </summary>
        /// <value>
        /// The load command.
        /// </value>
        public ICommand LoadCommand => _loadCommand;

        /// <summary>
        /// Gets the command to create a new game.
        /// </summary>
        /// <value>
        /// The new command.
        /// </value>
        public ICommand NewCommand => _newCommand;

        /// <summary>
        /// Gets the command to save the current game as a new file.
        /// </summary>
        /// <value>
        /// The save as command.
        /// </value>
        public ICommand SaveAsCommand => _saveAsCommand;

        /// <summary>
        /// Gets the command to save the current game to the loaded file.
        /// </summary>
        /// <value>
        /// The save command.
        /// </value>
        public ICommand SaveCommand => _saveCommand;

        #endregion Public Properties

        #region Protected Methods

        protected virtual void RaiseExitRequest()
        {
            ExitRequest?.Invoke(this, EventArgs.Empty);
        }

        #endregion Protected Methods

        #region Private Methods

        private bool CanCreate()
        {
            return !_gameViewModel.IsIterating;
        }

        private bool CanLoad()
        {
            return !_gameViewModel.IsIterating;
        }

        private bool CanSave()
        {
            return CanSaveAs() && _currentGameFilePath != null;
        }

        private bool CanSaveAs()
        {
            return _gameService.CurrentGame != null && !_gameViewModel.IsIterating;
        }

        private void Create()
        {
            ActiveViewModel = _mainViewModel;
        }

        private void Exit()
        {
            if (_interactivityService.Ask("Quit", "Do you really want to quit?")) {
                RaiseExitRequest();
            }
        }

        /// <summary>
        /// Asks the user to provide a file path to a save game and loads it via the <see cref="GameService"/>.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        private async void Load(string filePath)
        {
            if (_gameService.CurrentGame == null || _interactivityService.Ask("Load", "There's an open game. Do you want to continue?")) {
                filePath = filePath ?? _interactivityService.GetFilePath(false);

                if (!string.IsNullOrEmpty(filePath)) {
                    try {
                        await _gameService.LoadAsync(filePath).ConfigureAwait(false);
                        SetCurrentGameFilePath(filePath);
                    } catch {
                        _interactivityService.Notify("Error", "Couldn't load game!");
                    }
                }
            }
        }

        /// <summary>
        /// Saves the current game to the current loaded file.
        /// </summary>
        private async void Save()
        {
            if (_gameService.CurrentGame == null || string.IsNullOrEmpty(_currentGameFilePath)) {
                return;
            }

            try {
                await _gameService.SaveAsync(_currentGameFilePath).ConfigureAwait(false);
            } catch {
                _interactivityService.Notify("Error", "Couldn't save game!");
            }
        }

        /// <summary>
        /// Saves the current game as a user provided file.
        /// </summary>
        private async void SaveAs()
        {
            if (_gameService.CurrentGame == null) {
                return;
            }

            string filePath = _interactivityService.GetFilePath(true);

            if (!string.IsNullOrEmpty(filePath)) {
                try {
                    await _gameService.SaveAsync(filePath).ConfigureAwait(false);
                    SetCurrentGameFilePath(filePath);
                } catch {
                    _interactivityService.Notify("Error", "Couldn't save game!");
                }
            }
        }

        private void SetCurrentGameFilePath(string filePath)
        {
            _currentGameFilePath = filePath;
            _uiDispatcher.Invoke(_saveCommand.RaiseCanExecuteChanged);
        }

        private void UpdateActiveViewModel()
        {
            ActiveViewModel = _gameService.CurrentGame != null ? _gameViewModel : _mainViewModel as object;
        }

        #endregion Private Methods

        #region Internal Structs

        internal struct SaveGame
        {
            #region Public Properties

            public string FilePath { get; set; }
            public string Name => Path.GetFileNameWithoutExtension(FilePath);

            #endregion Public Properties
        }

        #endregion Internal Structs
    }
}