using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HarbauerApp.classes
{
    public class Report
    {
        public long reportId { get; set; }

        public bool delete()
        {
            return Job.Database.deleteReport(reportId);
        }

        public DateTime? reportTime { get; set; }

        public string DisplayString
        {
            get
            {
                return (reportTime.HasValue ? reportTime.Value.ToShortDateString() + " " : "") + "#" + reportId;
            }
        }

        public string AStatus
        {
            get
            {
                return "Arsenic Contamination: " + aTreated + "                      " + aSafe + " Drinking Water";
            }
        }

        public string aRaw
        {
            get
            {
                return double.Parse(reportContaminations[0].rawQ.ToString()).ToString("0.000");
            }
        }
        public string aLimit
        {
            get
            {
                return (double.Parse(reportContaminations[0].limit.ToString())).ToString("0.000");
            }
        }
        public string aTreated
        {
            get
            {
                return double.Parse(reportContaminations[0].treatedQ.ToString()).ToString("0.000");
            }
        }

        public string aDesirable
        {
            get
            {
                double pL = double.Parse(reportContaminations[0].limit.ToString());
                double tL = double.Parse(reportContaminations[0].treatedQ.ToString());
                return tL <= pL ? "Below" : "Above";
            }
        }
        public string aSafe
        {
            get
            {
                return aDesirable == "Below" ? "Safe" : "Unsafe";
            }
        }

        //i

        public string IStatus
        {
            get
            {
                return "Iron Contamination: " + iTreated + "                      " + iSafe + " Drinking Water";
                //return iSafe + " Drinking Water";
            }
        }

        public string iRaw
        {
            get
            {
                return double.Parse(reportContaminations[1].rawQ.ToString()).ToString("0.000");
            }
        }
        public string iLimit
        {
            get
            {
                return double.Parse(reportContaminations[1].limit.ToString()).ToString("0.000");
            }
        }
        public string iTreated
        {
            get
            {
                return double.Parse(reportContaminations[1].treatedQ.ToString()).ToString("0.000");
            }
        }

        public string iDesirable
        {
            get
            {
                double pL = double.Parse(reportContaminations[1].limit.ToString());
                double tL = double.Parse(reportContaminations[1].treatedQ.ToString());
                return tL <= pL ? "Below" : "Above";
            }
        }
        public string iSafe
        {
            get
            {
                return iDesirable == "Below" ? "Safe" : "Unsafe";
            }
        }

        //b

        public string BStatus
        {
            get
            {
                return "Bacteriological Contamination: " + bTreated + "                      " + bSafe + " Drinking Water";
                //return bSafe + " Drinking Water";
            }
        }

        public string bRaw
        {
            get
            {
                return reportContaminations[2].rawQ.ToString();
            }
        }
        public string bLimit
        {
            get
            {
                return reportContaminations[2].limit.ToString();
            }
        }
        public string bTreated
        {
            get
            {
                return reportContaminations[2].treatedQ.ToString();
            }
        }

        public string bTreated_UC
        {
            get
            {
                return char.ToUpper(bTreated[0]) + bTreated.Substring(1);
            }
        }

        public string bDesirable
        {
            get
            {
                return bTreated;
            }
        }
        public string bSafe
        {
            get
            {
                switch (bTreated.ToLower().Trim())
                {
                    case "positive": return "Unsafe";
                    case "negative": return "Safe";
                    default: return bTreated;
                }
            }
        }

        public string Sticker
        {
            get
            {
                String var1 = "Unsafe";
                if(aSafe.Equals("Unsafe") || bSafe.Equals("Unsafe") || iSafe.Equals("Unsafe"))
                {
                    var1 = "Unsafe";
                } else
                {
                    var1 = "Safe";
                }

                String output = "The Water is " + var1 + " for drinking as per the Water Test Report of ";

                if(reportTime.HasValue)
                {
                    output += reportTime.Value.ToString("dddd, dd MMMM, yyyy");
                } else
                {
                    output += "[n-a]";
                }

                return output;
            }
        }

        public List<classes.ReportContamination> reportContaminations { get; set; }
    }
}
