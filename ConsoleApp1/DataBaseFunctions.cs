using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class DataBaseFunctions
    {
        public static SqlConnection Connection;

        public static void SetConnection(SqlConnection c) 
        {
            Connection = c;
        }

        /*https://o7planning.org/10515/work-with-sql-server-database-in-csharp*/

        public static void InsertData(string fName) //, SqlConnection Conn)
        {
            try
            {

                var fi2 = new FileInfo(fName);

                string sql = "Insert into FilesChecked (File, LastChanged, Size, Created) "
                                                 + " values (@File, @lastChanged, @size, @created) ";
                SqlCommand cmd = Connection.CreateCommand();
                cmd.CommandText = sql;

                // Create Parameter.
                SqlParameter File = new SqlParameter("@File", SqlDbType.NVarChar);
                File.Value = fName;
                cmd.Parameters.Add(File);

                // Add parameter (Write shorter)
                SqlParameter lastChanged = cmd.Parameters.Add("@lastChanged", SqlDbType.DateTime);
                lastChanged.Value = fi2.LastWriteTime;

                // Add parameter
                SqlParameter created = cmd.Parameters.Add("@created", SqlDbType.DateTime);
                created.Value = fi2.CreationTime;

                // Add parameter (more shorter).
                cmd.Parameters.Add("@size", SqlDbType.Int).Value = fi2.Length;

                // Execute Command (for Delete,Insert or Update).
                int rowCount = cmd.ExecuteNonQuery();

                //Console.WriteLine("Row Count affected = " + rowCount);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                //connection.Close();
                //connection.Dispose();
                //connection = null;
            }

            //Console.Read();

        }
        
        public static void QueryData(string arg, string table) 
        {
            try
            {
                QueryTable(table);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                // Close connection.
                // Dispose object, freeing Resources.
                //Connection.Close();
                //Connection.Dispose();
                //Connection = null;
            }
            Console.Read();
        }

        private static void QueryTable(string tableName)
        {
            string sql = "Select Emp_Id, Emp_No, Emp_Name, Mng_Id from " + tableName;

            // Create command.
            SqlCommand cmd = new SqlCommand();

            // Set connection for Command.
            cmd.Connection = Connection;
            cmd.CommandText = sql;


            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {

                    while (reader.Read())
                    {
                        // Get index of Column Emp_ID in query statement.
                        int empIdIndex = reader.GetOrdinal("Emp_Id"); // 0


                        long empId = Convert.ToInt64(reader.GetValue(0));

                        // Index of Emp_ID = 1
                        string empNo = reader.GetString(1);
                        int empNameIndex = reader.GetOrdinal("Emp_Name");// 2
                        string empName = reader.GetString(empNameIndex);

                        // Index of column Mng_Id.
                        int mngIdIndex = reader.GetOrdinal("Mng_Id");

                        long? mngId = null;


                        if (!reader.IsDBNull(mngIdIndex))
                        {
                            mngId = Convert.ToInt64(reader.GetValue(mngIdIndex));
                        }
                        Console.WriteLine("--------------------");
                        Console.WriteLine("empIdIndex:" + empIdIndex);
                        Console.WriteLine("EmpId:" + empId);
                        Console.WriteLine("EmpNo:" + empNo);
                        Console.WriteLine("EmpName:" + empName);
                        Console.WriteLine("MngId:" + mngId);
                    }
                }
            }
        }

        public static void UpdateData(string fName, SqlConnection Conn)
        {

            try
            {
                //var fi2 = new FileInfo(fName);

                //string sql = "Insert into FILES_CHECKED (File, LastChangeDate, Size, CreateDate/*, TimeStamp*/) "
                //                                 + " values (@File, @lastChangeDate, @size, @createDate/*, @timeStamp*/) ";
                //SqlCommand cmd = Connection.CreateCommand();
                //cmd.CommandText = sql;

                //// Create Parameter.
                //SqlParameter File = new SqlParameter("@File", SqlDbType.NVarChar);
                //File.Value = fName;
                //cmd.Parameters.Add(File);

                //// Add parameter (Write shorter)
                //SqlParameter lastChangeDate = cmd.Parameters.Add("@lastChangeDate", SqlDbType.DateTime);
                //lastChangeDate.Value = fi2.LastWriteTime;

                //// Add parameter
                //SqlParameter createDate = cmd.Parameters.Add("@createDate", SqlDbType.DateTime);
                //createDate.Value = fi2.CreationTime;

                //// Add parameter
                ////SqlParameter timeStamp = cmd.Parameters.Add("@timeStamp", SqlDbType.DateTime);
                ////timeStamp.Value = DateTime.Now;

                //// Add parameter (more shorter).
                //cmd.Parameters.Add("@size", SqlDbType.Int).Value = fi2.Length;

                //// Execute Command (for Delete,Insert or Update).
                //int rowCount = cmd.ExecuteNonQuery();

                string sql = "Update Employee set Salary = @salary where Emp_Id = @empId";

                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = sql;

                // Add and set value for parameter.
                cmd.Parameters.Add("@salary", SqlDbType.Float).Value = 850;
                cmd.Parameters.Add("@empId", SqlDbType.Decimal).Value = 7369;

                // Execute Command (for Delete, Insert,Update).
                int rowCount = cmd.ExecuteNonQuery();

                Console.WriteLine("Row Count affected = " + rowCount);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                //Connection.Close();
                //Connection.Dispose();
                //Connection = null;
            }
            Console.Read();

        }

        // Create a Command with 2 parameter: Command Text & Connection.
        public static void DoSomethingIntheDatabase(string sql, SqlConnection conn)
        {
            // Create command.
            SqlCommand cmd = new SqlCommand();

            // Set connection for Command.
            cmd.Connection = Connection;
            cmd.CommandText = sql;
        }

        public static void UpdateFileChecked(string fName)
        {
            try
            {
                var fi2 = new FileInfo(fName);
                // Create command.
                SqlCommand cmd = new SqlCommand();
                string sql = "IF NOT EXISTS (SELECT * FROM FilesChecked b WHERE b.File = ";
                sql = sql + "'" + fName + "'\r\n";
                sql = sql + "BEGIN\r\n";
                sql = sql + "INSERT FilesChecked(File, LastChanged, Created, Size)\r\n";
                sql = sql + "VALUES ('" + fName + ",'" + fi2.LastWriteTime + ",'" + fi2.CreationTime + ",'" + fi2.Length + ",')\r\n";
                sql = sql + "END ELSE BEGIN \r\n";
                sql = sql + "UPDATE FilesChecked SET\r\n";
                sql = sql + "LastChanged = " + fi2.LastWriteTime + ",'\r\n";
                sql = sql + "Created = " + fi2.CreationTime + ",'\r\n";
                sql = sql + "Size = " + fi2.LastWriteTime + ",'\r\n";
                sql = sql + "WHERE File = '" + fName + "'\r\n";
                sql = sql + "END\r\n";

                // Set connection for Command.
                cmd.Connection = Connection;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                // SqlCommand vorbereiten 
                //SqlCommand cmd = new SqlCommand();
                //cmd.Connection = Connection;
                //cmd.CommandType = CommandType.StoredProcedure;
                //cmd.CommandText = "FileCheckedUpdate";
                //// Parameter-Auflistung füllen 
                //cmd.Parameters.Add("@File", SqlDbType.Char);
                //cmd.Parameters.Add("@LastChanged", SqlDbType.DateTime);
                //cmd.Parameters.Add("@Created", SqlDbType.DateTime);
                //cmd.Parameters.Add("@Size", SqlDbType.Int);

                //cmd.Parameters["@File"].Value = fName;
                //cmd.Parameters["@LastChanged"].Value = fi2.LastWriteTime;
                //cmd.Parameters["@Created"].Value = fi2.CreationTime;
                //cmd.Parameters["@Size"].Value = fi2.Length;
                //// SqlCommand ausführen 
                //SqlDataReader dr = cmd.ExecuteReader();
                //while (dr.Read())
                //    //Console.WriteLine(dr["File"]);
                //    dr.Close();
                ////con.Close();
                ////Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                //Connection.Close();
                //Connection.Dispose();
                //Connection = null;
            }
        }

        public static void DBCallFilesCheckedUpdate(string fullFilePath, string branch, string language, string project, string file, string BuildType)
        {            
            var fi2 = new FileInfo(fullFilePath);
            if (branch == "docu") 
                branch = "HEAD";
            try
            {                
                // SqlCommand vorbereiten 
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = Connection;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "FilesCheckedUpdate";
                // Parameter-Auflistung füllen 
                cmd.Parameters.Add("@Branch", SqlDbType.Char);
                cmd.Parameters.Add("@Language", SqlDbType.Char);
                cmd.Parameters.Add("@Project", SqlDbType.Char);
                cmd.Parameters.Add("@File", SqlDbType.Char);
                cmd.Parameters.Add("@LastChanged", SqlDbType.DateTime);
                cmd.Parameters.Add("@Created", SqlDbType.DateTime);
                cmd.Parameters.Add("@Size", SqlDbType.Int);
                cmd.Parameters.Add("@LastChecked", SqlDbType.DateTime);               
                cmd.Parameters.Add("@FilePath", SqlDbType.Char);
                cmd.Parameters.Add("@BuildType", SqlDbType.Int);

                cmd.Parameters["@Branch"].Value = branch;
                cmd.Parameters["@Language"].Value = language;
                cmd.Parameters["@Project"].Value = project;
                cmd.Parameters["@File"].Value = file;
                cmd.Parameters["@LastChanged"].Value = fi2.LastWriteTime;
                cmd.Parameters["@Created"].Value = fi2.CreationTime;
                cmd.Parameters["@Size"].Value = fi2.Length;
                cmd.Parameters["@LastChecked"].Value = DateTime.Now;
                cmd.Parameters["@FilePath"].Value = fullFilePath;
                switch (BuildType) //(2^n Codierung, wird in der DB geodert)
                {
                    case "GLHelp":
                        cmd.Parameters["@BuildType"].Value = 2;
                        break;
                    case "V4Help":
                        cmd.Parameters["@BuildType"].Value = 1;
                        break;
                    default:
                        cmd.Parameters["@BuildType"].Value = 0;
                        break;
                }
                // SqlCommand ausführen 
                using (var reader = cmd.ExecuteReader())
                {
                    //var value = reader.GetString(reader.GetOrdinal("SpaltenName"));
                }

                //SqlDataReader dr = cmd.ExecuteReader();
                //while (dr.Read())
                //    //Console.WriteLine(dr["File"]);
                //dr.Close();
                //con.Close();
                //Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Console.WriteLine(e.StackTrace);
                //Console.WriteLine(  "Arguments:");
                Console.WriteLine("@Branch:     " +  branch);
                Console.WriteLine("@Language:   " +  language);
                Console.WriteLine("@Project:    " +  project);
                Console.WriteLine("@File:       " +  file);
                Console.WriteLine("@LastChanged:" +  fi2.LastWriteTime);
                Console.WriteLine("@Created:    " +  fi2.CreationTime);
                Console.WriteLine("@Size:       " +  fi2.Length);
                Console.WriteLine("@LastChecked:" +  DateTime.Now);
                Console.WriteLine("@FilePath:   " +  fullFilePath);
                Console.ReadLine();
            }
            finally
            {
                //Connection.Close();
                //Connection.Dispose();
                //Connection = null;
            }

            //if(branch == "00") 
            //{
            //    Console.WriteLine("--> Logged: ");
            //    Console.WriteLine("@Branch:     " + branch);
            //    Console.WriteLine("@Language:   " + language);
            //    Console.WriteLine("@Project:    " + project);
            //    Console.WriteLine("@File:       " + file);
            //    Console.WriteLine("@LastChanged:" + fi2.LastWriteTime);
            //    Console.WriteLine("@Created:    " + fi2.CreationTime);
            //    Console.WriteLine("@Size:       " + fi2.Length);
            //    Console.WriteLine("@LastChecked:" + DateTime.Now);
            //    Console.WriteLine("@FilePath:   " + fullFilePath);
            //    Console.ReadLine();
            //}

        }


        public static void DBCallProjectsUpdate(string token, string solidConverter, string BuildType)
        {
            try
            {
                // SqlCommand vorbereiten 
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = Connection;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ProjectsUpdate";
                // Parameter-Auflistung füllen 
                cmd.Parameters.Add("@Token", SqlDbType.Char);
                cmd.Parameters.Add("@SolidConverter", SqlDbType.Char);
                cmd.Parameters.Add("@BuildType", SqlDbType.Int);

                cmd.Parameters["@Token"].Value = token;
                cmd.Parameters["@SolidConverter"].Value = null;
                /*
                1: V4Help
                2: GLHelp
                4: Interface
                8: AME
                16:
                */
                switch (BuildType) //(2^n Codierung, wird in der DB geodert)
                {
                    case "V4Help":
                        cmd.Parameters["@BuildType"].Value = 1;
                        break;
                    case "GLHelp":
                        cmd.Parameters["@BuildType"].Value = 2;
                        break;
                    case "Interface":
                        cmd.Parameters["@BuildType"].Value = 4;
                        break;
                    case "AME":
                        cmd.Parameters["@BuildType"].Value = 8;
                        break;
                    default:
                        cmd.Parameters["@BuildType"].Value = 0;
                        break;
                }
                // SqlCommand ausführen 
                using (var reader = cmd.ExecuteReader())
                {
                    //var value = reader.GetString(reader.GetOrdinal("SpaltenName"));
                }

                //SqlDataReader dr = cmd.ExecuteReader();
                //while (dr.Read())
                //    //Console.WriteLine(dr["File"]);
                //dr.Close();
                //con.Close();
                //Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Console.WriteLine(e.StackTrace);
                //Console.WriteLine(  "Arguments:");
                Console.WriteLine("@Token:     " + token);
                Console.WriteLine("@SolidConverter:   " + solidConverter);
                Console.WriteLine("@BuildType:    " + BuildType);
                Console.ReadLine();
            }
            finally
            {
                //Connection.Close();
                //Connection.Dispose();
                //Connection = null;
            }
        }

        public static void emptyTable(string table) 
        {
            try
            {

                string sql = "DELETE FROM " + table;

                SqlCommand cmd = new SqlCommand();

                cmd.Connection = Connection;
                cmd.CommandText = sql;

                //cmd.Parameters.Add("@grade", SqlDbType.Int).Value = 3;

                // Execute Command (for Delete, insert, update).
                int rowCount = cmd.ExecuteNonQuery();

                Console.WriteLine("Row Count affected = " + rowCount);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                //Connection.Close();
                //Connection.Dispose();
                //Connection = null;
            }
        }

        public static void DeleteData(string fName)
        {
            try
            {

                string sql = "Delete from Salary_Grade where Grade = @grade ";

                SqlCommand cmd = new SqlCommand();

                cmd.Connection = Connection;
                cmd.CommandText = sql;

                cmd.Parameters.Add("@grade", SqlDbType.Int).Value = 3;

                // Execute Command (for Delete, insert, update).
                int rowCount = cmd.ExecuteNonQuery();

                Console.WriteLine("Row Count affected = " + rowCount);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                //Connection.Close();
                //Connection.Dispose();
                //Connection = null;
            }

            //Console.Read();

        }
    }
}
