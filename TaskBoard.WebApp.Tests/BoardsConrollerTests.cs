using TaskBoard.WebApp.Controllers;
using TaskBoard.WebApp.Models;

using Microsoft.AspNetCore.Mvc;

namespace TaskBoard.WebApp.Tests
{
    public class BoardsConrollerTests : UnitTestsBase
    {
        private BoardsController controller;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.controller = new BoardsController(
                this.testDb.CreateDbContext());
        }

        [Test]
        public void Test_All()
        {
            // Arrange

            // Act
            var result = controller.All();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            // Assert boards count is correct
            var resultModel = viewResult.Model as List<BoardViewModel>;
            Assert.IsNotNull(resultModel);
            Assert.AreEqual(this.dbContext.Boards.Count(), resultModel.Count);

            var resultBoardsNames = resultModel.Select(m => m.Name);

            // Assert tasks counts are correct
            var openTasks = resultModel[0].Tasks.Count();
            var openTasksInDb = this.testDb.OpenBoard.Tasks.Count();
            Assert.AreEqual(openTasksInDb, openTasks);

            var inProgressTasks = resultModel[1].Tasks.Count();
            var inProgressTasksInDb = this.testDb.InProgressBoard.Tasks.Count();
            Assert.AreEqual(inProgressTasksInDb, inProgressTasks);

            var doneTasks = resultModel[2].Tasks.Count();
            var doneTasksInDb = this.testDb.DoneBoard.Tasks.Count();
            Assert.AreEqual(doneTasksInDb, doneTasks);
        }
    }
}
