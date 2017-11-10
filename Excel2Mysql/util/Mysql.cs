using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Threading;

namespace Excel2Mysql.util
{
    class Mysql
    {
        public static readonly Mysql Instance = new Mysql();
        public string UserName;
        public entity.DbConfig dbConfig;
        private MySqlConnection conn = null;

        private const string DATABASE = "mqjx_base";
        private const string LOCK_TABLE = "table_lock";
        private const string LOCK_TABLENAME = "table_name";
        private const string LOCK_USER = "recent_lock_user";
        private const string LOCK_ISLOCK = "is_lock";
        private const int LOCK_STATUS = 1;
        private const string DB_CHARSET = "utf8";

        private bool NewDbConnect()
        {
            try
            {
                conn = new MySqlConnection("Data Source=" + dbConfig.host
                                + ";Port=" + dbConfig.port
                                + ";User ID=" + dbConfig.user
                                + ";Password=" + dbConfig.password
                                + ";DataBase=" + DATABASE
                                + ";Charset=" + DB_CHARSET + ";");
                conn.Open();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "连接数据库出错");
                return false;
            }
            if (conn.State == ConnectionState.Open)
            {
                return true;
            }
            else
            {
                conn.Dispose();
                conn = null;
            }
            return false;
        }

        private bool ConnectToDB()
        {
            if (conn == null)
            {
                if (!NewDbConnect())
                {
                    return false;
                }
            }
            if (conn.DataSource != dbConfig.host)
            {
                conn.Close();
                if (!NewDbConnect())
                {
                    return false;
                }
            }
            if (conn.State == ConnectionState.Open)
            {
                return true;
            }
            else
            {
                conn.Dispose();
                conn = null;
            }
            return false;
        }

        public void DownloadToExcel(entity.SetMaxPro setFunc, entity.UpdataPro updateFunc)
        {
            if (!ConnectToDB())
            {
                MessageBox.Show("数据库连接失败, ErrCode = 1", "失败");
                return;
            }
            List<string> tblNames = null;
            QueryAllTableNames(out tblNames);
            if(tblNames.Count == 0)
            {
                MessageBox.Show("数据库：" + DATABASE + "没有任何表", "下载表");
                return;
            }
            int totalCnt = tblNames.Count;
            if (tblNames.Contains(LOCK_TABLE))
            {
                totalCnt--;
            }
            setFunc(totalCnt);
            //根据各个表名获取每个表的数据
            for (int i = 0; i < tblNames.Count; i++)
            {
                if (tblNames[i] == LOCK_TABLE)
                {
                    continue;
                }
                try
                {
                    //查表数据
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT * FROM " + tblNames[i];
                    cmd.CommandType = CommandType.Text;
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    adapter.Dispose();

                    //查表的字段注释
                    Dictionary<string, string> columnInfo = new Dictionary<string, string>();
                    cmd.CommandText = "SELECT COLUMN_NAME,COLUMN_COMMENT FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tblNames[i] + "' and TABLE_SCHEMA = '" + DATABASE + "'";
                    DbDataReader reader2 = cmd.ExecuteReader();
                    if (reader2.HasRows)
                    {
                        while (reader2.Read())
                        {
                            columnInfo.Add(reader2["COLUMN_NAME"].ToString(), reader2["COLUMN_COMMENT"].ToString());
                        }
                        reader2.Close();
                    }
                    cmd.Dispose();

                    //写数据到Excel
                    Excel.Instance.DataTabletoExcel(tblNames[i], dt, columnInfo);
                    updateFunc(tblNames[i]);
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "下载表: " + tblNames[i]);
                }
            }
        }

        public void UploadExecl(string filePath, out string errMsg)
        {
            errMsg = "";
            if (!ConnectToDB())
            {
                MessageBox.Show("数据库连接失败, ErrCode = 2", "失败");
                return;
            }
            string mysqlError = "";
            DataSet excelDataSet = Excel.Instance.Load(filePath, out mysqlError);
            if (mysqlError != "")
            {
                errMsg = mysqlError;
                return;
            }
            string tableName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            string query = createSql(excelDataSet, tableName);
            if (query == "")
            {
                errMsg = "create sql table: " + tableName + " error";
                return;
            }
            if (query == "data empty")
            {
                errMsg = query;
                return;
            }
            try
            {
                MySqlCommand cmd = new MySqlCommand("set names " + DB_CHARSET, conn);
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception err)
            {
                errMsg = "上传[" + filePath + "]失败！\n失败原因：" + err.Message + "\nquery:" + query;
            }
        }

        public bool QueryTableLock(List<string> checkTbl, out Dictionary<string, string> retTbl)
        {
            retTbl = new Dictionary<string, string>();
            if (!ConnectToDB())
            {
                MessageBox.Show("数据库连接失败, ErrCode = 3", "失败");
                return false;
            }
            try
            {
                //每次都查一下锁表是否存在
                List<string> tblNames = null;
                QueryTableNames(LOCK_TABLE, out tblNames);
                if (tblNames.Count == 0)
                {
                    InitTableLock();
                }

                string sql = "SELECT * FROM " + LOCK_TABLE + " WHERE " + LOCK_TABLENAME + " in (";
                for (int i = 0; i < checkTbl.Count; i++)
                {
                    if (i != 0)
                    {
                        sql += ", ";
                    }
                    sql += "'" + checkTbl[i] + "'";
                }
                sql += ")";
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string key = reader[LOCK_TABLENAME].ToString();
                        string lock_user = reader[LOCK_USER].ToString();
                        int status = int.Parse(reader[LOCK_ISLOCK].ToString());
                        if (lock_user != "" && lock_user != UserName && status == LOCK_STATUS)
                        {
                            retTbl[key] = lock_user;
                        }
                    }
                }
                reader.Close();
                cmd.Dispose();
                return true;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "查询锁表");
            }
            return false;
        }

        public bool UpdateTableLock(List<string> updateList, bool isLock)
        {
            if (!ConnectToDB())
            {
                MessageBox.Show("数据库连接失败, ErrCode = 4", "失败");
                return false;
            }
            try
            {
                int lockNum = 0;
                if (isLock)
                {
                    lockNum = 1;
                }
                string sql = "UPDATE " + LOCK_TABLE + " SET " + LOCK_ISLOCK + " = " + lockNum + ", " + LOCK_USER + " = '" + UserName + "' WHERE " + LOCK_TABLENAME + " in (";
                for (int i = 0; i < updateList.Count; i++)
                {
                    if (i != 0)
                    {
                        sql += ", ";
                    }
                    sql += "'" + updateList[i] + "'";
                }
                sql += ")";
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = sql;
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                return true;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "更新锁表");
            }
            return false;
        }

        public void InitTableLock()
        {
            try
            {
                //创建锁表
                MySqlCommand cmd = new MySqlCommand();
                string sql = "CREATE TABLE " + LOCK_TABLE + " (" + LOCK_TABLENAME + " VARCHAR(255), " + LOCK_USER + " VARCHAR(255), " + LOCK_ISLOCK + " int(0))";
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();

                //插入所有表名到锁表中
                List<string> tblNames = null;
                QueryAllTableNames(out tblNames);
                if (tblNames.Count == 0)
                {
                    MessageBox.Show("没有找到任何表", "初始化锁表");
                    return;
                }
                sql = "INSERT INTO " + LOCK_TABLE + " (" + LOCK_TABLENAME + ", " + LOCK_USER + ", " + LOCK_ISLOCK + ") VALUES ";
                for (int i = 0; i < tblNames.Count; i++)
                {
                    if (i != 0)
                    {
                        sql += ",";
                    }
                    sql += "('" + tblNames[i] + "', '', 0)";
                }
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "初始化锁表");
            }
        }

        public void ResetTableLock()
        {
            List<string> tblNames = null;
            QueryAllTableNames(out tblNames);
            if (tblNames.Count == 0)
            {
                MessageBox.Show("没有找到任何表", "重置锁表");
                return;
            }
            UpdateTableLock(tblNames, false);
        }

        private string createSql(DataSet execlDataSet, string dbTableName)
        {
            DataTable dataTbl = execlDataSet.Tables[@"Sheet1"];
            if (dataTbl == null)
            {
                return "";
            }
            if (dataTbl.Rows.Count <= 2)
            {
                return "data empty";
            }
            List<string> sqlKeys = new List<string>();
            List<int> sqlKeyIndexs = new List<int>();
            for (int i = 0; i < dataTbl.Columns.Count; i++)
            {
                string sqlKeyName = dataTbl.Rows[0][i].ToString();
                if (string.IsNullOrEmpty(sqlKeyName))
                {
                    continue;
                }
                sqlKeys.Add("`" + sqlKeyName + "`");
                sqlKeyIndexs.Add(i);
            }
            List<string> sqlItems = new List<string>();
            for (int i = 2; i < dataTbl.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(dataTbl.Rows[i][0].ToString()))
                {
                    break;
                }

                List<string> sqlItem = new List<string>();

                for (int j = 0; j < sqlKeys.Count; j++)
                {
                    sqlItem.Add("'" + dataTbl.Rows[i][sqlKeyIndexs[j]].ToString().Replace("'", "''") + "'");
                }

                sqlItems.Add("(" + string.Join(",", sqlItem.ToArray()) + ")");
            }
            return "TRUNCATE TABLE `" + dbTableName + "`;\nINSERT INTO `" + dbTableName + "` (" +
                string.Join(", ", sqlKeys) + ") VALUES " + string.Join(", ", sqlItems.ToArray()) + ";";
        }

        public void QueryAllTableNames(out List<string> queryResult)
        {
            queryResult = new List<string>();
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '" + DATABASE + "'";
                cmd.CommandType = CommandType.Text;
                DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string tblName = reader["TABLE_NAME"].ToString();
                        queryResult.Add(tblName);
                    }
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "查询所有表");
            }
        }

        public void QueryTableNames(string tableName, out List<string> queryResult)
        {
            queryResult = new List<string>();
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '" + DATABASE + "' AND TABLE_NAME = '" + tableName + "'";
                cmd.CommandType = CommandType.Text;
                DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string tblName = reader["TABLE_NAME"].ToString();
                        queryResult.Add(tblName);
                    }
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "查询表：" + tableName);
            }
        }
    }
}
