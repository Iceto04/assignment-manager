﻿using AssignmentManager.Data;
using AssignmentManager.Services.Implementations;
using AssignmentManager.Services.Models.Assignment;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentManager.Services.Tests.Assignments
{
    [TestFixture]
    public class AssignmentsTests
    {
        private AssignmentManagerDbContext context;
        private AssignmentsService service;
        public static DbContextOptions<AssignmentManagerDbContext> dbContextOptions { get; }
        public static string connectionString =
            "Server=.;Database=AssignmentManagerTests;Integrated Security=true;";

        static AssignmentsTests()
        {
            dbContextOptions = new DbContextOptionsBuilder<AssignmentManagerDbContext>()
                .UseSqlServer(connectionString)
                .Options;
        }

        [OneTimeSetUp]
        public void Init()
        {
            context = new AssignmentManagerDbContext(dbContextOptions);
            TestDBInitializer db = new TestDBInitializer();
            db.Seed(context);

            service = new AssignmentsService(context);
        }

        [TestCase(1, "Algebra Final Exam", 5, "Grade 12 Final Exam", false)]
        [TestCase(3, "Homework", 3, "", false)]
        [TestCase(4, "Test", 4, null, true)]
        public void GetByIdShouldWorkCorrectly(
            int id, string name, int classId,
            string description, bool isCompleted)
        {
            var serviceModel = service.GetById(id);

            Assert.AreEqual(id, serviceModel.Id);
            Assert.AreEqual(name, serviceModel.Name);
            Assert.AreEqual(classId, serviceModel.ClassId);
            Assert.AreEqual(description, serviceModel.Description);
            Assert.AreEqual(isCompleted, serviceModel.IsCompleted);
        }

        [TestCase(8)]
        [TestCase(-1)]
        [TestCase(1000000)]
        public void GetByIdShouldThrowExceptionForInvalidId(int id)
        {
            Assert.Throws<InvalidOperationException>(() => service.GetById(id));
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(7)]
        public void ExistsShouldReturnTrueIfAssignmentExists(int id)
        {
            var expected = service.Exists(id);

            Assert.True(expected);
        }

        [TestCase(-1)]
        [TestCase(10000)]
        [TestCase(-55555)]
        public void ExistsShouldReturnFalseIfAssignmentNotExists(int id)
        {
            var expected = service.Exists(id);

            Assert.False(expected);
        }

        [Test]
        public void GetAllShouldWorkCorrectly()
        {
            var assignments = service.GetAll().ToList();

            Assert.AreEqual(7, assignments.Count);
            Assert.AreEqual("Teamwork", assignments[0].Name);
            Assert.AreEqual("Grammar Exam", assignments[6].Name);
            Assert.AreEqual("empty", assignments[2].Description);
            Assert.AreEqual(string.Empty, assignments[3].Description);
            Assert.AreEqual(null, assignments[1].Description);
            Assert.AreEqual(5, assignments[5].ClassId);
            Assert.AreEqual(true, assignments[4].IsCompleted);
        }

        [Test]
        public void CreateShouldWorkCorrectly()
        {
            var model = new CreateAssignmentServiceModel()
            {
                Name = "Square Homework",
                ClassId = 1,
                DueDate = new DateTime(2022, 02, 24),
                Description = "Nothing Special"
            };

            var assignmentsCountBefore = service.GetAll().ToList().Count;
            service.Create(model);
            var assignmentsCountAfter = service.GetAll().ToList().Count;


            Assert.AreEqual(assignmentsCountAfter, assignmentsCountBefore + 1);

            // To Use Same Data
            service.Remove(assignmentsCountAfter);
        }

        [TestCase("Square Homework", -1, "Nothing Special")]
        [TestCase("Square Homework", 888888, "Nothing Special")]
        [TestCase("Square Homework", 0, "Nothing Special")]
        public void CreateShouldThrowExceptionForInvalidClassId(
            string name, int classId, string description)
        {
            var model = new CreateAssignmentServiceModel()
            {
                Name = name,
                ClassId = classId,
                DueDate = new DateTime(2022, 02, 24),
                Description = description
            };

            Assert.Throws<InvalidOperationException>(() => { service.Create(model); });
        }

        [TestCase(1, "Algebra Exam", 2, "Grade 12 Exam")]
        [TestCase(3, "homework 2", 5, null)]
        [TestCase(4, "Test 999", 1, "")]
        public void EditShouldWorkCorrectly(
            int id, string name, int classId, string description)
        {
            var model = new EditAssignmentServiceModel()
            {
                Id = id,
                Name = name,
                ClassId = classId,
                DueDate = new DateTime(2022, 01, 01),
                Description = description
            };

            var oldAssignment = service.GetById(id);
            service.Edit(model);
            var newAssignment = service.GetById(id);

            Assert.AreEqual(name, newAssignment.Name);
            Assert.AreEqual(classId, newAssignment.ClassId);
            Assert.AreEqual(description, newAssignment.Description);

            // To Use Same Data
            var oldModel = new EditAssignmentServiceModel()
            {
                Id = oldAssignment.Id,
                Name = oldAssignment.Name,
                ClassId = oldAssignment.ClassId,
                DueDate = oldAssignment.DueDate,
                Description = oldAssignment.Description
            };

            service.Edit(oldModel);
        }

        [TestCase(1, "Algebra Exam", -1, "Grade 12 Exam")]
        [TestCase(3, "homework 2", 5000, null)]
        [TestCase(4, "Test 999", 0, "")]
        public void EditShouldThrowExceptionForInvalidClassId(
            int id, string name, int classId, string description)
        {
            var model = new EditAssignmentServiceModel()
            {
                Id = id,
                Name = name,
                ClassId = classId,
                DueDate = new DateTime(2022, 01, 01),
                Description = description
            };

            Assert.Throws<InvalidOperationException>(() => { service.Edit(model); });
        }

        [TestCase(-1)]
        [TestCase(100000000)]
        [TestCase(0)]
        public void RemoveShouldReturnFalseIfAssignmentNotExists(int id)
        {
            var success = service.Remove(id);

            Assert.False(success);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        public void CompleteShouldWorkCorrectly(int id)
        {
            var oldAssignment = service.GetById(id);
            service.Complete(id);
            var assignment = service.GetById(id);

            Assert.True(assignment.IsCompleted);

            if (oldAssignment.IsCompleted == false)
            {
                service.Uncomplete(id);
            }
        }

        [TestCase(-1)]
        [TestCase(5000)]
        [TestCase(0)]
        public void CompleteShouldThrowExceptionForInvalidAssignment(int id)
        {
            Assert.Throws<InvalidOperationException>(() => service.Complete(id));
        }


        [TestCase(1)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        public void UncompleteShouldWorkCorrectly(int id)
        {
            var oldAssignment = service.GetById(id);
            service.Uncomplete(id);
            var assignment = service.GetById(id);

            Assert.False(assignment.IsCompleted);

            if (oldAssignment.IsCompleted)
            {
                service.Uncomplete(id);
            }
        }

        [TestCase(-1)]
        [TestCase(5000)]
        [TestCase(0)]
        public void UncompleteShouldThrowExceptionForInvalidAssignment(int id)
        {
            Assert.Throws<InvalidOperationException>(() => service.Uncomplete(id));
        }
    }
}