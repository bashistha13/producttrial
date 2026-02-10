using System;

namespace trial.Models
{
    public class PurchaseEntry
    {
        public int PurchaseEntryId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public string Supplier { get; set; } = string.Empty;
        public string SupplierVAT { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}