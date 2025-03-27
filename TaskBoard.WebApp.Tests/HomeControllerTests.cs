using TaskBoard.Tests.Common;
using TaskBoard.WebApp.Controllers;

using Microsoft.AspNetCore.Mvc;

namespace TaskBoard.WebApp.Tests
{
    public class HomeControllerTests : UnitTestsBase
    {
        private HomeController controller;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.controller = new HomeController(
                this.testDb.CreateDbContext());
            TestingUtils
                .AssignCurrentUserForController(this.controller, this.testDb.UserMaria);
        }

        [Test]
        public void Test_Index()
        {
            // Arrange

            // Act: invoke the controller method
            var result = controller.Index();

            // Assert a view is returned
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
        }

        [Test]
        public void Test_Error()
        {
            // Arrange

            // Act
            var result = controller.Error();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
        }

        [Test]
        public void Test_Error401()
        {
            // Arrange

            // Act
            var result = controller.Error401();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
        }

        [Test]
        public void Test_Error404()
        {
            // Arrange

            // Act
            var result = controller.Error404();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
        }
    }
}
