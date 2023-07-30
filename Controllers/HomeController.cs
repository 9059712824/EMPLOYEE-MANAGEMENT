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
        public async Task<IActionResult> Login([FromBody] LoginDTO user)
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
                return BadRequest(new { message = "Entered Incorrect Password, Check and Re-Enter Correct Password" });
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
                    var response = new
                    {
                        role = newUser.Role,
                        redirectUrl = Url.Action("Register", "Home")
                    };

                    return Ok(response);
                }
                else if (newUser.ProfilesetupCompleted == ProfileStatus.PENDING.ToString())
                {
                    var response = new
                    {
                        role = newUser.Role,
                        redirectUrl = Url.Action("AddUserDetails", "Home")
                    };

                    return Ok(response);
                }
            }
            var defaultresponse = new
            {
                role = newUser.Role,
                redirectUrl = Url.Action("Index", "Home")
            };
            return Ok(defaultresponse);
        }

        public IActionResult EmailValidation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EmailValidation(EmailDTO emailDTO)
        {
            var email = emailDTO.Email;
            var user = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if(user == null)
            {
                return NotFound("Email Doesn't Exists " + email);
            }
            if (TimeDifference(user.OTPGeneratedTime))
            {
                double otp = RandomNumber(100000, 999999);

                DateTime date = DateTime.Now;

                user.OTP = otp;
                user.OTPGeneratedTime=date;

                applicationDbContext.Users.Update(user);
                await applicationDbContext.SaveChangesAsync();
                sendOTPEmail(user.Email, "", otp, user.Role.ToString());
            }
            else
            {
                sendOTPEmail(user.Email, "", user.OTP, user.Role.ToString());
            }
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            return RedirectToAction("ForgotPassword");
        }

        public IActionResult ForgotPassword()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDTO)
        {
            Guid userId = Guid.Parse(HttpContext.Session.GetString("UserId"));
            var user = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return BadRequest(new { message = "No User Found" });
            }
            if (user.OTP != Double.Parse(forgotPasswordDTO.OTP))
            {
                return BadRequest(new { message = "Entered OTP is incorrect, Please Re-Enter correct OTP" });
            }
            if(forgotPasswordDTO.Password.Length < 8)
            {
                return BadRequest(new { message = "Length of Password must be equal or more than 8 Characters" });
            }
            if (forgotPasswordDTO.ConfirmPassword.Length < 8)
            {
                return BadRequest(new { message = "Length of ConfirmPassword must be equal or more than 8 Characters" });
            }
            if (!forgotPasswordDTO.Password.Equals(forgotPasswordDTO.ConfirmPassword))
            {
                return BadRequest(new { message = "Entered Password and Confirm Password Not Matched" });
            }
            if (VerifyPassword(forgotPasswordDTO.Password, user.Password))
            {
                return BadRequest(new { message = "Entered Password already exists in our System, Please create a new Password" });
            }
            user.Password = Encrypt(forgotPasswordDTO.Password);
            applicationDbContext.Users.Update(user);
            await applicationDbContext.SaveChangesAsync();
            return RedirectToAction("Login");
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
        public async Task<IActionResult> Add([FromBody]AddUserDTO user)
        {
            if (user == null)
            {
                return BadRequest(new { message = "Invalid user data." });
            }

            var users = await applicationDbContext.Users.ToListAsync();

            foreach(var i in users)
            {
                if (i.Email.Equals(user.Email))
                {
                    return BadRequest(new { message = "Email Already exists " + user.Email });
                }
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
        public async Task<IActionResult> Register([FromBody] RegisterDTO register)
        {
            String UserId = HttpContext.Session.GetString("UserId");
            var newUser = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.UserId == Guid.Parse(UserId));

            if (register == null)
            {
                throw new ArgumentNullException();
            }
            if(register.NewPassword.Length < 8)
            {
                return BadRequest(new { message = "Please Enter NewPassword with Length equal or more than 8 characters" });
            }
            if (register.ConfirmNewpassword.Length < 8)
            {
                return BadRequest(new { message = "Please Enter ConfirmNewpassword with Length equal or more than 8 characters" });
            }
            if (!register.NewPassword.Equals(register.ConfirmNewpassword))
            {
                return BadRequest(new { message = "New Password and Confirm New Password Not Matched" });
            }
            if(register.Password.Equals(register.NewPassword))
            {
                return BadRequest(new { message = "Old Password and NewPassword Matched, Please Enter a New Password" });
            }
            if (!VerifyPassword(register.Password, newUser.Password))
            {
                return BadRequest( new { message = "Entered Password is Incorrect" } );
            }
            if(register.OTP != newUser.OTP)
            {
                return BadRequest(new { message = "Entered Incorrect OTP, Please check and Re-Enter Correct OTP" });
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

            // Get the list of existing academic details for the user
            var academicDetails = applicationDbContext.academicDetailsViews
                .Where(ad => ad.UserId == userId)
                .ToList();

            List<string> existingAcademicNames = academicDetails.Select(ad => ad.Name).ToList();

            // Validation 1: Check if the academic detail already exists for the user
            // Validation 3: Check if the academic detail already exists for the user
            if (existingAcademicNames.Contains(academicDetail.Name))
            {
                return BadRequest(new { message = $"Academic Details of {academicDetail.Name} Already exists" });
            }

            // Validation 4: Check specific cases based on academicDetail.Name and academics.Name
            switch (academicDetail.Name)
            {
                case "Intermediate(10+2)":

                    if (!existingAcademicNames.Contains("School"))
                    {
                        // School exists
                        return BadRequest(new { message = "Academic Details of School Not exists" });
                    }
                    if (existingAcademicNames.Contains("Diploma"))
                    {
                        // Diploma exists
                        return BadRequest(new { message = "Academic Details of Diploma Already exists" });
                    }
                    break;

                case "Diploma":
                    if (!existingAcademicNames.Contains("School"))
                    {
                        // School exists
                        return BadRequest(new { message = "Academic Details of School Not exists" });
                    }
                    if (existingAcademicNames.Contains("Intermediate(10+2)"))
                    {
                        // Intermediate exists
                        return BadRequest(new { message = "Academic Details of Intermediate(10+2) Already exists" });
                    }
                    break;

                case "BSC":
                    if (!existingAcademicNames.Contains("School"))
                    {
                        // School exists
                        return BadRequest(new { message = "Academic Details of School Not exists" });
                    }
                    if (!existingAcademicNames.Contains("Intermediate(10+2)") && !existingAcademicNames.Contains("Diploma"))
                    {
                        // Intermediate or Diploma exists
                        return BadRequest(new { message = "Academic Details of Intermediate(10+2) or Diploma Not exists" });
                    }
                    break;

                case "BCA":
                    if (!existingAcademicNames.Contains("School"))
                    {
                        // School exists
                        return BadRequest(new { message = "Academic Details of School Not exists" });
                    }
                    if (!existingAcademicNames.Contains("Intermediate(10+2)") && !existingAcademicNames.Contains("Diploma"))
                    {
                        // Intermediate or Diploma exists
                        return BadRequest(new { message = "Academic Details of Intermediate(10+2) or Diploma Not exists" });
                    }
                    break;

                case "BTech":
                    if (!existingAcademicNames.Contains("School"))
                    {
                        // School exists
                        return BadRequest(new { message = "Academic Details of School Not exists" });
                    }
                    if (!existingAcademicNames.Contains("Intermediate(10+2)") && !existingAcademicNames.Contains("Diploma"))
                    {
                        // Intermediate or Diploma exists
                        return BadRequest(new { message = "Academic Details of Intermediate(10+2) or Diploma Not exists" });
                    }
                    break;

                case "MTech":
                    if (!existingAcademicNames.Contains("School"))
                    {
                        // School exists
                        return BadRequest(new { message = "Academic Details of School Not exists" });
                    }
                    if (!existingAcademicNames.Contains("Intermediate(10+2)") && !existingAcademicNames.Contains("Diploma"))
                    {
                        // Intermediate or Diploma exists
                        return BadRequest(new { message = "Academic Details of Intermediate(10+2) or Diploma Not exists" });
                    }
                    if (!existingAcademicNames.Contains("BTech"))
                    {
                        // BTech exists
                        return BadRequest(new { message = "Academic Details of BTech Not exists" });
                    }
                    break;

                default:
                    break;
            }

            // Validation 5: Check for education gaps between academic years (if required)
            if (academicDetail.Name == "Intermediate(10+2)" || academicDetail.Name == "Diploma")
            {
                var schoolAcademicDetail = academicDetails.FirstOrDefault(ad => ad.Name == "School");
                /*if (schoolAcademicDetail != null && academicDetail.StartYear > schoolAcademicDetail.EndYear)
                {
                    return BadRequest(new { message = "Education gap "+ (academicDetail.StartYear-schoolAcademicDetail.EndYear) + " Years Exists" });
                }*/
                else if (schoolAcademicDetail != null && academicDetail.StartYear < schoolAcademicDetail.EndYear)
                {
                    return BadRequest(new { message = "Invalid Input data of " + academicDetail.Name+ " Start Year" });
                }
            }
            else if (academicDetail.Name == "BTech" || academicDetail.Name == "BSC" || academicDetail.Name == "BCA")
            {
                var intermediateOrDiplomaAcademicDetail = academicDetails.FirstOrDefault(ad => ad.Name == "Intermediate(10+2)" || ad.Name == "Diploma");
                /*if (intermediateOrDiplomaAcademicDetail != null && academicDetail.StartYear > intermediateOrDiplomaAcademicDetail.EndYear)
                {
                    return BadRequest(new { message = "Education gap "+ (academicDetail.StartYear-intermediateOrDiplomaAcademicDetail.EndYear) + " Years Exists" });
                }*/

                else if (intermediateOrDiplomaAcademicDetail != null && academicDetail.StartYear < intermediateOrDiplomaAcademicDetail.EndYear)
                {
                    return BadRequest(new { message = "Invalid Input data of "+academicDetail.Name + " Start year" });
                }
            }
            else if (academicDetail.Name == "MTech")
            {
                var bscOrBcaOrBTechAcademicDetail = academicDetails.FirstOrDefault(ad => ad.Name == "BSC" || ad.Name == "BCA" || ad.Name == "BTech");
                /*if (bscOrBcaOrBTechAcademicDetail != null && academicDetail.StartYear > bscOrBcaOrBTechAcademicDetail.EndYear)
                {
                    return BadRequest(new { message = "Education gap "+ (academicDetail.StartYear-bscOrBcaOrBTechAcademicDetail.EndYear) + " Years Exists" });
                }*/
                else  if (bscOrBcaOrBTechAcademicDetail != null && academicDetail.StartYear < bscOrBcaOrBTechAcademicDetail.EndYear)
                {
                    return BadRequest(new { message = "Invalid Input data of "+academicDetail.Name + " Start year" });
                }
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

            var responseDefault = new
            {
                message = "Academic Details Added Successfully",
                redirectUrl = Url.Action("ViewAcademicDetails", "Home")
            };

            return Ok(responseDefault);
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

        public IActionResult ViewPDF(Guid detailsId)
        {
            byte[] pdf = null;
            var academicDetails = applicationDbContext.AcademicDetails.FirstOrDefault(ad => ad.Id ==detailsId);
            var experience = applicationDbContext.Experience.FirstOrDefault(e => e.Id == detailsId);

            if(academicDetails != null && academicDetails.proof != null && academicDetails.proof.Length > 0 )
            {
                pdf = academicDetails.proof;
            }
            else if(experience != null && experience.proof != null && experience.proof.Length > 0)
            {
                pdf = experience.proof;
            }
            else
            {
                return BadRequest(new { errorMessage = "Invalid user data." });
            }

            return View(pdf);
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

        [HttpPost]
        public IActionResult ViewEmployeesDetails(string searchString)
        {
            ViewBag.CurrentFilter = searchString;

            var employeeDetails = applicationDbContext.employeeDetails.ToList();
            if (employeeDetails == null || employeeDetails.Count == 0)
            {
                return BadRequest(new { message = "No Users Found" });
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                int searchNumber;
                int.TryParse(searchString, out searchNumber);

                employeeDetails = employeeDetails.Where(e =>
                    e.FirstName.Contains(searchString) ||
                    e.LastName.Contains(searchString) ||
                    e.Email.Contains(searchString) ||
                    e.Role.Contains(searchString) ||
                    e.Number == searchNumber ||
                    e.Department.Contains(searchString)
                ).ToList(); 
            }

            if (employeeDetails == null || employeeDetails.Count == 0)
            {
                return BadRequest(new { message = "No Results Found" });
            }

            return View(employeeDetails);
        }

        [HttpPost]
        public IActionResult DeleteAll(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest(new { message = "No Id's Found" });
            }

            foreach (string id in ids)
            {
                var emp = applicationDbContext.Users.FirstOrDefault(u => u.UserId == Guid.Parse(id));
                if (emp != null)
                {
                    applicationDbContext.Users.Remove(emp);
                }
            }

            applicationDbContext.SaveChanges();

            return Json(new { success = true });
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
            if ((int)time.TotalMinutes > 5)
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