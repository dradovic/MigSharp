using System.Collections.Generic;
using System.Diagnostics;

using MigSharp.Core.Commands;
using MigSharp.Providers;

namespace MigSharp.Core
{
    internal class Scripter
    {
        private readonly IProvider _provider;

        public Scripter(IProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Translates the changes recorded on the <paramref name = "database" /> to a SQL script.
        /// </summary>
        public IEnumerable<string> GetCommandTexts(Database database)
        {
            Visitor visitor = new Visitor(_provider);
            return visitor.Visit(database.Root);
        }

        private class Visitor
        {
            private readonly IProvider _provider;

            public Visitor(IProvider provider)
            {
                _provider = provider;
            }

            public IEnumerable<string> Visit(ICommand root)
            {
                return Visit(new Stack<ICommand>(), root);
            }

            private IEnumerable<string> Visit(Stack<ICommand> parentNodes, ICommand command)
            {
                //PreProcess(command, parentNodes);
                parentNodes.Push(command);
                foreach (ICommand child in command.Children)
                {
                    foreach (string ct in Visit(parentNodes, child))
                    {
                        yield return ct;
                    }
                }
                parentNodes.Pop();
                string commandText = PostProcess(command, parentNodes);
                if (!string.IsNullOrEmpty(commandText))
                {
                    yield return commandText;
                }
                else
                {
                    yield break;
                }
            }

            //private void PreProcess(ChangeNode changeNode, Stack<ChangeNode> parentNodes)
            //{
            //}

            private string PostProcess(ICommand changeNode, Stack<ICommand> parentNodes)
            {
                AlterTableCommand alterTableCommand = changeNode as AlterTableCommand;
                if (alterTableCommand != null && alterTableCommand.AddColumnCommands.Count > 0)
                {
                    return _provider.AddColumns(alterTableCommand.TableName,
                        alterTableCommand.AddColumnCommands.ConvertAll(c => new AddedColumn(c.Name, c.Type, c.IsNullable, c.DefaultValue, c.Options)));
                }

                RenameCommand renameCommand = changeNode as RenameCommand;
                if (renameCommand != null)
                {
                    Debug.Assert(parentNodes.Count > 0);
                    ICommand parent = parentNodes.Peek();
                    AlterTableCommand parentAlterTableCommand;
                    AlterColumnCommand parentAlterColumnCommand;
                    if ((parentAlterTableCommand = parent as AlterTableCommand) != null)
                    {
                        return _provider.RenameTable(parentAlterTableCommand.TableName, renameCommand.NewName);
                    }
                    else if ((parentAlterColumnCommand = parent as AlterColumnCommand) != null)
                    {
                        return _provider.RenameColumn(parentAlterColumnCommand.ColumnName, renameCommand.NewName);
                    }
                    else
                    {
                        Debug.Assert(false, "The parent command of a RenameNode should either be a alterColumnCommand or a alterTableCommand.");
                    }
                }

                return null;
            }
        }
    }
}