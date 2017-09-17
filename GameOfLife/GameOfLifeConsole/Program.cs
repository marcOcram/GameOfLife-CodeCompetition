using GameOfLife;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            LifeBoard lifeBoard = GetLifeBoard();

            Rules rules = GetRules();
            
            var gameOfLife = new GameOfLife.GameController(rules, lifeBoard);

            uint iterations = 1;
            DisplayGameOfLife(gameOfLife);
            while (true) {
                iterations = GetUInt32($"Iterations [{iterations}]: ", iterations);
                for (int i = 0; i < iterations; ++i) {
                    await gameOfLife.IterateAsync().ConfigureAwait(false);
                    DisplayGameOfLife(gameOfLife);
                }
            }
        }

        private static LifeBoard GetLifeBoard()
        {
            Console.Clear();

            Console.WriteLine("Select your board:");
            Console.WriteLine("[1] Rectangle");
            Console.WriteLine("[2] Cuboid");

            int? answer = null;
            while (!answer.HasValue) {
                Console.Write("Your Choice: ");
                string input = Console.ReadLine();

                try {
                    answer = Convert.ToInt32(input);
                    switch (answer) {
                        case 1:
                            return GetRectangleLifeBoard();
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

        private static LifeBoard GetCuboidLifeBoard()
        {
            Console.Clear();

            uint width = GetUInt32("Enter width [3]: ", 3);
            uint height = GetUInt32("Enter height [3]: ", 3);
            uint depth = GetUInt32("Enter depth [3]: ", 3);
            IEnumerable<Position> alivePositions = GetAlivePositions();

            return CuboidLifeBoard.Create(width, height, depth, alivePositions);
        }

        private static LifeBoard GetRectangleLifeBoard()
        {
            Console.Clear();

            uint width = GetUInt32("Enter width: ");
            uint height = GetUInt32("Enter height: ");
            IEnumerable<Position> alivePositions = GetAlivePositions();

            return RectangularLifeBoard.Create(width, height, alivePositions);
        }

        private static uint GetUInt32(string prompt, uint? defaultValue = null)
        {
            while (true) {
                Console.Write(prompt);
                try {
                    string input = Console.ReadLine();
                    if (defaultValue.HasValue && string.IsNullOrEmpty(input)) {
                        return defaultValue.Value;
                    }
                    return Convert.ToUInt32(input);
                } catch {
                    Console.WriteLine("Invalid Value!");
                }
            }
        }

        private static IEnumerable<Position> GetAlivePositions()
        {
            Console.WriteLine("Enter positions to set alive (x, y): ");
            Console.WriteLine("Invalid positions (e. g. Coboid) are ignored");
            Console.WriteLine("Board");
            Console.WriteLine("      x-axis");
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
                uint x = GetUInt32("Enter x: ");
                uint y = GetUInt32("Enter y: ");

                positions.Add(new Position(x, y));

                Console.Write("Continue (Y/n)? ");
                string input = Console.ReadLine();
                if (!string.IsNullOrEmpty(input) || input.Equals("y", StringComparison.OrdinalIgnoreCase)) {
                    exit = true;
                }
            }

            return positions;
        }

        private static void DisplayGameOfLife(GameOfLife.GameController gameOfLife)
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

            for (uint y = 0; y < lifeBoard.Height; ++y) {

                Console.Write("│ ");

                ConsoleColor oldBackgroundColor = Console.BackgroundColor;
                ConsoleColor oldForegroundColor = Console.ForegroundColor;

                for (uint x = 0; x < lifeBoard.Width; ++x) {
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
    }
}
