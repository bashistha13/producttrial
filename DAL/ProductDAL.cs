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
                                ?? throw new ArgumentException("DefaultConnection is not configured");
        }

        // Add new product using stored procedure
        public bool AddProduct(Product product, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_AddProduct", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Name", product.ProductName);
                        cmd.Parameters.AddWithValue("@Price", product.Price);
                        cmd.Parameters.AddWithValue("@Stock", product.StockQuantity);
                        cmd.Parameters.AddWithValue("@CatId", product.CategoryId);

                        // Output parameter to get message from SQL
                        SqlParameter outputParam = new SqlParameter("@Message", SqlDbType.NVarChar, 200)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputParam);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        message = outputParam.Value?.ToString() ?? string.Empty;

                        return message.Contains("successfully"); // true if added
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2601 || ex.Number == 2627 || ex.Number == 50003)
                    {
                        message = "This product already exists. Cannot add.";
                        return false;
                    }

                    message = $"Database error: {ex.Message}";
                    return false;
                }
                catch (Exception ex)
                {
                    message = $"Error: {ex.Message}";
                    return false;
                }
            }
        }

        // Get all products
        public List<Product> GetAllProducts()
{
    List<Product> products = new List<Product>();

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
                        ProductName = reader["ProductName"]?.ToString() ?? string.Empty,
                        Price = Convert.ToDecimal(reader["Price"]),
                        StockQuantity = Convert.ToInt32(reader["StockQuantity"]),
                        CategoryId = Convert.ToInt32(reader["CategoryId"]),
                        CategoryName = reader["CategoryName"]?.ToString() ?? string.Empty
                    });
                }
            }
        }
    }

    return products;
}


        // Get all categories
      public List<Category> GetCategories()
{
    List<Category> categories = new List<Category>();

    using (SqlConnection conn = new SqlConnection(_connectionString))
    {
        using (SqlCommand cmd = new SqlCommand("sp_GetCategories", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            conn.Open();
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    categories.Add(new Category
                    {
                        CategoryId = Convert.ToInt32(reader["CategoryId"]),
                        CategoryName = reader["CategoryName"]?.ToString() ?? string.Empty
                    });
                }
            }
        }
    }

    return categories;
}


        // Update product
        public bool UpdateProduct(Product product, out string message)
{
    using (SqlConnection conn = new SqlConnection(_connectionString))
    {
        try
        {
            using (SqlCommand cmd = new SqlCommand("sp_UpdateProduct", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", product.ProductId);
                cmd.Parameters.AddWithValue("@Name", product.ProductName);
                cmd.Parameters.AddWithValue("@Price", product.Price);
                cmd.Parameters.AddWithValue("@Stock", product.StockQuantity);
                cmd.Parameters.AddWithValue("@CatId", product.CategoryId);

                SqlParameter msgParam = new SqlParameter("@Message", SqlDbType.NVarChar, 200)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(msgParam);

                conn.Open();
                cmd.ExecuteNonQuery();

                message = msgParam.Value?.ToString() ?? "Unknown result.";

                return message == "Product updated successfully.";
            }
        }
        catch (Exception ex)
        {
            message = $"Error: {ex.Message}";
            return false;
        }
    }
}


        // Delete product
        public bool DeleteProduct(int productId, out string message)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    string query = "DELETE FROM Product WHERE ProductId=@Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", productId);
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            message = "Product deleted successfully.";
                            return true;
                        }
                        else
                        {
                            message = "Product not found. Cannot delete.";
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    message = $"Error: {ex.Message}";
                    return false;
                }
            }
        }
    }
}
