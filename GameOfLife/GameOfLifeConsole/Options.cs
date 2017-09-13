using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLifeConsole
{
    internal class Options
    {
        enum BoardType
        {
            Rectangle,
            Cuboid
        }

        [VerbOption("Rectangle", DefaultValue = BoardType.Rectangle, Required = true, HelpText = "Sets the type of board.")]
        BoardType Board { get; set; }
        
        internal class RectangleOptions
        {
            [Option('w', "width", HelpText = "The Width of the Board", Required = true)]
            public int Width { get; set; }

            [Option('h', "height", HelpText = "The Height of the Board", Required = true)]
            public int Height { get; set; }
        }
    }
}
