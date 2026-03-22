using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace CifraShop.Components.Models
{
    public class Product : INotifyPropertyChanged
    {
        private string _name;
        private string _description;
        private uint _price;
        private uint _quantity;
        private StatusProduct _status;
        private string _thePathToTheImage;

        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }

        [Column("Name")]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        [Column("Description")]
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        [Column("Price")]
        public uint Price
        {
            get => _price;
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged();
                }
            }
        }

        [Column("Quantity")]
        public uint Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        [Column("Status")]
        public StatusProduct Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        [Column("ThePathToTheImage")]
        public string ThePathToTheImage
        {
            get => _thePathToTheImage;
            set
            {
                if (_thePathToTheImage != value)
                {
                    _thePathToTheImage = value;
                    OnPropertyChanged();
                }
            }
        }

        // для UI
        [NotMapped]
        public bool IsSelected { get; set; }

        // Навигационное свойство для связи с OrderItem
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum StatusProduct
    {
        InStock,
        OutOfStock,
        OnSaleSoon
    }
}
