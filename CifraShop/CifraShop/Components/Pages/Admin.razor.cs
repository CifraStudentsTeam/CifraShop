using CifraShop.Components.Models;
using CifraShop.Components.Services;
using Microsoft.AspNetCore.Components;


namespace CifraShop.Components.Pages
{
    public partial class Admin
    {
        // Данные из БД
        private List<Product> _allProductsList = new();
        private List<Order> _allOrdersList = new();
        private List<Student> _allUsersList = new();

        // Флаги загрузки
        private bool _loadingProducts;
        private bool _loadingOrders;
        private bool _loadingUsers;

        // Фильтры и пагинация товаров
        private string _searchProduct = "";
        private string _filterStatusProduct = "";
        private const int _pageSize = 4;
        private int _indexPageProduct = 0;
        private bool _selectAllProducts;

        // Настройки уведомлений
        private bool _notificationsEmail;
        private string _emailForNotifications = "";
        private bool _notificationsTelegram;

        // Отфильтрованные товары
        private IEnumerable<Product> FilteredProducts =>
            _allProductsList.Where(p => string.IsNullOrEmpty(_searchProduct)
                || p.Name.Contains(_searchProduct, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(_searchProduct, StringComparison.OrdinalIgnoreCase)))
                .Where(p => _filterStatusProduct switch
                {
                    "visible" => p.Status == StatusProduct.InStock,
                    "hidden" => p.Status != StatusProduct.InStock,
                    "out" => p.Status == StatusProduct.OutOfStock,
                    _ => true
                });

        // Всего страниц товаров
        private int TotalProductsPages => Math.Max(1, (int)Math.Ceiling(FilteredProducts.Count() / (double)_pageSize));

        // Товары на странице
        private IEnumerable<Product> ProductsOnPage => FilteredProducts.Skip(_indexPageProduct * _pageSize).Take(_pageSize);

        // Фильтры и пагинация заказов
        private string _searchOrder = "";
        private string _filterStatusOrder = "";
        private string _filterPeriodOrder = "";
        private int _indexPageOrder = 0;

        // Отфильтрованные заказы
        private IEnumerable<Order> FilteredOrders =>
            _allOrdersList.Where(o => string.IsNullOrEmpty(_searchOrder)
                || o.Id.ToString().Contains(_searchOrder)
                || (!string.IsNullOrEmpty(o.CustomerLogin) && o.CustomerLogin.Contains(_searchOrder, StringComparison.OrdinalIgnoreCase)))
                .Where(o => string.IsNullOrEmpty(_filterStatusOrder) || o.Status.ToString() == _filterStatusOrder)
                .Where(o => _filterPeriodOrder switch
                {
                    "today" => o.DateOfPurchase.Date == DateTime.Today,
                    "week" => o.DateOfPurchase >= DateTime.Today.AddDays(-7),
                    "month" => o.DateOfPurchase >= DateTime.Today.AddMonths(-1),
                    _ => true
                });

        // Всего страниц заказов
        private int TotalOrdersPages => Math.Max(1, (int)Math.Ceiling(FilteredOrders.Count() / (double)_pageSize));

        // Заказы на странице
        private IEnumerable<Order> OrdersOnPage => FilteredOrders.Skip(_indexPageOrder * _pageSize).Take(_pageSize);

        // Фильтры и пагинация пользователей
        private string _searchUsers = "";
        private string _filterBalansUsers = "";
        private int _indexPageUsers = 0;

        // Отфильтрованные пользователи
        private IEnumerable<Student> FilteredUsers =>
            _allUsersList.Where(u => string.IsNullOrEmpty(_searchUsers)
                || u.LoginName.Contains(_searchUsers, StringComparison.OrdinalIgnoreCase))
                .Where(u => _filterBalansUsers == "positive" ? u.Balance > 0 : true);

        // Всего страниц пользователей
        private int TotalUsersPages => Math.Max(1, (int)Math.Ceiling(FilteredUsers.Count() / (double)_pageSize));

        // Пользователи на странице
        private IEnumerable<Student> UsersOnPage => FilteredUsers.Skip(_indexPageUsers * _pageSize).Take(_pageSize);

        // МЕТРИКИ
        private int AllProductsCount => _allProductsList.Count;
        private int AllUsersCount => _allUsersList.Count;

        // Периоды для метрики "Заказы"
        private readonly (string value, string textInRussian)[] _periods =
        [
            ("today", "сегодня"),
            ("yesterday", "вчера"),
            ("week", "неделя"),
            ("month", "месяц"),
            ("year", "год")
        ];

        // Выбранный период заказов
        private string _selectedPeriodOrders = "today";

        // Количество заказов
        private int OrdersCount => _selectedPeriodOrders switch
        {
            "today" => _allOrdersList.Count(q => q.DateOfPurchase.Date == DateTime.Today),
            "yesterday" => _allOrdersList.Count(q => q.DateOfPurchase.Date == DateTime.Today.AddDays(-1)),
            "week" => _allOrdersList.Count(q => q.DateOfPurchase >= DateTime.Today.AddDays(-7)),
            "month" => _allOrdersList.Count(q => q.DateOfPurchase >= DateTime.Today.AddMonths(-1)),
            "year" => _allOrdersList.Count(q => q.DateOfPurchase >= DateTime.Today.AddYears(-1)),
            _ => 0
        };

        // Переменные для модальных окон
        private int _editableProductId = 0;
        private Product _formProduct = new();
        private int _editableOrderId = 0;
        private OrderForm _formOrder = new();
        private string _userSearchForAccrual = "";
        private Student? _selectedUserForAccrual;
        private uint _sumAccrual = 0;
        private string _commentAccrual = "";

        // Состояние модальных окон
        private bool _showProductModal;
        private bool _showOrderModal;
        private bool _showFundsModal;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
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
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
            finally
            {
                _loadingProducts = false;
                _loadingOrders = false;
                _loadingUsers = false;
                StateHasChanged();
            }
        }

        // ПАГИНАЦИЯ - с сбросом чекбоксов
        private void GoToProductPage(int newIndex)
        {
            if (newIndex >= 0 && newIndex < TotalProductsPages)
            {
                _indexPageProduct = newIndex;
                ResetProductSelection();
            }
        }

        private void GoToOrderPage(int newIndex)
        {
            if (newIndex >= 0 && newIndex < TotalOrdersPages)
                _indexPageOrder = newIndex;
        }

        private void GoToUserPage(int newIndex)
        {
            if (newIndex >= 0 && newIndex < TotalUsersPages)
                _indexPageUsers = newIndex;
        }

        private void ResetProductSelection()
        {
            _selectAllProducts = false;
            foreach (var product in _allProductsList)
                product.IsSelected = false;
        }

        private void SelectAllProductsChanged(bool value)
        {
            _selectAllProducts = value;
            foreach (var product in _allProductsList)
                product.IsSelected = value;
        }

        // УПРАВЛЕНИЕ МОДАЛКАМИ - без JS
        private void OpenProductModal(int id)
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
                        Description = original.Description ?? "",
                        Price = original.Price,
                        Quantity = original.Quantity,
                        Status = original.Status,
                        ThePathToTheImage = original.ThePathToTheImage ?? ""
                    };
                }
            }
            _showProductModal = true;
        }

        private void CloseProductModal() => _showProductModal = false;

        private async Task SaveProduct()
        {
            try
            {
                if (_editableProductId == 0)
                {
                    var newProduct = await ProductService.CreateProduct(
                        _formProduct.Name ?? "",
                        _formProduct.Description ?? "",
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
                        if (existing.Name != _formProduct.Name)
                            await ProductService.ChangeProductName(existing, _formProduct.Name ?? "");
                        if (existing.Price != _formProduct.Price)
                            await ProductService.ChangeProductPrice(existing, _formProduct.Price);
                        if (existing.Quantity != _formProduct.Quantity)
                            await ProductService.ChangeProductQuntity(existing, _formProduct.Quantity);
                        if (existing.Status != _formProduct.Status)
                            await ProductService.ChangeProductStatus(existing, _formProduct.Status);

                        existing.Name = _formProduct.Name;
                        existing.Price = _formProduct.Price;
                        existing.Quantity = _formProduct.Quantity;
                        existing.Status = _formProduct.Status;
                        existing.Description = _formProduct.Description;
                    }
                }
                CloseProductModal();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving product: {ex.Message}");
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
            StateHasChanged();
        }

        private async Task DeleteProduct(int id)
        {
            var product = _allProductsList.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                await ProductService.DeleteProduct(product);
                _allProductsList.Remove(product);
                AdjustProductPageIfEmpty();
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
            AdjustProductPageIfEmpty();
            StateHasChanged();
        }

        private void AdjustProductPageIfEmpty()
        {
            if (ProductsOnPage.Count() == 0 && _indexPageProduct > 0)
                _indexPageProduct--;
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
        private void OpenCreateOrderModal()
        {
            _editableOrderId = 0;
            _formOrder = new OrderForm();
            _showOrderModal = true;
        }

        private void OpenEditOrderModal(int id)
        {
            _editableOrderId = id;
            var existing = _allOrdersList.FirstOrDefault(o => o.Id == id);
            if (existing != null)
            {
                _formOrder = new OrderForm
                {
                    CustomerLogin = existing.CustomerLogin ?? "",
                    Positions = existing.OrderItems.Select(oi => new PositionOrder
                    {
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity
                    }).ToList()
                };
            }
            _showOrderModal = true;
        }

        private void CloseOrderModal() => _showOrderModal = false;

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
            try
            {
                var student = await StudentService.GetStudentByLoginName(_formOrder.CustomerLogin);
                if (student == null)
                {
                    Console.WriteLine("User not found");
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
                    if (orderWithItems != null)
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
                CloseOrderModal();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving order: {ex.Message}");
            }
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

        private void OpenFundsModal()
        {
            _userSearchForAccrual = "";
            _selectedUserForAccrual = null;
            _sumAccrual = 0;
            _commentAccrual = "";
            _showFundsModal = true;
        }

        private void CloseFundsModal() => _showFundsModal = false;

        private void SelectedInSearchUser()
        {
            _selectedUserForAccrual = _allUsersList.FirstOrDefault(u =>
                u.LoginName.Equals(_userSearchForAccrual, StringComparison.OrdinalIgnoreCase));
        }

        private async Task AddFunds()
        {
            if (_selectedUserForAccrual != null && _sumAccrual > 0)
            {
                uint newBalance = _selectedUserForAccrual.Balance + _sumAccrual;
                await StudentService.UpdateStudentBalance(_selectedUserForAccrual, newBalance);
                _selectedUserForAccrual.Balance = newBalance;

                _selectedUserForAccrual = null;
                _userSearchForAccrual = "";
                _sumAccrual = 0;
                _commentAccrual = "";
                CloseFundsModal();
                StateHasChanged();
            }
        }

        // Вспомогательные классы для формы заказа
        public class OrderForm
        {
            public string CustomerLogin { get; set; } = "";
            public List<PositionOrder> Positions { get; set; } = new();
        }

        public class PositionOrder
        {
            public uint ProductId { get; set; }
            public uint Quantity { get; set; }
        }
    }
}