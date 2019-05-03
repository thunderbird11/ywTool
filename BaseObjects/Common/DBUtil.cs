using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ywTool.BaseObjects.Common
{
    public class DBUtil
    {
        public static DataSet GetDataSet(string connectionstring,string sql, Dictionary<string, object> parameters = null)
        {
            SqlDataAdapter da = new SqlDataAdapter();
            using (SqlConnection conn = new SqlConnection(connectionstring)) {
                DataSet ds = new DataSet();
                
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 1200;
                if (parameters != null)
                {
                    foreach (var d in parameters)
                    {
                        cmd.Parameters.AddWithValue(d.Key, d.Value);
                    }
                }
                da.SelectCommand = cmd;
                da.Fill(ds);

                return ds;
            }
        }
    }
}
