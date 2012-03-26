using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace RussianElectionResultsScraper.BulkCopy
    {
    public class SendDatabaseDataCommand : BaseConsoleCommand
        {
        private string _destination;

        public SendDatabaseDataCommand()
            {
            this.IsCommand( "database:send-data", "Send database data" );
            this.HasConnectionOption();
            this.HasProviderOption();
            this.HasRequiredOption("d|destination=", "connection string or url", destination => this._destination = destination );            
            }

        public override int Run(string[] args)
            {
            var connection = new ServerConnection { ConnectionString = this._connectionString };
            connection.Connect();
            var serverObj = new Microsoft.SqlServer.Management.Smo.Server();
            var databaseObj = serverObj.Databases[connection.SqlConnectionObject.Database];
            var tables = databaseObj.Tables.Cast<Table>().Where( t => t.Owner == "dbo" ).ToArray();
            var destination = new SendDataDestinationFactory().Create( this._destination );

            destination.BeginReceiving();
            tables.AsParallel().SelectMany( this.SendTable ).ForAll( destination.ReceiveBlock );
            destination.EndReceiving();
            return 0;
            }

        private DataTable NewDataTableFromReader( IDataReader reader, out object[] values, string tableName )
            {
            var dataTable = new DataTable( tableName ) {Locale = CultureInfo.InvariantCulture};
            foreach (DataRow dataRow in (InternalDataCollectionBase) reader.GetSchemaTable().Rows)
            dataTable.Columns.Add(dataRow["ColumnName"] as string, (Type) dataRow["DataType"]);
            values = new object[dataTable.Columns.Count];
            return dataTable;
            }

        private IEnumerable<byte[]> SendTable( Table table )
            {
            using ( var conn = new SqlConnection( this._connectionString ) )
                {
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = "select * from " + table.Name;
                const int MaxBatchSize = 100*1024;
                int rowCount = 0;
                int nextCheckOn = 1;
                using ( var reader = command.ExecuteReader() )
                    {
                    object[] values;
                    var dt = this.NewDataTableFromReader( reader, out values, table.Name );
                    dt.RemotingFormat = SerializationFormat.Binary;

                    while ( reader.Read() )
                        {
                        rowCount++;
                        reader.GetValues( values );
                        dt.LoadDataRow( values, true );
                        if (dt.Rows.Count == nextCheckOn )
                            {
                            var block = this.DataTableToMemoryStream( dt ).ToArray();
                            if (block.Length > MaxBatchSize)
                                {
                                Console.WriteLine( "Sending {0} rows ({1} bytes) of table {2}. Sent overall {3} rows.", dt.Rows.Count, block.Length, table.Name, rowCount );
                                yield return block;
                                dt = this.NewDataTableFromReader( reader, out values, table.Name );
                                nextCheckOn = 1;
                                }
                            else
                                nextCheckOn *= 2;
                            }
                        }

                    yield return this.DataTableToMemoryStream( dt ).ToArray();
                    }
                }
            }

        private MemoryStream DataTableToMemoryStream( DataTable dt )
            {
            var serializationFormatter = new BinaryFormatter();
            using (var result = new MemoryStream())
                {
                using ( var m = new GZipStream( result, CompressionMode.Compress ) )
                    serializationFormatter.Serialize( m, dt );
                return result;
                }
            }
        }


    }