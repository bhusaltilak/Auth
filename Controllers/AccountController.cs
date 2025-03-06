using Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        // Registration View
        public IActionResult Register() => View();


        [HttpPost]
        public IActionResult Register(string fullName, string email, string phone, string password)
        {
            try
            {
                Console.WriteLine($"🔹 Register Attempt: {email}");

                _authService.Register(fullName, email, phone, password);

                Console.WriteLine("✅ User Registered Successfully, Redirecting to Verify OTP");

                TempData["Email"] = email;
                TempData.Keep("Email");

                return RedirectToAction("VerifyOtp");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Registration Error: {ex.Message}");

                if (ex.Message.Contains("Email is already exist"))
                {
                    ViewBag.EmailError = ex.Message;
                }

                // ✅ Keep user input when returning to the form
                ViewBag.FullName = fullName;
                ViewBag.Email = email;
                ViewBag.Phone = phone;
                ViewBag.Password = password;

                return View(); // ✅ Keeps user input if an error occurs
            }
        }




        [HttpGet]
    public IActionResult VerifyOtp() => View();
 


        // OTP Verification View
        [HttpPost]
        public IActionResult VerifyOtp(string otp)
        {
            try
            {
                // 🔹 Ensure TempData["Email"] exists before proceeding
                if (TempData["Email"] == null)
                {
                    return RedirectToAction("Register"); // Redirect if session expired
                }

                string email = TempData["Email"].ToString();

                if (_authService.VerifyOtp(email, otp))
                {
                    TempData["Message"] = "Verification successful. Please log in.";
                    return RedirectToAction("Login"); 
                }
                else
                {
                    ViewBag.Error = "Invalid OTP or OTP expired."; 
                    return View(); 
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message; 
                return View();
            }
        }




        // Login View
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (_authService.Login(email, password))
            {
                HttpContext.Session.SetString("UserEmail", email);
                return RedirectToAction("Dashboard");
            }
            ViewBag.Error = "Invalid email or password.";
            return View();
        }
       


        // Dashboard (Protected Route)
        public IActionResult Dashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
            {
                return RedirectToAction("Login");
            }
            ViewBag.Email = HttpContext.Session.GetString("UserEmail");
            return View();
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
