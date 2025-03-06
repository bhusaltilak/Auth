
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Services
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Security.Cryptography;
    using System.Text;

    public class AuthService
    {
        private readonly DbHelper _dbHelper;
        private readonly EmailService _emailService;

        public AuthService(DbHelper dbHelper, EmailService emailService)
        {
            _dbHelper = dbHelper;
            _emailService = emailService;
        }


        private bool EmailExists(string email)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            SqlParameter[] parameters = { new SqlParameter("@Email", email) };

            DataTable dt = _dbHelper.GetDataTable(query, parameters);
            int count = Convert.ToInt32(dt.Rows[0][0]);

         //   Console.WriteLine($"✅ Email Check: {email} exists? {count > 0}");
            return count > 0; // ✅ Returns TRUE if email exists
        }



        public void Register(string fullName, string email, string phone, string password)
        {
            // 🔹 Check if Email already exists
            if (EmailExists(email))
            {
                throw new Exception("Email is already exist.");
            }

            string hashedPassword = HashPassword(password);
            string otp = GenerateOtp();
            DateTime expiry = DateTime.UtcNow.AddMinutes(5);

            string query = @"
        INSERT INTO Users (FullName, Email, Phone, PasswordHash, Otp, OtpExpiry) 
        VALUES (@FullName, @Email, @Phone, @Password, @Otp, @OtpExpiry)";

            SqlParameter[] parameters = {
        new SqlParameter("@FullName", fullName),
        new SqlParameter("@Phone", phone),
        new SqlParameter("@Email", email),
        new SqlParameter("@Password", hashedPassword),
        new SqlParameter("@Otp", otp),
        new SqlParameter("@OtpExpiry", expiry)
    };

            _dbHelper.ExecuteQuery(query, parameters);
            _emailService.SendOtp(email, otp);
        }





        public bool VerifyOtp(string email, string otp)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));

            string query = "SELECT Otp, OtpExpiry FROM Users WHERE Email = @Email";
            SqlParameter[] parameters = { new SqlParameter("@Email", email) };

            DataTable dt = _dbHelper.GetDataTable(query, parameters);

            if (dt.Rows.Count == 0)
            {
                throw new Exception("No user found with this email.");
            }

            string storedOtp = dt.Rows[0]["Otp"].ToString();
            DateTime expiry = Convert.ToDateTime(dt.Rows[0]["OtpExpiry"]);

            if (storedOtp == otp && expiry > DateTime.UtcNow)
            {
                string updateQuery = "UPDATE Users SET IsVerified = 1 WHERE Email = @Email";
                SqlParameter[] updateParams = { new SqlParameter("@Email", email) };

                _dbHelper.ExecuteQuery(updateQuery, updateParams);
                return true; 
            }

            return false; 
        }






        public bool Login(string email, string password)
        {
            string query = "SELECT PasswordHash, IsVerified FROM Users WHERE Email = @Email";
            SqlParameter[] parameters = { new SqlParameter("@Email", email) };
            DataTable dt = _dbHelper.GetDataTable(query, parameters);

            if (dt.Rows.Count == 0) return false;
            if (!Convert.ToBoolean(dt.Rows[0]["IsVerified"])) return false;

            string storedPassword = dt.Rows[0]["PasswordHash"].ToString();
            return VerifyPassword(password, storedPassword);
        }

        private string GenerateOtp() => new Random().Next(100000, 999999).ToString();

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPassword(string inputPassword, string storedPassword) => HashPassword(inputPassword) == storedPassword;
    }

}
