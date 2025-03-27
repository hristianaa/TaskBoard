using TaskBoard.Tests.Common;
using TaskBoard.WebApp.Controllers;
using TaskBoard.WebApp.Models.Task;

using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Mvc;

using Task = TaskBoard.Data.Task;

namespace TaskBoard.WebApp.Tests
{
    public class TasksControllerTests : UnitTestsBase
    {
        private TasksController controller;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Instantiate the controller class with the testing database
            this.controller = new TasksController(
                this.testDb.CreateDbContext());
            // Set UserMaria as current logged user
            TestingUtils.AssignCurrentUserForController(controller, this.testDb.UserMaria);
        }

        [Test]
        public void Test_Details()
        {
            // Arrange: get the "CSSTask" task from the db
            var cssTask = this.testDb.CSSTask;

            // Act
            var result = controller.Details(cssTask.Id);

            // Assert a view is returned
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            // Assert the returned model has correct data
            var resultModel = viewResult.Model as TaskDetailsViewModel;
            Assert.IsNotNull(resultModel);
            Assert.AreEqual(cssTask.Title, resultModel.Title);
            Assert.AreEqual(cssTask.Description, resultModel.Description);
            Assert.AreEqual(
                cssTask.CreatedOn.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture), 
                resultModel.CreatedOn);
            Assert.AreEqual(cssTask.Board.Name, resultModel.Board);
            Assert.AreEqual(cssTask.Owner.UserName, resultModel.Owner);
        }

        [Test]
        public void Test_Create()
        {
            // Arrange

            // Act
            var result = controller.Create();

            // Assert a task model is returned
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            var resultModel = viewResult.Model as TaskFormModel;
            Assert.IsNotNull(resultModel);
        }

        [Test]
        public void Test_Create_PostValidData()
        {
            // Arrange: get tasks count before the creation
            int tasksCountBefore = this.dbContext.Tasks.Count();

            // Create a task form model 
            var newTaskData = new TaskFormModel()
            {
                Title = "Test Task" + DateTime.Now.Ticks,
                Description = "Task to test if the tasks creation is successful",
                BoardId = this.testDb.OpenBoard.Id
            };

            // Act
            var result = controller.Create(newTaskData);

            // Assert the user is redirected to "/Boards"
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("Boards", redirectResult.ControllerName);
            Assert.AreEqual("All", redirectResult.ActionName);

            // Assert the count of tasks is increased
            int tasksCountAfter = this.dbContext.Tasks.Count();
            Assert.AreEqual(tasksCountBefore + 1, tasksCountAfter);

            // Assert the new task appeared in the database
            var newTaskInDb =
                this.dbContext.Tasks.FirstOrDefault(t => t.Title == newTaskData.Title);
            Assert.IsTrue(newTaskInDb.Id > 0);
            Assert.AreEqual(newTaskData.Description, newTaskInDb.Description);
            Assert.AreEqual(newTaskData.BoardId, newTaskInDb.BoardId);
            Assert.AreEqual(this.testDb.UserMaria.Id, newTaskInDb.OwnerId);
        }

        [Test]
        public void Test_Create_PostInvalidData()
        {
            // Arrange: get tasks count before the creation
            int tasksCountBefore = this.dbContext.Tasks.Count();

            // Create a task form model 
            string invalidTitle = string.Empty;
            var newTaskData = new TaskFormModel()
            {
                Title = invalidTitle,
                Description = "Task to test if the tasks creation is successful",
                BoardId = this.testDb.OpenBoard.Id
            };

            // Add error to the controller for the invalid title
            controller.ModelState.AddModelError("Title", "The Title field is required");

            // Act
            var result = controller.Create(newTaskData);

            // Assert the new task is not created
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            // Assert the count of tasks is not changed
            int tasksCountAfter = this.dbContext.Tasks.Count();
            Assert.AreEqual(tasksCountBefore, tasksCountAfter);

            // Remove ModelState error for next tests
            controller.ModelState.Remove("Title");
        }

        [Test]
        public void Test_DeletePage_ValidId()
        {
            // Create a task form model 
            var newTask = new Task()
            {
                Title = "Test Task" + DateTime.Now.Ticks,
                Description = "Task to test if the tasks creation is successful",
                CreatedOn = DateTime.Now,
                BoardId = this.testDb.OpenBoard.Id,
                OwnerId = this.testDb.UserMaria.Id
            };
            this.dbContext.Add(newTask);
            this.dbContext.SaveChanges();

            // Act
            var result = controller.Delete(newTask.Id);

            // Assert a view is returned
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            // Assert the returned model has correct data
            var resultModel = viewResult.Model as TaskViewModel;
            Assert.IsNotNull(resultModel);
            Assert.AreEqual(resultModel.Id, newTask.Id);
            Assert.AreEqual(resultModel.Title, newTask.Title);
            Assert.AreEqual(resultModel.Description, newTask.Description);
        }

        [Test]
        public void Test_DeletePage_InvalidId()
        {
            var invalidId = -1;
            // Act: send invalid id
            var result = controller.Delete(invalidId);

            // Assert a view is returned
            var viewResult = result as BadRequestResult;
            Assert.IsNotNull(viewResult);
        }

        [Test]
        public void Test_Delete_PostValidData()
        {
            // Create a new task for deletion
            var newTask = new Task()
            {
                Title = "Test Task" + DateTime.Now.Ticks,
                Description = "Task to test if the tasks creation is successful",
                CreatedOn = DateTime.Now,
                BoardId = this.testDb.OpenBoard.Id,
                OwnerId = this.testDb.UserMaria.Id
            };
            this.dbContext.Add(newTask);
            this.dbContext.SaveChanges();

            // Get the count of tasks before the deletion
            int tasksCountBefore = this.dbContext.Tasks.Count();

            // Create a model with the task id
            TaskViewModel model = new TaskViewModel()
            {
                Id = newTask.Id
            };

            // Act
            var result = controller.Delete(model);

            // Assert the user is redirected to "/Boards"
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("Boards", redirectResult.ControllerName);
            Assert.AreEqual("All", redirectResult.ActionName);

            // Assert count of tasks is decreased
            this.dbContext = this.testDb.CreateDbContext();
            int tasksCountAfter = this.dbContext.Tasks.Count();
            Assert.AreEqual(tasksCountBefore - 1, tasksCountAfter);

            // Assert the task is deleted from the db
            var deletedTaskInDb = this.dbContext.Tasks.Find(model.Id);
            Assert.IsNull(deletedTaskInDb);
        }

        [Test]
        public void Test_Delete_PostInvalidData()
        {
            // Arrange

            // Create a model with invalid id
            var invalidId = -1;
            TaskViewModel model = new TaskViewModel()
            {
                Id = invalidId
            };

            // Act
            var result = controller.Delete(model);

            // Assert a BadRequestResult is returned
            var viewResult = result as BadRequestResult;
            Assert.IsNotNull(viewResult);
        }

        [Test]
        public void Test_Delete_UnauthorizedUser()
        {
            // Arrange
            int tasksCountBefore = this.dbContext.Tasks.Count();

            // Get the "CSSTask" task with owner GuestUser
            var cssTask = this.testDb.CSSTask;

            // Create a model with the task id
            TaskViewModel model = new TaskViewModel()
            {
                Id = cssTask.Id
            };

            // Act
            var result = controller.Delete(model);

            // Assert an "Unauthorized" result is returned
            var unauthorizedResult = result as UnauthorizedResult;
            Assert.AreEqual((int)HttpStatusCode.Unauthorized, unauthorizedResult.StatusCode);
            Assert.IsNotNull(unauthorizedResult);

            // Assert count of tasks is not changed
            int tasksCountAfter = this.dbContext.Tasks.Count();
            Assert.AreEqual(tasksCountBefore, tasksCountAfter);
        }

        [Test]
        public void Test_Edit_ValidId()
        {
            // Arrange: get the "EditTask" task from the db
           var editTask = this.testDb.EditTask;

            // Act
            var result = controller.Edit(editTask.Id);

            // Assert a view is returned
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            // Assert the returned model has correct data
            var resultModel = viewResult.Model as TaskFormModel;
            Assert.IsNotNull(resultModel);
            Assert.AreEqual(resultModel.Title, editTask.Title);
            Assert.AreEqual(resultModel.Description, editTask.Description);
            Assert.AreEqual(resultModel.BoardId, editTask.BoardId);
        }

        [Test]
        public void Test_Edit_InvalidId()
        {
            // Arrange
           
            var invalidId = -1;
            // Act: send an invalid task id
            var result = controller.Edit(invalidId);

            // Assert a view is returned
            var viewResult = result as BadRequestResult;
            Assert.IsNotNull(viewResult);
        }

        [Test]
        public void Test_Edit_PostValidData()
        {
            // Create a new task for editing
            var newTask = new Task()
            {
                Title = "Test Task" + DateTime.Now.Ticks,
                Description = "Task to test if the tasks creation is successful",
                CreatedOn = DateTime.Now,
                BoardId = this.testDb.OpenBoard.Id,
                OwnerId = this.testDb.UserMaria.Id
            };
            this.dbContext.Add(newTask);
            this.dbContext.SaveChanges();

            // Create a model with changed title
            var changedTitle = "Changed Test Task" + DateTime.Now.Ticks;
            TaskFormModel taskModel = new TaskFormModel()
            {
                Title = changedTitle,
                Description = newTask.Description,
                BoardId = newTask.BoardId
            };

            // Act
            var result = controller.Edit(newTask.Id, taskModel);

            // Assert the user is redirected to "/Boards"
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("Boards", redirectResult.ControllerName);
            Assert.AreEqual("All", redirectResult.ActionName);

            // Assert the task is changed in the db
            this.dbContext = this.testDb.CreateDbContext();
            var editedTask = this.dbContext.Tasks.Find(newTask.Id);
            Assert.IsNotNull(editedTask);
            Assert.AreEqual(taskModel.Title, editedTask.Title);
        }

        [Test]
        public void Test_Edit_PostInvalidData()
        {
            // Arrange: get the "EditTask" task from the db
            var editTask = this.testDb.EditTask;
         
            // Create a model with invalid title: string.Empty
            var invalidTitle = string.Empty;
            TaskFormModel taskModel = new TaskFormModel()
            {
                Title = invalidTitle,
                Description = editTask.Description,
                BoardId = editTask.BoardId,
            };

            // Add error to the controller
            controller.ModelState.AddModelError("Title", "The Title field is required");

            // Act
            var result = controller.Edit(editTask.Id, taskModel);

            // Assert the user is not redirected
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNull(redirectResult);

            // Assert the task title is not edited
            var editedTask = this.dbContext.Tasks.Find(editTask.Id);
            Assert.IsNotNull(editedTask);
            Assert.AreEqual(editTask.Title, editedTask.Title);

            // Remove ModelState error for next tests
            this.controller.ModelState.Remove("Title");
        }

        [Test]
        public void Test_Edit_PostInvalidBoard()
        {
            // Arrange: get the "CSSTask" task from the db
            var cssTask = this.testDb.CSSTask;
           
            // Create a model with invalid board id
            var invalidBoardId = -1;
            TaskFormModel taskModel = new TaskFormModel()
            {
                Title = cssTask.Title,
                Description = cssTask.Description,
                BoardId = invalidBoardId
            };

            // Add error to the controller
            controller.ModelState.AddModelError("BoardId", "Board does not exist.");

            // Act
            var result = controller.Edit(cssTask.Id, taskModel);

            // Assert the user is not redirected
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNull(redirectResult);

            // Assert the task board is not edited
            var editedTask = this.dbContext.Tasks.Find(cssTask.Id);
            Assert.IsNotNull(editedTask);
            Assert.AreEqual(cssTask.BoardId, editedTask.BoardId);

            // Remove ModelState error for next tests
            this.controller.ModelState.Remove("BoardId");
        }

        [Test]
        public void Test_Edit_UnauthorizedUser()
        {
            // Arrange: get the "CSSTask" task from the db
            // Note that its owner is GuestUser
            var cssTask = this.testDb.CSSTask;

            // Create a model with changed title
            var changedTitle = "Changed Test Task" + DateTime.Now.Ticks;
            TaskFormModel taskModel = new TaskFormModel()
            {
                Title = changedTitle,
                Description = cssTask.Description,
                BoardId = cssTask.BoardId
            };

            // Act
            var result = controller.Edit(cssTask.Id, taskModel);

            // Assert an "Unautorized" result is returned
            // As UserMaria is not the owner of the "CSSTask" task
            var unauthorizedResult = result as UnauthorizedResult;
            Assert.AreEqual((int)HttpStatusCode.Unauthorized, unauthorizedResult.StatusCode);
            Assert.IsNotNull(unauthorizedResult);

            // Assert the task title is not edited
            var editedTask = this.dbContext.Tasks.Find(cssTask.Id);
            Assert.IsNotNull(editedTask);
            Assert.AreEqual(cssTask.Title, editedTask.Title);
        }

        [Test]
        public void Test_Search()
        {
            // Arrange

            // Act
            var result = controller.Search();

            // Assert a view with model is returned
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            var resultModel = viewResult.Model as TaskSearchFormModel;
            Assert.IsNotNull(resultModel);
        }

        [Test]
        public void Test_Search_Post()
        {
            // Arrange
            
            // Create a model with keyword for searching
            var keyword = "CSS";
            var searchModel = new TaskSearchFormModel()
            {
                Keyword = keyword
            };

            // Act
            var result = controller.Search(searchModel);

            // Assert a view with model is returned
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var resultModel = viewResult.Model as TaskSearchFormModel;
            Assert.IsNotNull(resultModel);

            // Assert the count of returned tasks is correct
            var resultTaskModels = resultModel.Tasks;
            var foundTasksInDb = this.dbContext.Tasks
                .Where(t => t.Title.Contains(keyword) || t.Description.Contains(keyword))
                .Count();
            Assert.AreEqual(foundTasksInDb, resultTaskModels.Count());
        }
    }
}
