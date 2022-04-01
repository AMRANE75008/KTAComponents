using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTAComponents
{
    class class_DBUtils
    {
        //authentification windows
        //public static SqlConnection GetDBConnection(string datasource, string database)
        //{

        //    return DBSQLServerUtils.GetDBConnection(datasource, database);
        //}
        //authentification sql server
        public static SqlConnection GetDBConnection(string datasource, string database, string username, string password)
        {

            return DBSQLServerUtils.GetDBConnection(datasource, database, username, password);

        }
    }
}
