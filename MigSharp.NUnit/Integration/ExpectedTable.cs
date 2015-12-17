using System;
using System.Collections.Generic;
using System.Linq;
using MigSharp.Core;

namespace MigSharp.NUnit.Integration
{
    internal class ExpectedTable : List<object[]>
    {
        private readonly TableName _fullName;
        private readonly List<string> _columns;

        public string Name { get { return FullName.Name; } }
        public List<string> Columns { get { return _columns; } }
        public TableName FullName { get { return _fullName; } }

        public ExpectedTable(TableName fullName, IEnumerable<string> columnNames)
        {
            _fullName = fullName;
            _columns = new List<string>(columnNames);
        }

        public ExpectedTable(TableName fullName, string columnName1, params string[] columnNames)
            : this(fullName, new[] { columnName1 }.Concat(columnNames))
        {
        }

        public ExpectedTable(string name, IEnumerable<string> columnNames)
            : this(new TableName(name, null), columnNames)
        {
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