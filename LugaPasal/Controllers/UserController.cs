using Humanizer;
using LugaPasal.Data;
using LugaPasal.Entities;
using LugaPasal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using System.Data;
using System.Security.Claims;


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
                var user = await userManager.GetUserAsync(User);
                var role = await userManager.GetRolesAsync(user);

                if (role.Contains("Admin"))
                {
                    HttpContext.Session.SetString("Role", "Admin");
                }
                else
                {
                    HttpContext.Session.SetString("Role", "User");
                }
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

            var user = await userManager.GetUserAsync(User);
            var product = await dbContext.Products.Where(p => p.UserId == user.Id)
                                                    .Include(p => p.Ratings)
                                                   .ToListAsync();
            var userRating = 0.0;
            foreach (var pro in product)
            {
                userRating = userRating + (pro.Ratings != null && pro.Ratings.Any() ? pro.Ratings.Average(p => p.RatingValue) : 0.0);

            }
            var rating = userRating / product.Count();
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
                    ProductsOfUser = product,
                    Rating = rating

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
                                                  .Include(p => p.Ratings)
                                                  .ToListAsync();
            var userRating = 0.0;
            foreach (var pro in product)
            {
                userRating = userRating + (pro.Ratings != null && pro.Ratings.Any() ? pro.Ratings.Average(p => p.RatingValue) : 0.0);

            }
            var rating = userRating / product.Count();
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
                    ProductsOfUser = product,
                    Rating = rating

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

            var existingName = await userManager.Users.FirstOrDefaultAsync(u => u.UserName == profileModel.Username && u.Id != profileModel.Id);
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

            user.FirstName = profileModel.FirstName;
            user.LastName = profileModel.LastName;
            user.UserName = profileModel.Username;
            user.Email = profileModel.Email;
            user.PhoneNumber = profileModel.Phone;
            user.DateOfBirth = profileModel.DateOfBirth;
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
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
            }
            else
            {
                await signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "The profile was updated successfully!";
                return RedirectToAction("Profile", "User");
            }

            return View(profileModel);

        }

        public async Task<IActionResult> AddFavorites(Guid id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            var existingFavorite = await dbContext.Favorites.FirstOrDefaultAsync(p => p.UserID == user.Id && p.ProductID == id);
            if (existingFavorite == null)
            {
                var favorite = new Favorites
                {
                    FavoritesID = Guid.NewGuid(),
                    ProductID = id,
                    UserID = user.Id
                };
                await dbContext.Favorites.AddAsync(favorite);
                await dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product added to your favorites.";
                return RedirectToAction("ProductProfile", "Product", new { id });
            }
            else
            {
                TempData["ErrorMessage"] = "This product is already inn your favorites.";
                return RedirectToAction("ProductProfile", "Product", new { id });
            }
        }
        [HttpGet]

        public async Task<IActionResult> Favorites()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }
            var favorites = await dbContext.Favorites
                .Include(f => f.Product)
                .Where(f => f.UserID == user.Id)
                .ToListAsync();

            return View(favorites);
        }

        public async Task<IActionResult> RemoveFromFavorites(Guid id)
        {
            var productToRemove = await dbContext.Favorites.FirstOrDefaultAsync(p => p.ProductID == id);
            if (productToRemove != null)
            {
                dbContext.Favorites.Remove(productToRemove);
                await dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "The product has been sucessfully removed from favorites";
                return RedirectToAction("Favorites", "User");
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to remove the product from favorites";
                return RedirectToAction("favorites", "User");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            var totalProducts = await dbContext.Products.CountAsync();
            var productWithReviews = dbContext.Products.Count(p => p.Ratings.Any());
            var topRatedProductId = dbContext.Ratings.
                 GroupBy(r => r.ProductID)
                 .Select(g => new { ProductID = g.Key, AvgRating = g.Average(r => r.RatingValue) })
                 .OrderByDescending(g => g.AvgRating)
                 .FirstOrDefault();

            Products topRatedProduct = null;

            if (topRatedProductId != null)
            {
                topRatedProduct = dbContext.Products.FirstOrDefault(p => p.ProductID == topRatedProductId.ProductID.Value);
            }
            var LowestRatedProductId = dbContext.Ratings.GroupBy(p => p.ProductID)
                                            .Select(g => new { ProductID = g.Key, AvgRating = g.Average(g => g.RatingValue) })
                                            .OrderBy(p => p.AvgRating)
                                            .FirstOrDefault();

            Products LowestRatedProduct = null;
            if (LowestRatedProductId != null)
            {
                LowestRatedProduct = dbContext.Products.FirstOrDefault(p => p.ProductID == LowestRatedProductId.ProductID.Value);
            }


            var dashboardModel = new DashboardModel
            {
                totalUser = await dbContext.Users.CountAsync(),
                totalProducts = totalProducts,
                highRatedProducts = dbContext.Ratings.Count(p => p.RatingValue >= 4),
                categoryCounts = await dbContext.Products
                               .GroupBy(p => string.IsNullOrEmpty(p.ProductCategory) ? "Uncategorized" : p.ProductCategory)
                               .ToDictionaryAsync(
                                   g => g.Key,
                                   g => g.Count()
                               ),
                outOfStock = dbContext.Products.Count(p => p.ProductQuantity == 0),
                averageRating = dbContext.Ratings.Average(r => r.RatingValue),
                lowRated = dbContext.Ratings.Count(p => p.RatingValue <= 2),
                TotalReviews = dbContext.Ratings.Count(p => !string.IsNullOrEmpty(p.Review)),
                expensiveProducts = dbContext.Products.OrderByDescending(p => p.ProductPrice).Take(5).ToList(),
                averagePrice = dbContext.Products.Average(p => p.ProductPrice),
                lowStock = dbContext.Products.Count(p => p.ProductQuantity <= 5),
                topRatedProduct = topRatedProduct,
                lowRatedProduct = LowestRatedProduct,
                productWithReviews = productWithReviews,
                percentageReview = (double)productWithReviews / totalProducts * 100
            };
            return View(dashboardModel);
        }
        [HttpGet]
        public async Task<IActionResult> PostReview(Guid id)
        {
            var product = await dbContext.Products.Include(p => p.Ratings)
                                                  .FirstOrDefaultAsync(p => p.ProductID == id);
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to post a review.";
                return RedirectToAction("Login", "User");
            }

            else if (product != null)
            {
                var reviewModel = new ReviewModel
                {
                    product = product,
                    RatingValue = product.Ratings.Where(r => r.ProductID == id && r.UserID == user.Id).Any() ? product.Ratings.FirstOrDefault(r => r.ProductID == id && r.UserID == user.Id).RatingValue : 0,
                    UserID = user.Id,
                    Review = string.Empty

                };
                return View(reviewModel);
            }
            else
            {
                TempData["ErrorMessage"] = " Unable to post a review";
                return RedirectToAction("ProductProfile", "Product", id);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostReview(string Review, int ratingValue, Guid id)
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.ProductID == id);
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to post a review.";
                return RedirectToAction("Login", "User");
            }
            var ratings = await dbContext.Ratings.FirstOrDefaultAsync(p => p.ProductID == id && p.UserID == user.Id);
            if (ratings != null)
            {
                ratings.Review = Review;
                ratings.RatingValue = ratingValue;
                try
                {
                    var result = dbContext.Ratings.Update(ratings);
                    TempData["SuccessMessage"] = "The review has been posted successfully!!";
                    await dbContext.SaveChangesAsync();
                    return View(new ReviewModel { RatingValue = ratingValue, Review = Review, UserID = user.Id, product = product });
                }
                catch
                {

                    TempData["ErrorMessage"] = "Unable to post a review! Please, Try Again!";
                    return View(new ReviewModel { RatingValue = ratingValue, Review = Review, UserID = user.Id, product = product });
                }
            }
            else
            {
                Ratings rating = new Ratings
                {
                    RatingID = Guid.NewGuid(),
                    ProductID = id,
                    UserID = user.Id,
                    RatingValue = ratingValue,
                    Review = Review,
                };
                var result = await dbContext.Ratings.AddAsync(rating);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "The review has been posted successfully!!";
                    await dbContext.SaveChangesAsync();
                    return View(new ReviewModel { RatingValue = ratingValue, Review = Review, UserID = user.Id, product = product });
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to post a review! Please, Try Again!";
                    return View(new ReviewModel { RatingValue = ratingValue, Review = Review, UserID = user.Id, product = product });
                }

            }

        }
        public async Task<IActionResult> MyOrders()
        {
            var user = await userManager.GetUserAsync(User);

            if(user == null)
            {
                TempData["ErrorMessage"]= "Please login in first!";
                return RedirectToAction("Login", "User");
            }

            List<Orders> orders = await dbContext.Orders
                                      .Include(o => o.Product)  
                                      .Include(o => o.User)     
                                      .Where(o => o.UserID == user.Id)
                                      .OrderByDescending(o => o.OrderDate)
                                      .ToListAsync();

            return View(orders);
        }
    }   
}
