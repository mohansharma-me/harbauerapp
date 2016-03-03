using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HarbauerApp.classes
{
    public class ReportContamination
    {
        public long contId { get; set; }
        public long reportId { get; set; }
        public string title { get; set; }
        public object rawQ { get; set; }
        public object treatedQ { get; set; }
        public object limit { get; set; }
    }
}
