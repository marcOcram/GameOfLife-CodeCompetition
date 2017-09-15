using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameOfLife
{
    public class RectangularLifeBoard : LifeBoard
    {
        private readonly Position[][] _neighborCache;

        #region Private Fields

        private readonly uint _height;

        private readonly uint _width;

        #endregion Private Fields

        #region Private Constructors

        private RectangularLifeBoard(uint width, uint height, IEnumerable<Position> alivePositions) :
            base(new LifeState[width, height])
        {
            _width = width;
            _height = height;

            ApplyChanges(alivePositions, new List<Position>());

            _neighborCache = CreateNeighborCache();
        }

        #endregion Private Constructors

        #region Public Methods

        public static RectangularLifeBoard Create(uint width, uint height)
        {
            return Create(width, height, new List<Position>());
        }

        public static RectangularLifeBoard Create(uint width, uint height, IEnumerable<Position> alivePositions)
        {
            return new RectangularLifeBoard(width, height, alivePositions);
        }

        public override LifeState[] GetNeighbors(Position position)
        {
            if (!(0 <= position.X && position.X < _width)) throw new ArgumentOutOfRangeException(nameof(position), $"Position not inside {nameof(LifeBoard)}!");
            if (!(0 <= position.Y && position.Y < _height)) throw new ArgumentOutOfRangeException(nameof(position), $"Position not inside {nameof(LifeBoard)}!");

            return GetLifeStates(_neighborCache[ToIndex(position)]);
        }

        //public override IReadOnlyDictionary<Position, LifeState> GetPositionedNeighbors(Position position)
        //{
        //    if (!(0 <= position.X && position.X < _width)) throw new ArgumentOutOfRangeException(nameof(position), $"Position not inside {nameof(LifeBoard)}!");
        //    if (!(0 <= position.Y && position.Y < _height)) throw new ArgumentOutOfRangeException(nameof(position), $"Position not inside {nameof(LifeBoard)}!");

        //    //if (!_neighborCache.ContainsKey(position)) {
        //    //    IEnumerable<Position> neighborPositions = RectangleNeighborHelper.GetNeighborPositions(position, TotalWidth, TotalHeight);
        //    //    _neighborCache.AddOrUpdate(position, neighborPositions, (key, oldValue) => neighborPositions);
        //    //}

        //    //var neighbors = _neighborCache.GetOrAdd(position, new Lazy<IEnumerable<Position>>(() => RectangleNeighborHelper.GetNeighborPositions(position, TotalWidth, TotalHeight)));

        //    //IEnumerable<Position> neighborPositions = RectangleNeighborHelper.GetNeighborPositions(position, TotalWidth, TotalHeight);

        //    return GetLifeStates2(_arrayCache[position.Y * _width + position.X]);
        //}

        #endregion Public Methods

        private Position[][] CreateNeighborCache()
        {
            Position[][] positions = new Position[_width * _height][];

            for (uint y = 0; y < _height; ++y) {
                for (uint x = 0; x < _width; ++x) {
                    Position currentPosition = new Position(x, y);
                    positions[y * _width + x] = RectangleNeighborHelper.GetNeighborPositions(currentPosition, TotalWidth, TotalHeight).ToArray();
                }
            }

            return positions;
        }
    }
}