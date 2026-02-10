using System;
using System.Collections.Generic;

namespace trial.Models
{
    public class PurchaseEntry
    {
        public int PurchaseEntryId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public string Supplier { get; set; } = string.Empty;
        public string SupplierVAT { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }


        public List<PurchaseEntryDetail> Details { get; set; } = new List<PurchaseEntryDetail>();
    }

    public class PurchaseEntryDetail
    {
        public int PurchaseEntryDetailId { get; set; }
        public int PurchaseEntryId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        
      
        public string ProductName { get; set; } = string.Empty;
    }
}