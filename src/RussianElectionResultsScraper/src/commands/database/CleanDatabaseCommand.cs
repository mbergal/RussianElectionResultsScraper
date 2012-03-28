using System.Linq;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace RussianElectionResultsScraper.Commands.Database
    {
    public class CleanDatabaseCommand : BaseCommand
        {
        public CleanDatabaseCommand( string connectionString )
            : base( connectionString: connectionString )
            {
            }

        public override int Execute()
            {
            var connection = new ServerConnection { ConnectionString = this._connectionString };
            connection.Connect();
            var serverObj = new Microsoft.SqlServer.Management.Smo.Server();
            var databaseObj = serverObj.Databases[connection.SqlConnectionObject.Database];
            var tables = databaseObj.Tables.Cast<Table>().Where( t => t.Owner == "dbo" ).ToArray();
            var views = databaseObj.Views.Cast<View>().Where( v => v.Owner == "dbo" ).ToArray();
            var storedProcedures = databaseObj.StoredProcedures.Cast<StoredProcedure>().Where( s => s.Owner == "dbo" ).ToArray();
            var userDefinedFunctions =
                databaseObj.UserDefinedFunctions.Cast<UserDefinedFunction>().Where( udf => udf.Owner == "dbo" ).ToArray();

            var foreignKeys = tables.SelectMany( x => x.ForeignKeys.Cast<ForeignKey>() ).ToArray();

            foreach ( var foreignKey in foreignKeys )
                foreignKey.Drop();

            foreach ( var userDefinedFunction in userDefinedFunctions )
                userDefinedFunction.Drop();

            foreach ( var storedProcedure in storedProcedures )
                storedProcedure.Drop();

            foreach ( var view in views )
                view.Drop();

            foreach ( var table in tables )
                table.Drop();
            
            return 0;
            }
        }
    }