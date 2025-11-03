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
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Primitives;

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
        //[HttpGet]
        //public async Task<IActionResult> ListProducts()
        //{
        //    var products = await dbContext.Products
        //                                  .Include(p => p.User)
        //                                  .OrderByDescending(p => p.ProductID)
        //                                  .ToListAsync();

        //    return View(products);
        //}

        [HttpGet]
        public async Task<IActionResult> ListProducts(int page, string? searchString, int? minPrice, int? maxPrice, int? quantity, string? category, string? rating)
        {
            var usersQuery = dbContext.Products.Include(p => p.User)
                                                .Include(p => p.Ratings)
                                                .AsQueryable();

            bool isFiltering = minPrice != null || maxPrice != null || quantity != null || !string.IsNullOrEmpty(searchString) ||
                       (!string.IsNullOrEmpty(category) && category != "All") ||
                       (!string.IsNullOrEmpty(rating) && rating != "None");

            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(p =>
                    p.ProductName.Contains(searchString) ||
                    p.ProductDescription.Contains(searchString));
            }
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
            if (!string.IsNullOrEmpty(rating) && (rating != "None"))
            {
                if (rating == "lowRated")
                {
                    usersQuery = usersQuery.OrderBy(p => p.Ratings.Any() ? p.Ratings.Average(p => p.RatingValue) : 0);
                }
                else if (rating == "highRated")
                {
                    usersQuery = usersQuery.OrderByDescending(p => p.Ratings.Any() ? p.Ratings.Average(p => p.RatingValue) : 0);
                }
            }
            else if (!isFiltering)
            {
                usersQuery = usersQuery.OrderBy(p => Guid.NewGuid())
                                        .Distinct();

            }
            if (page < 1)
            {
                page = 1;
            }
            int pageSize = 12;
            var products = await usersQuery.Skip((page - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToListAsync();

            int totalProducts = await dbContext.Products.CountAsync();
            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            return View(products);
        }

        //[HttpGet]
        //public async Task<IActionResult> ListProductsFromSearch(string searchString)
        //{
        //    var productsQuery = dbContext.Products
        //                                 .Include(p => p.User)
        //                                 .AsQueryable();


        //    var products = await productsQuery
        //                        .OrderByDescending(p => p.ProductID)
        //                        .ToListAsync();

        //    return View("ListProducts", products); // Reuse same view
        //}



        [HttpGet]
        public async Task<IActionResult> ProductProfile(Guid id)
        {
            var user = await userManager.GetUserAsync(User);
            var foundProduct = await dbContext.Products
                                         .Include(p => p.User)
                                         .Include(p => p.Ratings)
                                         .FirstOrDefaultAsync(p => p.ProductID == id);


            if (foundProduct == null)
            {
                return NotFound();
            }

            var foundRecommendedProducts = await dbContext.Products.Include(p => p.User)
                                                              .OrderBy(p => Guid.NewGuid())
                                                              .Take(4)
                                                              .ToListAsync();
            var reviews = await dbContext.Ratings.Where(p => p.ProductID == id)
                                                    .Include(p => p.User)
                                                    .OrderByDescending(r => !string.IsNullOrEmpty(r.Review))
                                                    .Take(10)
                                                    .ToListAsync();


            var productModel = new ProductProfileModel
            {
                product = foundProduct,
                recommendedProducts = foundRecommendedProducts,
                reviews = reviews
            };
            return View(productModel);
        }

        [HttpGet]
        public async Task<IActionResult> AddToCart(Guid id)
        {
            var user = await userManager.GetUserAsync(User);
            if(user ==null)
            {
                return RedirectToAction("Login", "User");
            }
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
            var couponSession = HttpContext.Session.GetString("Coupon");
            if (sessionCart != null && couponSession != null)
            {
                var coupon = JsonSerializer.Deserialize<Coupons>(couponSession);
                ViewBag.Coupon = coupon;
            }
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
                TempData["ErrorMessage"] = "Item not found in the cart";
            }
            return RedirectToAction("Cart", "Product");
        }

        public async Task<IActionResult> Checkout(string couponCode)
        {
            var sessionCart = HttpContext.Session.GetString("Cart");
            var user = await userManager.GetUserAsync(User);
            List<Cart>? cart = JsonSerializer.Deserialize<List<Cart>>(sessionCart);
            decimal total = cart.Sum(p => p.ProductPrice * p.Quantity);
            decimal discountedPrice = total;
            if (!string.IsNullOrEmpty(couponCode))
            {
                var coupon = await dbContext.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode);
                if (coupon != null)
                {
                    discountedPrice = total - (total * coupon.DiscountAmount / 100);
                }
            }
            if (cart != null && cart.Any())
            {
                try
                {
                    var orderID = Guid.NewGuid();
                    var order = new Orders
                    {
                        OrderId = orderID,
                        OrderDate = DateTime.UtcNow,
                        UserID = user.Id,
                        OrderStatus = "Pending",
                        OrderTotalPrice= discountedPrice,
                        Items = new List<OrderItems>()
                    };
                    foreach (var product in cart)
                    {

                        var item = await dbContext.Products.FirstAsync(r => r.ProductID == product.ProductID);
                        if (item == null)
                        {
                            TempData["ErrorMessage"] = "One or more products in your cart are no longer available.";
                            return RedirectToAction("Cart", "Product");
                        }
                        if (item.ProductQuantity >= 1)
                        {

                            var orderItem = new OrderItems
                            {
                                OrderItemsId = Guid.NewGuid(),
                                OrderId = orderID,
                                ProductID = product.ProductID,
                                Quantity = product.Quantity,
                                UnitPrice = product.ProductPrice,
                                TotalPrice = product.ProductPrice * product.Quantity

                            };

                            order.Items.Add(orderItem);
                            
                        }

                        else
                        {
                            TempData["ErrorMessage"] = "This product is out of stock! Sorry, for the inconvenices caused! ";
                            return RedirectToAction("Cart", "Product");
                        }

                    }
                  
                    await dbContext.Orders.AddAsync(order);
                    await dbContext.SaveChangesAsync();
                    HttpContext.Session.Remove("Coupon");
                    HttpContext.Session.Remove("Cart");
                    TempData["SuccessMessage"] = "Your order has been placed successfully!";
                    return RedirectToAction("ListProducts", "Product");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Unable to place order. Please try again!";
                    return RedirectToAction("Cart", "Product");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add items to your cart before checking out.";
                return RedirectToAction("Cart", "Product");
            }
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
        [HttpPost]
        public async Task<IActionResult> Ratings(Guid productID, int ratingValue, string? review)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Please Log In First!";
                return RedirectToAction("Login", "User");
            }
            var existingRating = await dbContext.Ratings
                                                    .FirstOrDefaultAsync(p => p.ProductID == productID && p.UserID == user.Id);
            if (existingRating != null)
            {
                existingRating.RatingValue = ratingValue;
                existingRating.Review = review;
                dbContext.Ratings.Update(existingRating);
            }
            else
            {
                var ratings = new Ratings
                {
                    RatingID = Guid.NewGuid(),
                    ProductID = productID,
                    UserID = user.Id,
                    Review = review,
                    RatingValue = ratingValue,
                };
                await dbContext.Ratings.AddAsync(ratings);

            }
            await dbContext.SaveChangesAsync();
            return RedirectToAction("ProductProfile", new { id = productID });
        }

        public IActionResult ManipulateQuantity(Guid id, string quantityAction)
        {
            var sessionCart = HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(sessionCart))
                return RedirectToAction("Cart", "Product");

            var cart = JsonSerializer.Deserialize<List<Cart>>(sessionCart);
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Cart", "Product");
            }

            var product = cart.FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return RedirectToAction("Cart", "Product");
            }

            if (quantityAction == "Increase")
            {
                product.Quantity += 1;
            }

            else if (quantityAction == "Decrease")
            {
                if (product.Quantity > 1)
                {
                    product.Quantity -= 1;
                }
                else
                {
                    TempData["ErrorMessage"] = "Quantity cannot be less than 1.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to change the quantity of the product!";
            }

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
            return RedirectToAction("Cart", "Product");
        }
        public async Task<IActionResult> Coupon(String code)
        {
            var coupon = await dbContext.Coupons.FirstOrDefaultAsync(c => c.Code == code);
            if (coupon == null)
            {
                TempData["ErrorMessage"] = "Invalid Coupon Code.";
                return RedirectToAction("Cart", "Product");
            }
            if (coupon.ExpiryDate < DateTime.UtcNow)
            {
                TempData["ErrorMessage"] = "This coupon has expired.";
                return RedirectToAction("Cart", "Product");
            }
            HttpContext.Session.SetString("Coupon",JsonSerializer.Serialize(coupon));
            ViewBag.Coupon = coupon;
            TempData["SuccessMessage"] = $"Coupon applied! You saved {coupon.DiscountAmount}%.";
            return RedirectToAction("Cart", "Product");
        }

        [HttpGet]
        public async Task<IActionResult> AddCoupons()
        {
            var allCoupons = await dbContext.Coupons.ToListAsync();
            return View(allCoupons);
        }
        [HttpPost]
        public async Task<IActionResult> AddCoupons(string Code,decimal DiscountAmount, DateTime ExpiryDate)
        {
            var coupons = new Coupons
            {
                CouponID = Guid.NewGuid(),
                Code = Code,
                DiscountAmount= DiscountAmount,
                ExpiryDate= ExpiryDate
            };

            if (ModelState.IsValid)
            {
                await dbContext.Coupons.AddAsync(coupons);
                TempData["SuccessMessage"] = "The coupon has been successfully added into the system!";
                await dbContext.SaveChangesAsync();
                return RedirectToAction("AddCoupons", "Product");

            }
            else
            {
                TempData["ErrorMessage"] = "Coudn't add the coupon to the system";
                return RedirectToAction("AddCoupons", "Product");
            }

        }
    }


}


