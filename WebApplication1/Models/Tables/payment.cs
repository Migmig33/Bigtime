using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.Tables
{
    public class payment
    {
        public int id { get; set; }
        public int Tid { get; set; }
        public int Uid { get; set; }
        public decimal amount { get; set; }
        public DateTime dueDate { get; set; }
        public DateTime paidDate { get; set; }
        public string status { get; set; }
        public string billingPeriod { get; set; }
    }
}