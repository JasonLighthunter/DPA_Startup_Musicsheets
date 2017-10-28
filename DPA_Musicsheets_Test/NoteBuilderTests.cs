using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Models;
using System.Collections.Generic;
using DPA_Musicsheets.Builders;

namespace DPA_Musicsheets_Test
{
    [TestClass]
    public class NoteBuilderTests
    {
        private SJNoteBuilder _noteBuilder { get; set; }
        [TestInitialize]
        public void CreateBuilder()
        {
            SJNoteFactory noteFactory = new SJNoteFactory();
            noteFactory.AddNoteType("N", typeof(SJNote));
            noteFactory.AddNoteType("R", typeof(SJRest));
            noteFactory.AddNoteType("U", typeof(SJUnheardNote));
            _noteBuilder = new SJNoteBuilder(noteFactory);
        }
       
        [TestMethod]
        public void ListTest()
        {
            SJSong song = new SJSong();
            Assert.IsNotNull(song.Bars);
        }

        [TestMethod]
        public void CreateNotePositive()
        {
            int octave = 1;
            int pitchAlteration = 0;
            uint numberOfDots = 0;
            string value = "N";

            _noteBuilder.Prepare(value);
            _noteBuilder.SetPitch(SJPitchEnum.A);
            _noteBuilder.SetOctave(octave);
            _noteBuilder.SetPitchAlteration(pitchAlteration);
            _noteBuilder.SetDuration(SJNoteDurationEnum.Whole);
            _noteBuilder.SetNumberOfDots(numberOfDots);
            SJBaseNote note = _noteBuilder.Build();

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
            
            _noteBuilder.Prepare(value);
            _noteBuilder.SetPitch(SJPitchEnum.A);
            _noteBuilder.SetDuration(SJNoteDurationEnum.Whole);
            _noteBuilder.SetNumberOfDots(numberOfDots);
            SJBaseNote rest = _noteBuilder.Build();

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

            _noteBuilder.Prepare(value);
            SJBaseNote note = _noteBuilder.Build();

            Assert.IsNotNull(note);
            Assert.IsInstanceOfType(note, typeof(SJNote));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "necessary property have not been set.")]
        public void CreateNoteNegativeInvalidParameters()
        {
            //uint numberOfDots = 0;
            string value = "N";

            _noteBuilder.Prepare(value);
            SJBaseNote note = _noteBuilder.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "An null was inappropriately allowed.")]
        public void CreateNoteNegativeNull()
        {
            _noteBuilder.Prepare(null);
        }
    }
}
