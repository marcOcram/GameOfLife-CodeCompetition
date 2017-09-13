using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    public class CuboidLifeBoard : LifeBoard
    {
        private readonly ILookup<Position, Position> _edgeMapping;
        private readonly uint _width;
        private readonly uint _height;
        private readonly uint _depth;

        private CuboidLifeBoard(uint width, uint height, uint depth, LifeState[,] lifeBoard) :
            base(lifeBoard)
        {
            _width = width;
            _height = height;
            _depth = depth;

            List<Tuple<Position, Position>> edgeMapping = new List<Tuple<Position, Position>>();

            /*
             * A normal dice has six sides according to the image. To get the neighbors of the edges we use a lookup table.
             * 
             * Le => Left Side
             * Fr => Front Side
             * Ri => Right Side
             * Ba => Back Side
             * To => Top Side
             * Bo => Bottom Side
             * 
             *      ┌────┐
             *      │ To │
             * ┌────┼────┼────┬────┐
             * │ Le │ Fr │ Ri │ Ba │
             * └────┼────┼────┴────┘
             *      │ Bo │
             *      └────┘
             * 
             * We have 14 different edges where the neighbors can't be calculated like with a normal rectangle.
             * For the calculation of the neighbors the lifeboard is enlarged by 2 in every direction and the 
             * dice is moved one down and 1 to the right. This results in calculating the neighbors with virtual
             * coordinates instead of the real coordinates.
             * 
             * 
             *        0 1 2 3 4 5 6 7 8 9 A B    <-- real x-coordinates
             *      0 1 2 3 4 5 6 7 8 9 A B C D  <-- virtual x-coordinates
             * 
             *   0          B B B  
             * 0 1        1 █ █ █ 2
             * 1 2        1 █ █ █ 2
             * 2 3    5 5 \ █ █ █ / 6 6 9 9 9  
             * 3 4  D █ █ █ █ █ █ █ █ █ █ █ █ E
             * 4 5  D █ █ █ █ █ █ █ █ █ █ █ █ E
             * 5 6  D █ █ █ █ █ █ █ █ █ █ █ █ E
             * 6 7    7 7 / █ █ █ \ 8 8 A A A  
             * 7 8        3 █ █ █ 4
             * 8 9        3 █ █ █ 4
             * | A          C C C
             * | |
             * | └─ virtual y-coordinates  
             * └─── real y-coordinates
             */

            for (uint i = 0; i < depth; ++i) {
                // 1
                Position firstEdgePosition = new Position(depth, i + 1);
                edgeMapping.Add(Tuple.Create(firstEdgePosition, new Position(i + 1, depth + 1)));
                // 2
                Position secondEdgePosition = new Position(depth + width + 1, i + 1);
                edgeMapping.Add(Tuple.Create(secondEdgePosition, new Position(2 * depth + width - i, depth + 1)));
                // 3
                Position thirdEdgePosition = new Position(depth, depth + height + i + 1);
                edgeMapping.Add(Tuple.Create(thirdEdgePosition, new Position(depth - i, depth + height)));
                // 4
                Position fourthEdgePosition = new Position(depth + width + 1, depth + height + i + 1);
                edgeMapping.Add(Tuple.Create(fourthEdgePosition, new Position(depth + width + i + 1, depth + width)));
                // 5
                Position fifthEdgePosition = new Position(i + 1, depth);
                edgeMapping.Add(Tuple.Create(fifthEdgePosition, new Position(depth + 1, i + 1)));
                // 6
                Position sixthEdgePosition = new Position(depth + width + i + 1, depth);
                edgeMapping.Add(Tuple.Create(sixthEdgePosition, new Position(depth + width, depth - i)));
                // 7
                Position seventhEdgePosition = new Position(i + 1, depth + height + 1);
                edgeMapping.Add(Tuple.Create(seventhEdgePosition, new Position(depth + 1, 2 * depth + height - i)));
                // 8
                Position eigthEdgePosition = new Position(depth + width + i + 1, depth + height + 1);
                edgeMapping.Add(Tuple.Create(eigthEdgePosition, new Position(depth + width, depth + height + i + 1)));
            }

            for (uint i = 0; i < width; ++i) {
                // 9
                Position ninthEdgePosition = new Position(2 * depth + width + i + 1, depth);
                edgeMapping.Add(Tuple.Create(ninthEdgePosition, new Position(depth + width - i, 1)));
                // A
                Position tenthEdgePosition = new Position(2 * depth + width + i + 1, depth + height + 1);
                edgeMapping.Add(Tuple.Create(tenthEdgePosition, new Position(depth + width - i, 2 * depth + height)));
                // B
                Position eleventhEdgePosition = new Position(depth + i + 1, 0);
                edgeMapping.Add(Tuple.Create(eleventhEdgePosition, new Position(2 * depth + 2 * width - i, depth + 1)));
                // C
                Position twelthEdgePosition = new Position(depth + i + 1, 2 * depth + height + 1);
                edgeMapping.Add(Tuple.Create(twelthEdgePosition, new Position(2 * depth + 2 * width - i, depth + height)));
            }

            for (uint i = 0; i < height; ++i) {
                // D
                Position thirteenthEdgePosition = new Position(0, depth + i + 1);
                edgeMapping.Add(Tuple.Create(thirteenthEdgePosition, new Position(2 * depth + 2 * width, depth + i + 1)));
                // E
                Position fourteenthEdgePosition = new Position(2 * depth + 2 * width + 1, depth + i + 1);
                edgeMapping.Add(Tuple.Create(fourteenthEdgePosition, new Position(1, depth + i + 1)));
            }
            
            foreach (var tuple in edgeMapping) {
                Debug.WriteLine($"{tuple.Item1} => {tuple.Item2}");
            }

            _edgeMapping = edgeMapping.ToLookup(g => g.Item1, g => g.Item2);
        }

        public static CuboidLifeBoard Create(uint width, uint height, uint depth, IEnumerable<Position> alivePositions)
        {
            if (width < 1) throw new ArgumentOutOfRangeException(nameof(width), "Cannot be lower 1!");
            if (height < 1) throw new ArgumentOutOfRangeException(nameof(height), "Cannot be lower 1!");
            if (depth < 1) throw new ArgumentOutOfRangeException(nameof(height), "Cannot be lower 1!");

            uint lifeBoardWidth = 2 * width + 2 * depth;
            uint lifeBoardHeight = 2 * depth + height;

            LifeState[,] lifeBoard = new LifeState[lifeBoardWidth, lifeBoardHeight];
            for (uint hIndex = 0; hIndex < lifeBoardWidth; ++hIndex) {
                for (uint vIndex = 0; vIndex < lifeBoardHeight; ++vIndex) {
                    if (!IsLifePossible(hIndex, vIndex, width, height, depth)) {
                        lifeBoard[hIndex, vIndex] = LifeState.NoLifePossible;
                    }
                }
            }

            foreach (var alivePosition in alivePositions) {
                if (lifeBoard[alivePosition.X, alivePosition.Y] != LifeState.NoLifePossible) {
                    lifeBoard[alivePosition.X, alivePosition.Y] = LifeState.Alive;
                }
            }

            return new CuboidLifeBoard(width, height, depth, lifeBoard);
        }

        public override IReadOnlyDictionary<Position, LifeState> GetNeighbors(Position position)
        {
            if (!IsLifePossible(position.X, position.Y, _width, _height, _depth)) throw new ArgumentOutOfRangeException(nameof(position), "Life is for position impossible");

            Position virtualPosition = new Position(position.X + 1, position.Y + 1);

            // by adding a height of two and and moving the Y position one down, we can simulate an empty row above and below the dice fields.
            // So every neighbor on Y-axis 0 or ((3 * _edgeLength) - 1) has to be remapped.
            IEnumerable<Position> virtualNeighborPositions = RectangleNeighborHelper.GetNeighborPositions(virtualPosition, TotalWidth + 2, TotalHeight + 2);

            List<Position> realNeighborPositions = new List<Position>();
            foreach (var virtualNeighborPosition in virtualNeighborPositions) {
                if (_edgeMapping.Contains(virtualNeighborPosition)) {
                    realNeighborPositions.AddRange(_edgeMapping[virtualNeighborPosition].Select(vp => new Position(vp.X - 1, vp.Y - 1)));
                } else if (virtualNeighborPosition.X != 0 && virtualNeighborPosition.X != (2 * _depth + 2 * _width) + 1 && virtualNeighborPosition.Y != 0 && virtualNeighborPosition.Y != (2 * _depth + _height) + 1) {
                    realNeighborPositions.Add(new Position(virtualNeighborPosition.X - 1, virtualNeighborPosition.Y - 1));
                }
            }
            
            return GetLifeStates(realNeighborPositions.Distinct()).Where(n => n.Value == LifeState.Alive || n.Value == LifeState.Dead).ToDictionary(n => n.Key, n => n.Value);
        }

        private static bool IsLifePossible(uint x, uint y, uint width, uint height, uint depth)
        {
            return (depth <= x && x < (depth + width)) || (depth <= y && y < (depth + height));

            //if ((x < edgeLength && (y < edgeLength || (y >= edgeLength * 2 && y < height))) || (x >= edgeLength * 2 && x < width && (y < edgeLength || (y >= edgeLength * 2 && y < height)))) {
            //    return false;
            //}

            //return true;
        }
    }
}
