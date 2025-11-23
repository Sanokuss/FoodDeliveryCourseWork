using CourseWork.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace CourseWork.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Apply pending migrations
            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                // Log the error (you might want to use ILogger in a real application)
                Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
            }
            
            // Always check and add missing columns manually (in case migration doesn't apply)
            // This ensures columns exist even if migration system doesn't recognize the migration
            try
            {
                var connection = context.Database.GetDbConnection();
                var wasOpen = connection.State == System.Data.ConnectionState.Open;
                if (!wasOpen)
                {
                    await connection.OpenAsync();
                }
                
                using var command = connection.CreateCommand();
                
                // Check and add IsNew column
                command.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'IsNew')
                    BEGIN
                        ALTER TABLE [dbo].[Products] ADD [IsNew] bit NOT NULL DEFAULT 0;
                        PRINT 'Column IsNew added successfully';
                    END";
                await command.ExecuteNonQueryAsync();
                
                // Check and add IsBestSeller column
                command.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'IsBestSeller')
                    BEGIN
                        ALTER TABLE [dbo].[Products] ADD [IsBestSeller] bit NOT NULL DEFAULT 0;
                        PRINT 'Column IsBestSeller added successfully';
                    END";
                await command.ExecuteNonQueryAsync();
                
                if (!wasOpen)
                {
                    await connection.CloseAsync();
                }
                Console.WriteLine("Database columns verified/added successfully.");
            }
            catch (Exception manualEx)
            {
                Console.WriteLine($"Warning: Failed to verify/add columns manually: {manualEx.Message}");
                Console.WriteLine($"Stack trace: {manualEx.StackTrace}");
                // Don't throw - let the application continue, it will fail later if columns are missing
            }

            // Create Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Create Admin User
            if (await userManager.FindByEmailAsync("admin@fooddelivery.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@fooddelivery.com",
                    Email = "admin@fooddelivery.com",
                    EmailConfirmed = true,
                    FullName = "Адміністратор"
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Check if database is already seeded
            if (context.Categories.Any())
            {
                return;
            }

            // Seed Categories
            var categories = new Category[]
            {
                new Category { Name = "Піца", DisplayOrder = 1 },
                new Category { Name = "Суші", DisplayOrder = 2 },
                new Category { Name = "Напої", DisplayOrder = 3 },
                new Category { Name = "Десерти", DisplayOrder = 4 },
                new Category { Name = "Бургери", DisplayOrder = 5 },
                new Category { Name = "Салати", DisplayOrder = 6 },
                new Category { Name = "Снеки", DisplayOrder = 7 }
            };

            foreach (var category in categories)
            {
                context.Categories.Add(category);
            }
            await context.SaveChangesAsync();

            // Seed Products
            var products = new Product[]
            {
                // Піца
                new Product
                {
                    Name = "Піца Папероні",
                    Description = "Класична піца з гострою ковбасою папероні, моцарелою та томатним соусом",
                    Price = 299.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=500&h=500&fit=crop",
                    CategoryId = categories[0].Id,
                    IsNew = true,
                    IsBestSeller = true
                },
                new Product
                {
                    Name = "Піца Маргарита",
                    Description = "Традиційна італійська піца з моцарелою, томатами та базиліком",
                    Price = 249.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=500&h=500&fit=crop",
                    CategoryId = categories[0].Id
                },
                new Product
                {
                    Name = "Піца Чотири Сири",
                    Description = "Піца з моцарелою, горгонзолою, пармезаном та чеддером",
                    Price = 329.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=500&h=500&fit=crop",
                    CategoryId = categories[0].Id
                },
                new Product
                {
                    Name = "Піца Гавайська",
                    Description = "Піца з куркою, ананасами та моцарелою",
                    Price = 279.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1628840042765-356cda07504e?w=500&h=500&fit=crop",
                    CategoryId = categories[0].Id
                },
                new Product
                {
                    Name = "Піца М'ясна",
                    Description = "Піца з беконом, салямі, ковбасою та моцарелою",
                    Price = 349.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1628840042765-356cda07504e?w=500&h=500&fit=crop",
                    CategoryId = categories[0].Id,
                    IsBestSeller = true
                },
                // Суші
                new Product
                {
                    Name = "Філадельфія з лососем",
                    Description = "Роли з лососем, авокадо та вершковим сиром",
                    Price = 350.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=500&h=500&fit=crop",
                    CategoryId = categories[1].Id,
                    IsNew = true,
                    IsBestSeller = true
                },
                new Product
                {
                    Name = "Каліфорнія",
                    Description = "Роли з крабом, авокадо та огірком",
                    Price = 280.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1617196034183-421b4917c92d?w=500&h=500&fit=crop",
                    CategoryId = categories[1].Id
                },
                new Product
                {
                    Name = "Сет Філадельфія",
                    Description = "Набір ролів: Філадельфія класична, Філадельфія з тунцем, Філадельфія з креветкою",
                    Price = 450.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?w=500&h=500&fit=crop",
                    CategoryId = categories[1].Id
                },
                new Product
                {
                    Name = "Роли Дракон",
                    Description = "Роли з вугрем, авокадо та соусом унагі",
                    Price = 380.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1617196034183-421b4917c92d?w=500&h=500&fit=crop",
                    CategoryId = categories[1].Id
                },
                // Напої
                new Product
                {
                    Name = "Кока-Кола",
                    Description = "Освіжаючий газований напій, 0.5л",
                    Price = 45.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1554866585-cd94860890b7?w=500&h=500&fit=crop",
                    CategoryId = categories[2].Id
                },
                new Product
                {
                    Name = "Пепсі",
                    Description = "Газований напій, 0.5л",
                    Price = 45.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1554866585-cd94860890b7?w=500&h=500&fit=crop",
                    CategoryId = categories[2].Id
                },
                new Product
                {
                    Name = "Сік апельсиновий",
                    Description = "Натуральний сік, 0.5л",
                    Price = 55.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=500&h=500&fit=crop",
                    CategoryId = categories[2].Id
                },
                new Product
                {
                    Name = "Вода мінеральна",
                    Description = "Мінеральна вода, 0.5л",
                    Price = 30.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1548839140-5a941f94e0ea?w=500&h=500&fit=crop",
                    CategoryId = categories[2].Id
                },
                // Десерти
                new Product
                {
                    Name = "Тірамісу",
                    Description = "Класичний італійський десерт з кави та маскарпоне",
                    Price = 120.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=500&h=500&fit=crop",
                    CategoryId = categories[3].Id
                },
                new Product
                {
                    Name = "Чізкейк",
                    Description = "Ніжний чізкейк з ягідним соусом",
                    Price = 135.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1524351199678-941a58a3df50?w=500&h=500&fit=crop",
                    CategoryId = categories[3].Id
                },
                new Product
                {
                    Name = "Шоколадний торт",
                    Description = "Шоколадний торт з кремом",
                    Price = 150.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1578985545062-69928b1d9587?w=500&h=500&fit=crop",
                    CategoryId = categories[3].Id
                },
                // Бургери
                new Product
                {
                    Name = "Класичний бургер",
                    Description = "Бургер з яловичиною, салатом, томатом та соусом",
                    Price = 199.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&h=500&fit=crop",
                    CategoryId = categories[4].Id
                },
                new Product
                {
                    Name = "Чізбургер",
                    Description = "Бургер з яловичиною, сиром, салатом та соусом",
                    Price = 219.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&h=500&fit=crop",
                    CategoryId = categories[4].Id
                },
                new Product
                {
                    Name = "Чікенбургер",
                    Description = "Бургер з курячим філе, салатом та соусом",
                    Price = 189.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&h=500&fit=crop",
                    CategoryId = categories[4].Id
                },
                // Салати
                new Product
                {
                    Name = "Цезар з куркою",
                    Description = "Салат з куркою, салатом ромен, пармезаном та соусом цезар",
                    Price = 179.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1546793665-c74683f339c1?w=500&h=500&fit=crop",
                    CategoryId = categories[5].Id
                },
                new Product
                {
                    Name = "Грецький салат",
                    Description = "Салат з томатами, огірками, оливками, сиром фета та олією",
                    Price = 159.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1546793665-c74683f339c1?w=500&h=500&fit=crop",
                    CategoryId = categories[5].Id
                },
                new Product
                {
                    Name = "Салат з тунцем",
                    Description = "Салат з тунцем, яйцем, томатами та оливковою олією",
                    Price = 169.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1546793665-c74683f339c1?w=500&h=500&fit=crop",
                    CategoryId = categories[5].Id
                },
                // Снеки
                new Product
                {
                    Name = "Картопля фрі",
                    Description = "Хрустка картопля фрі з соусом",
                    Price = 89.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=500&h=500&fit=crop",
                    CategoryId = categories[6].Id
                },
                new Product
                {
                    Name = "Нагетси курячі",
                    Description = "Хрусткі курячі нагетси, 6 шт",
                    Price = 129.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?w=500&h=500&fit=crop",
                    CategoryId = categories[6].Id
                },
                new Product
                {
                    Name = "Крильця курячі",
                    Description = "Гострі курячі крильця з соусом",
                    Price = 149.00m,
                    ImageUrl = "https://images.unsplash.com/photo-1527477396000-e27163b481c2?w=500&h=500&fit=crop",
                    CategoryId = categories[6].Id
                }
            };

            foreach (var product in products)
            {
                context.Products.Add(product);
            }
            await context.SaveChangesAsync();
        }
    }
}

