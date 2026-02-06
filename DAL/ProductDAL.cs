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
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            message = "Product deleted permanently.";
                            return true;
                        }
                        else
                        {
                            message = "Product not found or already deleted.";
                            return false;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547)
                    {
                        message = "Cannot delete: This product is used in orders/bills.";
                        return false;
                    }
                    message = "Database Error: " + ex.Message;
                    return false;
                }
            }
        }

        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
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
                                ImagePath = reader["ImagePath"] != DBNull.Value ? reader["ImagePath"].ToString() : null
                            });
                        }
                    }
                }
            }
            return products;
        }

        public List<Category> GetCategories()
        {
             var list = new List<Category>();
             using (SqlConnection conn = new SqlConnection(_connectionString))
             {
                 // Assuming sp_GetCategories exists, otherwise change to "SELECT * FROM Category"
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