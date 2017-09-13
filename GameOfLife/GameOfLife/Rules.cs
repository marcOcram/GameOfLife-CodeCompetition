using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GameOfLife
{
    public class Rules
    {
        #region Private Fields

        private readonly Regex _rulePattern = new Regex(@"^(?<alive>\d+)\/(?<birth>\d+)$");

        #endregion Private Fields

        #region Public Constructors

        public Rules()
        {
        }

        public Rules(IEnumerable<int> alive, IEnumerable<int> birth)
        {
            Alive = alive ?? throw new ArgumentNullException(nameof(alive));
            Birth = birth ?? throw new ArgumentNullException(nameof(birth));
        }

        public Rules(string rules)
        {
            if (rules == null) {
                throw new ArgumentNullException(nameof(rules));
            }

            var matches = _rulePattern.Match(rules);

            if (!matches.Success) {
                throw new ArgumentException(@"Invalid format! Expected ^\d+/\d+$ (e. g. 23/3)", nameof(rules));
            }
            
            string aliveDescription = matches.Groups["alive"].Value;
            Alive = ParseDescription(aliveDescription);
            
            string birthDescription = matches.Groups["birth"].Value;
            Birth = ParseDescription(birthDescription);
        }

        #endregion Public Constructors

        #region Public Properties

        public IEnumerable<int> Alive { get; } = new HashSet<int>() { 2, 3 };

        public IEnumerable<int> Birth { get; } = new HashSet<int>() { 3 };

        #endregion Public Properties

        #region Private Methods

        private IEnumerable<int> ParseDescription(string description)
        {
            HashSet<int> container = new HashSet<int>();

            for (int index = 0; index < description.Length; ++index) {
                container.Add(Convert.ToInt32(description[index].ToString()));
            }

            return container;
        }

        #endregion Private Methods
    }
}