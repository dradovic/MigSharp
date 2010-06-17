using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class CustomQuery : ICustomQuery
    {
        private readonly CustomQueryCommand _parent;

        public CustomQuery(CustomQueryCommand parent)
        {
            _parent = parent;
        }

        public void IfUsing(string providerInvariantName)
        {
            _parent.IfUsing = providerInvariantName;
        }
    }
}