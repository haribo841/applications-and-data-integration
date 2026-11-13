using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace TreeCrud.Tests
{
    internal sealed class FakeDatabase
    {
        internal List<KeyValuePair<int, string>> Rows { get; } = new List<KeyValuePair<int, string>>();
        internal List<ExecutedCommand> Commands { get; } = new List<ExecutedCommand>();
        internal Func<ExecutedCommand, int> Execute { get; set; } = command => 1;
        internal int Commits { get; private set; }
        internal int Rollbacks { get; private set; }

        internal IDbConnection Connect() => new Connection(this);

        internal void Add(int id, params string[] paths)
        {
            Rows.AddRange(paths.Select(path => new KeyValuePair<int, string>(id, path)));
        }

        private sealed class Connection : IDbConnection
        {
            private readonly FakeDatabase database;
            internal Connection(FakeDatabase database) { this.database = database; }
            public string ConnectionString { get; set; } = "";
            public int ConnectionTimeout => 0;
            public string Database => "Tests";
            public ConnectionState State { get; private set; }
            public void Open() { State = ConnectionState.Open; }
            public void Close() { State = ConnectionState.Closed; }
            public void Dispose() { Close(); }
            public void ChangeDatabase(string databaseName) { throw new NotSupportedException(); }
            public IDbCommand CreateCommand() => new Command(database, this);
            public IDbTransaction BeginTransaction() => new Transaction(database, this);
            public IDbTransaction BeginTransaction(IsolationLevel isolationLevel) => BeginTransaction();
        }

        private sealed class Transaction : IDbTransaction
        {
            private readonly FakeDatabase database;
            internal Transaction(FakeDatabase database, IDbConnection connection)
            {
                this.database = database;
                Connection = connection;
            }
            public IDbConnection Connection { get; }
            public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
            public void Commit() { database.Commits++; }
            public void Rollback() { database.Rollbacks++; }
            public void Dispose() { }
        }

        private sealed class Command : IDbCommand
        {
            private readonly FakeDatabase database;
            private readonly SqlCommand parameters = new SqlCommand();
            internal Command(FakeDatabase database, IDbConnection connection)
            {
                this.database = database;
                Connection = connection;
            }
            public string CommandText { get; set; }
            public int CommandTimeout { get; set; }
            public CommandType CommandType { get; set; }
            public IDbConnection Connection { get; set; }
            public IDataParameterCollection Parameters => parameters.Parameters;
            public IDbTransaction Transaction { get; set; }
            public UpdateRowSource UpdatedRowSource { get; set; }
            public void Cancel() { throw new NotSupportedException(); }
            public void Prepare() { throw new NotSupportedException(); }
            public object ExecuteScalar() { throw new NotSupportedException(); }
            public void Dispose() { parameters.Dispose(); }
            public IDbDataParameter CreateParameter() => parameters.CreateParameter();

            public int ExecuteNonQuery()
            {
                var command = new ExecutedCommand(CommandText,
                    parameters.Parameters.Cast<SqlParameter>().ToDictionary(parameter => parameter.ParameterName, parameter => parameter.Value),
                    Transaction != null);
                database.Commands.Add(command);
                return database.Execute(command);
            }

            public IDataReader ExecuteReader()
            {
                var table = new DataTable();
                table.Columns.Add("DAE_DeAId", typeof(int));
                table.Columns.Add("DAE_No", typeof(int));
                table.Columns.Add("DAE_Value", typeof(string));
                foreach (var row in database.Rows) table.Rows.Add(row.Key, table.Rows.Count + 1, row.Value);
                return table.CreateDataReader();
            }

            public IDataReader ExecuteReader(CommandBehavior behavior) => ExecuteReader();
        }
    }

    internal sealed class ExecutedCommand
    {
        internal ExecutedCommand(string sql, Dictionary<string, object> parameters, bool inTransaction)
        {
            Sql = sql;
            Parameters = parameters;
            InTransaction = inTransaction;
        }
        internal string Sql { get; }
        internal Dictionary<string, object> Parameters { get; }
        internal bool InTransaction { get; }
    }
}
