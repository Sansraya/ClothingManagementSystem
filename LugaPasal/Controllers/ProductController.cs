using LugaPasal.Entities;
using LugaPasal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LugaPasal.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using System.Text.Json;

namespace LugaPasal.Controllers
{
    public class ProductController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly ApplicationDbContext dbContext;

        public ProductController(ApplicationDbContext dbContext, UserManager<User> userManager, SignInManager<User> signInManager)
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
        public IActionResult PostProduct()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PostProduct(PostProductModel productModel)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var product = new Products
            {
                ProductID = Guid.NewGuid(),
                ProductName = productModel.ProductName,
                ProductDescription = productModel.ProductDescription,
                ProductPrice = productModel.ProductPrice,
                ProductQuantity = productModel.ProductQuantity,
                ProductCategory = productModel.ProductCategory,
                UserId = user.Id
            };
            if (productModel.ProductImage != null && productModel.ProductImage.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
                    Directory.CreateDirectory(uploadsFolder);
                    var extension = Path.GetExtension(productModel.ProductImage.FileName).ToLower();
                    var uniqueFileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await productModel.ProductImage.CopyToAsync(stream);
                    }

                    product.ProductImagePath = "/images/profiles/" + uniqueFileName; // store relative path
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ProfileImage", "Error saving profile picture: " + ex.Message);
                    return View(productModel);
                }
            }
            await dbContext.Products.AddAsync(product);
            var count = await dbContext.SaveChangesAsync();
            if (count > 0)
            {
                TempData["SuccessMessage"] = "Product Posted Successfully";
                return View();
            }
            else
            {
                TempData["ErrorMessage"] = "Product Was Not Posted, Please Try Again!";
                return View(productModel);
            }


        }
        [HttpGet]
        public async Task<IActionResult> ListProducts()
        {
            var products = await dbContext.Products
                                          .Include(p => p.User)
                                          .OrderByDescending(p => p.ProductID)
                                          .ToListAsync();

            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> ListProducts(int? minPrice, int? maxPrice, int? quantity, string? category)
        {
            var usersQuery = dbContext.Products.Include(p => p.User)
                                                .AsQueryable();

            if (minPrice != null)
            {
                usersQuery = usersQuery.Where(p => p.ProductPrice >= minPrice);
            }
            if (maxPrice != null)
            {
                usersQuery = usersQuery.Where(p => p.ProductPrice <= maxPrice);
            }
            if (quantity != null)
            {
                usersQuery = usersQuery.Where(p => p.ProductQuantity >= quantity);
            }
            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                usersQuery = usersQuery.Where(p => p.ProductCategory == category);
            }
            var products = await usersQuery.ToListAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> ListProductsFromSearch(string searchString)
        {
            var productsQuery = dbContext.Products
                                         .Include(p => p.User)
                                         .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p =>
                    p.ProductName.Contains(searchString) ||
                    p.ProductDescription.Contains(searchString));
            }

            var products = await productsQuery
                                .OrderByDescending(p => p.ProductID)
                                .ToListAsync();

            return View("ListProducts", products); // Reuse same view
        }



        [HttpGet]
        public async Task<IActionResult> ProductProfile(Guid id)
        {

            var foundProduct = await dbContext.Products
                                         .Include(p => p.User)
                                         .FirstOrDefaultAsync(p => p.ProductID == id);


            if (foundProduct == null)
            {
                return NotFound();
            }

            var foundRecommendedProducts = await dbContext.Products.Include(p => p.User)
                                                              .OrderBy(p => Guid.NewGuid())
                                                              .Take(4)
                                                              .ToListAsync();
            var productModel = new ProductProfileModel
            {
                product = foundProduct,
                recommendedProducts=foundRecommendedProducts
            };
            return View(productModel);
        }

        [HttpGet]
        public async Task<IActionResult> AddToCart(Guid id)
        {

            var product = await dbContext.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Unable to add product to card";
                return RedirectToAction("ProductProfile", "Profile");
            }
            var sessionCart = HttpContext.Session.GetString("Cart");
            List<Cart>? cart = string.IsNullOrEmpty(sessionCart)
                                ? new List<Cart>()
                                : JsonSerializer.Deserialize<List<Cart>>(sessionCart);
            var existing = cart.FirstOrDefault(p => p.ProductID == id);
            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                var itemCart = new Cart
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ProductPrice = product.ProductPrice,
                    Quantity = 1,
                    ProductImagePath = product.ProductImagePath

                };
                cart.Add(itemCart);
            }

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
            TempData["SuccessMessage"] = "Items have been added to the cart successfully";
            return RedirectToAction("ProductProfile", new { id });

        }



        [HttpGet]
        public async Task<IActionResult> EditProduct(Guid id)
        {
            var product = await dbContext.Products
                                        .Include(p => p.User)
                                        .FirstOrDefaultAsync(p => p.ProductID == id);
            var productModel = new PostProductModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                ProductPrice = product.ProductPrice,
                ProductQuantity = product.ProductQuantity,
                ProductCategory = product.ProductCategory,
                ProductImagePath = product.ProductImagePath

            };

            return View(productModel);
        }
        [HttpPost]
        public async Task<IActionResult> EditProduct(PostProductModel productModel)
        {
            var product = await dbContext.Products
                                        .FirstOrDefaultAsync(p => p.ProductID == productModel.ProductID);
            product.ProductName = productModel.ProductName;
            product.ProductDescription = productModel.ProductDescription;
            product.ProductPrice = productModel.ProductPrice;
            product.ProductQuantity = productModel.ProductQuantity;
            product.ProductCategory = productModel.ProductCategory;

            if (productModel.ProductImage != null && productModel.ProductImage.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
                    Directory.CreateDirectory(uploadsFolder);
                    var extension = Path.GetExtension(productModel.ProductImage.FileName).ToLower();
                    var uniqueFileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await productModel.ProductImage.CopyToAsync(stream);
                    }

                    product.ProductImagePath = "/images/profiles/" + uniqueFileName; // store relative path
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ProfileImage", "Error saving profile picture: " + ex.Message);
                    return View(productModel);
                }
            }

            var result = dbContext.Products.Update(product);
            if (result != null)
            {
                TempData["SuccessMessage"] = "Product Updated Successfully";
                await dbContext.SaveChangesAsync();
                return RedirectToAction("ProductProfile", new { id = product.ProductID });
            }
            else
            {
                return View(productModel);
            }
        }

        [HttpGet]
        public IActionResult Cart()
        {
            var sessionCart = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(sessionCart)
                ? new List<Cart>()
                : JsonSerializer.Deserialize<List<Cart>>(sessionCart);
            return View(cart);
        }

        [HttpGet]
        public IActionResult RemoveFromCart(Guid id)
        {
            var sessionCart = HttpContext.Session.GetString("Cart");
            List<Cart>? cart = JsonSerializer.Deserialize<List<Cart>>(sessionCart);
            var product = cart.FirstOrDefault(p => p.ProductID == id);
            if (product != null)
            {
                cart.Remove(product);
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
                TempData["SuccessMessage"] = "Your item has been successfully removed.";
            }
            else
            {
                TempData["ErrorMessage"] = "Item nto found in the cart";
            }
            return RedirectToAction("Cart", "Product");
        }

        public async Task<IActionResult> SearchProduct(string searchQuery)
        {
            var usersQuery = dbContext.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                usersQuery = usersQuery.Where(p => p.ProductName.Contains(searchQuery) || p.ProductDescription.Contains(searchQuery));
            }
            var users = await usersQuery.ToListAsync();
            return View(users);
        }
    }
}

