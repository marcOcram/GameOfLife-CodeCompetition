using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace GameOfLifeWPF.Controls
{
    /// <summary>
    /// Provides the available default colors with the given name.
    /// </summary>
    internal static class ColorProvider
    {
        #region Private Fields

        /// <summary>
        /// The default colors. Loaded in a lazy way.
        /// </summary>
        private static readonly Lazy<IEnumerable<ColorInfo>> _colors = new Lazy<IEnumerable<ColorInfo>>(GetColors);

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets the default colors.
        /// </summary>
        /// <value>
        /// The colors.
        /// </value>
        public static IEnumerable<ColorInfo> Colors => _colors.Value;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Gets the default colors from the <see cref="Colors"/> class.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<ColorInfo> GetColors()
        {
            return typeof(Colors).GetProperties().Select(pi => new ColorInfo(pi.Name, (Color)pi.GetValue(null, null)));
        }

        #endregion Public Methods
    }
}