using GameOfLife;
using GameOfLifeWPF.Mvvm;
using GameOfLifeWPF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace GameOfLifeWPF.ViewModel
{
    /// <summary>
    /// This view model is the interface between a <see cref="Game"/> and a view.
    /// </summary>
    /// <seealso cref="GameOfLifeWPF.Mvvm.PropertyChangedBase" />
    internal class GameViewModel : PropertyChangedBase
    {
        #region Private Fields

        private readonly SimpleCommand _cancelIterateCommand;
        private readonly SimpleCommand _changeRuleCommand;
        private readonly GameService _gameService;
        private readonly SimpleCommand _iterateCommand;
        private readonly Dispatcher _uiDispatcher;
        private CancellationTokenSource _cancellationTokenSource;
        private FieldViewModel[][] _fields;
        private Game _gameController;
        private bool _isIterating;
        private string _newRuleDescription;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GameViewModel"/> class.
        /// </summary>
        /// <param name="gameService">The game service.</param>
        /// <param name="uiDispatcher">The UI dispatcher.</param>
        /// <exception cref="ArgumentNullException">
        /// gameService
        /// or
        /// uiDispatcher
        /// </exception>
        public GameViewModel(GameService gameService, Dispatcher uiDispatcher)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _uiDispatcher = uiDispatcher ?? throw new ArgumentNullException(nameof(uiDispatcher));

            _iterateCommand = new SimpleCommand(IterateAsync, CanIterate);
            _cancelIterateCommand = new SimpleCommand(CancelIterate, CanCancelIterate);
            _changeRuleCommand = new SimpleCommand(ChangeRule, CanChangeRule);

            _gameService.CurrentGameChanged += (s, e) => _uiDispatcher.Invoke(() => ChangeGame(_gameService.CurrentGame));

            if (_gameService.CurrentGame != null) {
                ChangeGame(_gameService.CurrentGame);
            }
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets the command to cancel the current iteration.
        /// </summary>
        /// <value>
        /// The command to cancel the current iteration.
        /// </value>
        public ICommand CancelIterateCommand => _cancelIterateCommand;

        /// <summary>
        /// Gets the command to change the current rule.
        /// </summary>
        /// <value>
        /// The command to change the current rule.
        /// </value>
        public ICommand ChangeRuleCommand => _changeRuleCommand;

        /// <summary>
        /// Gets the columns of the lifeboard.
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        public int Columns => _gameController.LifeBoard.Width;

        /// <summary>
        /// Gets the current iteration of the life board.
        /// </summary>
        /// <value>
        /// The current iteration.
        /// </value>
        public int CurrentIteration => _gameController.Iteration;

        /// <summary>
        /// Gets the current rule description.
        /// </summary>
        /// <value>
        /// The current rule description.
        /// </value>
        public string CurrentRuleDescription => _gameController?.Rules.RuleDescription;

        /// <summary>
        /// Gets or sets the delay in milliseconds between each iteration.
        /// </summary>
        /// <value>
        /// The delay in milliseconds.
        /// </value>
        public int DelayInMilliseconds { get; set; } = 20;

        /// <summary>
        /// Gets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public IEnumerable<FieldViewModel> Fields => _fields.SelectMany(fvm => fvm);

        /// <summary>
        /// Gets or sets a value indicating whether this instance is iterating.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is iterating; otherwise, <c>false</c>.
        /// </value>
        public bool IsIterating {
            get {
                return _isIterating;
            }
            set {
                if (SetField(ref _isIterating, value)) {
                    _iterateCommand.RaiseCanExecuteChanged();
                    _cancelIterateCommand.RaiseCanExecuteChanged();
                    _changeRuleCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Gets the command to iterate.
        /// </summary>
        /// <value>
        /// The iterate command.
        /// </value>
        public ICommand IterateCommand => _iterateCommand;

        /// <summary>
        /// Gets or sets the iterations which should be done when the <see cref="IterateCommand"/> is executed.
        /// </summary>
        /// <value>
        /// The iterations.
        /// </value>
        public uint Iterations { get; set; } = 1;

        /// <summary>
        /// Gets or sets the rule description for a new rule.
        /// </summary>
        /// <value>
        /// The new rule description.
        /// </value>
        public string NewRuleDescription {
            get { return _newRuleDescription; }
            set { SetField(ref _newRuleDescription, value); }
        }

        /// <summary>
        /// Gets the rows of the current life board.
        /// </summary>
        /// <value>
        /// The rows.
        /// </value>
        public int Rows => _gameController.LifeBoard.Height;

        #endregion Public Properties

        #region Private Methods

        private bool CanCancelIterate()
        {
            return IsIterating;
        }

        private void CancelIterate()
        {
            if (IsIterating) {
                _cancellationTokenSource?.Cancel();
            }
        }

        private bool CanChangeRule()
        {
            return !IsIterating && Rules.IsValid(NewRuleDescription);
        }

        private bool CanIterate()
        {
            return !IsIterating;
        }

        /// <summary>
        /// Changes the old game to a new game.
        /// </summary>
        /// <param name="game">The new game.</param>
        private void ChangeGame(Game game)
        {
            if (_gameController != null) {
                _gameController.LifeBoardChanged -= ReflectChanges;
                _fields = null;
            }

            if (game != null) {
                _gameController = _gameService.CurrentGame;
                _gameController.LifeBoardChanged += ReflectChanges;

                _fields = new FieldViewModel[_gameController.LifeBoard.Width][];

                for (int y = 0; y < Rows; ++y) {
                    for (int x = 0; x < Columns; ++x) {
                        if (y == 0) {
                            _fields[x] = new FieldViewModel[_gameController.LifeBoard.Height];
                        }

                        _fields[x][y] = CreateFieldViewModel(x, y);
                    }
                }

                NewRuleDescription = _gameController.Rules.RuleDescription;
                RaisePropertyChanged(null); // Refresh every property
            }
        }

        /// <summary>
        /// Changes the rule to the new rule description if it is valid.
        /// </summary>
        private void ChangeRule()
        {
            string newRuleDescription = NewRuleDescription;
            if (Rules.IsValid(newRuleDescription)) {
                _gameController.ChangeRule(newRuleDescription);
                RaisePropertyChanged(nameof(CurrentRuleDescription));
            }
        }

        private FieldViewModel CreateFieldViewModel(int x, int y)
        {
            var fieldViewModel = new FieldViewModel(x, y, _gameController.LifeBoard[x, y]);
            fieldViewModel.LifeStateChangeRequested += ToggleLifeState;
            return fieldViewModel;
        }

        /// <summary>
        /// Does the in <see cref="Iterations"/> defined iterations and waits between each iteration the in <see cref="DelayInMilliseconds"/> defined time.
        /// </summary>
        private async void IterateAsync()
        {
            // save the iterations so it cannot be changed during the iteration itself
            uint iterations = Iterations;
            try {
                IsIterating = true;
                using (_cancellationTokenSource = new CancellationTokenSource()) {
                    for (int i = 0; i < iterations; ++i) {
                        await _gameController.IterateAsync(_cancellationTokenSource.Token).ConfigureAwait(true);
                        RaisePropertyChanged(nameof(CurrentIteration));
                        await Task.Delay(TimeSpan.FromMilliseconds(DelayInMilliseconds)).ConfigureAwait(true);
                    }
                }
            } catch (OperationCanceledException) {
                // is expected in case of cancellation -> do nothing
            } finally {
                IsIterating = false;
            }
        }

        /// <summary>
        /// Reflects the changes of the life board back into the <see cref="Fields"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="LifeBoardChangeEventArgs"/> instance containing the event data.</param>
        private void ReflectChanges(object sender, LifeBoardChangeEventArgs e)
        {
            foreach (Position position in e.Changes) {
                _fields[position.X][position.Y].IsAlive = _gameController.LifeBoard[position] == LifeState.Alive;
            }
        }

        /// <summary>
        /// Toggles the life state of a <see cref="FieldViewModel"/> in <see cref="Fields"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToggleLifeState(object sender, EventArgs e)
        {
            if (sender is FieldViewModel fieldViewModel) {
                _gameController.ToggleLifeState(fieldViewModel.X, fieldViewModel.Y);
            }
        }

        #endregion Private Methods
    }
}