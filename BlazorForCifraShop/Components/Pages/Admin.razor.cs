using ConsoleApp3.Models;
using ConsoleApp3.Servise;
using Microsoft.AspNetCore.Components;

namespace BlazorForCifraShop.Components.Pages
{
    public partial class Admin : ComponentBase
    {

        [Inject] private IProductService ProductService { get; set; }
        [Inject] private IOrderService OrderService { get; set; }
        [Inject] private IStudentService StudentService { get; set; }

        // Данные из БД
        private List<Product> всеТовары = new();
        private List<Order> всеЗаказы = new();
        private List<Student> всеПользователи = new();

        // Флаги загрузки
        private bool загрузкаТоваров = true;
        private bool загрузкаЗаказов = true;
        private bool загрузкаПользователей = true;

        // Фильтры и пагинация товаров
        private string поискТоваров = "";
        private string фильтрСтатусТоваров = ""; // "", "visible", "hidden", "out"
        private const int РазмерСтраницы = 4;
        private int индексСтраницыТоваров = 0;

        private IEnumerable<Product> отфильтрованныеТовары =>
            всеТовары
                .Where(т => string.IsNullOrEmpty(поискТоваров) ||
                            т.Name.Contains(поискТоваров, StringComparison.OrdinalIgnoreCase) ||
                            (т.Description != null && т.Description.Contains(поискТоваров, StringComparison.OrdinalIgnoreCase)))
                .Where(т => фильтрСтатусТоваров switch
                {
                    "visible" => т.Status == StatusProduct.InStock,
                    "hidden" => т.Status != StatusProduct.InStock,
                    "out" => т.Status == StatusProduct.OutOfStock,
                    _ => true
                });

        private int всегостраницТоваров => Math.Max(1, (int)Math.Ceiling(отфильтрованныеТовары.Count() / (double)РазмерСтраницы));
        private IEnumerable<Product> товарыНаСтранице =>
            отфильтрованныеТовары
                .Skip(индексСтраницыТоваров * РазмерСтраницы)
                .Take(РазмерСтраницы);

        // Фильтры и пагинация заказов
        private string поискЗаказов = "";
        private string фильтрСтатусЗаказов = ""; // значения из StatusOrder
        private string фильтрПериодЗаказов = "";
        private int индексСтраницыЗаказов = 0;

        private IEnumerable<Order> отфильтрованныеЗаказы =>
            всеЗаказы
                .Where(з => string.IsNullOrEmpty(поискЗаказов) ||
                            з.Id.ToString().Contains(поискЗаказов) ||
                            (з.CustomerLogin != null && з.CustomerLogin.Contains(поискЗаказов, StringComparison.OrdinalIgnoreCase)))
                .Where(з => string.IsNullOrEmpty(фильтрСтатусЗаказов) || з.Status.ToString() == фильтрСтатусЗаказов)
                .Where(з => фильтрПериодЗаказов switch
                {
                    "today" => з.DateOfPurchase.Date == DateTime.Today,
                    "week" => з.DateOfPurchase >= DateTime.Today.AddDays(-7),
                    "month" => з.DateOfPurchase >= DateTime.Today.AddMonths(-1),
                    _ => true
                });

        private int всегостраницЗаказов => Math.Max(1, (int)Math.Ceiling(отфильтрованныеЗаказы.Count() / (double)РазмерСтраницы));
        private IEnumerable<Order> заказыНаСтранице =>
            отфильтрованныеЗаказы
                .Skip(индексСтраницыЗаказов * РазмерСтраницы)
                .Take(РазмерСтраницы);

        // Фильтры и пагинация пользователей
        private string поискПользователей = "";
        private string фильтрБалансПользователей = "";
        private int индексСтраницыПользователей = 0;

        private IEnumerable<Student> отфильтрованныеПользователи =>
            всеПользователи
                .Where(п => string.IsNullOrEmpty(поискПользователей) ||
                            п.LoginName.Contains(поискПользователей, StringComparison.OrdinalIgnoreCase))
                .Where(п => фильтрБалансПользователей == "positive" ? п.Balance > 0 : true);

        private int всегостраницПользователей => Math.Max(1, (int)Math.Ceiling(отфильтрованныеПользователи.Count() / (double)РазмерСтраницы));
        private IEnumerable<Student> пользователиНаСтранице =>
            отфильтрованныеПользователи
                .Skip(индексСтраницыПользователей * РазмерСтраницы)
                .Take(РазмерСтраницы);

        // Метрики
        private int всегоТоваров => всеТовары.Count;
        private int всегоПользователей => всеПользователи.Count;

        // Периоды для метрики "Заказы"
        private record Период(string Значение, string Текст);
        private Период[] периодыЗаказов = new[]
        {
            new Период("today", "сегодня"),
            new Период("yesterday", "вчера"),
            new Период("week", "неделя"),
            new Период("month", "месяц"),
            new Период("year", "год")
        };
        private string выбранныйПериодЗаказов = "today";
        private int количествоЗаказов => выбранныйПериодЗаказов switch
        {
            "today" => всеЗаказы.Count(з => з.DateOfPurchase.Date == DateTime.Today),
            "yesterday" => всеЗаказы.Count(з => з.DateOfPurchase.Date == DateTime.Today.AddDays(-1)),
            "week" => всеЗаказы.Count(з => з.DateOfPurchase >= DateTime.Today.AddDays(-7)),
            "month" => всеЗаказы.Count(з => з.DateOfPurchase >= DateTime.Today.AddMonths(-1)),
            "year" => всеЗаказы.Count(з => з.DateOfPurchase >= DateTime.Today.AddYears(-1)),
            _ => 0
        };

        // Настройки уведомлений (пока не сохраняются)
        private bool уведомлениеEmail = true;
        private bool уведомлениеTelegram = false;
        private string emailДляУведомлений = "admin@shop.com";

        // Переменные для модальных окон
        private int редактируемыйТоварId = 0;
        private Product формаТовара = new();
        private int редактируемыйЗаказId = 0;
        private ФормаЗаказа формаЗаказа = new();
        private string поискПользователяДляНачисления = "";
        private Student выбранныйПользовательДляНачисления = null;
        private uint суммаНачисления;
        private string комментарийНачисления;

        // Вспомогательный класс для формы заказа
        public class ФормаЗаказа
        {
            public string CustomerLogin { get; set; }
            public List<ПозицияЗаказа> Позиции { get; set; } = new();
        }

        public class ПозицияЗаказа
        {
            public uint ProductId { get; set; }
            public uint Quantity { get; set; }
        }

        protected override async Task OnInitializedAsync()
        {
            await ЗагрузитьДанные();
        }

        private async Task ЗагрузитьДанные()
        {
            загрузкаТоваров = true;
            загрузкаЗаказов = true;
            загрузкаПользователей = true;

            try
            {
                всеТовары = await ProductService.UploadingProductData();
                всеЗаказы = await OrderService.UploadingOrderData();
                всеПользователи = await StudentService.UpdatingStudentData();

                // Для каждого заказа загружаем OrderItems, если они не были загружены автоматически (включили Include в сервисе)
                // В UploadingOrderData мы уже использовали Include(o => o.OrderItems), поэтому коллекции заполнены.
            }
            catch (Exception ex)
            {
                // Обработка ошибок (можно показать уведомление)
            }
            finally
            {
                загрузкаТоваров = false;
                загрузкаЗаказов = false;
                загрузкаПользователей = false;
                StateHasChanged();
            }
        }

        // Пагинация
        private void ПерейтиНаСтраницуТоваров(int новыйИндекс)
        {
            if (новыйИндекс >= 0 && новыйИндекс < всегостраницТоваров)
                индексСтраницыТоваров = новыйИндекс;
        }

        private void ПерейтиНаСтраницуЗаказов(int новыйИндекс)
        {
            if (новыйИндекс >= 0 && новыйИндекс < всегостраницЗаказов)
                индексСтраницыЗаказов = новыйИндекс;
        }

        private void ПерейтиНаСтраницуПользователей(int новыйИндекс)
        {
            if (новыйИндекс >= 0 && новыйИндекс < всегостраницПользователей)
                индексСтраницыПользователей = новыйИндекс;
        }

        private void ИзменитьПериодЗаказов(ChangeEventArgs e)
        {
            выбранныйПериодЗаказов = e.Value?.ToString();
            StateHasChanged();
        }

        // Товары
        private void ОткрытьМодалкуТовара(int id)
        {
            редактируемыйТоварId = id;
            if (id == 0)
            {
                формаТовара = new Product
                {
                    Status = StatusProduct.InStock,
                    Price = 0,
                    Quantity = 0
                };
            }
            else
            {
                var исходный = всеТовары.FirstOrDefault(т => т.Id == id);
                if (исходный != null)
                {
                    формаТовара = new Product
                    {
                        Id = исходный.Id,
                        Name = исходный.Name,
                        Description = исходный.Description,
                        Price = исходный.Price,
                        Quantity = исходный.Quantity,
                        Status = исходный.Status,
                        ThePathToTheImage = исходный.ThePathToTheImage
                    };
                }
            }
            // Открыть модальное окно через JS (можно вызвать из JS)
        }

        private async Task СохранитьТовар()
        {
            try
            {
                if (редактируемыйТоварId == 0)
                {
                    var новый = await ProductService.CreateProduct(
                        формаТовара.Name,
                        формаТовара.Description,
                        формаТовара.Price,
                        формаТовара.Quantity,
                        формаТовара.Status);
                    всеТовары.Add(новый);
                }
                else
                {
                    var существующий = всеТовары.FirstOrDefault(т => т.Id == редактируемыйТоварId);
                    if (существующий != null)
                    {
                        await ProductService.ChangeProductName(существующий, формаТовара.Name);
                        await ProductService.ChangeProductPrice(существующий, формаТовара.Price);
                        await ProductService.ChangeProductQuntity(существующий, формаТовара.Quantity);
                        await ProductService.ChangeProductStatus(существующий, формаТовара.Status);
                        // Обновляем локальную копию
                        существующий.Name = формаТовара.Name;
                        существующий.Price = формаТовара.Price;
                        существующий.Quantity = формаТовара.Quantity;
                        существующий.Status = формаТовара.Status;
                        существующий.Description = формаТовара.Description;
                    }
                }
                StateHasChanged();
                // Закрыть модалку
            }
            catch (Exception ex)
            {
                // Обработка ошибок
            }
        }

        private async Task УдалитьТовар(int id)
        {
            var товар = всеТовары.FirstOrDefault(т => т.Id == id);
            if (товар != null)
            {
                await ProductService.DeleteProduct(товар);
                всеТовары.Remove(товар);
                if (товарыНаСтранице.Count() == 0 && индексСтраницыТоваров > 0)
                    индексСтраницыТоваров--;
                StateHasChanged();
            }
        }

        private async Task УдалитьВыбранныеТовары()
        {
            var выбранные = всеТовары.Where(т => т.IsSelected).ToList();
            foreach (var т in выбранные)
            {
                await ProductService.DeleteProduct(т);
                всеТовары.Remove(т);
            }
            if (товарыНаСтранице.Count() == 0 && индексСтраницыТоваров > 0)
                индексСтраницыТоваров--;
            StateHasChanged();
        }

        private async Task СкрытьВыбранныеТовары()
        {
            foreach (var т in всеТовары.Where(т => т.IsSelected))
            {
                await ProductService.ChangeProductStatus(т, StatusProduct.OutOfStock);
                т.Status = StatusProduct.OutOfStock;
            }
            StateHasChanged();
        }

        private async Task ПоказатьВыбранныеТовары()
        {
            foreach (var т in всеТовары.Where(т => т.IsSelected))
            {
                await ProductService.ChangeProductStatus(т, StatusProduct.InStock);
                т.Status = StatusProduct.InStock;
            }
            StateHasChanged();
        }

        // Заказы
        private void ОткрытьМодалкуСозданияЗаказа()
        {
            редактируемыйЗаказId = 0;
            формаЗаказа = new ФормаЗаказа();
            // Открыть модалку
        }

        private void ОткрытьМодалкуРедактированияЗаказа(int id)
        {
            редактируемыйЗаказId = id;
            var существующий = всеЗаказы.FirstOrDefault(з => з.Id == id);
            if (существующий != null)
            {
                формаЗаказа = new ФормаЗаказа
                {
                    CustomerLogin = существующий.CustomerLogin,
                    Позиции = существующий.OrderItems.Select(oi => new ПозицияЗаказа
                    {
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity
                    }).ToList()
                };
            }
        }

        private void ДобавитьПозициюЗаказа()
        {
            формаЗаказа.Позиции.Add(new ПозицияЗаказа());
        }

        private void УдалитьПозициюЗаказа(int индекс)
        {
            if (индекс >= 0 && индекс < формаЗаказа.Позиции.Count)
                формаЗаказа.Позиции.RemoveAt(индекс);
        }

        private async Task СохранитьЗаказ()
        {
            // Проверяем существование пользователя
            var студент = await StudentService.GetStudentByLoginName(формаЗаказа.CustomerLogin);
            if (студент == null)
            {
                // Обработка: пользователь не найден
                return;
            }

            uint сумма = 0;
            foreach (var п in формаЗаказа.Позиции)
            {
                var товар = всеТовары.FirstOrDefault(т => т.Id == п.ProductId);
                if (товар != null)
                    сумма += товар.Price * п.Quantity;
            }

            if (редактируемыйЗаказId == 0)
            {
                var новыйЗаказ = await OrderService.CreateOrder(StatusOrder.AwaitingPayment, сумма, формаЗаказа.CustomerLogin);
                // Добавляем позиции
                foreach (var п in формаЗаказа.Позиции)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = новыйЗаказ.Id,
                        ProductId = п.ProductId,
                        Quantity = п.Quantity,
                        Price = всеТовары.First(т => т.Id == п.ProductId).Price
                    };
                    await OrderService.AddOrderItem(orderItem);
                }
                // Загружаем заказ с позициями для отображения
                var заказСПозициями = await OrderService.GetOrderById(новыйЗаказ.Id);
                всеЗаказы.Add(заказСПозициями);
            }
            else
            {
                var заказ = всеЗаказы.FirstOrDefault(з => з.Id == редактируемыйЗаказId);
                if (заказ != null)
                {
                    заказ.CustomerLogin = формаЗаказа.CustomerLogin;
                    заказ.Sum = сумма;
                    // Удаляем старые позиции и добавляем новые (проще всего удалить все и добавить заново)
                    foreach (var oldItem in заказ.OrderItems.ToList())
                    {
                        await OrderService.RemoveOrderItem(oldItem);
                    }
                    заказ.OrderItems.Clear();
                    foreach (var п in формаЗаказа.Позиции)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = заказ.Id,
                            ProductId = п.ProductId,
                            Quantity = п.Quantity,
                            Price = всеТовары.First(т => т.Id == п.ProductId).Price
                        };
                        await OrderService.AddOrderItem(orderItem);
                        заказ.OrderItems.Add(orderItem);
                    }
                }
            }
            StateHasChanged();
        }

        private async Task ИзменитьСтатусЗаказа(Order заказ, StatusOrder новыйСтатус)
        {
            await OrderService.ChangeOrderStatus(заказ, новыйСтатус);
            заказ.Status = новыйСтатус;
            StateHasChanged();
        }

        private void ПросмотретьЗаказ(int id)
        {
            // Можно открыть модалку с деталями
        }

        // Пользователи
        private void ОткрытьМодалкуРедактированияПользователя(int id)
        {
            // Можно реализовать
        }

        private async Task УдалитьПользователя(int id)
        {
            var пользователь = всеПользователи.FirstOrDefault(п => п.Id == id);
            if (пользователь != null)
            {
                await StudentService.DeleteStudent(пользователь);
                всеПользователи.Remove(пользователь);
                StateHasChanged();
            }
        }

        // Начисление средств
        private void ВыбратьНайденногоПользователя()
        {
            выбранныйПользовательДляНачисления = всеПользователи.FirstOrDefault(п =>
                п.LoginName.Contains(поискПользователяДляНачисления, StringComparison.OrdinalIgnoreCase));
        }

        private async Task НачислитьСредства()
        {
            if (выбранныйПользовательДляНачисления != null && суммаНачисления > 0)
            {
                uint новыйБаланс = выбранныйПользовательДляНачисления.Balance + суммаНачисления;
                await StudentService.UpdateStudentBalance(выбранныйПользовательДляНачисления, новыйБаланс);
                выбранныйПользовательДляНачисления.Balance = новыйБаланс;

                // Сброс полей
                выбранныйПользовательДляНачисления = null;
                поискПользователяДляНачисления = "";
                суммаНачисления = 0;
                комментарийНачисления = "";
                StateHasChanged();
            }
        }
    }
}