using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DPA_Musicsheets.Managers;
using DPA_Musicsheets.Models;

namespace DPA_Musicsheets_Test
{
    [TestClass]
    public class LilypondParserTest
    {

        [TestMethod]
        public void TestMethod1()
        {
            SJBaseNote note = SJLilypondParser.ParseNote();
            Assert.IsNotNull(note);
        }
    }
}
