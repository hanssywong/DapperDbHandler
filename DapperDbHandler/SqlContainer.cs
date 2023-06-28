using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDbHandler
{
    public record SqlContainer
    {
        public string Sql { get; }
        public ImmutableDictionary<string, object?> Parameters { get; }
        public SqlContainer(string sql, ImmutableDictionary<string, object?> parameters)
        {
            Sql = sql;
            Parameters = parameters;
        }
    }
}
