using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class FileScripter : ISqlScripter, IDisposable
    {
        private readonly DirectoryInfo _targetDirectory;
        private readonly IProvider _provider;
        private readonly IProviderMetadata _providerMetadata;
        private readonly StreamWriter _writer;

        public FileScripter(DirectoryInfo targetDirectory, string stepName, IProvider provider, IProviderMetadata providerMetadata)
        {
            if (!targetDirectory.Exists) throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The directory '{0}' does not exist.", targetDirectory.FullName), "targetDirectory");

            _targetDirectory = targetDirectory;
            _provider = provider;
            _providerMetadata = providerMetadata;
            _writer = CreateWriter(stepName);
        }

        public void Script(IDbCommand command)
        {
            string sql = command.CommandText;
            foreach (IDataParameter parameter in command.Parameters)
            {
                string pattern = Regex.Escape(_providerMetadata.GetParameterSpecifier(parameter));
                int count = int.MaxValue;
                if (_providerMetadata.UsesPositionalParameters())
                {
                    count = 1;
                }
                else // provider supports full parameter names
                {
                    pattern += @"\b"; // make sure to not match "@foofoo" when looking for "@foo"
                }
                var regex = new Regex(pattern);
                sql = regex.Replace(sql, _provider.ConvertToSql(parameter.Value, parameter.DbType), count);
            }
            _writer.WriteLine(sql);

            // make sure that the commands are separated by an empty line (the scripting integration tests expect this
            // in order to be able to replay the commands individually)
            if (!sql.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                _writer.WriteLine();
            }
        }

        private StreamWriter CreateWriter(string name)
        {
            return File.CreateText(Path.Combine(_targetDirectory.FullName, name + ".sql"));
        }

        # region Disposing

        private bool _alreadyDisposed;

        ~FileScripter()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_alreadyDisposed) return;

            try
            {
                if (isDisposing)
                {
                    _writer.Dispose();
                }
            }
            finally
            {
                _alreadyDisposed = true;
            }
        }

        #endregion
    }
}