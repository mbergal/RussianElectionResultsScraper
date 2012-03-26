using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using log4net;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using RussianElectionResultsScraper.Commons;

namespace RussianElectionResultsScraper.BulkCopy
    {
    public class SqlServerDatabaseDataDestination : ISendDataDestination
        {
        private static readonly ILog log = LogManager.GetLogger( "SqlServerDatabaseDataDestination" );
        private readonly string _connectionString;

        public SqlServerDatabaseDataDestination( string connectionString )
            {
            this._connectionString = connectionString;
            }

        public string[] GetTableNames()
            {
            var connection = new ServerConnection { ConnectionString = this._connectionString };
            var serverObj = new Microsoft.SqlServer.Management.Smo.Server();
            var databaseObj = serverObj.Databases[connection.SqlConnectionObject.Database];
            return databaseObj.Tables.Cast<Table>().Where( t => t.Owner == "dbo" ).Select( x => x.Name ).ToArray();
            }

        public void BeginReceiving()
            {
            log.Info( string.Format( "BeginReceiving" ), () =>
                {
                this.CleanTables();
                this.EnableConstraints( false );
                } );
            }

        private void EnableConstraints( bool enable )
            {
            using (var conn = new SqlConnection(this._connectionString) )
                {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandTimeout = int.MaxValue;
                foreach ( var t in this.GetTableNames() )
                    {
                    command.CommandText = "ALTER TABLE " + t + " NOCHECK CONSTRAINT ALL";
                    command.ExecuteNonQuery();
                    }
                }
            }

        private void CleanTables()
            {
            var connection = new ServerConnection { ConnectionString = this._connectionString };
            var serverObj = new Microsoft.SqlServer.Management.Smo.Server();
            var databaseObj = serverObj.Databases[connection.SqlConnectionObject.Database];

            var foreignKeys = databaseObj.Tables.Cast<Table>().SelectMany( x => x.ForeignKeys.Cast<ForeignKey>() ).ToArray();
            var script =
                foreignKeys.Select( x => x.Script( new ScriptingOptions() {ScriptDrops = true} ) ).SelectMany( x=>x.Cast<string>() )
                    .Union( databaseObj.Tables.Cast<Table>().Select( x=> "truncate table " + x.Name ) )
                    .Union(
                        foreignKeys.Select( x => x.Script( new ScriptingOptions() { DriForeignKeys = true, Indexes = true} ) ).SelectMany(
                            x => x.Cast<string>() )
                    ).ToArray();


            using (var conn = new SqlConnection(this._connectionString))
                {
                conn.Open();
                using (var transaction= conn.BeginTransaction())
                    {
                    var command = conn.CreateCommand();
                    command.Transaction = transaction;
                    script.ForEach( x =>
                                        {
                                        command.CommandText = x;
                                        command.ExecuteNonQuery();
                                        } );
                    transaction.Commit();
                    }
                }

            }

        public void ReceiveBlock( byte[] block )
            {
            log.Info( string.Format( "ReceiveBlock  {0}", block.Length ), () => 
                {
                using (var m = new MemoryStream( block ) )
                using (var g = new GZipStream( m, CompressionMode.Decompress ))
                    {
                    var dt = (DataTable) new BinaryFormatter().Deserialize( g );
                    using ( var conn = new SqlConnection( this._connectionString ) )
                        {
                        conn.Open();
                        new SqlBulkCopy( conn ) { DestinationTableName = dt.TableName }.WriteToServer( dt );
                        }
                    }
                } );
            }

        public void EndReceiving()
            {
            this.EnableConstraints( true );
            }
        }
    }