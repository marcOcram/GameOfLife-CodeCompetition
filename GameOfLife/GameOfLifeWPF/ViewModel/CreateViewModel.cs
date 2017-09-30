using GameOfLife;
using GameOfLifeWPF.Mvvm;
using GameOfLifeWPF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace GameOfLifeWPF.ViewModel
{
    /// <summary>
    /// ViewModel for the <see cref="View.CreateView"/>.
    /// </summary>
    /// <seealso cref="GameOfLifeWPF.Mvvm.PropertyChangedBase" />
    internal class CreateViewModel : PropertyChangedBase
    {
        #region Private Fields

        private readonly SimpleCommand _cancelCommand;
        private readonly GameService _gameService;
        private readonly SimpleCommand _startCommand;
        private readonly Dispatcher _uiDispatcher;
        private string _ruleDescription = "23/3";
        private ILifeBoardFactory _selectedFactory;
        private readonly InteractivityService _interactivityService;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateViewModel"/> class.
        /// </summary>
        /// <param name="gameService">The game service.</param>
        /// <param name="interactivityService">The interactivity service.</param>
        /// <param name="uiDispatcher">The UI dispatcher.</param>
        /// <exception cref="ArgumentNullException">
        /// gameService
        /// or
        /// uiDispatcher
        /// </exception>
        public CreateViewModel(GameService gameService, InteractivityService interactivityService, Dispatcher uiDispatcher)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _interactivityService = interactivityService ?? throw new ArgumentNullException(nameof(interactivityService));
            _uiDispatcher = uiDispatcher ?? throw new ArgumentNullException(nameof(uiDispatcher));

            _startCommand = new SimpleCommand(Start, CanStart);
            _cancelCommand = new SimpleCommand(Cancel, CanCancel);

            _gameService.CurrentGameChanged += (s, e) => _uiDispatcher.Invoke(_cancelCommand.RaiseCanExecuteChanged);

            Factories = new List<ILifeBoardFactory>() {
                new ToroidLifeBoardFactory(),
                new CuboidLifeBoardFactory()
            };

            SelectedFactory = Factories.FirstOrDefault();
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        /// Occurs when the user canceles the creation.
        /// </summary>
        public event EventHandler Canceled;

        /// <summary>
        /// Occurs when a new game has been started.
        /// </summary>
        public event EventHandler Started;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets the command to cancel the creation.
        /// </summary>
        /// <value>
        /// The cancel command.
        /// </value>
        public ICommand CancelCommand => _cancelCommand;

        /// <summary>
        /// Gets the factories to create a life board.
        /// </summary>
        /// <value>
        /// The factories.
        /// </value>
        public IEnumerable<ILifeBoardFactory> Factories { get; }

        /// <summary>
        /// Gets or sets the rule description.
        /// </summary>
        /// <value>
        /// The rule description.
        /// </value>
        public string RuleDescription {
            get { return _ruleDescription; }
            set {
                if (SetField(ref _ruleDescription, value)) {
                    _startCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected factory to create a life board.
        /// </summary>
        /// <value>
        /// The selected factory.
        /// </value>
        public ILifeBoardFactory SelectedFactory {
            get { return _selectedFactory; }
            set {
                if (SetField(ref _selectedFactory, value)) {
                    _startCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Gets the command to start a new game.
        /// </summary>
        /// <value>
        /// The start command.
        /// </value>
        public ICommand StartCommand => _startCommand;

        #endregion Public Properties

        #region Protected Methods

        protected virtual void RaiseCanceled()
        {
            Canceled?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void RaiseStarted()
        {
            Started?.Invoke(this, EventArgs.Empty);
        }

        #endregion Protected Methods

        #region Private Methods

        private bool CanCancel()
        {
            return _gameService.CurrentGame != null;
        }

        private void Cancel()
        {
            RaiseCanceled();
        }

        private bool CanStart()
        {
            return SelectedFactory != null && !string.IsNullOrEmpty(RuleDescription) && Rules.IsValid(RuleDescription);
        }

        private void Start()
        {
            LifeBoard lifeBoard = SelectedFactory?.Create();
            Rules rules = new Rules(RuleDescription);

            if (_gameService.CurrentGame != null) {
                bool continueCreation = _interactivityService.Ask("Start", "A game is currently open. Do you want to proceed?");
                
                if (!continueCreation) {
                    RaiseCanceled();
                    return;
                }
            }

            _gameService.Start(rules, lifeBoard);

            RaiseStarted();
        }

        #endregion Private Methods
    }
}