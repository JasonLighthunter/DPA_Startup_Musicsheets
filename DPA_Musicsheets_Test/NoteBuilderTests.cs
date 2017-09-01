using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Models;

namespace DPA_Musicsheets_Test
{
    [TestClass]
    public class NoteBuilderTests
    {
        private SJNoteBuilder builder;

        [TestInitialize]
        public void CreateBuilder()
        {
            SJNoteFactory factory = new SJNoteFactory();
            factory.AddNoteType("N", typeof(SJNote));
            factory.AddNoteType("R", typeof(SJRest));
            builder = new SJNoteBuilder(factory);
        }

        [TestMethod]
        public void CreateNotePositive()
        {
            int octave = 1;
            int pitchAlteration = 0;
            uint numberOfDots = 0;
            string value = "N";

            builder.Prepare(value);
            builder.SetPitch(SJPitchEnum.A);
            builder.setOctave(octave);
            builder.SetPitchAlteration(pitchAlteration);
            builder.setDuration(SJNoteDurationEnum.Whole);
            builder.SetNumberOfDots(numberOfDots);
            SJBaseNote note = builder.Build();

            Assert.IsNotNull(note);
            Assert.IsInstanceOfType(note, typeof(SJNote));
            Assert.AreEqual(((SJNote)note).Duration, SJNoteDurationEnum.Whole);
            Assert.AreEqual(((SJNote)note).Pitch, SJPitchEnum.A);
            Assert.AreEqual(((SJNote)note).PitchAlteration, pitchAlteration);
            Assert.AreEqual(((SJNote)note).Octave, octave);
            Assert.AreEqual(((SJNote)note).NumberOfDots, numberOfDots);
        }

        [TestMethod]
        public void CreateRestPositive()
        {
            uint numberOfDots = 0;
            string value = "R";
            
            builder.Prepare(value);
            builder.SetPitch(SJPitchEnum.A);
            builder.setDuration(SJNoteDurationEnum.Whole);
            builder.SetNumberOfDots(numberOfDots);
            SJBaseNote rest = builder.Build();

            Assert.IsNotNull(rest);
            Assert.IsInstanceOfType(rest, typeof(SJRest));
            Assert.AreEqual(rest.Duration, SJNoteDurationEnum.Whole);
            Assert.AreEqual(rest.NumberOfDots, numberOfDots);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "necessary property have not been set.")]
        public void CreateNoteNegativeWithoutParameters()
        {
            string value = "N";

            builder.Prepare(value);
            SJBaseNote note = builder.Build();

            Assert.IsNotNull(note);
            Assert.IsInstanceOfType(note, typeof(SJNote));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "necessary property have not been set.")]
        public void CreateNoteNegativeInvalidParameters()
        {
            uint numberOfDots = 0;
            string value = "N";

            builder.Prepare(value);
            SJBaseNote note = builder.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "An null was inappropriately allowed.")]
        public void CreateNoteNegativeNull()
        {
            builder.Prepare(null);
        }
    }
}
