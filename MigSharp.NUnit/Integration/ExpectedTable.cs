using System;
using System.Collections.Generic;
using System.Linq;

namespace MigSharp.NUnit.Integration
{
    internal class ExpectedTable : List<object[]>
    {
        private readonly string _name;
        private readonly List<string> _columns;

        public string Name { get { return _name; } }
        public List<string> Columns { get { return _columns; } }

        public ExpectedTable(string name, IEnumerable<string> columnNames)
        {
            _name = name;
            _columns = new List<string>(columnNames);
        }

        public ExpectedTable(string name, string columnName1, params string[] columnNames)
            : this(name, new[] { columnName1 }.Concat(columnNames))
        {
        }

        public new void Add(params object[] values)
        {
            if (values.Length != Columns.Count) throw new ArgumentException("The provided number of values does not match number of columns.");

            base.Add(values);
        }

        /// <summary>
        /// Gets an expected value.
        /// </summary>
        public object Value(int row, int column)
        {
            return this[row][column];
        }
    }
}