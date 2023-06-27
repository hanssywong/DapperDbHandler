using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperDbHandler
{
    public class SqlCommandContainer
    {
        public string Sql { get; }
        public object? Parameters { get; }
        public SqlCommandContainer(string sql, object? parameters = null)
        {
            Sql = sql;
            Parameters = parameters;
        }
    }
}
