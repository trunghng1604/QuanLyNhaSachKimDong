using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace QuanLyNhaSachKimDong.DAL
{
    /// <summary>
    /// Lớp tiện ích dùng chung để làm việc với SQL Server.
    /// Mọi hàm đều nhận tham số qua SqlParameter (Parameterized Query).
    /// </summary>
    public static class SqlHelper
    {
        // Đọc chuỗi kết nối từ App.config
        private static readonly string connStr =
            ConfigurationManager.ConnectionStrings["KimDongConn"].ConnectionString;

        /// <summary>Tạo kết nối mới (chưa mở). Dùng khi cần tự điều khiển Transaction.</summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connStr);
        }

        /// <summary>Chạy SELECT, trả về DataTable.</summary>
        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt); // Fill tự mở/đóng kết nối
                }
                return dt;
            }
        }

        /// <summary>Chạy INSERT/UPDATE/DELETE, trả về số dòng bị ảnh hưởng.</summary>
        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Chạy câu lệnh trả về 1 giá trị (COUNT, SUM, SCOPE_IDENTITY...).</summary>
        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }
    }
}