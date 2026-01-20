using System.Collections.ObjectModel;

namespace Exercise.Models
{

    public class CableDataWrapper
    {
        public ObservableCollection<CableItem> items { get; set; }
        public int totalCount { get; set; }
    }

    public class CableItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string ItemGroupName { get; set; }
        public string ItemDescription { get; set; }
        public string SizeName { get; set; }
        public string Symbol { get; set; }
        public double Quantity { get; set; }
    }
}