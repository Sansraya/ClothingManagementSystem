using LugaPasal.Entities;
using LugaPasal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LugaPasal.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace LugaPasal.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.dbContext = dbContext;

        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Register(RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerModel);
            }
            else
            {
                var existingName = await userManager.FindByNameAsync(registerModel.Username);
                if (existingName != null)
                {
                    ModelState.AddModelError("Username", "The username already exists");
                    return View(registerModel);
                }
                var existingEmail = await userManager.FindByEmailAsync(registerModel.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "The email already exists");
                    return View(registerModel);
                }
                var existingNumber = await dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == registerModel.PhoneNumber);
                if (existingNumber != null)
                {
                    ModelState.AddModelError("PhoneNumber", "The phone number already exists");
                    return View(registerModel);
                }

                var user = new User
                {
                    FirstName = registerModel.FirstName,
                    LastName = registerModel.LastName,
                    UserName = registerModel.Username,
                    Email = registerModel.Email,
                    PhoneNumber = registerModel.PhoneNumber,
                    DateOfBirth = registerModel.DateOfBirth,
                    //Password = registerModel.Password
                };
                if (registerModel.ProfilePicture != null && registerModel.ProfilePicture.Length > 0)
                {
                    try
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
                        Directory.CreateDirectory(uploadsFolder);
                        var extension = Path.GetExtension(registerModel.ProfilePicture.FileName).ToLower();
                        var uniqueFileName = Guid.NewGuid().ToString() + extension;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await registerModel.ProfilePicture.CopyToAsync(stream);
                        }

                        user.ProfilePicturePath = "/images/profiles/" + uniqueFileName; // store relative path
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ProfileImage", "Error saving profile picture: " + ex.Message);
                        return View(registerModel);
                    }
                }

                var result = await userManager.CreateAsync(user, registerModel.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                    
                    return RedirectToAction("Login", "User");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(registerModel);
            }
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]

        public async Task<IActionResult> Login(LoginViewModel loginModel)
        {
            var result = await signInManager.PasswordSignInAsync(loginModel.Username, loginModel.Password, loginModel.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["ErrorMessage"] = "Incorrect Credentials , Please Try Again! ";
                return View(loginModel);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]

        public async Task<IActionResult> Profile()
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userID);
            var product = await dbContext.Products.Where(p => p.UserId == user.Id)
                                                   .ToListAsync();
            if (user != null)
            {
                var profileModel = new ProfileViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.UserName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    DateOfBirth = user.DateOfBirth,
                    ProfilePicturePath = user.ProfilePicturePath,
                    ProductsOfUser= product

                };
                return View(profileModel);
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }

        }
        [HttpGet]
        public async Task<IActionResult> ProfileByOthers(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            var product = await dbContext.Products.Where(p => p.UserId == user.Id)
                                                  .ToListAsync();
            if (user != null)
            {
                var profileModel = new ProfileViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.UserName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    DateOfBirth = user.DateOfBirth,
                    ProfilePicturePath = user.ProfilePicturePath,
                    ProductsOfUser = product

                };
                return View(profileModel);
            }
            else
            {
                return RedirectToAction("ListProducts", "Products");
            }

        }
        [HttpGet]
        public async Task<IActionResult> Edit(String Id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(Id);
            if (userId != Id && !(await userManager.IsInRoleAsync(user, "Admin")))
            {
                return Forbid();
            }
            var profileModel = new ProfileViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.UserName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                ProfilePicturePath = user.ProfilePicturePath
            };

            return View(profileModel);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ProfileViewModel profileModel)
        {
            
            var existingName = await userManager.Users.FirstOrDefaultAsync(u => u.UserName == profileModel.Username && u.Id!=profileModel.Id);
            if (existingName != null)
            {
                ModelState.AddModelError("Username", "The username already exists");
                return View(profileModel);
            }
            var existingEmail = await userManager.Users.FirstOrDefaultAsync(u => u.Email == profileModel.Username && u.Id != profileModel.Id);
            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "The email already exists");
                return View(profileModel);
            }
            var existingNumber = await dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == profileModel.Phone && u.Id != profileModel.Id);
            if (existingNumber != null)
            {
                ModelState.AddModelError("Phone", "The phone number already exists");
                return View(profileModel);
            }

            var user = await userManager.FindByIdAsync(profileModel.Id);
            if (user == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                return View(profileModel);
            }

            user.FirstName= profileModel.FirstName;
            user.LastName= profileModel.LastName;
            user.UserName= profileModel.Username;
            user.Email= profileModel.Email;
            user.PhoneNumber= profileModel.Phone;
            user.DateOfBirth= profileModel.DateOfBirth;
            if (profileModel.ProfilePicture != null && profileModel.ProfilePicture.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
                    Directory.CreateDirectory(uploadsFolder);
                    var extension = Path.GetExtension(profileModel.ProfilePicture.FileName).ToLower();
                    var uniqueFileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileModel.ProfilePicture.CopyToAsync(stream);
                    }

                    user.ProfilePicturePath = "/images/profiles/" + uniqueFileName; // store relative path
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ProfileImage", "Error saving profile picture: " + ex.Message);
                    return View(profileModel);
                }
            }
            
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach( var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }    
            }
            else
            {
                await signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Profile", "User"); 
            }

            return View(profileModel);

        }

    }
}
