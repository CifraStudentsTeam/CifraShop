namespace CifraShop.Client.Models;

public enum StatusProduct
{
    InStock,
    OutOfStock,
    ComingSoon
}

public enum StatusOrder
{
    Pending,
    AwaitingPayment,
    Paid,
    Manufactured,
    Completed
}

public enum UserRole
{
    Student,
    Admin
}
