//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Rapidex.Utils.MockData;
//using Rapidex.Applications.ProjectManagement.Entities;


//namespace Rapidex.UnitTest.Data
//{
//    public class MockDataTest
//    {

//        [Fact]
//        public void GenerateMockTaskInTurkish_ShouldReturnValidData()
//        {
//            var taskFaker = TestDataGenerator.GetTaskFaker();
//            MockDataManager.Register(taskFaker, "tr");
//            var task = MockDataManager.Generate<Applications.ProjectManagement.Entities.Task>("tr");


//            Assert.NotNull(taskFaker);
//            Assert.False(string.IsNullOrWhiteSpace(task.Title));

//            //Assert.InRange(task.Priority, 1, 5);  
//            //Assert.InRange(task.Status, 0, 3);  
//        }


//        [Fact]
//        public void GenerateMockTaskInEnglish_ShouldReturnValidData()
//        {
//            var taskFaker = TestDataGenerator.GetTaskFaker();
//            MockDataManager.Register(taskFaker, "en");
//            var task = MockDataManager.Generate<Applications.ProjectManagement.Entities.Task>("en");


//            Assert.NotNull(taskFaker);
//            Assert.False(string.IsNullOrWhiteSpace(task.Title));

//            //Assert.InRange(task.Priority, 1, 5); 
//            //Assert.InRange(task.Status, 0, 3);  
//        }
//    }
//}
