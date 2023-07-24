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
            HttpContext.Session.SetString("UserId", newUser.UserId.ToString());
            HttpContext.Session.SetString("Role", newUser.Role.ToString());
            if (newUser.Email == user.Email && VerifyPassword(user.Password, newUser.Password))
            {
                if (newUser.ProfilesetupCompleted == ProfileStatus.INACTIVE.ToString())
                {
                    if (TimeDifference(newUser.OTPGeneratedTime))
                    {
                        double otp = RandomNumber(100000, 999999);

                        DateTime date = DateTime.Now;

                        newUser.OTP = otp;
                        newUser.OTPGeneratedTime=date;

                        applicationDbContext.Users.Update(newUser);
                        await applicationDbContext.SaveChangesAsync();
                        sendOTPEmail(newUser.Email, "", otp, newUser.Role.ToString());
                    }
                    else
                    {
                        sendOTPEmail(newUser.Email,"",newUser.OTP, newUser.Role.ToString());
                    }
                    return RedirectToAction("Register");
                }
                else if (newUser.ProfilesetupCompleted == ProfileStatus.PENDING.ToString())
                {
                    return RedirectToAction("AddUserDetails");
                }
                if (newUser.Role == Role.ADMIN.ToString() || newUser.Role == Role.DEPARTMENT_HEAD.ToString())
                {
                    return RedirectToAction("Add");
                }
                if(newUser.Role == Role.EMPLOYEE.ToString())
                {
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Add");
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
        [HttpGet]
        public async Task<IActionResult> ViewUserDetails()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                // Session value not found, handle accordingly
                return RedirectToAction("Login");
            }
            var userDetails = await applicationDbContext.UserDetails.FirstOrDefaultAsync(u => u.UserId == Guid.Parse(userId));
            if (userDetails == null)
            {
                return RedirectToAction("AddUserDetails");
            }
            return View(userDetails);
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
            var departments = await applicationDbContext.Departments.ToListAsync();
            foreach (var i in departments)
            {
                if (i.DepartmentName.ToLower() == userDetails.Department.ToLower())
                {
                    NewUserDetails.DepartmentHead = i.DepartmentHead;
                    NewUserDetails.Department = i.DepartmentName;
                }
            }
            if (NewUserDetails.DepartmentHead == Guid.Empty)
            {
                return NotFound("Invalid Department");
            }
            await applicationDbContext.UserDetails.AddAsync(NewUserDetails);
            await applicationDbContext.SaveChangesAsync();

            newUser.ProfilesetupCompleted = ProfileStatus.ACTIVE.ToString();
            applicationDbContext.Users.Update(newUser);
            await applicationDbContext.SaveChangesAsync();

            return RedirectToAction("AddAcademicDetails");
        }

        public async Task<IActionResult> UpdateUserDetails(Guid id)
        {
            String userId = HttpContext.Session.GetString("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var details = await applicationDbContext.Users
                .Where(user => user.UserId == id)
                .Join(
                    applicationDbContext.UserDetails,
                    users => users.UserId,
                    userDetails => userDetails.UserId,
                    (users, userDetails) => new ViewUserDetails
                    {
                        UserId = users.UserId,
                        Email = users.Email,
                        Role = users.Role,
                        FirstName = userDetails.FirstName,
                        LastName = userDetails.LastName,
                        EmployeeNumber = userDetails.Number,
                        Gender = userDetails.Gender,
                        DOB = userDetails.DOB,
                        Age = userDetails.Age,
                        Address = userDetails.Address,
                        Department = userDetails.Department,
                        DepartmentHead = userDetails.DepartmentHead,
                        Salary = userDetails.Salary
                    })
                .FirstOrDefaultAsync();

            return View(details); // You should pass the 'details' object to the View to render the data.
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserDetails(ViewUserDetails viewUserDetails)
        {
            String userId = HttpContext.Session.GetString("UserId");
            var newUser = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.UserId == viewUserDetails.UserId);
            var newUserDetails = await applicationDbContext.UserDetails.FirstOrDefaultAsync(u =>u.UserId == viewUserDetails.UserId);
            if(viewUserDetails == null)
            {
                return RedirectToAction("ViewEmployeesDetails");
            }
            if(newUser.Role != viewUserDetails.Role)
            {
                if(viewUserDetails.Role.ToLower() == Role.DEPARTMENT_HEAD.ToString().ToLower())
                {
                    newUser.Role = Role.DEPARTMENT_HEAD.ToString();
                }
                else if (viewUserDetails.Role.ToLower() == Role.EMPLOYEE.ToString().ToLower())
                {
                    newUser.Role = Role.EMPLOYEE.ToString();
                }
            }
            newUserDetails.FirstName = viewUserDetails.FirstName;
            newUserDetails.LastName = viewUserDetails.LastName;
            newUserDetails.Gender = viewUserDetails.Gender;
            newUserDetails.DOB = viewUserDetails.DOB;
            newUserDetails.Age = viewUserDetails.Age;
            newUserDetails.Address = viewUserDetails.Address;
            newUserDetails.Department= viewUserDetails.Department;
            var departments = await applicationDbContext.Departments.ToListAsync();
            foreach (var i in departments)
            {
                if (i.DepartmentName.ToLower() == viewUserDetails.Department.ToLower())
                {
                    newUserDetails.DepartmentHead = i.DepartmentHead;
                    newUserDetails.Department = i.DepartmentName;
                }
            }
            newUserDetails.Salary = viewUserDetails.Salary;

            applicationDbContext.Users.Update(newUser);
            applicationDbContext.UserDetails.Update(newUserDetails);
            await applicationDbContext.SaveChangesAsync();

            return RedirectToAction("ViewEmployeesDetails");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteUserDetails(Guid id)
        {
            var user = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            applicationDbContext.Users.Remove(user);
            await applicationDbContext.SaveChangesAsync();
            return Ok();
        }

        public IActionResult AddAcademicDetails()
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
        public async Task<IActionResult> AddAcademicDetails(AcademicDetailsDTO academicDetail, IFormFile proof)
        {
            if (!ModelState.IsValid)
            {
                // Debug: Output the model validation errors to the console
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }

                return RedirectToAction("Login");
            }

            Guid userId = Guid.Parse(HttpContext.Session.GetString("UserId"));
            var newUser = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (newUser == null)
            {
                throw new InvalidDataException(userId.ToString());
            }

            if (academicDetail == null)
            {
                throw new InvalidDataException();
            }

            if (proof != null && proof.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    var fileName = proof.FileName;
                    await proof.CopyToAsync(memoryStream);
                    byte[] proofBytes = memoryStream.ToArray();

                    var result = new AcademicDetails()
                    {
                        Name = academicDetail.Name,
                        StartYear = Convert.ToInt32(academicDetail.StartYear),
                        EndYear = Convert.ToInt32(academicDetail.EndYear),
                        proof = proofBytes,
                        fileName = fileName,
                        User = newUser
                    };

                    await applicationDbContext.AcademicDetails.AddAsync(result);
                    await applicationDbContext.SaveChangesAsync();
                }
            }

            // Return a success response or redirect to another page
            return RedirectToAction("ViewAcademicDetails");
        }

        public async Task<IActionResult> ViewAcademicDetails()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            List<AcademicDetails> academicDetails = await applicationDbContext.AcademicDetails
                .Where(ad => ad.UserId == Guid.Parse(userId))
                .ToListAsync();

            if(academicDetails == null)
            {
                return RedirectToAction("AddAcademicDetails");
            }

            return View(academicDetails);
        }

        public IActionResult AddExperienceDetails()
        {
            var userId=HttpContext.Session.GetString("UserId");
            if(string.IsNullOrEmpty(userId)) {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddExperienceDetails(ExperienceDTO experience,IFormFile proof)
        {
            if(!ModelState.IsValid)
            {
                return RedirectToAction("Login");
            }

            Guid userId=Guid.Parse(HttpContext.Session.GetString("UserId"));

            var newUser=await applicationDbContext.Users.FirstOrDefaultAsync(ad => ad.UserId == userId);

            if (newUser == null)
            {
                throw new InvalidDataException(userId.ToString());
            }
            if(experience==null)
            {
                throw new ArgumentNullException("invalid data" +experience);
            }
            if (proof != null && proof.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    var fileName = proof.FileName;
                    await proof.CopyToAsync(memoryStream);
                    byte[] proofBytes = memoryStream.ToArray();
                    Experience result = new Experience()
                    {
                        CompanyName = experience.CompanyName,
                        StartDate = experience.StartDate,
                        EndDate = experience.EndDate,
                        YearsOfWorking = experience.YearsOfWorking,
                        proof = proofBytes,
                        fileName = fileName,
                        User = newUser
                    };
                    await applicationDbContext.Experience.AddAsync(result);
                    await applicationDbContext.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ViewExperienceDetails()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            List<Experience> experience = await applicationDbContext.Experience
                .Where(ad => ad.UserId == Guid.Parse(userId))
                .ToListAsync();

            if (experience == null)
            {
                return RedirectToAction("AddAcademicDetails");
            }

            return View(experience);
        }


        public async Task<IActionResult> ViewAcademicAndExperienceDetails(Guid id)
        {
            var academicDetails = await applicationDbContext.AcademicDetails
                .Where(ad => ad.UserId == id).ToListAsync();

            var experience = await applicationDbContext.Experience
                .Where(e => e.UserId == id).ToListAsync();

            AcademicAndExperience ae = new AcademicAndExperience()
            {
                academicDetails = academicDetails,
                experiences = experience
            };

            return View(ae);
        }

        public IActionResult ViewImage(Guid detailsId)
        {
            var base64Image = "";
            var academicDetail = applicationDbContext.AcademicDetails.FirstOrDefault(ad => ad.Id == detailsId);
            var experience = applicationDbContext.Experience.FirstOrDefault(ad => ad.Id == detailsId);

            if (academicDetail != null && academicDetail.proof != null && academicDetail.proof.Length > 0)
            {
                base64Image = Convert.ToBase64String(academicDetail.proof);
            }
            else if (experience != null && experience.proof != null && experience.proof.Length > 0)
            {
                base64Image = Convert.ToBase64String(experience.proof);
            }
            else
            {
                return NotFound();
            }

            // Convert the byte array to a Base64-encoded string
            var imageDataUrl = $"data:image;base64,{base64Image}";

            ViewBag.ImageDataUrl = imageDataUrl;
            return View();
        }


        public IActionResult ViewText(Guid detailsId)
        {
            String textContent = "";
            var academicDetail = applicationDbContext.AcademicDetails.FirstOrDefault(ad => ad.Id == detailsId);
            var experience = applicationDbContext.Experience.FirstOrDefault(ad => ad.Id == detailsId);

            if (academicDetail == null && experience == null)
            {
                return NotFound();
            }

            if (academicDetail != null && academicDetail.proof != null && academicDetail.proof.Length > 0)
            {
                textContent = Encoding.UTF8.GetString(academicDetail.proof);
            }
            else if (experience != null && experience.proof != null && experience.proof.Length > 0)
            {
                textContent = Encoding.UTF8.GetString(experience.proof);
            }
            else
            {
                return NotFound();
            }

            return View((object)textContent);
        }

        public async Task<IActionResult> ViewEmployeesDetails()
        {
            string userId = HttpContext.Session.GetString("UserId");
            string role = HttpContext.Session.GetString("Role");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            Guid Id = Guid.Parse(userId);
            List<EmployeeDetails> details;
            if (role == Role.ADMIN.ToString())
            {
                details = await applicationDbContext.employeeDetails

                    .Where(users => users.Role == Role.EMPLOYEE.ToString() || users.Role == Role.DEPARTMENT_HEAD.ToString())
                    .ToListAsync();
            }
            else if (role == Role.DEPARTMENT_HEAD.ToString())
            {
                details = await applicationDbContext.employeeDetails
                    
                    .Where(users => users.Role == Role.EMPLOYEE.ToString())
                    .ToListAsync();
            }
            else
            {
                details = new List<EmployeeDetails>();
            }

            return View(details);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
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
                client.Connect("smtp.gmail.com", 587);
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
            String role = HttpContext.Session.GetString("Role");
            ViewBag.role = role;
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