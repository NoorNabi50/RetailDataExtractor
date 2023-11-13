using Dapper;
using DobaWebScraper;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.IO;
internal class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SaveProduct(List<ProductResponse> products)
    {
        List<int> Ids = new List<int>();
        List<int> ProductAtrributesIds = new List<int>();
        List<int> ProductCategoryIds = new List<int>();
        List<int> ProductEntityIds = new List<int>();
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            foreach (var product in products)
            {

                try
                {
                    string query = "INSERT INTO Catalog_Product (Name, Slug,isPublished,Description,Specification,ImagesUrl,BrandId,IsDeleted,CreatedById,CreatedOn,LatestUpdatedOn,LatestUpdatedById,Price,HasOptions,IsVisibleIndividually,IsFeatured,IsCallForPricing,IsAllowToOrder,StockTrackingIsEnabled,StockQuantity,DisplayOrder,ReviewsCount,Sku) " +
                  "VALUES (@Name, @Slug,@isPublished,@Description,@Specification,@ImagesUrl,5,0,10,SYSDATETIMEOFFSET(),SYSDATETIMEOFFSET(),10,0,0,1,1,1,1,1,0,0,0,@Sku); SELECT CAST(SCOPE_IDENTITY() as int);";

                    int ProductId = await connection.ExecuteScalarAsync<int>(query, new
                    {
                        product.Name,
                        product.Slug,
                        product.Sku,
                        @isPublished = true,
                        product.Description,
                        product.Specification,
                        product.ImagesUrl
                    });
                    Ids.Add(ProductId);
                    query = "INSERT INTO Core_Entity (Slug, Name, EntityId, EntityTypeId) SELECT @Slug, @Name, @EntityId, @EntityTypeId WHERE NOT EXISTS (SELECT 1 FROM Core_Entity WHERE Slug = @Slug);";
                    int ProductEntityId = await connection.ExecuteAsync(query, new
                    {
                        product.Slug,
                        @EntityId = ProductId,
                        product.Name,
                        @EntityTypeId = "Product"
                    });
                    ProductEntityIds.Add(ProductEntityId);

                    if (ProductEntityId > 0)
                    {
                        product.EntityId = ProductEntityId;

                        query = "INSERT INTO Catalog_ProductCategory SELECT 1,0,@CategoryId,@ProductId;SELECT CAST(SCOPE_IDENTITY() as int);";
                        int ProductCategoryId = await connection.ExecuteScalarAsync<int>(query, new
                        {
                            @CategoryId = 10969,
                            ProductId

                        });
                        ProductCategoryIds.Add(ProductCategoryId);
                        product.ProductCategoryId = ProductCategoryId;
                        query = "INSERT INTO Catalog_ProductAttributeValue SELECT @AttributeId,@ProductId,@Value;SELECT CAST(SCOPE_IDENTITY() as int);";
                        if (product.productAttributes is not null)
                        {
                            foreach (var attribute in product.productAttributes)
                            {

                                int ProductAttributeId = await connection.ExecuteScalarAsync<int>(query, new
                                {
                                    attribute.AttributeId,
                                    ProductId,
                                    attribute.Value,
                                });
                                ProductAtrributesIds.Add(ProductAttributeId);
                            }
                        }

                    }



                }

                catch (Exception ex)
                {


                    Console.WriteLine(ex.Message);
                }

            }
            Console.WriteLine($"Total  {Ids.Count()} Products Saved into Database  !");



        }



    }



    public List<ProductDTO> GetAllHomeRootsImagesUrl(){

        try
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                String Query = "select * from Catalog_Product where imagesurl like '%http%';";

                var products =   connection.Query<ProductDTO>(Query);
                ReadAndSaveImages(products);

            }

        }
        catch(Exception e) {
        
        }

              return null;
    }



    public void ReadAndSaveImages(IEnumerable<ProductDTO> products) {

        String folderPath = "D:\\Learning Workspace\\Dotnet\\PreakerDot\\src\\SimplCommerce.WebHost\\wwwroot\\user-contentTemp";


        try
        {          
            using (WebClient webClient = new WebClient())
            {
             
                    int count = 1;
                    foreach (var product in products)
                    {
                    try
                    {
                        String[] imageLinks = product.ImagesUrl.Split(",");
                        product.ImagesUrl = null;
                        for (int i = 0; i < imageLinks.Length; i++)
                        {
                            try
                            {
                                webClient.Headers.Add("User-Agent", "\r\nMozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36\r\n");
                                webClient.Headers.Add("Accept", "\r\ntext/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-");
                                webClient.DownloadFile(new Uri(imageLinks[i]), $"{folderPath}\\{product.Id}_{i}.jpeg");
                                product.ImagesUrl += $"{product.Id}_{i}.jpeg,";
                            }
                            catch (Exception e)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Id = {product.Id} , Image ={imageLinks[i]} Not Downloaded");
                                Console.ForegroundColor = ConsoleColor.White;

                            }

                        }

                        using (IDbConnection connection = new SqlConnection(_connectionString))
                        {
                            connection.Open();
                            String Query = $"UPDATE CATALOG_PRODUCT SET ImagesUrl =  @ImagesUrl WHERE Id = @Id";
                            int affectedRows = connection.Execute(Query, new { @ImagesUrl = product.ImagesUrl, @Id = product.Id });
                            Console.WriteLine(count + " - PRODUCT ID " + product.Id + "Rows affected" + affectedRows);
                            connection.Close();
                        }
                        count++;
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine("ERROR HERE :");
                    }


                }
            };
        }

        catch (Exception ex)
        {

            Console.WriteLine(ex.Message);
        }


        Console.WriteLine(products.Count());
    }



}

