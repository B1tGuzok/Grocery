using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Grocery.Controllers;
using Grocery.Models;

namespace Grocery.Tests.Controllers
{
    [TestClass]
    public class ProductsControllerTests
    {
        private Mock<GroceryDBEntities> _mockContext;
        private Mock<DbSet<Product>> _mockProductSet;
        private Mock<DbSet<Supplier>> _mockSupplierSet;
        private ProductsController _controller;
        private List<Product> fakeProducts;
        private List<Supplier> fakeSuppliers;

        [TestInitialize]
        public void Setup()
        {
            // Создаем тестовые данные
            fakeProducts = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Яблоко", Quantity = 10, ShelfNumber = 1, SupplierId = 1, Supplier = new Supplier { SupplierId = 1, Name = "Поставщик 1" } },
                new Product { ProductId = 2, ProductName = "Банан", Quantity = 20, ShelfNumber = 2, SupplierId = 2, Supplier = new Supplier { SupplierId = 2, Name = "Поставщик 2" } },
                new Product { ProductId = 3, ProductName = "Апельсин", Quantity = 15, ShelfNumber = 3, SupplierId = 1, Supplier = new Supplier { SupplierId = 1, Name = "Поставщик 1" } }
            };
            var productsData = fakeProducts.AsQueryable();

            fakeSuppliers = new List<Supplier>
            {
                new Supplier { SupplierId = 1, Name = "Поставщик 1" },
                new Supplier { SupplierId = 2, Name = "Поставщик 2" }
            };
            var suppliersData = fakeSuppliers.AsQueryable();

            // Мокируем DbSet<Product>
            _mockProductSet = new Mock<DbSet<Product>>();
            _mockProductSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(productsData.Provider);
            _mockProductSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(productsData.Expression);
            _mockProductSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(productsData.ElementType);
            _mockProductSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(productsData.GetEnumerator());
            _mockProductSet.Setup(m => m.Include(It.IsAny<string>())).Returns(_mockProductSet.Object);
            _mockProductSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns((object[] keyValues) => fakeProducts.FirstOrDefault(p => p.ProductId == (int)keyValues[0]));
            _mockProductSet.Setup(m => m.Add(It.IsAny<Product>())).Callback<Product>(p => fakeProducts.Add(p));
            _mockProductSet.Setup(m => m.Remove(It.IsAny<Product>())).Callback<Product>(p => fakeProducts.Remove(p));

            // Мокируем DbSet<Supplier>
            _mockSupplierSet = new Mock<DbSet<Supplier>>();
            _mockSupplierSet.As<IQueryable<Supplier>>().Setup(m => m.Provider).Returns(suppliersData.Provider);
            _mockSupplierSet.As<IQueryable<Supplier>>().Setup(m => m.Expression).Returns(suppliersData.Expression);
            _mockSupplierSet.As<IQueryable<Supplier>>().Setup(m => m.ElementType).Returns(suppliersData.ElementType);
            _mockSupplierSet.As<IQueryable<Supplier>>().Setup(m => m.GetEnumerator()).Returns(suppliersData.GetEnumerator());
            _mockSupplierSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns((object[] keyValues) => fakeSuppliers.FirstOrDefault(s => s.SupplierId == (int)keyValues[0]));

            // Мокируем GroceryDBEntities
            _mockContext = new Mock<GroceryDBEntities>();
            _mockContext.Setup(c => c.Products).Returns(_mockProductSet.Object);
            _mockContext.Setup(c => c.Suppliers).Returns(_mockSupplierSet.Object);
            _mockContext.Setup(c => c.SaveChanges()).Returns(1); // Имитируем успешное сохранение

            // Создаем экземпляр контроллера с мокированным контекстом
            _controller = new ProductsController(_mockContext.Object);
        }

        [TestMethod]
        public void Index_ReturnsViewWithProductList() //Тестируется, что операция Index() возвращает представление,
                                                       //содержащее список всех продуктов из базы данных
        {
            var result = _controller.Index(null, null) as ViewResult;
            var model = result?.Model as List<Product>;

            Assert.IsNotNull(result); //проверяет, что результат является ViewResult (действие вернуло представление).
            Assert.IsNotNull(model); //проверяет, что модель является списком объектов Product
            Assert.AreEqual(3, model.Count); //проверяет, что кол-во продуктов в модели равно 3 (количество элементов в fakeProducts)
        }
        
        [TestMethod]
        public void Details_ExistingId_ReturnsProduct() //Тестируется, что операция Details() возвращает
                                                        //представление с информацией
                                                        //о существующем продукте, найденном по его ID.
        {
            int existingProductId = 2;
            _mockProductSet.Setup(m => m.Find(existingProductId)).Returns(fakeProducts.FirstOrDefault(p => p.ProductId == existingProductId));

            var result = _controller.Details(existingProductId) as ViewResult;
            var model = result?.Model as Product;

            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual("Банан", model.ProductName); //Проверяет, что свойства модели (ProductName и Quantity)
            Assert.AreEqual(20, model.Quantity);        //имеют ожидаемые значения для продукта с ID 2.
        }

        [TestMethod]
        public void Details_NonExistingId_ReturnsNotFound() //Тестируется, что операция Details() корректно обрабатывает случай,
        {                                                  //когда продукт с переданным ID не существует, и возвращает HTTP-статус
            int nonExistingProductId = 99;               //404 (Not Found).  

            _mockProductSet.Setup(m => m.Find(nonExistingProductId)).Returns((Product)null);

            var result = _controller.Details(nonExistingProductId) as HttpNotFoundResult;

            Assert.IsNotNull(result); //проверяет, что результат является HttpNotFoundResult (действие вернуло результат "не найдено").
        }
    }
}