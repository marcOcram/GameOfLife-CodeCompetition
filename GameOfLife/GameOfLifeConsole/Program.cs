using GameOfLife;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameOfLifeConsole
{
    internal static class Program
    {
        #region Public Methods

        public static async Task Main(string[] args)
        {
            if (args.Length > 0) {
                Options options = new Options();
                if (CommandLine.Parser.Default.ParseArgumentsStrict(args, options)) {
                    await CreateGameFromParameters(options).ConfigureAwait(false);
                }
            } else {
                Console.WriteLine("Welcome!");

                int decision = GetDecision(new string[] { "New Game", "Load Example", "Load Save" });

                switch (decision) {
                    case 0:
                        await CreateNewGame().ConfigureAwait(false);
                        break;
                    case 1:
                        await LoadExample().ConfigureAwait(false);
                        break;
                    case 2:
                        await LoadGame().ConfigureAwait(false);
                        break;
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static async Task CreateGame(Rules rules, LifeBoard lifeBoard)
        {
            Game game = new Game(rules, lifeBoard);

            await StartGame(game).ConfigureAwait(false);
        }

        private static async Task CreateGameFromParameters(Options options)
        {
            if (options.GameFilePath != null) {
                await LoadGame(options.GameFilePath).ConfigureAwait(false);
            } else {
                List<Position> alivePositions = new List<Position>();

                for (int i = 0; i < (options.AlivePositions.Length - 1); i += 2) {
                    alivePositions.Add(new Position(options.AlivePositions[i], options.AlivePositions[i + 1]));
                }

                if (Rules.IsValid(options.Rules)) {
                    Rules rules = new Rules(options.Rules);

                    LifeBoard lifeBoard = null;
                    switch (options.Board) {
                        case Options.BoardType.Toroid:
                            lifeBoard = ToroidLifeBoard.Create(options.Width, options.Height, alivePositions);
                            break;

                        case Options.BoardType.Cuboid:
                            lifeBoard = CuboidLifeBoard.Create(options.Width, options.Height, options.Depth, alivePositions);
                            break;
                    }

                    if (lifeBoard != null) {
                        await CreateGame(rules, lifeBoard);
                    }
                }
            }
        }

        private static async Task CreateNewGame()
        {
            LifeBoard lifeBoard = GetLifeBoard();
            Rules rules = GetRules();

            await CreateGame(rules, lifeBoard).ConfigureAwait(false);
        }

        private static void DisplayGameOfLife(GameOfLife.Game gameOfLife)
        {
            Console.Clear();

            Console.WriteLine("Iteration " + gameOfLife.Iteration);

            DisplayHabitants(gameOfLife.LifeBoard);
        }

        private static void DisplayHabitants(IReadOnlyLifeBoard lifeBoard)
        {
            for (int i = 0; i < lifeBoard.Width; ++i) {
                if (i == 0) {
                    Console.Write("┌───");
                } else if (i == (lifeBoard.Width - 1)) {
                    Console.Write("───┐");
                } else {
                    Console.Write("──");
                }
            }

            Console.WriteLine();

            for (int y = 0; y < lifeBoard.Height; ++y) {
                Console.Write("│ ");

                ConsoleColor oldBackgroundColor = Console.BackgroundColor;
                ConsoleColor oldForegroundColor = Console.ForegroundColor;

                for (int x = 0; x < lifeBoard.Width; ++x) {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;

                    switch (lifeBoard[new Position(x, y)]) {
                        case LifeState.NoLifePossible:
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            Console.Write("  ");
                            break;

                        case LifeState.Dead:
                            Console.Write("  ");
                            break;

                        case LifeState.Alive:
                            Console.Write("██");
                            break;
                    }
                }

                Console.BackgroundColor = oldBackgroundColor;
                Console.ForegroundColor = oldForegroundColor;

                Console.Write(" │");
                Console.WriteLine();
            }

            for (int i = 0; i < lifeBoard.Width; ++i) {
                if (i == 0) {
                    Console.Write("└───");
                } else if (i == (lifeBoard.Width - 1)) {
                    Console.Write("───┘");
                } else {
                    Console.Write("──");
                }
            }

            Console.WriteLine();
        }

        private static IEnumerable<Position> GetAlivePositions()
        {
            Console.WriteLine("Enter positions to set alive (x, y): ");
            Console.WriteLine("Invalid positions (e. g. cuboid) are ignored");
            Console.WriteLine("Board");
            Console.WriteLine("  x 0   1   2   3");
            Console.WriteLine("y ┌───┬───┬───┬───┐");
            Console.WriteLine("0 │   │   │   │   │");
            Console.WriteLine("  ├───┼───┼───┼───┤");
            Console.WriteLine("1 │   │   │ █ │   │");
            Console.WriteLine("  ├───┼───┼───┼───┤");
            Console.WriteLine("2 │   │   │   │   │");
            Console.WriteLine("  ├───┼───┼───┼───┤");
            Console.WriteLine("3 │   │   │   │   │");
            Console.WriteLine("  └───┴───┴───┴───┘");
            Console.WriteLine();
            Console.WriteLine("█ -> (2, 1)");

            List<Position> positions = new List<Position>();

            bool exit = false;
            while (!exit) {
                int x = GetInt32("Enter x: ");
                int y = GetInt32("Enter y: ");

                positions.Add(new Position(x, y));

                Console.Write("Continue (Y/n)? ");
                string input = Console.ReadLine();
                if (!string.IsNullOrEmpty(input) || input.Equals("y", StringComparison.OrdinalIgnoreCase)) {
                    exit = true;
                }
            }

            return positions;
        }

        private static LifeBoard GetCuboidLifeBoard()
        {
            Console.Clear();

            int width = GetInt32("Enter width [3]: ", 3);
            int height = GetInt32("Enter height [3]: ", 3);
            int depth = GetInt32("Enter depth [3]: ", 3);
            IEnumerable<Position> alivePositions = GetAlivePositions();

            return CuboidLifeBoard.Create(width, height, depth, alivePositions);
        }

        private static int GetDecision(string[] decisions)
        {
            for (int i = 0; i < decisions.Length; ++i) {
                Console.WriteLine($"[{i}] {decisions[i]}");
            }

            int? decision = null;
            while (!decision.HasValue) {
                Console.Write("Your Choice: ");
                int tempDecision;
                if (int.TryParse(Console.ReadLine(), out tempDecision) && tempDecision < decisions.Length) {
                    decision = tempDecision;
                }
            }

            return decision.Value;
        }

        private static int GetInt32(string prompt, int? defaultValue = null)
        {
            while (true) {
                Console.Write(prompt);
                try {
                    string input = Console.ReadLine();
                    if (defaultValue.HasValue && string.IsNullOrEmpty(input)) {
                        return defaultValue.Value;
                    }
                    return Convert.ToInt32(input);
                } catch {
                    Console.WriteLine("Invalid Value!");
                }
            }
        }

        private static LifeBoard GetLifeBoard()
        {
            Console.Clear();

            Console.WriteLine("Select your board:");
            Console.WriteLine("[1] Toroid");
            Console.WriteLine("[2] Cuboid");

            int? answer = null;
            while (!answer.HasValue) {
                Console.Write("Your Choice: ");
                string input = Console.ReadLine();

                try {
                    answer = Convert.ToInt32(input);
                    switch (answer) {
                        case 1:
                            return GetToroidLifeBoard();

                        case 2:
                            return GetCuboidLifeBoard();

                        default:
                            Console.WriteLine("Invalid value!");
                            answer = null;
                            break;
                    }
                } catch {
                    Console.WriteLine("Invalid value!");
                }
            }

            throw new InvalidOperationException();
        }

        private static Rules GetRules()
        {
            while (true) {
                Console.Write("Enter rules [23/3]: ");
                string rules = Console.ReadLine();
                if (string.IsNullOrEmpty(rules)) {
                    return new Rules("23/3");
                } else {
                    try {
                        return new Rules(rules);
                    } catch {
                        Console.WriteLine("Invalid rule!");
                    }
                }
            }
        }

        private static LifeBoard GetToroidLifeBoard()
        {
            Console.Clear();

            int width = GetInt32("Enter width: ");
            int height = GetInt32("Enter height: ");
            IEnumerable<Position> alivePositions = GetAlivePositions();

            return ToroidLifeBoard.Create(width, height, alivePositions);
        }

        private static async Task LoadExample()
        {
            string[] examples = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Examples"));
            string[] decisions = examples.Select(Path.GetFileNameWithoutExtension).ToArray();

            int decision = GetDecision(decisions);

            await LoadGame(examples[decision]).ConfigureAwait(false);
        }

        private static async Task LoadGame()
        {
            string filePath = null;
            while (filePath == null || !File.Exists(filePath)) {
                Console.Write("Path: ");
                filePath = Console.ReadLine();
            }

            await LoadGame(filePath).ConfigureAwait(false);
        }

        private static async Task LoadGame(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath)) {
                Game game = await Game.Load(fileStream).ConfigureAwait(false);

                await StartGame(game).ConfigureAwait(false);
            }
        }

        private static async Task StartGame(Game gameOfLife)
        {
            int iterations = 1;
            DisplayGameOfLife(gameOfLife);
            while (true) {
                iterations = GetInt32($"Iterations [{iterations}]: ", iterations);
                for (int i = 0; i < iterations; ++i) {
                    await gameOfLife.IterateAsync(CancellationToken.None).ConfigureAwait(false);
                    DisplayGameOfLife(gameOfLife);
                }
            }
        }

        #endregion Private Methods
    }
}