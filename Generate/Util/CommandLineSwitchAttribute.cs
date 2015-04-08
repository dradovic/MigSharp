using System;

namespace MigSharp.Generate.Util
{
    /// <summary>
    /// Implements a basic command-line switch by taking the
    /// switching name and the associated description.
    /// </summary>
    /// <remark>
    /// Only currently is implemented for properties, so all
    /// auto-switching variables should have a get/set method supplied.
    /// From: http://www.codeproject.com/KB/recipes/commandlineparser.aspx, by Ray Hayes
    /// </remark>
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class CommandLineSwitchAttribute : Attribute
    {
        #region Private Variables

        private readonly string m_name = "";
        private readonly bool m_required;
        private readonly string m_description = "";

        #endregion

        #region Public Properties

        /// <summary>Accessor for retrieving the switch-name for an associated
        /// property.</summary>
        public string Name { get { return m_name; } }

        /// <summary>
        /// Denotes if an argument is required.
        /// </summary>
        public bool Required { get { return m_required; } }

        /// <summary>Accessor for retrieving the description for a switch of
        /// an associated property.</summary>
        public string Description { get { return m_description; } }

        #endregion

        #region Constructors

        /// <summary>Attribute constructor.</summary>
        public CommandLineSwitchAttribute(string name, bool required, string description)
        {
            m_name = name;
            m_required = required;
            m_description = description;
        }

        #endregion
    }

    /// <summary>
    /// This class implements an alias attribute to work in conjunction
    /// with the <see cref="CommandLineSwitchAttribute">CommandLineSwitchAttribute</see>
    /// attribute.  If the CommandLineSwitchAttribute exists, then this attribute
    /// defines an alias for it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CommandLineAliasAttribute : Attribute
    {
        #region Private Variables

        private readonly string m_Alias = "";

        #endregion

        #region Public Properties

        public string Alias { get { return m_Alias; } }

        #endregion

        #region Constructors

        public CommandLineAliasAttribute(string alias)
        {
            m_Alias = alias;
        }

        #endregion
    }
}