using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;
using GameOfLifeWPF.Properties;
using System.Windows.Data;

namespace GameOfLifeWPF
{
    internal sealed class SettingsBinding : Binding
    {
        #region Public Constructors

        public SettingsBinding() :
            this(null)
        {
        }

        public SettingsBinding(string path) :
            base(path)
        {
            Source = Settings.Default;
            Mode = BindingMode.TwoWay;
        }

        #endregion Public Constructors
    }
}