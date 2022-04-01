using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTAComponents
{
    class DBSQLServerUtils
    {
        //athentification sql server
        public static SqlConnection GetDBConnection(string datasource, string database, string username, string password)
        {

            string connString = @"Data Source=" + datasource + ";Initial Catalog="
                        + database + ";Persist Security Info=True;User ID=" + username + ";Password=" + password;
            SqlConnection conn = new SqlConnection(connString);
            return conn;



        }

        //athentification windows
        //public static SqlConnection GetDBConnection(string datasource, string database)
        //{
        //    string connString = @"Persist Security Info = False; Trusted_Connection = True;  server = " + datasource + " Initial Catalog = " + database;
            
        //    SqlConnection conn = new SqlConnection(connString);
        //    return conn;



        //}
    }
}
