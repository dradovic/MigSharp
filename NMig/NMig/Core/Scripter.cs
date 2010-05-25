using System;

using NMig.Commands;
using NMig.Providers;

namespace NMig.Core
{
    internal class Scripter
    {
        private readonly IProvider _provider;

        public Scripter(IProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        ///   Translates the changes recorded on the <paramref name = "database" /> to a SQL script.
        /// </summary>
        public string Script(Database database)
        {
            string sql = string.Empty;
            foreach (Command command in database.GetCommands())
            {
                var rename = command as Rename;
                if (rename != null)
                {
                    sql += ScriptRename(rename);
                }
            }
            return sql;
        }

        private string ScriptRename(Rename rename)
        {
            if (rename.Target is Table)
            {
                return _provider.RenameTable(rename.Target.Name, rename.NewName);
            }
            else if (rename.Target is Column)
            {
                return _provider.RenameColumn(rename.Target.Name, rename.NewName);                
            }
            throw new NotImplementedException();
        }
    }
}