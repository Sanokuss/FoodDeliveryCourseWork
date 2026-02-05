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
            await context.Database.EnsureCreatedAsync();

            // Create Roles
            if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!await roleManager.RoleExistsAsync("User")) await roleManager.CreateAsync(new IdentityRole("User"));

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
                if (result.Succeeded) await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // --- Seed Categories ---
            if (!context.Categories.Any())
            {
                var categories = new Category[]
                {
                    new Category { Name = "Піца", DisplayOrder = 1 },
                    new Category { Name = "Суші", DisplayOrder = 2 },
                    new Category { Name = "Бургери", DisplayOrder = 3 },
                    new Category { Name = "Напої", DisplayOrder = 4 },
                    new Category { Name = "Десерти", DisplayOrder = 5 },
                    new Category { Name = "Салати", DisplayOrder = 6 },
                    new Category { Name = "Снеки", DisplayOrder = 7 }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // --- Seed Restaurants ---
            if (!context.Restaurants.Any())
            {
                var restaurants = new Restaurant[]
                {
                    new Restaurant { Name = "Sushi Master", Description = "Найкращі суші та роли у місті. Свіжа риба щодня.", Address = "вул. Дерибасівська, 10", WorkingHours = "10:00 - 23:00", LogoUrl = "https://cdn-icons-png.flaticon.com/512/2276/2276931.png" },
                    new Restaurant { Name = "Pizza King", Description = "Справжня італійська піца з дров'яної печі.", Address = "пр. Шевченка, 5", WorkingHours = "09:00 - 22:00", LogoUrl = "https://cdn-icons-png.flaticon.com/512/1037/1037762.png" },
                    new Restaurant { Name = "Burger Joint", Description = "Соковиті бургери з мармурової яловичини.", Address = "вул. Грецька, 25", WorkingHours = "11:00 - 00:00", LogoUrl = "https://cdn-icons-png.flaticon.com/512/4264/4264850.png" },
                    new Restaurant { Name = "Green Salad", Description = "Здорове харчування, свіжі салати та смузі.", Address = "вул. Садова, 12", WorkingHours = "08:00 - 21:00", LogoUrl = "https://cdn-icons-png.flaticon.com/512/4264/4264770.png" },
                    new Restaurant { Name = "Sweet Dreams", Description = "Авторські десерти та найкраща кава.", Address = "пл. Соборна, 1", WorkingHours = "09:00 - 21:00", LogoUrl = "https://cdn-icons-png.flaticon.com/512/2935/2935394.png" }
                };
                context.Restaurants.AddRange(restaurants);
                await context.SaveChangesAsync();
            }

            // --- Seed Promotions ---
            if (!context.Promotions.Any())
            {
                var promotions = new Promotion[]
                {
                    new Promotion { Title = "1+1 на роли", Description = "Замовляй один рол Філадельфія і отримуй другий у подарунок!", DiscountPercent = 50, ImageUrl = "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?w=600&h=400&fit=crop", PromoCode = "SUSHI11" },
                    new Promotion { Title = "Щасливі години", Description = "Знижка -20% на все меню з 12:00 до 15:00 по буднях.", DiscountPercent = 20, ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=600&h=400&fit=crop", PromoCode = "HAPPYHOUR" },
                    new Promotion { Title = "Безкоштовна доставка", Description = "При замовленні від 500 грн доставка за наш рахунок.", DiscountPercent = 0, ImageUrl = "https://images.unsplash.com/photo-1619652576082-f54f76274488?w=600&h=400&fit=crop", PromoCode = "FREEDELIVERY" }
                };
                context.Promotions.AddRange(promotions);
                await context.SaveChangesAsync();
            }

            // --- Seed Products ---
            if (!context.Products.Any())
            {
                var cats = context.Categories.ToDictionary(c => c.Name, c => c.Id);
                var rests = context.Restaurants.ToDictionary(r => r.Name, r => r.Id);
                
                var products = new List<Product>
                {
                    // Pizza - Pizza King
                    new Product { Name = "Пепероні", Description = "Гостра ковбаса пепероні, моцарела, томатний соус, орегано.", Price = 210, CategoryId = cats["Піца"], RestaurantId = rests["Pizza King"], ImageUrl = "https://images.unsplash.com/photo-1628840042765-356cda07504e?auto=format&fit=crop&w=800&q=80", IsBestSeller = true },
                    new Product { Name = "Чотири Сири", Description = "Моцарела, пармезан, горгонзола, емменталь, вершковий соус.", Price = 240, CategoryId = cats["Піца"], RestaurantId = rests["Pizza King"], ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?auto=format&fit=crop&w=800&q=80" },
                    new Product { Name = "Маргарита", Description = "Класична піца з томатами, моцарелою та свіжим базиліком.", Price = 180, CategoryId = cats["Піца"], RestaurantId = rests["Pizza King"], ImageUrl = "https://images.unsplash.com/photo-1595854341625-f33ee10432fa?auto=format&fit=crop&w=800&q=80", IsNew = true },
                    new Product { Name = "Гавайська", Description = "Курка, свіжі ананаси, моцарела, томатний соус.", Price = 220, CategoryId = cats["Піца"], RestaurantId = rests["Pizza King"], ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=800&q=80" },
                    new Product { Name = "М'ясна", Description = "Бекон, шинка, салямі, мисливські ковбаски, барбекю соус.", Price = 260, CategoryId = cats["Піца"], RestaurantId = rests["Pizza King"], ImageUrl = "https://images.unsplash.com/photo-1590947132387-155cc02f3212?auto=format&fit=crop&w=800&q=80", IsBestSeller = true },
                    
                    // Sushi - Sushi Master
                    new Product { Name = "Філадельфія", Description = "Свіжий лосось, авокадо, огірок, ніжний крем-сир.", Price = 320, CategoryId = cats["Суші"], RestaurantId = rests["Sushi Master"], ImageUrl = "https://images.unsplash.com/photo-1611143669185-af224c5e3252?auto=format&fit=crop&w=800&q=80", IsBestSeller = true },
                    new Product { Name = "Каліфорнія", Description = "Сніжний краб, авокадо, огірок, ікра тобіко, японський майонез.", Price = 290, CategoryId = cats["Суші"], RestaurantId = rests["Sushi Master"], ImageUrl = "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?auto=format&fit=crop&w=800&q=80" },
                    new Product { Name = "Зелений Дракон", Description = "Копчений вугор, авокадо, унагі соус, кунжут, рис.", Price = 360, CategoryId = cats["Суші"], RestaurantId = rests["Sushi Master"], ImageUrl = "https://images.unsplash.com/photo-1579584425555-c3ce17fd4351?auto=format&fit=crop&w=800&q=80", IsNew = true },
                    new Product { Name = "Макі з лососем", Description = "Класичні роли зі свіжим лососем та норі.", Price = 150, CategoryId = cats["Суші"], RestaurantId = rests["Sushi Master"], ImageUrl = "https://images.unsplash.com/photo-1553621042-f6e147245754?auto=format&fit=crop&w=800&q=80" },
                    
                    // Burgers - Burger Joint
                    new Product { Name = "Чізбургер XL", Description = "Подвійна яловича котлета, подвійний чеддер, маринований огірок, соус.", Price = 230, CategoryId = cats["Бургери"], RestaurantId = rests["Burger Joint"], ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=800&q=80", IsBestSeller = true },
                    new Product { Name = "Чікен Бургер", Description = "Хрустка куряча котлета, свіжий салат, томат, фірмовий майонез.", Price = 190, CategoryId = cats["Бургери"], RestaurantId = rests["Burger Joint"], ImageUrl = "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?auto=format&fit=crop&w=800&q=80" },
                    new Product { Name = "BBQ Бургер", Description = "Яловичина, хрусткий бекон, цибулеві кільця, насичений BBQ соус.", Price = 250, CategoryId = cats["Бургери"], RestaurantId = rests["Burger Joint"], ImageUrl = "https://images.unsplash.com/photo-1594212699903-ec8a3eca50f5?auto=format&fit=crop&w=800&q=80", IsNew = true },

                    // Snacks - Burger Joint
                    new Product { Name = "Картопля Фрі", Description = "Золотиста хрустка картопля з сіллю.", Price = 80, CategoryId = cats["Снеки"], RestaurantId = rests["Burger Joint"], ImageUrl = "https://images.unsplash.com/photo-1630384060421-cb20d0e0649d?auto=format&fit=crop&w=800&q=80" },
                    new Product { Name = "Нагетси (9 шт)", Description = "Ніжне куряче філе в хрусткій паніровці.", Price = 120, CategoryId = cats["Снеки"], RestaurantId = rests["Burger Joint"], ImageUrl = "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d?auto=format&fit=crop&w=800&q=80" },
                    new Product { Name = "Цибулеві кільця", Description = "Смажені кільця цибулі в золотистому клярі.", Price = 95, CategoryId = cats["Снеки"], RestaurantId = rests["Burger Joint"], ImageUrl = "https://images.unsplash.com/photo-1639024471283-03518883512d?auto=format&fit=crop&w=800&q=80" },

                    // Desserts - Sweet Dreams
                    new Product { Name = "Тірамісу", Description = "Італійський десерт з маскарпоне, кавою та печивом савоярді.", Price = 140, CategoryId = cats["Десерти"], RestaurantId = rests["Sweet Dreams"], ImageUrl = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?auto=format&fit=crop&w=800&q=80" },
                    new Product { Name = "Чізкейк Нью-Йорк", Description = "Класичний вершковий чізкейк на пісочній основі.", Price = 130, CategoryId = cats["Десерти"], RestaurantId = rests["Sweet Dreams"], ImageUrl = "https://images.unsplash.com/photo-1533134242443-d4fd215305ad?auto=format&fit=crop&w=800&q=80", IsBestSeller = true },
                    new Product { Name = "Брауні", Description = "Шоколадний десерт з вологою серединкою та волоськими горіхами.", Price = 110, CategoryId = cats["Десерти"], RestaurantId = rests["Sweet Dreams"], ImageUrl = "https://images.unsplash.com/photo-1606313564200-e75d5e30476d?auto=format&fit=crop&w=800&q=80" },
                    
                    // Salads - Green Salad
                    new Product { Name = "Грецький салат", Description = "Свіжі огірки, помідори, оливки Каламата, сир фета, орегано.", Price = 120, CategoryId = cats["Салати"], RestaurantId = rests["Green Salad"], ImageUrl = "https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=800&q=80" },
                    new Product { Name = "Цезар", Description = "Салат айсберг, курка гриль, сухарики, пармезан, соус Цезар.", Price = 150, CategoryId = cats["Салати"], RestaurantId = rests["Green Salad"], ImageUrl = "https://images.unsplash.com/photo-1550304943-4f24f54ddde9?auto=format&fit=crop&w=800&q=80", IsBestSeller = true },
                    new Product { Name = "Овочевий мікс", Description = "Мікс свіжих сезонних овочів з оливковою олією.", Price = 95, CategoryId = cats["Салати"], RestaurantId = rests["Green Salad"], ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=800&q=80" },

                    // Drinks - Various Restaurants
                    new Product { Name = "Coca-Cola 0.5л", Description = "Класичний освіжаючий напій.", Price = 45, CategoryId = cats["Напої"], RestaurantId = rests["Burger Joint"], ImageUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?auto=format&fit=crop&w=800&q=80" },
                    new Product { Name = "Апельсиновий сік", Description = "Свіжовичавлений апельсиновий сік.", Price = 80, CategoryId = cats["Напої"], RestaurantId = rests["Green Salad"], ImageUrl = "https://images.unsplash.com/photo-1621506289937-a8e4df240d0b?auto=format&fit=crop&w=800&q=80" },
                    new Product { Name = "Лимонад", Description = "Домашній лимонад з м'ятою та льодом.", Price = 60, CategoryId = cats["Напої"], RestaurantId = rests["Pizza King"], ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?auto=format&fit=crop&w=800&q=80", IsBestSeller = true },
                    new Product { Name = "Американо", Description = "Ароматна чорна кава зі свіжообсмажених зерен.", Price = 50, CategoryId = cats["Напої"], RestaurantId = rests["Sweet Dreams"], ImageUrl = "https://images.unsplash.com/photo-1559496417-e7f25cb247f3?auto=format&fit=crop&w=800&q=80" }
                };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }
}

