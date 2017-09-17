using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace GameOfLifeWPF.Controls
{
    internal static class ColorProvider
    {
        #region Private Fields

        private static readonly Lazy<IEnumerable<ColorInfo>> _colors = new Lazy<IEnumerable<ColorInfo>>(GetColors);

        #endregion Private Fields

        #region Public Properties

        public static IEnumerable<ColorInfo> Colors => _colors.Value;

        #endregion Public Properties

        #region Public Methods

        private static IEnumerable<ColorInfo> GetColors()
        {
            return typeof(Colors).GetProperties().Select(pi => new ColorInfo(pi.Name, (Color)pi.GetValue(null, null)));
        }

        #endregion Public Methods
    }
}