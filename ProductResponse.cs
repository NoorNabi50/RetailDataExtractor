using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DobaWebScraper
{
    internal class ProductResponse
    {
        public string? Sku { get; set; }

        public int ProductId { get; set; }
        public string? Name { get; set; }

        public string? ProductDetailUrl { get; set; }

        public float ShippingPrice { get; set; }

        public string? ShippingCountry { get; set; }

        public string? ShippingProcessDescription { get; set; }

        public string? ImagesUrl { get; set; }

        public string? Description { get; set; }

        public string? Specification { get; set; }

        public List<ProductAttributes>? productAttributes { get; set; }


        private string slug = string.Empty;
        public string Slug
        {
            get
            {
                return slug;
            }
            set
            {
                slug = value.ToLower().Replace(" ","-");
            }
        }
    }

    internal class ProductAttributes
    {
        public int AttributeId { get; set;}

        public string Value { get; set;}


    }
}
