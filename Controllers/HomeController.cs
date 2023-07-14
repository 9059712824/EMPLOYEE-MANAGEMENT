using EMPLOYEE_MANAGEMENT.DAL;
using EMPLOYEE_MANAGEMENT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace EMPLOYEE_MANAGEMENT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext applicationDbContext;

        public HomeController(ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
        }

        public IActionResult Add()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Add(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if(user !=null)
            {
                var newUser = new User()
                {
                    Email = user.Email,
                    Password = Encrypt(RandomPassword()),
                    Role = user.Role,
                    ProfilesetupCompleted = ProfileStatus.INACTIVE.ToString()
                };
                await applicationDbContext.Users.AddAsync(newUser);
                await applicationDbContext.SaveChangesAsync();
            }    
            return RedirectToAction("Index");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            if (user.Email == null || user.Password == null)
            {
                throw new ArgumentNullException("user");
            }
            var newUser = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (newUser == null)
            {
                throw new ArgumentException("user");
            }
            if (newUser.Email != user.Email || !VerifyPassword(user.Password, newUser.Password)) {
                throw new InvalidDataException("user");
            }
            /*if (newUser.Email == user.Email && VerifyPassword(user.Password, newUser.Password){
                if (newUser.Role == Role.ADMIN.ToString()) {
                    return RedirectToAction("Add");
                }
            }*/
            return RedirectToAction("Add");
        }
        public static string Encrypt(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashedBytes = sha.ComputeHash(passwordBytes);
                string encryptedPassword = Convert.ToBase64String(hashedBytes);
                return encryptedPassword;
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            byte[] storedHashedBytes = Convert.FromBase64String(storedHashedPassword);

            using (SHA256 sha = SHA256.Create())
            {
                byte[] enteredPasswordBytes = Encoding.UTF8.GetBytes(enteredPassword);
                byte[] enteredHashedBytes = sha.ComputeHash(enteredPasswordBytes);

                return StructuralComparisons.StructuralEqualityComparer.Equals(storedHashedBytes, enteredHashedBytes);
            }
        }

        public string RandomPassword()
        {
            var passwordBuilder = new StringBuilder();

            // 4-Letters lower case
            passwordBuilder.Append(RandomString(4, true));

            // 4-Digits between 1000 and 9999
            passwordBuilder.Append(RandomNumber(1000, 9999));

            // 2-Letters upper case
            passwordBuilder.Append(RandomString(2));
            return passwordBuilder.ToString();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}