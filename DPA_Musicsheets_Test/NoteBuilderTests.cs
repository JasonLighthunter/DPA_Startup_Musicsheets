using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Models;
using System.Collections.Generic;

namespace DPA_Musicsheets_Test
{
    [TestClass]
    public class NoteBuilderTests
    {

        [TestInitialize]
        public void CreateBuilder()
        {
            SJNoteFactory.AddNoteType("N", typeof(SJNote));
            SJNoteFactory.AddNoteType("R", typeof(SJRest));
        }
       
        [TestMethod]
        public void ListTest()
        {
            SJSong song = new SJSong();
            Assert.IsNotNull(song.Notes);
        }

        [TestMethod]
        public void CreateNotePositive()
        {
            int octave = 1;
            int pitchAlteration = 0;
            uint numberOfDots = 0;
            string value = "N";

            SJNoteBuilder.Prepare(value);
            SJNoteBuilder.SetPitch(SJPitchEnum.A);
            SJNoteBuilder.setOctave(octave);
            SJNoteBuilder.SetPitchAlteration(pitchAlteration);
            SJNoteBuilder.setDuration(SJNoteDurationEnum.Whole);
            SJNoteBuilder.SetNumberOfDots(numberOfDots);
            SJBaseNote note = SJNoteBuilder.Build();

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
            
            SJNoteBuilder.Prepare(value);
            SJNoteBuilder.SetPitch(SJPitchEnum.A);
            SJNoteBuilder.setDuration(SJNoteDurationEnum.Whole);
            SJNoteBuilder.SetNumberOfDots(numberOfDots);
            SJBaseNote rest = SJNoteBuilder.Build();

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

            SJNoteBuilder.Prepare(value);
            SJBaseNote note = SJNoteBuilder.Build();

            Assert.IsNotNull(note);
            Assert.IsInstanceOfType(note, typeof(SJNote));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "necessary property have not been set.")]
        public void CreateNoteNegativeInvalidParameters()
        {
            uint numberOfDots = 0;
            string value = "N";

            SJNoteBuilder.Prepare(value);
            SJBaseNote note = SJNoteBuilder.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "An null was inappropriately allowed.")]
        public void CreateNoteNegativeNull()
        {
            SJNoteBuilder.Prepare(null);
        }
    }
}
