using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiTest;
using WebApiTest.Controllers;
using WebApiTest.Models;

namespace WebApiTest.Tests.Controllers
{
    [TestClass]
    public class ValuesControllerTest
    {
        [TestMethod]
        public void Get()
        {
            // Arrange
            DogsController controller = new DogsController();

            // Act
            IEnumerable<string> result = controller.Dogs();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Boats", result.ElementAt(0));
            Assert.AreEqual("Julie", result.ElementAt(1));
        }

        [TestMethod]
        public void GetById()
        {
            // Arrange
            DogsController controller = new DogsController();

            // Act
            string result = controller.Dogs(5);

            // Assert
            Assert.AreEqual("Boats", result);
        }

        [TestMethod]
        public void Post()
        {
            // Arrange
            DogsController controller = new DogsController();

            // Act
            controller.Dogs("value");

            // Assert
        }

        [TestMethod]
        public void Delete()
        {
            // Arrange
            DogsController controller = new DogsController();

            // Act
            controller.DogsDelete(5);

            // Assert
        }
    }
}
