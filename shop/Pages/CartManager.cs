using shop.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace shop.Pages
{
    public static class CartManager
    {
        private static basket _currentCart;
        private static readonly string CartFilePath = "cart.xml";

        public static basket CurrentCart
        {
            get
            {
                if (_currentCart == null)
                {
                    LoadCart();
                }
                return _currentCart ?? (_currentCart = new basket());
            }
        }

        public static void SaveCart()
        {
            var serializer = new XmlSerializer(typeof(basket));
            using (var writer = new StreamWriter(CartFilePath))
            {
                serializer.Serialize(writer, _currentCart);
            }
        }

        private static void LoadCart()
        {
            if (File.Exists(CartFilePath))
            {
                var serializer = new XmlSerializer(typeof(basket));
                using (var reader = new StreamReader(CartFilePath))
                {
                    _currentCart = (basket)serializer.Deserialize(reader);
                }
            }
        }
    }
}