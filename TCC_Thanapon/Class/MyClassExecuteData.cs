using Microsoft.Data.SqlClient;
using System.Data;

namespace TCC_Thanapon.Class
{
    public class MyClassExecuteData
    {
        private string connectString = "Data Source=.\\SQLEXPRESS;Initial Catalog=TCC_TEST;TrustServerCertificate=True;Integrated Security=True";
        public DataTable GetDataTable(string sql)
        {
            string query = sql.ToString();
            DataTable t1 = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataAdapter a = new SqlDataAdapter(cmd))
                    {
                        a.Fill(t1); // ดึงข้อมูลจากฐานข้อมูลมาเติมใส่ใน DataTable
                    }
                } // ระบบจะ Close และ Dispose ตัว cmd และ a ให้โดยอัตโนมัติ
            } // ระบบจะ Close และ Dispose ตัว conn ให้โดยอัตโนมัติเมื่อออกจากบล็อกนี้

            return t1;
        }
        public bool ExecuteNonQuery(string sql)
        {
            string query = sql.ToString();

            using (SqlConnection conn = new SqlConnection(connectString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 1024;
                    cmd.ExecuteNonQuery();
                } // ระบบจะทำการปิด cmd และ conn (Close/Dispose) ให้โดยอัตโนมัติเมื่อออกจากบล็อก using
            }

            return true;
        }
    }
}
