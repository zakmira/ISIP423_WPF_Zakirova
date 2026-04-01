using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class BasePart
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ManufacturerId { get; set; }
        public int PartTypeId { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public string ManufacturerName { get; set; }
        public string PartTypeName { get; set; }

        // Дополнительные свойства для отображения характеристик
        public string SocketName { get; set; }
        public string FormFactorName { get; set; }
        public string MemoryTypeName { get; set; }
        public int? PowerConsumption { get; set; }
        public int? RecommendPower { get; set; }
    }
}
