﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Models;
using DPA_Musicsheets.Builders;

namespace DPA_Musicsheets_Test
{
    [TestClass]
    public class NoteFactoryTests
    {
        private SJNoteFactory factory { get; set; }

        [TestInitialize]
        public void CreateFactory()
        {
            factory = new SJNoteFactory();
            factory.AddNoteType("N", typeof(SJNote));
            factory.AddNoteType("R", typeof(SJRest));
            factory.AddNoteType("U", typeof(SJUnheardNote));
        }

        [TestMethod]
        public void CreateNotePositive()
        {
            string value = "N";
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
