using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;
using GameOfLifeWPF.Properties;
using System.Windows.Data;

namespace GameOfLifeWPF
{
    /// <summary>
    /// A <see cref="Binding"/> bound to the application <see cref="Settings"/>.
    /// </summary>
    /// <seealso cref="System.Windows.Data.Binding" />
    internal sealed class SettingsBinding : Binding
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsBinding"/> class.
        /// </summary>
        public SettingsBinding() :
            this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsBinding"/> class.
        /// </summary>
        /// <param name="path">The initial <see cref="P:System.Windows.Data.Binding.Path" /> for the binding.</param>
        public SettingsBinding(string path) :
            base(path)
        {
            Source = Settings.Default;
            Mode = BindingMode.TwoWay;
        }

        #endregion Public Constructors
    }
}