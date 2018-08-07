using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace PalApi.Tests
{
    using Plugins.Linguistics;
    using Plugins.Linguistics.StorageType;

    [TestClass]
    public class LinguisticsTests
    {
        #region Defaults
        private static List<Localization> Localizations = new List<Localization>
        {
            new Localization("forward-en", "test.word", "Hello world"),
            new Localization("newline-en", "test.word", "Hello\r\nWorld"),
            new Localization("reverse-en", "test.word", "dlrow olleH")
        };

        private static string SerLocalizations = @"[forward-en]
test.word=Hello world
[newline-en]
test.word=Hello\r\nWorld
[reverse-en]
test.word=dlrow olleH
";

        private LocalizationStorageFlatFile GetFlatFile()
        {
            return (LocalizationStorageFlatFile)LocalizationStorageFlatFile.Create("test.local");
        }
        #endregion

        [TestMethod]
        public void FlatFile_SerializationTest()
        {
            var le = GetFlatFile();
            var ser = le.Serialization(Localizations);

            Assert.AreEqual(SerLocalizations, ser, "Localization Serialization");
        }

        [TestMethod]
        public void FlatFile_DeserializationTest()
        {
            var le = GetFlatFile();
            var des = le.Deserialization(SerLocalizations);

            Assert.AreEqual(Localizations.Count, des.Count, "Localization Deserialization Count");

            for (var i = 0; i < Localizations.Count; i++)
            {
                Assert.AreEqual(Localizations[i].Text, des[i].Text, $"{i} Localization Test");
            }
        }
    }
}
