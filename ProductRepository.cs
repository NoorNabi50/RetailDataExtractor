using Dapper;
using DobaWebScraper;
using System;
using System.Data.SqlClient;
using System.Xml.Linq;

internal class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SaveProduct(List<ProductResponse> products)
    {
        List<int> Ids= new List<int>();
        List<int> ProductAtrributesIds = new List<int>();
        List<int> ProductCategoryIds = new List<int>();
        List<int> ProductEntityIds = new List<int>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var tran = await connection.BeginTransactionAsync())
            {
               
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
                        }, transaction: tran);
                        Ids.Add(ProductId);
                        query = "INSERT INTO Catalog_ProductAttributeValue SELECT @AttributeId,@ProductId,@Value;SELECT 1 as int";
                        if (product.productAttributes is not null)
                        {
                            foreach (var attribute in product.productAttributes)
                            {

                                ProductAtrributesIds.Add( await connection.ExecuteScalarAsync<int>(query, new
                                {
                                    attribute.AttributeId,
                                    ProductId,
                                    attribute.Value,
                                }, transaction: tran));

                            }
                        }
                        query = "INSERT INTO Catalog_ProductCategory SELECT 1,0,@CategoryId,@ProductId;";
                        ProductCategoryIds.Add(await connection.ExecuteScalarAsync<int>(query, new
                        {
                            @CategoryId = 901,
                            ProductId

                        }, transaction: tran));
                       
                        query = "INSERT INTO Core_Entity SELECT @Slug,@Name,@EntityId,@EntityTypeId";
                       ProductEntityIds.Add(await connection.ExecuteScalarAsync<int>(query, new
                        {
                            product.Slug,
                            @EntityId = ProductId,
                            product.Name,
                            @EntityTypeId = "Product"
                        }, transaction: tran));

                    await tran.CommitAsync();

                    }
                    catch (Exception ex)
                    {
                        
                    }

                }

                    Console.WriteLine($"Total  {Ids.Count()} Products Saved into Database  !"); 
                    
              
               
            }

        }


    }
}

