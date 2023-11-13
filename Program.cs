using DobaWebScraper;
using HtmlAgilityPack;
using System.Net;

List<ProductResponse> productResponses = new List<ProductResponse>();
List<ProductAttributes> productAttributesList = null;
string baseUrl = string.Empty;

ProductRepository repository = new("Database SConnection Goes here");

for (int page = 1; page <= 3; page++)
{
    try
    {

        baseUrl = $"WEBSITE URL";
        HtmlWeb web = new();

        var htmlDoc = await web.LoadFromWebAsync(baseUrl);

        var products = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'image-wrap')]");
        Console.WriteLine($"EXTRACTING DATA PAGE {page} :  .......................................................");

        int counter = 0;
        foreach (var product in products)
        {
            try
            {
                ProductResponse productResponse = new()
                {
                    ProductDetailUrl = product.FirstChild.Attributes[0].Value,
                    Name = product.FirstChild.Attributes[1].Value
                };
                htmlDoc = web.Load(productResponse.ProductDetailUrl);
                var productDetail = htmlDoc.DocumentNode;
                var productSkuDiv = productDetail.SelectSingleNode("//div[contains(@class, 'prod-info-text')]").InnerText;
                productResponse.Sku = productSkuDiv.Split(".:")[1];
                productResponse.Slug = productResponse.Name;
                var imagesUrl = productDetail.SelectNodes("//div[contains(@class, 'hv-align-inner')]");
                foreach (var image in imagesUrl)
                {
                    if (image.FirstChild.Attributes[0].Name == "src")
                        productResponse.ImagesUrl += image.FirstChild.Attributes[0].Value + ",";
                }
                var attributes = productDetail.SelectNodes("//div[contains(@class, 'attr-item')]");
                productAttributesList = new();
                if (attributes is not null)
                {
                    productAttributesList.AddRange(attributes.Select(x =>
                    {
                        string[] attributeData = x.InnerText.Split(":");
                        return new ProductAttributes()
                        {
                            AttributeId = attributeData[0].Equals("Packing Size") ? 24 : 20,
                            Value = attributeData[1]
                        };
                    }));
                    productResponse.productAttributes = productAttributesList;
                }

                var productDescriptionDetail = productDetail.SelectNodes("//div[contains(@class, 'detail-container')]")[0].ChildNodes;
                for (int i = 0; i < productDescriptionDetail.Count; i++)
                {
                    if (productDescriptionDetail[i].InnerHtml.Equals("Highlights"))
                    {
                        productResponse.Description = $"<ul class='customDescriptionClass'> {productDescriptionDetail[i + 1].InnerHtml}</ul>";
                        productResponse.Specification = $"<ul class='customSpecificationClass'>{productDescriptionDetail[i + 2].InnerHtml}</ul>";
                        break;
                    }
                    else if (productDescriptionDetail[i].InnerHtml.Equals("Specification"))
                    {
                        productResponse.Specification = $"<ul class='customSpecificationClass'> {productDescriptionDetail[i + 1].InnerHtml}</ul>";
                        break;
                    }
                }

                productResponses.Add(productResponse);
                counter++;
                Console.WriteLine($"Product {counter} Extracted ");
                Thread.Sleep(6000);
            }
            catch (Exception e) { }
        }

    }

    catch (Exception e)
    {

    }

}

await repository.SaveProduct(productResponses);
Console.WriteLine($"\n\n Data Extraction Done : Base Url : {baseUrl} Total {productResponses.Count()} Extracted");


repository.GetAllHomeRootsImagesUrl();
