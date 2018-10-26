using Microsoft.VisualStudio.TestTools.UnitTesting;
using DIDA_LIBRARY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIDA_LIBRARY.Tests
{
    [TestClass()]
    public class TupleTests
    {
        List<Object> _fields;
        List<Object> _fields2;
        List<Object> _fields3;
        Tuple tuple1;
        Tuple tuple2;


        [TestInitialize]
        public void TestInitialize()
        {
            _fields = new List<object>();
            _fields2 = new List<object>();
            _fields2.Add("inner tuple");
            _fields.Add("dog");
            _fields.Add("brown");
            tuple2 = new Tuple(_fields2);
            _fields.Add(tuple2);

            tuple1 = new Tuple(_fields);
        }

        [TestMethod()]
        public void TupleTest()
        {
            Assert.AreEqual(tuple1.GetType(), typeof(Tuple));
        }

        [TestMethod()]
        public void GetFieldByNumber()
        {
            Assert.AreEqual(tuple1.GetFieldByNumber(0), "dog");
            Assert.AreEqual(tuple1.GetFieldByNumber(1), "brown");
            Assert.AreEqual(tuple1.GetFieldByNumber(2), tuple2);
        }

        [TestMethod()]
        public void GetTypeOfFieldsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetAllFieldsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void EqualsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetNumberOfFieldsTest()
        {
            Assert.AreEqual(tuple1.GetNumberOfFields(), 3);
            Assert.AreEqual(tuple2.GetNumberOfFields(), 1);
        }
    }
}