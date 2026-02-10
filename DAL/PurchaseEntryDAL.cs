using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using trial.Models;

namespace trial.DAL
{
    public class PurchaseEntryDAL
    {
        private readonly string _connectionString;

        public PurchaseEntryDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        // 1. GET ALL
        public List<PurchaseEntry> GetAllEntries()
        {
            var list = new List<PurchaseEntry>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllPurchaseEntries", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new PurchaseEntry
                            {
                                PurchaseEntryId = Convert.ToInt32(reader["PurchaseEntryId"]),
                                InvoiceNo = reader["InvoiceNo"]?.ToString() ?? "",
                                PurchaseDate = Convert.ToDateTime(reader["PurchaseDate"]),
                                Supplier = reader["Supplier"]?.ToString() ?? "",
                                SupplierVAT = reader["SupplierVAT"]?.ToString() ?? "",
                                TotalAmount = Convert.ToDecimal(reader["TotalAmount"])
                            });
                        }
                    }
                }
            }
            return list;
        }

        // 2. DELETE (Soft Delete)
        public bool DeleteEntry(int id, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_DeletePurchaseEntry", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id", id);
                        
                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            message = "Entry deleted successfully.";
                            return true;
                        }
                        else
                        {
                            message = "Entry not found.";
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = "Error: " + ex.Message;
                    return false;
                }
            }
        }
    }
}