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

        // 2. GET SINGLE ENTRY WITH DETAILS
        public PurchaseEntry GetEntryById(int id)
        {
            PurchaseEntry entry = new PurchaseEntry();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetPurchaseEntryDetails", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            entry.PurchaseEntryId = Convert.ToInt32(reader["PurchaseEntryId"]);
                            entry.InvoiceNo = reader["InvoiceNo"]?.ToString() ?? "";
                            entry.PurchaseDate = Convert.ToDateTime(reader["PurchaseDate"]);
                            entry.Supplier = reader["Supplier"]?.ToString() ?? "";
                            entry.SupplierVAT = reader["SupplierVAT"]?.ToString() ?? "";
                            entry.TotalAmount = Convert.ToDecimal(reader["TotalAmount"]);
                        }

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                entry.Details.Add(new PurchaseEntryDetail
                                {
                                    PurchaseEntryDetailId = Convert.ToInt32(reader["PurchaseEntryDetailId"]),
                                    ProductId = Convert.ToInt32(reader["ProductId"]),
                                    ProductName = reader["ProductName"]?.ToString() ?? "", 
                                    Quantity = Convert.ToInt32(reader["Quantity"])
                                });
                            }
                        }
                    }
                }
            }
            return entry;
        }

        // 3. INSERT (Updated: Logic moved to SQL)
        public bool InsertEntry(PurchaseEntry entry, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_InsertPurchaseEntryWithDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        
                        cmd.Parameters.AddWithValue("@InvoiceNo", entry.InvoiceNo);
                        cmd.Parameters.AddWithValue("@PurchaseDate", entry.PurchaseDate);
                        cmd.Parameters.AddWithValue("@Supplier", entry.Supplier);
                        cmd.Parameters.AddWithValue("@SupplierVAT", entry.SupplierVAT);
                        
                        // NOTE: @TotalAmount is NO LONGER passed. SQL calculates it.

                        // Create DataTable for TVP
                        DataTable dtDetails = new DataTable();
                        dtDetails.Columns.Add("ProductId", typeof(int));
                        dtDetails.Columns.Add("Quantity", typeof(int));

                        foreach (var item in entry.Details)
                        {
                            dtDetails.Rows.Add(item.ProductId, item.Quantity);
                        }

                        SqlParameter tvpParam = cmd.Parameters.AddWithValue("@Details", dtDetails);
                        tvpParam.SqlDbType = SqlDbType.Structured;
                        tvpParam.TypeName = "PurchaseEntryDetailType"; 

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        message = "Entry saved & Stock updated successfully.";
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    message = "Error: " + ex.Message;
                    return false;
                }
            }
        }

        // 4. DELETE
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
                        message = rows > 0 ? "Entry deleted successfully." : "Entry not found.";
                        return rows > 0;
                    }
                }
                catch (Exception ex) { message = "Error: " + ex.Message; return false; }
            }
        }
    }
}