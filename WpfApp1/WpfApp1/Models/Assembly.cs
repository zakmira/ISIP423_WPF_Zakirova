using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class Assembly
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public List<BasePart> Parts { get; set; } = new List<BasePart>();
        public decimal TotalPrice => Parts.Sum(p => p.Price);
    }
}
