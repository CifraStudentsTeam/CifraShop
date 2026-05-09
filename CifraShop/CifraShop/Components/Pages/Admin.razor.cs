using CifraShop.Components.Models;
using CifraShop.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CifraShop.Components.Pages
{
    public partial class Admin
    {
        // Данные из БД
        private List<Product> _allProductsList = new();
        private List<Order> _allOrdersList = new();
        private List<Student> _allUsersList = new();

        // Флаги загрузки
        private bool _loadingProducts = true;
        private bool _loadingOrders = true;
        private bool _loadingUsers = true;

        // Фильтры и пагинация товаров
        private string _searchProduct = "";
        private string _filterStatusProduct = "";
        private const ulong _sizePage = 4;
        private ulong _indexPageProduct = 0;
        private bool _selectAllProducts;

        // Выбрать все товары на странице
        private bool SelectAllProducts
        {
            get => _selectAllProducts;
            set
            {
                if (_selectAllProducts == value) return;
                _selectAllProducts = value;
                foreach (var product in _productThePage)
                {
                    product.IsSelected = _selectAllProducts;
                }
            }
        }

        // Отфильтрованные товары
        private IEnumerable<Product> _filteredProducts =>
            _allProductsList.Where(p => string.IsNullOrEmpty(_searchProduct) || p.Name.Contains(_searchProduct, StringComparison.OrdinalIgnoreCase) || (p.Description != null && p.Description.Contains(_searchProduct, StringComparison.OrdinalIgnoreCase))).Where(p => _filterStatusProduct switch
            {
                "visible" => p.Status == StatusProduct.InStock,
                "hidden" => p.Status != StatusProduct.InStock,
                "out" => p.Status == StatusProduct.OutOfStock,
                _ => true
            });

        // Всего страниц товаров
        private ulong _totalProductsPages => Math.Max(1, (ulong)Math.Ceiling(_filteredProducts.Count() / (double)_sizePage));

        // Товары на странице
        private IEnumerable<Product> _productThePage => _filteredProducts.Skip((int)(_indexPageProduct * _sizePage)).Take((int)_sizePage);

        // Фильтры и пагинация заказов
        private string _searchOrder = "";
        private string _filterStatusOrder = "";
        private string _filterPeriodOrder = "";
        private ulong _indexPageOrder = 0;

        // Отфильтрованные заказы
        private IEnumerable<Order> _filteredOrders =>
            _allOrdersList.Where(o => string.IsNullOrEmpty(_searchOrder) ||
                            o.Id.ToString().Contains(_searchOrder) ||
                            (o.CustomerLogin != null && o.CustomerLogin.Contains(_searchOrder, StringComparison.OrdinalIgnoreCase))).Where(o => string.IsNullOrEmpty(_filterStatusOrder) || o.Status.ToString() == _filterStatusOrder)
            .Where(o => _filterPeriodOrder switch
            {
                "today" => o.DateOfPurchase.Date == DateTime.Today,
                "week" => o.DateOfPurchase >= DateTime.Today.AddDays(-7),
                "month" => o.DateOfPurchase >= DateTime.Today.AddMonths(-1),
                _ => true
            });

        // Всего страниц заказов
        private ulong _totalPagesOrders => Math.Max(1, (ulong)Math.Ceiling(_filteredOrders.Count() / (double)_sizePage));

        // Заказы на странице
        private IEnumerable<Order> _orderThePage => _filteredOrders.Skip((int)(_indexPageOrder * _sizePage)).Take((int)_sizePage);

        // Фильтры и пагинация пользователей
        private string _searchUsers = "";
        private string _filterBalansUsers = "";
        private ulong _indexPageUsers = 0;

        // Отфильтрованные пользователи
        private IEnumerable<Student> _filteredUsers => _allUsersList.Where(u => string.IsNullOrEmpty(_searchUsers) || u.LoginName.Contains(_searchUsers, StringComparison.OrdinalIgnoreCase)).Where(u => _filterBalansUsers == "positive" ? u.Balance > 0 : true);

        // Всего страниц пользователей
        private ulong _totalPagesUsers => Math.Max(1, (ulong)Math.Ceiling(_filteredUsers.Count() / (double)_sizePage));

        // Пользователи на странице
        private IEnumerable<Student> _usersThePage => _filteredUsers.Skip((int)(_indexPageUsers * _sizePage)).Take((int)_sizePage);

        // МЕТРИКИ
        private ulong _allProduts => (ulong)_allProductsList.Count;
        private ulong _allUsers => (ulong)_allUsersList.Count;

        // Периоды для метрики "Заказы"
        private record Period(string value, string textInRussian);
        private Period[] _periods = new[]
        {
            new Period("today", "сегодня"),
            new Period("yesterday", "вчера"),
            new Period("week", "неделя"),
            new Period("month", "месяц"),
            new Period("year", "год")
        };

        // Выбранный период заказов
        private string _selectedPeriodOrders = "today";

        // Количество заказов
        private ulong _quantityOrders => _selectedPeriodOrders switch
        {
            "today" => (ulong)_allOrdersList.Count(q => q.DateOfPurchase.Date == DateTime.Today),
            "yesterday" => (ulong)_allOrdersList.Count(q => q.DateOfPurchase.Date == DateTime.Today.AddDays(-1)),
            "week" => (ulong)_allOrdersList.Count(q => q.DateOfPurchase >= DateTime.Today.AddDays(-7)),
            "month" => (ulong)_allOrdersList.Count(q => q.DateOfPurchase >= DateTime.Today.AddMonths(-1)),
            "year" => (ulong)_allOrdersList.Count(q => q.DateOfPurchase >= DateTime.Today.AddYears(-1)),
            _ => 0
        };

        // Настройки уведомлений
        private bool _notificationsEmail = true;
        private bool _notificationsTelegram = false;
        private string _emailForNotifications = "admin@shop.com";

        // Переменные для модальных окон
        private int _editableProductId = 0;
        private Product _formProduct = new();
        private int _editableOrderId = 0;
        private OrderForm _formOrder = new();
        private string _userSezrchForAccrual = "";
        private Student? _selectedUserForAccrual;
        private uint _sumAccrual = 0;
        private string _commentAccrual = "";

        // Вспомогательный класс для формы заказа
        public class OrderForm
        {
            public string CustomerLogin { get; set; }
            public List<PositionOrder> Positions { get; set; } = new();
        }

        public class PositionOrder
        {
            public uint ProductId { get; set; }
            public uint Quantity { get; set; }
        }

        protected override async Task OnInitializedAsync()
        {
            await DownloadData();
        }

        private async Task DownloadData()
        {
            _loadingProducts = true;
            _loadingOrders = true;
            _loadingUsers = true;

            try
            {
                _allProductsList = await ProductService.UploadingProductData();
                _allOrdersList = await OrderService.UploadingOrderData();
                _allUsersList = await StudentService.UpdatingStudentData();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _loadingProducts = false;
                _loadingOrders = false;
                _loadingUsers = false;
                StateHasChanged();
            }
        }

        // ПАГИНАЦИЯ
        private void GoToTheProductPage(ulong newIndex)
        {
            if (newIndex >= 0 && newIndex < _totalProductsPages)
                _indexPageProduct = newIndex;
            _selectAllProducts = false;
            foreach (var p in _productThePage) p.IsSelected = false;
        }

        private void GoToTheOrderPage(ulong newIndex)
        {
            if (newIndex >= 0 && newIndex < _totalPagesOrders)
                _indexPageOrder = newIndex;
        }

        private void GoToTheUserPage(ulong newIndex)
        {
            if (newIndex >= 0 && newIndex < _totalPagesUsers)
                _indexPageUsers = newIndex;
        }

        private void ChangePeriodOrder(ChangeEventArgs e)
        {
            _selectedPeriodOrders = e.Value?.ToString();
            StateHasChanged();
        }

        // УПРАВЛЕНИЕ МОДАЛКАМИ (JS)
        private async Task ShowModal(string modalId)
        {
            await JS.InvokeVoidAsync("bootstrap.Modal.getOrCreateInstance", modalId, "show");
        }

        private async Task HideModal(string modalId)
        {
            await JS.InvokeVoidAsync("bootstrap.Modal.getInstance", modalId, "hide");
        }

        // ТОВАРЫ
        private async Task OpenProductModal(int id)
        {
            _editableProductId = id;
            if (id == 0)
            {
                _formProduct = new Product
                {
                    Status = StatusProduct.InStock,
                    Price = 0,
                    Quantity = 0
                };
            }
            else
            {
                var original = _allProductsList.FirstOrDefault(p => p.Id == id);
                if (original != null)
                {
                    _formProduct = new Product
                    {
                        Id = original.Id,
                        Name = original.Name,
                        Description = original.Description,
                        Price = original.Price,
                        Quantity = original.Quantity,
                        Status = original.Status,
                        ThePathToTheImage = original.ThePathToTheImage
                    };
                }
            }
            await ShowModal("productModal");
        }

        private async Task SaveProduct()
        {
            try
            {
                if (_editableProductId == 0)
                {
                    var newProduct = await ProductService.CreateProduct(
                        _formProduct.Name,
                        _formProduct.Description,
                        _formProduct.Price,
                        _formProduct.Quantity,
                        _formProduct.Status);
                    _allProductsList.Add(newProduct);
                }
                else
                {
                    var existing = _allProductsList.FirstOrDefault(p => p.Id == _editableProductId);
                    if (existing != null)
                    {
                        await ProductService.ChangeProductName(existing, _formProduct.Name);
                        await ProductService.ChangeProductPrice(existing, _formProduct.Price);
                        await ProductService.ChangeProductQuntity(existing, _formProduct.Quantity);
                        await ProductService.ChangeProductStatus(existing, _formProduct.Status);

                        existing.Name = _formProduct.Name;
                        existing.Price = _formProduct.Price;
                        existing.Quantity = _formProduct.Quantity;
                        existing.Status = _formProduct.Status;
                        existing.Description = _formProduct.Description;
                    }
                }
                StateHasChanged();
                await HideModal("productModal");
            }
            catch (Exception ex)
            {
                // Обработка ошибок
            }
        }

        private async Task UpdateProductQuantity(Product product)
        {
            await ProductService.ChangeProductQuntity(product, product.Quantity);
        }

        private async Task ToggleProductVisibility(Product product, bool visible)
        {
            var newStatus = visible ? StatusProduct.InStock : StatusProduct.OutOfStock;
            await ProductService.ChangeProductStatus(product, newStatus);
            product.Status = newStatus;
        }

        private async Task DeleteProduct(int id)
        {
            var product = _allProductsList.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                await ProductService.DeleteProduct(product);
                _allProductsList.Remove(product);
                if (_productThePage.Count() == 0 && _indexPageProduct > 0)
                    _indexPageProduct--;
                StateHasChanged();
            }
        }

        private async Task DeleteSelectedProducts()
        {
            var selected = _allProductsList.Where(p => p.IsSelected).ToList();
            foreach (var product in selected)
            {
                await ProductService.DeleteProduct(product);
                _allProductsList.Remove(product);
            }
            if (_productThePage.Count() == 0 && _indexPageProduct > 0)
                _indexPageProduct--;
            StateHasChanged();
        }

        private async Task HideSelectedProducts()
        {
            foreach (var product in _allProductsList.Where(p => p.IsSelected))
            {
                await ProductService.ChangeProductStatus(product, StatusProduct.OutOfStock);
                product.Status = StatusProduct.OutOfStock;
            }
            StateHasChanged();
        }

        private async Task ShowSelectedProducts()
        {
            foreach (var product in _allProductsList.Where(p => p.IsSelected))
            {
                await ProductService.ChangeProductStatus(product, StatusProduct.InStock);
                product.Status = StatusProduct.InStock;
            }
            StateHasChanged();
        }

        // ЗАКАЗЫ
        private async Task OpenCreateOrderModal()
        {
            _editableOrderId = 0;
            _formOrder = new OrderForm();
            await ShowModal("orderModal");
        }

        private async Task OpenEditOrderModal(int id)
        {
            _editableOrderId = id;
            var existing = _allOrdersList.FirstOrDefault(o => o.Id == id);
            if (existing != null)
            {
                _formOrder = new OrderForm
                {
                    CustomerLogin = existing.CustomerLogin,
                    Positions = existing.OrderItems.Select(oi => new PositionOrder
                    {
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity
                    }).ToList()
                };
            }
            await ShowModal("orderModal");
        }

        private void AddOrderItem()
        {
            _formOrder.Positions.Add(new PositionOrder());
        }

        private void DeleteOrderItem(int index)
        {
            if (index >= 0 && index < _formOrder.Positions.Count)
                _formOrder.Positions.RemoveAt(index);
        }

        private async Task SaveOrder()
        {
            var student = await StudentService.GetStudentByLoginName(_formOrder.CustomerLogin);
            if (student == null)
            {
                // Показать ошибку
                return;
            }

            uint sum = 0;
            foreach (var positionOrder in _formOrder.Positions)
            {
                var product = _allProductsList.FirstOrDefault(p => p.Id == positionOrder.ProductId);
                if (product != null)
                    sum += product.Price * positionOrder.Quantity;
            }

            if (_editableOrderId == 0)
            {
                var newOrder = await OrderService.CreateOrder(StatusOrder.AwaitingPayment, sum, _formOrder.CustomerLogin);

                foreach (var positionOrder in _formOrder.Positions)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = newOrder.Id,
                        ProductId = positionOrder.ProductId,
                        Quantity = positionOrder.Quantity,
                        Price = _allProductsList.First(p => p.Id == positionOrder.ProductId).Price
                    };
                    await OrderService.AddOrderItem(orderItem);
                }

                var orderWithItems = await OrderService.GetOrderById(newOrder.Id);
                _allOrdersList.Add(orderWithItems);
            }
            else
            {
                var order = _allOrdersList.FirstOrDefault(o => o.Id == _editableOrderId);
                if (order != null)
                {
                    order.CustomerLogin = _formOrder.CustomerLogin;
                    order.Sum = sum;

                    foreach (var oldItem in order.OrderItems.ToList())
                    {
                        await OrderService.RemoveOrderItem(oldItem);
                    }
                    order.OrderItems.Clear();
                    foreach (var positionOrder in _formOrder.Positions)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = positionOrder.ProductId,
                            Quantity = positionOrder.Quantity,
                            Price = _allProductsList.First(p => p.Id == positionOrder.ProductId).Price
                        };
                        await OrderService.AddOrderItem(orderItem);
                        order.OrderItems.Add(orderItem);
                    }
                }
            }
            StateHasChanged();
            await HideModal("orderModal");
        }

        private async Task ChangeStatusOrder(Order order, StatusOrder newStatusOrder)
        {
            await OrderService.ChangeOrderStatus(order, newStatusOrder);
            order.Status = newStatusOrder;
            StateHasChanged();
        }

        private void ViewOrder(Order order)
        {
            // Можно открыть детальную информацию
        }

        // ПОЛЬЗОВАТЕЛИ
        private async Task DeleteUser(int id)
        {
            var user = _allUsersList.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                await StudentService.DeleteStudent(user);
                _allUsersList.Remove(user);
                StateHasChanged();
            }
        }

        private async Task OpenFundsModal()
        {
            _userSezrchForAccrual = "";
            _selectedUserForAccrual = null;
            _sumAccrual = 0;
            _commentAccrual = "";
            await ShowModal("fundsModal");
        }

        private void SelectedInSearchUser()
        {
            _selectedUserForAccrual = _allUsersList.FirstOrDefault(u =>
                u.LoginName.Equals(_userSezrchForAccrual, StringComparison.OrdinalIgnoreCase));
        }

        private async Task AddFunds()
        {
            if (_selectedUserForAccrual != null && _sumAccrual > 0)
            {
                uint newBalance = _selectedUserForAccrual.Balance + _sumAccrual;
                await StudentService.UpdateStudentBalance(_selectedUserForAccrual, newBalance);
                _selectedUserForAccrual.Balance = newBalance;

                _selectedUserForAccrual = null;
                _userSezrchForAccrual = "";
                _sumAccrual = 0;
                _commentAccrual = "";
                StateHasChanged();
                await HideModal("fundsModal");
            }
        }
    }
}