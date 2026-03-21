using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class CompatibilityResult
    {
        public bool IsCompatible { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
