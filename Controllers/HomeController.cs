using EMPLOYEE_MANAGEMENT.DAL;
using EMPLOYEE_MANAGEMENT.DTO;
using EMPLOYEE_MANAGEMENT.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace EMPLOYEE_MANAGEMENT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext applicationDbContext;

        private readonly Random _random = new Random();

        public HomeController(ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
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
               return NotFound(user.Email);
            }
            if (newUser.Email != user.Email || !VerifyPassword(user.Password, newUser.Password))
            {
                throw new InvalidDataException("user");
            }
            if (newUser.Email == user.Email && VerifyPassword(user.Password, newUser.Password))
            {
                if (newUser.ProfilesetupCompleted == ProfileStatus.INACTIVE.ToString())
                {
                    HttpContext.Session.SetString("UserId", newUser.UserId.ToString());
                    return RedirectToAction("Register");
                }
                else if (newUser.ProfilesetupCompleted == ProfileStatus.PENDING.ToString())
                {
                    HttpContext.Session.SetString("UserId", newUser.UserId.ToString());
                    return RedirectToAction("AddUserDetails");
                }
                if (newUser.Role == Role.ADMIN.ToString())
                {
                    HttpContext.Session.SetString("UserId", newUser.UserId.ToString());
                    return RedirectToAction("Add");
                }
                else if (newUser.Role == Role.DEPARTMENT_HEAD.ToString())
                {
                    HttpContext.Session.SetString("UserId", newUser.UserId.ToString());
                    HttpContext.Session.SetString("Role", newUser.Role.ToString());
                    return RedirectToAction("Add");
                }
                else
                {
                    return RedirectToAction();
                }
            }
            return RedirectToAction("Home");
        }

        public IActionResult Add()
        {
            var userId = HttpContext.Session.GetString("UserId");
            ViewBag.Role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var password = RandomPassword();
            if (user != null)
            {
                var newUser = new User()
                {
                    Email = user.Email,
                    Password = Encrypt(password),
                    Role = user.Role,
                    ProfilesetupCompleted = ProfileStatus.INACTIVE.ToString()
                };
                double otp = RandomNumber(100000, 999999);
                DateTime dateTime = DateTime.Now;
                newUser.OTP = otp;
                newUser.OTPGeneratedTime= dateTime;
                await applicationDbContext.Users.AddAsync(newUser);
                await applicationDbContext.SaveChangesAsync();
                //sending email using SMTP
                sendOTPEmail(user.Email, password, otp, user.Role.ToString());
            }
            return RedirectToAction("Index");
        }

        public IActionResult Register()
        {
            String UserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(UserId))
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO register)
        {
            String UserId = HttpContext.Session.GetString("UserId");
            var newUser = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.UserId == Guid.Parse(UserId));

            if (register == null)
            {
                throw new ArgumentNullException();
            }
            if (TimeDifference(newUser.OTPGeneratedTime))
            {
                double otp = RandomNumber(100000, 999999);

                DateTime date = DateTime.Now;

                newUser.OTP = otp;
                newUser.OTPGeneratedTime=date;

                applicationDbContext.Users.Update(newUser);
                await applicationDbContext.SaveChangesAsync();
                HttpContext.Session.SetString("UserId", UserId);
                sendOTPEmail(newUser.Email, "", otp, newUser.Role.ToString());
                return RedirectToAction("Register");
            }
            if (!VerifyPassword(register.Password, newUser.Password))
            {
                throw new InvalidDataException("Entered Password is Incorrect");
            }
            if (register.NewPassword == register.ConfirmNewpassword && register.OTP == newUser.OTP)
            {
                newUser.Password = Encrypt(register.NewPassword);
                newUser.ProfilesetupCompleted = ProfileStatus.PENDING.ToString();
                applicationDbContext.Users.Update(newUser);
                await applicationDbContext.SaveChangesAsync();
                HttpContext.Session.SetString("UserId", UserId);
                return RedirectToAction("AddUserDetails");
            }

            return View();
        }

        public IActionResult AddUserDetails()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                // Session value not found, handle accordingly
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddUserDetails(UserDetails userDetails)
        {
            String userId = HttpContext.Session.GetString("UserId");
            var newUser = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.UserId == Guid.Parse(userId));
            if (newUser == null)
            {
                throw new InvalidDataException($"User {userId} was not found.");
            }
            var NewUserDetails = new UserDetails()
            {
                FirstName = userDetails.FirstName,
                LastName = userDetails.LastName,
                Number = userDetails.Number,
                Gender = userDetails.Gender,
                Address = userDetails.Address,
                DOB = userDetails.DOB,
                Age = userDetails.Age,
                Department = userDetails.Department,
                Salary = userDetails.Salary,
                User = newUser,
            };
            await applicationDbContext.UserDetails.AddAsync(NewUserDetails);
            await applicationDbContext.SaveChangesAsync();

            newUser.ProfilesetupCompleted = ProfileStatus.ACTIVE.ToString();
            applicationDbContext.Users.Update(newUser);
            await applicationDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult AddAcademicDetails()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddAcademicDetails(List<AcademicDetailsDTO> academicDetails)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Login");
            }

            string userId = HttpContext.Session.GetString("UserId");
            var newUser = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.UserId == Guid.Parse(userId));
            if (newUser == null)
            {
                throw new InvalidDataException(userId);
            }

            if (academicDetails == null)
            {
                throw new InvalidDataException();
            }

            if (academicDetails.Count == 0)
            {
                throw new InvalidDataException("No academic details provided");
            }

            foreach (var academicDetail in academicDetails)
            {
                if (academicDetail.proof != null && academicDetail.proof.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        /*await academicDetail.proof.CopyToAsync(memoryStream);
                        byte[] proofBytes = memoryStream.ToArray();*/

                        var result = new AcademicDetails()
                        {
                            Name = academicDetail.Name,
                            StartYear = academicDetail.StartYear,
                            EndYear = academicDetail.EndYear,
                            proof = academicDetail.proof,
                            User = newUser
                        };

                        await applicationDbContext.AcademicDetails.AddAsync(result);
                        await applicationDbContext.SaveChangesAsync();
                    }
                }
            }

            // Return a success response or redirect to another page
            return RedirectToAction("Index", "Home");
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

        public int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        public string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length=26

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
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

        public void sendOTPEmail(String email, String password, double otp, String role)
        {
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com");
                client.Authenticate("v4431365@gmail.com", "iponuhntztqltauh");
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $"<p>Your Password is {password}</p> <p>Your OTP is {otp}</p> <p>Your Role is {role}</p>",
                    TextBody = "{password} \r\n {otp} \r\n {role}"
                };

                var message = new MimeMessage
                {
                    Body = bodyBuilder.ToMessageBody()
                };
                message.From.Add(new MailboxAddress("OTP", "v4431365@gmail.com"));
                message.To.Add(new MailboxAddress("", email));
                message.Subject= "Otp For Account Validation";
                client.Send(message);

                client.Disconnect(true);
            }
        }

        public Boolean TimeDifference(DateTime timeDifference)
        {
            DateTime currentTime = DateTime.Now;
            TimeSpan time = currentTime-timeDifference;
            if ((int)time.TotalMinutes >= 5)
            {
                return true;
            }
            return false;
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