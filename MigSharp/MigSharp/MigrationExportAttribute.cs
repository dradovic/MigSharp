using System;
using System.ComponentModel.Composition;
using System.Globalization;

namespace MigSharp
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MigrationExportAttribute : ExportAttribute
    {
        public const int MaximumModuleNameLength = 250;

        private readonly int _year;
        private readonly int _month;
        private readonly int _day;
        private readonly int _hour;
        private readonly int _minute;
        private readonly int _second;

        public int Year { get { return _year; } }
        public int Month { get { return _month; } }
        public int Day { get { return _day; } }
        public int Hour { get { return _hour; } }
        public int Minute { get { return _minute; } }
        public int Second { get { return _second; } }

        private string _moduleName = string.Empty;
        public string ModuleName
        {
            get { return _moduleName; }
            set
            {
                if (value != null && value.Length > MaximumModuleNameLength) throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The provided module name exceeds the maximum length of {0}.", MaximumModuleNameLength));
                _moduleName = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public string Tag { get; set; }

        public MigrationExportAttribute(int year, int month, int day, int hour, int minute, int second)
            : base(typeof(IMigration))
        {
            _year = year;
            _month = month;
            _day = day;
            _hour = hour;
            _minute = minute;
            _second = second;
        }
    }
}