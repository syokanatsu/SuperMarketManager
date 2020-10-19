using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMarketManager
{
    class MysqlConnector
    {
        static MySqlConnection conn = null;
        public static MySqlConnection GetInstance()
        {
            if(conn == null)
            {
                String connetStr = "server=139.9.154.220;port=3306;user=dazuoye;password=dazuoye; database=csdazuoye233db;";
                conn = new MySqlConnection(connetStr);
            }
            return conn;
        }
    }
}
