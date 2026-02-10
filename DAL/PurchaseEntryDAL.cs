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

        // NEW: Get Entry with Details
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
                        // 1. Read Master
                        if (reader.Read())
                        {
                            entry.PurchaseEntryId = Convert.ToInt32(reader["PurchaseEntryId"]);
                            entry.InvoiceNo = reader["InvoiceNo"]?.ToString() ?? "";
                            entry.PurchaseDate = Convert.ToDateTime(reader["PurchaseDate"]);
                            entry.Supplier = reader["Supplier"]?.ToString() ?? "";
                            entry.SupplierVAT = reader["SupplierVAT"]?.ToString() ?? "";
                            entry.TotalAmount = Convert.ToDecimal(reader["TotalAmount"]);
                        }

                        // 2. Read Details (Next Result Set)
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

        public bool InsertEntry(PurchaseEntry entry, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    int newId = 0;
                    using (SqlCommand cmd = new SqlCommand("sp_InsertPurchaseEntry", conn, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@InvoiceNo", entry.InvoiceNo);
                        cmd.Parameters.AddWithValue("@PurchaseDate", entry.PurchaseDate);
                        cmd.Parameters.AddWithValue("@Supplier", entry.Supplier);
                        cmd.Parameters.AddWithValue("@SupplierVAT", entry.SupplierVAT);
                        cmd.Parameters.AddWithValue("@TotalAmount", entry.TotalAmount);
                        SqlParameter outputId = new SqlParameter("@Id", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(outputId);
                        cmd.ExecuteNonQuery();
                        newId = (int)outputId.Value;
                    }

                    foreach (var item in entry.Details)
                    {
                        using (SqlCommand cmdDetail = new SqlCommand("sp_InsertPurchaseEntryDetail", conn, transaction))
                        {
                            cmdDetail.CommandType = CommandType.StoredProcedure;
                            cmdDetail.Parameters.AddWithValue("@PurchaseEntryId", newId);
                            cmdDetail.Parameters.AddWithValue("@ProductId", item.ProductId);
                            cmdDetail.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmdDetail.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    message = "Entry saved successfully.";
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    message = "Error: " + ex.Message;
                    return false;
                }
            }
        }

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