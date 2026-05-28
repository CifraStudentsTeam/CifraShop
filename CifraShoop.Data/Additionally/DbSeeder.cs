using CifraShop.Data.AppDbContext;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Models; // предполагаем, что здесь лежат enum'ы StatusOrder и StatusProduct
using System;
using System.Collections.Generic;

namespace CifraShop.Data.Additionally
{

    //public static class DbSeeder
    //{
    //    public static (List<Admin> Admins, List<Student> Students, List<Product> Products, List<Order> Orders, List<OrderItem> OrderItems) GenerateSeedData()
    //    {
    //        var random = new Random(42); // фиксированное зерно для повторяемости

    //        // 1. Генерация 50 администраторов
    //        var admins = new List<Admin>();
    //        for (int i = 1; i <= 50; i++)
    //        {
    //            admins.Add(new Admin
    //            {
    //                Id = (uint)i,
    //                Name = $"AdminName{i}",
    //                SurName = $"AdminSurName{i}",
    //                EMail = $"admin{i}@cifrashop.com",
    //                Password = $"AdminPass{i}" // в реальном проекте хешировать
    //            });
    //        }

    //        // 2. Генерация 50 студентов (покупателей)
    //        var students = new List<Student>();
    //        var studentLogins = new List<string>();
    //        for (int i = 1; i <= 50; i++)
    //        {
    //            var login = $"student{i}";
    //            studentLogins.Add(login);
    //            students.Add(new Student
    //            {
    //                Id = (uint)i,
    //                LoginName = login,
    //                Password = $"StudentPass{i}",
    //                DateOfBirth = new DateTime(random.Next(1990, 2006), random.Next(1, 13), random.Next(1, 28)),
    //                Balance = (uint)random.Next(0, 50000)
    //            });
    //        }

    //        // 3. Генерация 50 товаров
    //        var productNames = new[]
    //        {
    //        "Ноутбук", "Смартфон", "Наушники", "Клавиатура", "Мышь", "Монитор", "Системный блок", "Принтер", "Сканер", "Веб-камера",
    //        "Флешка", "Внешний диск", "Чехол для телефона", "Зарядное устройство", "Батарейка", "Коврик для мыши", "Подставка для ноутбука",
    //        "USB-хаб", "Сетевой фильтр", "Кабель HDMI", "Адаптер", "Микрофон", "Колонки", "Гарнитура", "Стилус", "Планшет", "Электронная книга",
    //        "Фитнес-браслет", "Умные часы", "Роутер", "Сервер", "Видеокарта", "Процессор", "Материнская плата", "Оперативная память", "SSD накопитель",
    //        "Жесткий диск", "Блок питания", "Кулер", "Корпус", "Игровая консоль", "Джойстик", "VR-очки", "Трекбол", "Графический планшет",
    //        "Док-станция", "Сетевой коммутатор", "ИБП", "Проектор", "3D-принтер"
    //    };
    //        var products = new List<Product>();
    //        for (int i = 0; i < 50; i++)
    //        {
    //            var price = (uint)random.Next(500, 150000);
    //            var quantity = (uint)random.Next(0, 100);
    //            products.Add(new Product
    //            {
    //                Id = (uint)i,
    //                Name = productNames[i],
    //                Description = $"Описание товара {productNames[i]}",
    //                Price = price,
    //                Quantity = quantity,
    //                Status = (quantity == 0 ? StatusProduct.OutOfStock : StatusProduct.InStock),
    //                ThePathToTheImage = $"/images/product{i + 1}.jpg",
    //                IsSelected = false
    //            });
    //        }

    //        // 4. Генерация 50 заказов (каждый заказ привязан к случайному студенту)
    //        var orders = new List<Order>();
    //        //uint orderIdCounter = 1; // для связи с OrderItem будем использовать временные ID, но потом они заменятся БД. Здесь просто для вычислений.
    //        for (int i = 1; i <= 50; i++)
    //        {
    //            var order = new Order
    //            {
    //                Id = (uint)i,
    //                Status = (StatusOrder)random.Next(0, Enum.GetValues(typeof(StatusOrder)).Length),
    //                DateOfPurchase = DateTime.Now.AddDays(-random.Next(0, 365)),
    //                CustomerLogin = studentLogins[random.Next(studentLogins.Count)],
    //                Sum = 0 // временно, будет пересчитано после добавления позиций
    //            };
    //            orders.Add(order);
    //        }

    //        // 5. Генерация OrderItems (от 1 до 5 позиций на заказ)
    //        var orderItems = new List<OrderItem>();
    //        for (int i = 0; i < orders.Count; i++)
    //        {
    //            var order = orders[i];
    //            uint orderSum = 0;
    //            int itemsCount = random.Next(1, 6); // 1-5 позиций в заказе
    //            for (int j = 0; j < itemsCount; j++)
    //            {
    //                var product = products[random.Next(products.Count)];
    //                var quantity = (uint)random.Next(1, 4); // от 1 до 3 штук
    //                var priceAtPurchase = product.Price; // фиксируем цену товара на момент заказа
    //                orderSum += priceAtPurchase * quantity;

    //                orderItems.Add(new OrderItem
    //                {
    //                    // OrderId и ProductId будут установлены БД автоматически при вставке,
    //                    // но для связи в памяти мы их пока не задаём (или задаём позже).
    //                    // Здесь мы просто создаём объект. Order и Product навигационные свойства можно прилинковать,
    //                    // но для Seed'а EF Core обычно достаточно установить внешние ключи.
    //                    // Поскольку у нас нет реальных Id, мы оставим их нулевыми.
    //                    // При реальном добавлении через DbContext сначала сохраняем Order и Product, потом OrderItem с FK.
    //                    Id = (uint)++i,
    //                    Order = order,
    //                    OrderId = order.Id,
    //                    Product = product,
    //                    Quantity = quantity,
    //                    Price = priceAtPurchase
    //                });
    //            }
    //            order.Sum = orderSum;
    //        }

    //        return (admins, students, products, orders, orderItems);
    //    }
    //}
    public static class DbSeeder
    {
        public static void Seed(ApplicationContext context)
        {
            // Проверяем, есть ли уже данные, чтобы не плодить дубли
            if (context.Admins.Any())
                return;

            // ---- Генерация тестовых объектов ----
            // Обратите внимание: Id не задаём (будет 0 по умолчанию)

            var admins = new List<Admin>
        {
            new Admin { Name = "Иван", SurName = "Иванов", EMail = "i@mail.ru" },
            new Admin { Name = "Пётр", SurName = "Петров", EMail = "p@mail.ru" }
        };

            var students = new List<Student>
        {
            new Student { LoginName = "Анна", Password = "a@mail.ru", DateOfBirth = DateTime.Now }
        };

            var products = new List<Product>
        {
            new Product { Name = "Товар 1", Price = 100, Description = "Desc" },
            new Product { Name = "Товар 2", Price = 200, Description = "Desc" }
        };

            // Создаём заказы и их позиции, связывая через навигационные свойства
            var order1 = new Order { DateOfPurchase = DateTime.UtcNow, CustomerLogin = "hopa" };
            var order2 = new Order { DateOfPurchase = DateTime.UtcNow, CustomerLogin = "hopa" };

            var orderItems = new List<OrderItem>
        {
            new OrderItem { Order = order1, Product = products[0], Quantity = 2 },
            new OrderItem { Order = order1, Product = products[1], Quantity = 1 },
            new OrderItem { Order = order2, Product = products[0], Quantity = 3 }
        };

            // ---- Добавляем всё в контекст ----
            context.Admins.AddRange(admins);
            context.Students.AddRange(students);
            context.Products.AddRange(products);
            context.Orders.AddRange(order1, order2);   // можно и так
            context.OrderItems.AddRange(orderItems);

            // Сохраняем – теперь база проставит все Id (0, 1, 2...) и внешние ключи
            context.SaveChanges();
        }
    }

}