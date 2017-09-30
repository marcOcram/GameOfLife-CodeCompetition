using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace GameOfLife
{
    /// <summary>
    /// Contains the rules for a game of life.
    /// </summary>
    [DataContract(Name = nameof(Rules))]
    public class Rules
    {
        #region Private Fields

        /// <summary>
        /// The pattern to determine whether a given rule description is structurally valid.
        /// </summary>
        private readonly static Regex _rulePattern = new Regex(@"^(?<alive>\d+)\/(?<birth>\d+)$");

        /// <summary>
        /// A set of n neighbors which are needed to keep a habitant alive.
        /// </summary>
        [DataMember(Name = "Alive")]
        private readonly HashSet<int> _alive = new HashSet<int>() { 2, 3 };

        /// <summary>
        /// A set of n neighbors which are needed to revive a habitant.
        /// </summary>
        [DataMember(Name = "Birth")]
        private readonly HashSet<int> _birth = new HashSet<int>() { 3 };

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Rules"/> class.
        /// </summary>
        /// <param name="alive">An enumeration of the amount of neighbors which are needed to keep an habitant alive.</param>
        /// <param name="birth">An enumeration of the amount of neighbors which are needed to revive an habitant.</param>
        /// <exception cref="ArgumentNullException">
        /// alive
        /// or
        /// birth
        /// </exception>
        public Rules(IEnumerable<int> alive, IEnumerable<int> birth)
        {
            if (alive == null) {
                throw new ArgumentNullException(nameof(alive));
            }

            if (birth == null) {
                throw new ArgumentNullException(nameof(birth));
            }

            _alive = new HashSet<int>(alive);
            _birth = new HashSet<int>(birth);

            RuleDescription = CreateRuleDescription(_alive, _birth);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rules"/> class.
        /// </summary>
        /// <param name="rules">The rules in format \d+\/\d+. Valid ones are: 23/3, 1/1, 1234567/52486, 2233/6655; Invalid: /3, 3/ 12a/32</param>
        /// <exception cref="ArgumentNullException">rules</exception>
        /// <exception cref="ArgumentException">Invalid format! Expected ^\d+/\d+$ where each digit can only appear once! (e. g. 23/3) - rules</exception>
        public Rules(string rules)
        {
            if (rules == null) {
                throw new ArgumentNullException(nameof(rules));
            }

            if (!IsValid(rules)) {
                throw new ArgumentException(@"Invalid format! Expected ^\d+/\d+$ where each digit can only appear once! (e. g. 23/3)", nameof(rules));
            }

            var matches = _rulePattern.Match(rules);

            string aliveDescription = matches.Groups["alive"].Value;
            _alive = ParseDescription(aliveDescription);

            string birthDescription = matches.Groups["birth"].Value;
            _birth = ParseDescription(birthDescription);

            RuleDescription = CreateRuleDescription(_alive, _birth);
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets an enumeration of the amount of neighbors which are needed to keep an habitant alive.
        /// </summary>
        /// <value></value>
        public IEnumerable<int> Alive {
            get { return _alive; }
        }

        /// <summary>
        /// Gets an enumeration of the amount of neighbors which are needed to revive an habitant.
        /// </summary>
        /// <value></value>
        public IEnumerable<int> Birth {
            get { return _birth; }
        }

        /// <summary>
        /// Gets the rule description.
        /// </summary>
        /// <value>
        /// The rule description.
        /// </value>
        public string RuleDescription { get; private set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Returns true if the given rule description is valid.
        /// </summary>
        /// <param name="ruleDescription">The rule description.</param>
        /// <returns>
        ///   <c>true</c> if the specified rule description is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValid(string ruleDescription)
        {
            if (string.IsNullOrEmpty(ruleDescription)) {
                return false;
            }

            if (_rulePattern.IsMatch(ruleDescription)) {
                return ruleDescription.Split('/').All(s => s.GroupBy(c => c).All(g => g.Count() == 1));
            }

            return false;
        }

        public override string ToString() => RuleDescription;

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Creates the rule description.
        /// </summary>
        /// <param name="alive">An enumeration of the amount of neighbors which are needed to keep an habitant alive.</param>
        /// <param name="birth">An enumeration of the amount of neighbors which are needed to revive an habitant.</param>
        /// <returns></returns>
        private static string CreateRuleDescription(IEnumerable<int> alive, IEnumerable<int> birth)
        {
            return string.Join("", alive.OrderBy(v => v)) + "/" + string.Join("", birth.OrderBy(v => v));
        }

        /// <summary>
        /// Called when an instance has finished deserializing.
        /// </summary>
        /// <param name="streamingContext">The streaming context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext streamingContext)
        {
            RuleDescription = CreateRuleDescription(_alive, _birth);
        }

        /// <summary>
        /// Parses the description to an <see cref="HashSet{T}"/>. Turns '233' into HashSet&lt;int&gt;(Count = 2) {2, 3}
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        private HashSet<int> ParseDescription(string description)
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