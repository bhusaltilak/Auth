using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class DbHelper
{
    private readonly string _connectionString;

    public DbHelper(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public void ExecuteQuery(string query, SqlParameter[] parameters = null)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public DataTable GetDataTable(string query, SqlParameter[] parameters = null)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    // 🔹 Clone parameters before adding them
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.Add(new SqlParameter(param.ParameterName, param.Value));
                    }
                }
                else
                {
                    throw new Exception("SQL query missing required parameters.");
                }

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }
    }

}
