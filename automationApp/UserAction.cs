using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace automationApp
{
        public class UserAction
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Action { get; set; }
        public DateTime ActionDate { get; set; }
        public string Status { get; set; }
        public string FullName { get; set; }
        public Color StatusColor { get; set; }
    }
}
