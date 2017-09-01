using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Models;

namespace DPA_Musicsheets_Test
{
    [TestClass]
    public class NoteFactoryTests
    {
        private SJNoteFactory factory;

        [TestInitialize]
        public void CreateFactory()
        {
            factory = new SJNoteFactory();
            factory.AddNoteType("A", typeof(SJNote));
            factory.AddNoteType("B", typeof(SJNote));
            factory.AddNoteType("C", typeof(SJNote));
            factory.AddNoteType("D", typeof(SJNote));
            factory.AddNoteType("E", typeof(SJNote));
            factory.AddNoteType("F", typeof(SJNote));
            factory.AddNoteType("G", typeof(SJNote));
            factory.AddNoteType("R", typeof(SJRest));
        }

        [TestMethod]
        public void CreateNotePositive()
        {
            string value = "A";
            SJBaseNote note = factory.CreateNote(value);
            Assert.IsNotNull(note);
            Assert.IsInstanceOfType(note, typeof(SJNote));
        }

        [TestMethod]
        public void CreateRestPositive()
        {
            string value = "R";
            SJBaseNote note = factory.CreateNote(value);
            Assert.IsNotNull(note);
            Assert.IsInstanceOfType(note, typeof(SJRest));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "A whitespace string was inappropriately allowed.")]
        public void CreateNoteNegativeWhitespace()
        {
            string value = " ";
            SJBaseNote note = factory.CreateNote(value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "An null was inappropriately allowed.")]
        public void CreateNoteNegativeNull()
        {
            SJBaseNote note = factory.CreateNote(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "An invallid string was inappropriately allowed.")]
        public void CreateNoteNegativeUnknown()
        {
            string value = "Q";
            SJBaseNote note = factory.CreateNote(value);
        }
    }
}
