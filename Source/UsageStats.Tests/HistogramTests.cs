using System;
using NUnit.Framework;

namespace UsageStats
{
    [TestFixture]
    public class HistogramTests
    {
        [Test]
        public void MethodName_NegativeNumberAs1stParam_ExceptionThrown()
        {
            var h = new Histogram();
            h.Add(0);
            h.Add(0);
            h.Add(0.3);
            h.Add(0.7);
            h.Add(0.8);
            h.Add(1.4);
            h.Add(3.7);
            h.Add(10.2);
            h.Add(10.4);
            foreach (var kvp in h.Data)
                Console.WriteLine("{0}:{1}", kvp.Key, kvp.Value);
        }

        [Test]
        public void ToIndex_NegativeNumberAs1stParam_ExpectedResult()
        {
            var h = new Histogram();
            Assert.AreEqual(-1, h.ToIndex(-0.51));
            Assert.AreEqual(0, h.ToIndex(-0.5));
            Assert.AreEqual(0, h.ToIndex(-0.5 + 1e-8));
            Assert.AreEqual(0, h.ToIndex(-0.3));
            Assert.AreEqual(0, h.ToIndex(0));
            Assert.AreEqual(0, h.ToIndex(0.3));
            Assert.AreEqual(0, h.ToIndex(0.5 - 1e-8));
            Assert.AreEqual(1, h.ToIndex(0.5));
            Assert.AreEqual(1, h.ToIndex(0.7));
            Assert.AreEqual(2, h.ToIndex(1.5));
        }
    }
}