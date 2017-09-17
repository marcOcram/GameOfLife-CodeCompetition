using GameOfLife;
using GameOfLifeWPF.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameOfLifeWPF.ViewModel
{
    internal class GameOfLifeViewModel : PropertyChangedBase
    {
        #region Private Fields

        private readonly FieldViewModel[,] _fields;
        private readonly List<FieldViewModel> _flattenedFields = new List<FieldViewModel>();
        private readonly GameController _gameController;
        private readonly SimpleCommand _iterateCommand;
        private readonly LifeBoard _lifeBoard;
        private readonly Rules _rules;
        private uint _currentIteration;
        private bool _isIterating;
        private bool _performanceIssue;

        #endregion Private Fields

        #region Public Constructors

        public GameOfLifeViewModel(/*GameController gameController*/)
        {
            //if (rules == null) throw new ArgumentNullException(nameof(rules));
            //if (lifeBoard == null) throw new ArgumentNullException(nameof(lifeBoard));

            _lifeBoard = CuboidLifeBoard.Create(10, 10, 10, new List<Position>() {
                //new Position(30, 22),
                //new Position(30, 23),
                //new Position(30, 24)
            });
            //_lifeBoard = RectangularLifeBoard.Create(1, 1, new List<Position>() {
            //    //new Position(8, 8),
            //    //new Position(8, 9),
            //    //new Position(8, 10),
            //    //new Position(9, 10),
            //    //new Position(10, 8),
            //    //new Position(10, 9),
            //    //new Position(10, 10),
            //    //new Position(10, 11),
            //    //new Position(10, 12),
            //    //new Position(12, 8),
            //    //new Position(12, 11),
            //    //new Position(12, 12),
            //    //new Position(13, 8),
            //    //new Position(13, 10),
            //    //new Position(13, 12),
            //    //new Position(14, 8),
            //    //new Position(14, 9),
            //    //new Position(14, 12),
            //});
            _rules = new Rules("23/3");

            _gameController = new GameController(_rules, _lifeBoard);
            _gameController.LifeBoardChanged += ReflectChanges;
            _fields = new FieldViewModel[_gameController.LifeBoard.Width, _gameController.LifeBoard.Height];

            _iterateCommand = new SimpleCommand(IterateAsync, CanIterate);

            for (uint y = 0; y < Rows; ++y) {
                for (uint x = 0; x < Columns; ++x) {
                    _fields[x, y] = CreateFieldViewModel(x, y);
                    _flattenedFields.Add(_fields[x, y]);
                }
            }
        }

        #endregion Public Constructors

        #region Public Properties

        public uint Columns => _gameController.LifeBoard.Width;

        public uint CurrentIteration {
            get { return _currentIteration; }
            private set { SetField(ref _currentIteration, value); }
        }

        public uint DelayInMilliseconds { get; set; }

        public IEnumerable<FieldViewModel> Fields => _flattenedFields;

        public bool IsIterating {
            get {
                return _isIterating;
            }
            set {
                SetField(ref _isIterating, value);
                _iterateCommand.RaiseCanExecuteChanged();
            }
        }

        public ICommand IterateCommand => _iterateCommand;

        public uint Iterations { get; set; } = 1;

        public bool PerformanceIssue {
            get { return _performanceIssue; }
            private set { SetField(ref _performanceIssue, value); }
        }

        public uint Rows => _gameController.LifeBoard.Height;

        #endregion Public Properties

        #region Private Methods

        private bool CanIterate()
        {
            return !_isIterating;
        }

        private FieldViewModel CreateFieldViewModel(uint x, uint y)
        {
            var fieldViewModel = new FieldViewModel(x, y, _lifeBoard[x, y]);
            fieldViewModel.LifeStateChangeRequested += ToggleLifeState;
            return fieldViewModel;
        }

        private async void IterateAsync()
        {
            try {
                IsIterating = true;
                await Task.Run(async () => {
                    for (int i = 0; i < Iterations; ++i) {
                        await _gameController.IterateAsync().ConfigureAwait(false);
                        CurrentIteration = _gameController.Iteration;
                        await Task.Delay(TimeSpan.FromMilliseconds(DelayInMilliseconds));
                    }
                });
            } finally {
                IsIterating = false;
            }
        }

        private void ReflectChanges(object sender, LifeBoardChangeEventArgs e)
        {
            foreach (Position position in e.Changes) {
                _fields[position.X, position.Y].IsAlive = _gameController.LifeBoard[position] == LifeState.Alive;
            }
        }

        private void ToggleLifeState(object sender, EventArgs e)
        {
            if (sender is FieldViewModel fieldViewModel) {
                _gameController.ToggleLifeState(fieldViewModel.X, fieldViewModel.Y);
            }
        }

        #endregion Private Methods
    }
}