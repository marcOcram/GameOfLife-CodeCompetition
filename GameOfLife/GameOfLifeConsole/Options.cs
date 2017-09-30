using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLifeConsole
{
    /// <summary>
    /// Contains the structure of the arguments.
    /// </summary>
    internal class Options
    {
        #region Public Enums

        /// <summary>
        /// The available board types.
        /// </summary>
        public enum BoardType
        {
            /// <summary>
            /// A board in a toroid-like format.
            /// </summary>
            Toroid,

            /// <summary>
            /// A board in a cuboid-like format.
            /// </summary>
            Cuboid
        }

        #endregion Public Enums

        #region Public Properties

        /// <summary>
        /// Gets or sets the alive positions. The first value is the x-coordinate, the following value is the y-coordinate, the following value is the next x-coordinate and so on.
        /// </summary>
        /// <value>
        /// The alive positions.
        /// </value>
        [OptionArray('a', "alive-positions", HelpText = "The alive positions: x y x y x y x y x y x y x y", DefaultValue = new int[0], MutuallyExclusiveSet = "Custom")]
        public int[] AlivePositions { get; set; }

        /// <summary>
        /// Gets or sets the type of the board.
        /// </summary>
        /// <value>
        /// The board.
        /// </value>
        [Option('b', "board", MutuallyExclusiveSet = "Custom", DefaultValue = BoardType.Toroid)]
        public BoardType Board { get; set; }

        /// <summary>
        /// Gets or sets the depth of the cuboid. Only used when the board is a <see cref="BoardType.Cuboid"/>
        /// </summary>
        /// <value>
        /// The depth.
        /// </value>
        [Option('d', "depth", HelpText = "The Depth of the Cuboid.", DefaultValue = 10, MutuallyExclusiveSet = "Custom")]
        public int Depth { get; set; }

        /// <summary>
        /// Gets or sets the file path to a saved game. When this setting is set all other settings are ignored.
        /// </summary>
        /// <value>
        /// The game file path.
        /// </value>
        [Option('l', "load", HelpText = "Path to a saved game", MutuallyExclusiveSet = "LoadGame", DefaultValue = null)]
        public string GameFilePath { get; set; }

        /// <summary>
        /// Gets or sets the height of the toroid / cuboid.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        [Option('h', "height", HelpText = "The Height of the Toroid / Cuboid", DefaultValue = 10, MutuallyExclusiveSet = "Custom")]
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the rules for the game.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        [Option('r', "rules", HelpText = "The rules for the game.", DefaultValue = "23/3", MutuallyExclusiveSet = "Custom")]
        public string Rules { get; set; }

        /// <summary>
        /// Gets or sets the width of the toroid / cuboid.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        [Option('w', "width", HelpText = "The Width of the Toroid / Cuboid", DefaultValue = 10, MutuallyExclusiveSet = "Custom")]
        public int Width { get; set; }

        #endregion Public Properties
    }
}