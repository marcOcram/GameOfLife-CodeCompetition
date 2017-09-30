# Game of Life - Code Competition - 09.2017

## Installation

1. Install .Net Framework 4.7
2. Download the latest GameOfLife-<Version>.zip
3. Unzip the downloaded file

## Running on the Console

In order to run the Game of Life on the console start up the *GameOfLifeConsole.exe*. It guides you through the process of starting a game. It supports starting a new game, loading an example or loading a saved game.

### Parameters

The console version supports the following parameters:

| Parameter	| Value |
| ---------	| ----- |
| -b		| "Toroid" or "Cuboid" (Default: "Toroid") |
| -w 		| The width of the toroid or cuboid (Default: 10) |
| -h 		| The height of the torid or cuboid (Default: 10) |
| -d 		| The depth of the cuboid (Default: 10) |
| -r 		| The rules (Default: 23/3) |
| -a 		| List of initial alive positions. First value is x, then y, then x, then y, ... |
| -l 		| Path to a saved game (can only be used without any other parameter) |
    
#### Examples

This opens a toroid game with a width and height of 10. A ruleset of 23/3 and initial alive positions at (2, 1), (2, 2) & (2, 3).

    GameOfLifeConsole.exe -b Toroid -w 10 -h 10 -r 23/3 -a 2 1 2 2 2 3

This opens a cuboid game with a width, height and depth of 10. A ruleset of 23/3 and initial alive positions at (9, 10), (10, 9) & (11, 9).
 
    GameOfLifeConsole.exe -b Cuboid -w 10 -h 10 -d 10 -a 9 10 10 9 11 9

This opens a save game from the example folder.

    GameOfLifeConsole.exe -l ".\Examples\Cuboid_MWSS.cgol"

## Running as Desktop Application

In order to run the Game of Life as desktop application you have to start the *GameOfLifeWPF.exe*. It supports every feature through the graphical user interface.

## Used Software

The following software is used:

| Library 																	| License |
| [NETStandard.Library](https://github.com/dotnet/standard) 				| [MIT License](https://github.com/dotnet/standard/blob/master/LICENSE.TXT) |
| [System.Runtime.Serialization.Primitives](https://www.microsoft.com/net) 	| [MICROSOFT SOFTWARE LICENSE](https://www.microsoft.com/net/dotnet_library_license.htm)
| [CommandLineParser](https://github.com/gsscoder/commandline) 				| [MIT License](https://github.com/gsscoder/commandline/blob/master/License.md) |
| [Extended WPF Toolkit](https://github.com/xceedsoftware/wpftoolkit) 		| [Microsoft Public License](https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md) | 
| [WriteableBitmapEx](https://github.com/teichgraf/WriteableBitmapEx) 		| [MIT License](https://github.com/teichgraf/WriteableBitmapEx/blob/master/LICENSE) |