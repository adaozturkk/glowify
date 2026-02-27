using Glowify.Data;
using Glowify.Data.DbInitializer;
using Glowify.Models;
using Glowify.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Glowify.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
            {
                return;
            }

            _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();

            var adminUser = new ApplicationUser
            {
                UserName = "admin@glowify.com",
                Email = "admin@glowify.com",
                Name = "Glowify Admin",
                EmailConfirmed = true
            };

            var employeeUser = new ApplicationUser
            {
                UserName = "employee@glowify.com",
                Email = "employee@glowify.com",
                Name = "Glowify Staff",
                EmailConfirmed = true
            };

            _userManager.CreateAsync(adminUser, "Admin123*").GetAwaiter().GetResult();
            _userManager.CreateAsync(employeeUser, "Employee123*").GetAwaiter().GetResult();

            ApplicationUser adminUserFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@glowify.com");
            _userManager.AddToRoleAsync(adminUserFromDb, SD.Role_Admin).GetAwaiter().GetResult();

            ApplicationUser employeeUserFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "employee@glowify.com");
            _userManager.AddToRoleAsync(employeeUserFromDb, SD.Role_Employee).GetAwaiter().GetResult();

            var products = new List<Product>
            {
                new Product
                {
                    Name = "Gentle Foaming Cleanser",
                    Description = "A gentle daily cleanser that removes dirt and excess oil without drying the skin.",
                    ListPrice = 249.90,
                    Price = 199.90,
                    ImageUrl = @"\images\products\cleanser1.jpg",
                    TargetSkinType = SkinType.All,
                    Category = CategoryType.Cleanser,
                    MainIngredient = "Green Tea Extract",
                    IngredientsList = "Water, Glycerin, Green Tea Extract, Cocamidopropyl Betaine",
                    Stock = 150
                },
                new Product
                {
                    Name = "Salicylic Acid Cleanser",
                    Description = "Deep cleansing gel that helps unclog pores and reduce acne.",
                    ListPrice = 279.90,
                    Price = 229.90,
                    ImageUrl = @"\images\products\cleanser2.jpg",
                    TargetSkinType = SkinType.Oily,
                    Category = CategoryType.Cleanser,
                    MainIngredient = "Salicylic Acid",
                    IngredientsList = "Water, Salicylic Acid, Niacinamide, Zinc PCA",
                    Stock = 120
                },
                new Product
                {
                    Name = "Hydrating Daily Moisturizer",
                    Description = "Lightweight moisturizer that provides long-lasting hydration.",
                    ListPrice = 319.90,
                    Price = 269.90,
                    ImageUrl = @"\images\products\moisturizer1.jpg",
                    TargetSkinType = SkinType.Normal,
                    Category = CategoryType.Moisturizer,
                    MainIngredient = "Hyaluronic Acid",
                    IngredientsList = "Water, Hyaluronic Acid, Panthenol, Ceramides",
                    Stock = 200
                },
                new Product
                {
                    Name = "Rich Repair Cream",
                    Description = "Intensive moisturizing cream for dry and damaged skin.",
                    ListPrice = 359.90,
                    Price = 299.90,
                    ImageUrl = @"\images\products\moisturizer2.jpg",
                    TargetSkinType = SkinType.Dry,
                    Category = CategoryType.Moisturizer,
                    MainIngredient = "Shea Butter",
                    IngredientsList = "Shea Butter, Glycerin, Vitamin E, Ceramides",
                    Stock = 90
                },
                new Product
                {
                    Name = "Vitamin C Brightening Serum",
                    Description = "Boosts radiance and helps even out skin tone.",
                    ListPrice = 399.90,
                    Price = 329.90,
                    ImageUrl = @"\images\products\serum1.jpg",
                    TargetSkinType = SkinType.All,
                    Category = CategoryType.Serum,
                    MainIngredient = "Vitamin C",
                    IngredientsList = "Water, Ascorbic Acid, Ferulic Acid, Vitamin E",
                    Stock = 110
                },
                new Product
                {
                    Name = "Niacinamide Balancing Serum",
                    Description = "Helps control oil production and minimize pores.",
                    ListPrice = 379.90,
                    Price = 309.90,
                    ImageUrl = @"\images\products\serum2.jpg",
                    TargetSkinType = SkinType.Combination,
                    Category = CategoryType.Serum,
                    MainIngredient = "Niacinamide",
                    IngredientsList = "Water, Niacinamide, Zinc PCA, Allantoin",
                    Stock = 130
                },
                new Product
                {
                    Name = "SPF 50+ Daily Sunscreen",
                    Description = "High protection sunscreen for daily use.",
                    ListPrice = 349.90,
                    Price = 289.90,
                    ImageUrl = @"\images\products\sunscreen1.jpg",
                    TargetSkinType = SkinType.All,
                    Category = CategoryType.Sunscreen,
                    MainIngredient = "UV Filters",
                    IngredientsList = "Zinc Oxide, Titanium Dioxide, Aloe Vera",
                    Stock = 180
                },
                new Product
                {
                    Name = "Sensitive Skin Sunscreen SPF 30",
                    Description = "Gentle sunscreen specially formulated for sensitive skin.",
                    ListPrice = 329.90,
                    Price = 269.90,
                    ImageUrl = @"\images\products\sunscreen2.jpg",
                    TargetSkinType = SkinType.Sensitive,
                    Category = CategoryType.Sunscreen,
                    MainIngredient = "Mineral UV Filters",
                    IngredientsList = "Zinc Oxide, Chamomile Extract, Panthenol",
                    Stock = 140
                },
                new Product
                {
                    Name = "Hydrating Facial Toner",
                    Description = "Refreshes and hydrates skin after cleansing.",
                    ListPrice = 239.90,
                    Price = 189.90,
                    ImageUrl = @"\images\products\toner1.jpg",
                    TargetSkinType = SkinType.Dry,
                    Category = CategoryType.Toner,
                    MainIngredient = "Rose Water",
                    IngredientsList = "Rose Water, Glycerin, Aloe Vera",
                    Stock = 160
                },
                new Product
                {
                    Name = "Pore Tightening Toner",
                    Description = "Helps refine pores and balance oily skin.",
                    ListPrice = 259.90,
                    Price = 209.90,
                    ImageUrl = @"\images\products\toner2.jpg",
                    TargetSkinType = SkinType.Oily,
                    Category = CategoryType.Toner,
                    MainIngredient = "Witch Hazel",
                    IngredientsList = "Witch Hazel, Niacinamide, Zinc",
                    Stock = 150
                },
                new Product
                {
                    Name = "Hydrating Sheet Mask",
                    Description = "Instant hydration and glow with a single-use sheet mask.",
                    ListPrice = 89.90,
                    Price = 69.90,
                    ImageUrl = @"\images\products\mask1.jpg",
                    TargetSkinType = SkinType.All,
                    Category = CategoryType.Mask,
                    MainIngredient = "Hyaluronic Acid",
                    IngredientsList = "Hyaluronic Acid, Aloe Vera, Glycerin",
                    Stock = 300
                },
                new Product
                {
                    Name = "Purifying Clay Mask",
                    Description = "Deep cleansing clay mask that removes impurities.",
                    ListPrice = 299.90,
                    Price = 249.90,
                    ImageUrl = @"\images\products\mask2.jpg",
                    TargetSkinType = SkinType.Combination,
                    Category = CategoryType.Mask,
                    MainIngredient = "Kaolin Clay",
                    IngredientsList = "Kaolin, Bentonite, Charcoal",
                    Stock = 100
                },
                new Product
                {
                    Name = "Revitalizing Eye Cream",
                    Description = "Reduces the appearance of dark circles and fine lines.",
                    ListPrice = 349.90,
                    Price = 299.90,
                    ImageUrl = @"\images\products\eye1.jpg",
                    TargetSkinType = SkinType.All,
                    Category = CategoryType.Eye,
                    MainIngredient = "Caffeine",
                    IngredientsList = "Caffeine, Peptides, Hyaluronic Acid",
                    Stock = 80
                }
            };

            _db.Products.AddRange(products);
            _db.SaveChanges();
        }
    }
}
