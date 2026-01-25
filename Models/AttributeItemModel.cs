using Exercise.Models; 

namespace Exercise.Models
{
    public class AttributeItemModel : BindableBase
    {
        private string _symbol;
        public string Symbol
        {
            get => _symbol;
            set { _symbol = value; OnPropertyChanged(); }
        }

        private int _quantity = 1; 
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }
    }
}