using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
namespace BusinessLogic
{
    public class ShoppingCartTests
    {
        // Construction
        [Fact]
        public void When_Construct_Cart_Then_Total_Should_Be_Zero()
        {
            var cart = new ShoppingCart();
            cart.Total.Should().Be(0);
        }

        [Fact]
        public void When_Construct_Cart_Then_Cart_Should_Be_Empty()
        {
            var cart = new ShoppingCart();
            cart.Items.Should().BeEmpty();
        }

        // Add Items
        [Fact]
        public void Given_Null_Product_When_Call_Add_Then_Throw_MissingProduct_Exception()
        {
            var cart = new ShoppingCart();
            Action add = () => cart.Add(null, 3);
            add.Should().ThrowExactly<MissingProduct>().WithMessage("We must have a product.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        [InlineData(-5)]
        [InlineData(-100)]
        [InlineData(-3000)]
        public void Given_Non_Positive_Quantity_When_Call_Add_Then_Throw_InvalidQuantity_Exception(int quantity)
        {
            var cart = new ShoppingCart();
            Action add = () => cart.Add(Apple, quantity);
            add.Should().ThrowExactly<InvalidQuantity>().WithMessage($"{quantity} is not a valid quantity.");
        }

        [Theory]
        [InlineData(1, "Apple", 0.35, 3)]
        [InlineData(2, "Banana", 0.75, 5)]
        [InlineData(3, "Donut", 2.5, 11)]
        public void Given_Have_Product_And_Quantity_When_Call_Add_Then_Should_Have_Item_In_Cart(int id, string productName,
            decimal unitPrice, int quantity)
        {
            var cart = new ShoppingCart();
            var product = new Product(id, productName, unitPrice);
            cart.Add(product, quantity);
            VerifyShoppingCartAndItem(cart, product, quantity);
        }

        [Fact]
        public void Given_3_Apples_And_9_Bananas_When_Call_Add_Then_Have_3_Apples_And_9_Bananas_In_Cart()
        {
            var cart = new ShoppingCart();
            cart.Add(Apple, 3);
            cart.Add(Banana, 9);
            cart.Items.Count().Should().Be(2);
            cart.Total.Should().Be(7.80m);
        }

        [Fact]
        public void Given_A_Bunch_Of_Products_When_Call_Add_Then_Have_Those_Products_In_Cart()
        {
            var cart = new ShoppingCart();
            cart.Add(Apple, 10);
            cart.Add(Banana, 20);
            cart.Add(Donut, 40);
            cart.Items.Count().Should().Be(3);
            cart.Total.Should().Be(118.5m);
        }

        [Theory]
        [InlineData(1, "Apple", 0.35)]
        [InlineData(2, "Banana", 0.75)]
        [InlineData(3, "Donut", 2.5)]
        public void Given_4_Products_When_Call_Add_With_Another_4_Products_Then_8_Products_Are_In_Cart(int id, string productName,
            decimal unitPrice)
        {
            var cart = new ShoppingCart();
            var product = new Product(id, productName, unitPrice);
            cart.Add(product, 4);
            cart.Add(product, 4);
            cart.Items.Count().Should().Be(1);
            cart.Items[0].Quantity.Should().Be(8);
        }

        [Fact]
        public void Given_4_Apples_And_3_Bananas_And_1_Donut_When_Call_Add_With_Another_4_Apples_And_2_Bananas_Then_8_Apples_And_5_Bananas_Are_In_Cart()
        {
            var cart = new ShoppingCart();
            cart.Add(Donut, 1);
            cart.Add(Apple, 4);
            cart.Add(Banana, 3);
            cart.Add(Apple, 4);
            cart.Add(Banana, 2);
            cart.Items.Count().Should().Be(3);

            var expectedItems = new List<ShoppingCartItem>()
            {
                new ShoppingCartItem(Apple, 8),
                new ShoppingCartItem(Banana, 5),
                new ShoppingCartItem(Donut, 1),
            };
            cart.Items.Should().BeEquivalentTo(expectedItems);

        }

        [Fact]
        public void Given_A_Product_When_Call_Remove_Then_Products_Is_Not_In_Cart()
        {
            var cart = new ShoppingCart();
            cart.Add(Apple, 10);
            cart.Add(Banana, 2);
            cart.Add(Apple, 2);
            cart.Remove(1);
            cart.Items.Count().Should().Be(1);
            VerifyShoppingCartAndItem(cart, Banana, 2);
        }

        [Fact]
        public void Given_A_Product_Not_In_Cart_When_Call_Remove_Then_Throw_Exception_Product_Not_In_Cart()
        {
            var cart = new ShoppingCart();
            cart.Add(Apple, 10);
            cart.Add(Banana, 2);

            Action remove = () => cart.Remove(4);
            remove.Should().ThrowExactly<ProductNotInCart>().WithMessage("Product Id 4 is not in the cart.");
        }

        [Fact]
        public void Given_A_Bunch_Of_Products_When_Call_Clear_Then_No_Items_In_Cart()
        {
            var cart = new ShoppingCart();
            cart.Add(Apple, 10);
            cart.Add(Banana, 2);
            cart.Add(Apple, 2);
            cart.Clear();
            cart.Items.Count().Should().Be(0);
        }

        private static readonly Product Apple = new Product(1, "Apple", 0.35m);
        private static readonly Product Banana = new Product(2, "Banana", 0.75m);
        private static readonly Product Donut = new Product(3, "Donut", 2.5m);
        private void VerifyShoppingCart(ShoppingCart cart, Product product, int quantity)
        {
            cart.Items.Count().Should().Be(1);
            cart.Total.Should().Be(quantity * product.UnitPrice);
        }
        private void VerifyShoppingCartItem(ShoppingCartItem item, Product product, int quantity)
        {
            item.Product.Should().BeEquivalentTo(product);
            item.Quantity.Should().Be(quantity);
        }
        private void VerifyShoppingCartAndItem(ShoppingCart cart, Product product, int quantity)
        {
            VerifyShoppingCart(cart, product, quantity);
            VerifyShoppingCartItem(cart.Items[0], product, quantity);
        }
    }

    public class ShoppingCart
    {
        public decimal Total => Items.Sum(x => x.Subtotal);
        public IList<ShoppingCartItem> Items { get; private set; } = new List<ShoppingCartItem>();
        public void Add(Product product, int quantity)
        {
            Validate(product, quantity);
            if (Items.Where(x => x.Product.Id == product.Id).Any())
            {
                foreach (ShoppingCartItem x in Items.Where(x => x.Product.Id == product.Id))
                {
                    x.Quantity += quantity;
                }
            }
            else
            {
                Items.Add(new ShoppingCartItem(product, quantity));
            }
        }

        public void Remove(int id)
        {
            if (!Items.Where(x => x.Product.Id == id).Any())
            {
                throw new ProductNotInCart(id);
            }

            var itemToRemove = Items.Where(x => x.Product.Id == id).FirstOrDefault();
            Items.Remove(itemToRemove);
        }

        public void Clear()
        {
            Items = new List<ShoppingCartItem>();
        }

        private void Validate(Product product, int quantity)
        {
            if (product == null)
                throw new MissingProduct();
            if (quantity <= 0)
                throw new InvalidQuantity(quantity);
        }
    }
    public class ShoppingCartItem
    {
        public Product Product { get; }
        public int Quantity { get; set; }
        public decimal Subtotal => Product.UnitPrice * Quantity;
        public ShoppingCartItem(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }
    }
    public class Product
    {
        public string Name { get; }
        public decimal UnitPrice { get; }
        public int Id { get; }
        public Product(int id, string name, decimal unitPrice)
        {
            Id = id;
            Name = name;
            UnitPrice = unitPrice;
        }
    }
    public class MissingProduct : Exception
    {
        public MissingProduct()
            : base("We must have a product.")
        { }
    }
    public class ZeroQuantity : Exception
    {
        public ZeroQuantity()
            : base("Zero quantity is not valid.")
        { }
    }
    public class InvalidQuantity : Exception
    {
        public InvalidQuantity(int quantity)
            : base($"{quantity} is not a valid quantity.")
        { }
    }
    public class ProductAlreadyInCart : Exception
    {
        public ProductAlreadyInCart(string productName)
            : base($"Product {productName} is already in the cart.")
        { }
    }

    public class ProductNotInCart : Exception
    {
        public ProductNotInCart(int id)
            : base($"Product Id {id} is not in the cart.")
        { }
    }
}