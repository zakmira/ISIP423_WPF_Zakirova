using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp17
{
    internal class Core
    {
        public static CosmicLodgeEntities Context = new CosmicLodgeEntities();

        public static int CurrentUserId = 0;
        public static string CurrentUserRole = "";

        public static List<OrderItems> CartItems = new List<OrderItems>();
    }
}
