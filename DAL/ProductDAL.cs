using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using trial.Models;

namespace trial.DAL
{
    public class ProductDAL
    {
        private readonly string _connectionString;

        public ProductDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                                ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        // --- 1. UPSERT (Add/Edit) ---
        public bool UpsertProduct(Product product, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_UpsertProduct", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                        cmd.Parameters.AddWithValue("@Name", product.ProductName);
                        cmd.Parameters.AddWithValue("@Price", product.Price);
                        cmd.Parameters.AddWithValue("@Stock", product.StockQuantity);
                        cmd.Parameters.AddWithValue("@CatId", product.CategoryId);
                        cmd.Parameters.AddWithValue("@ImagePath", product.ImagePath != null ? (object)product.ImagePath : DBNull.Value);

                        SqlParameter outputParam = new SqlParameter("@Message", SqlDbType.NVarChar, 200)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputParam);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        message = outputParam.Value?.ToString() ?? string.Empty;
                        return message.Contains("successfully");
                    }
                }
                catch (Exception ex)
                {
                    message = "Error: " + ex.Message;
                    return false;
                }
            }
        }

        // --- 2. DELETE (Now Soft Delete) ---
        public bool DeleteProduct(int id, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_DeleteProduct", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id", id);
                        
                        conn.Open();
                        // This now performs an UPDATE (setting IsDeleted=1)
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            message = "Product deleted successfully.";
                            return true;
                        }
                        else
                        {
                            message = "Product not found.";
                            return false;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    message = "Database Error: " + ex.Message;
                    return false;
                }
            }
        }

        // --- 3. GET ALL (Filtered) ---
        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Calls the updated SP which filters WHERE IsDeleted = 0
                using (SqlCommand cmd = new SqlCommand("sp_GetAllProducts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                ProductName = reader["ProductName"] != DBNull.Value ? reader["ProductName"].ToString() ?? string.Empty : string.Empty,
                                Price = Convert.ToDecimal(reader["Price"]),
                                StockQuantity = Convert.ToInt32(reader["StockQuantity"]),
                                CategoryId = Convert.ToInt32(reader["CategoryId"]),
                                CategoryName = reader["CategoryName"] != DBNull.Value ? reader["CategoryName"].ToString() ?? string.Empty : string.Empty,
                                ImagePath = reader["ImagePath"] != DBNull.Value ? reader["ImagePath"].ToString() : null,
                                
                                // Map the new column
                                IsDeleted = reader["IsDeleted"] != DBNull.Value && Convert.ToBoolean(reader["IsDeleted"])
                            });
                        }
                    }
                }
            }
            return products;
        }

        // --- 4. GET CATEGORIES ---
        public List<Category> GetCategories()
        {
             var list = new List<Category>();
             using (SqlConnection conn = new SqlConnection(_connectionString))
             {
                 using (SqlCommand cmd = new SqlCommand("SELECT * FROM Category", conn))
                 {
                     conn.Open();
                     using (SqlDataReader reader = cmd.ExecuteReader())
                     {
                         while (reader.Read())
                         {
                             list.Add(new Category
                             {
                                 CategoryId = Convert.ToInt32(reader["CategoryId"]),
                                 CategoryName = reader["CategoryName"] != DBNull.Value ? reader["CategoryName"].ToString() ?? string.Empty : string.Empty
                             });
                         }
                     }
                 }
             }
             return list;
        }
    }
}