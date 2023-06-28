using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDbHandler
{
    public record SqlContainer4Dapper
    {
        public string Sql { get; }
        public object? Parameters { get; }
        public SqlContainer4Dapper(string sql, object? parameters = null)
        {
            Sql = sql;
            Parameters = parameters;
        }
    }
}
