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
        Tuple tuple1;
        Tuple tuple2;
        Tuple tuple3;


        [TestInitialize]
        public void TestInitialize()
        {
            _fields = new List<object>();
            _fields2 = new List<object>();
            _fields2.Add("inner tuple");
            _fields.Add("dog");
            _fields.Add("brown");
            tuple2 = new Tuple(_fields2);
            tuple3 = new Tuple(_fields2);
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
            Assert.IsTrue(tuple1.GetAllFields().Count().Equals(_fields.Count()));
            List<Type> _types = new List<Type>();

            foreach (Object obj in _fields)
            {
                _types.Add(obj.GetType());
            }

            for (int i = 0; i < tuple1.GetAllFields().Count(); i++)
            {
                Assert.AreEqual(tuple1.GetTypeOfFields().ElementAt(i), _types.ElementAt(i));
            }
        }

        [TestMethod()]
        public void GetAllFieldsTest()
        {
            Assert.IsTrue(tuple1.GetAllFields().Count().Equals(_fields.Count()));
            for(int i = 0; i < tuple1.GetAllFields().Count(); i++)
            {
                Assert.AreEqual(tuple1.GetAllFields().ElementAt(i), _fields.ElementAt(i));
            }   
        }

        [TestMethod()]
        public void EqualsTest()
        {
            Assert.IsFalse(tuple1.Equals(tuple2));
            Assert.IsTrue(tuple2.Equals(tuple3));
            Assert.IsTrue(tuple1.Equals(tuple1));
        }

        [TestMethod()]
        public void GetNumberOfFieldsTest()
        {
            Assert.AreEqual(tuple1.GetNumberOfFields(), 3);
            Assert.AreEqual(tuple2.GetNumberOfFields(), 1);
        }
    }
}