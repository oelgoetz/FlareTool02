using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ConsoleApp1
{
    class DBUtils
    {
        public static SqlConnection GetDBConnection()
        {
            string datasource = @"tran-vmware\SQLEXPRESS";

            string database = "docuDB01";
            string username = "sa";
            string password = "sa";

            return DBSQLServerUtils.GetDBConnection(datasource, database, username, password);
        }

        // Create a Command with 2 parameter: Command Text & Connection.
        public static void DoSomethingIntheDatabase(string sql, SqlConnection conn) 
        {            
            // Create command.
            SqlCommand cmd = new SqlCommand();

            // Set connection for Command.
            cmd.Connection = conn;
            cmd.CommandText = sql;
        }
    }
}
