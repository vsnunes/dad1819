using Microsoft.VisualStudio.TestTools.UnitTesting;
using DIDA_TUPLE_XL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DIDA_CLIENT;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_TUPLE_XL.Tests
{
    [TestClass()]
    public class TupleSpaceXLTests
    {
        FrontEndXL _frontEnd;
        List<Object> _fields;
        List<Object> _fields2;
        List<Object> _fields3;
        List<Object> _fields4;
        Tuple _tuple1;
        Tuple _tuple2;
        Tuple _tuple3;
        Tuple _tuple4;

        [TestInitialize]
        public void TestInitialize()
        {
            _frontEnd = new FrontEndXL(1);
            _fields = new List<Object>();
            _fields2 = new List<Object>();
            _fields3 = new List<Object>();
            _fields4 = new List<Object>();
        }

        [TestMethod()]
        public void testBasicIntersection()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _tuple1 = new Tuple(_fields);

            _fields2.Add("cat");
            _fields2.Add("white");
            _tuple2 = new Tuple(_fields);

            List<Tuple> list1 = new List<Tuple>();
            List<Tuple> list2 = new List<Tuple>();

            list1.Add(_tuple1);
            list2.Add(_tuple2);

            List<List<Tuple>> listOfListsOfTuples = new List<List<Tuple>>();
            listOfListsOfTuples.Add(list1);
            listOfListsOfTuples.Add(list2);

            IEnumerable<Tuple> intersection = FrontEndXL.Intersection(listOfListsOfTuples);

            Assert.AreEqual(1, intersection.Count());

            //tuple1 or tuple2 must be equals to the intersection
            Assert.AreEqual(_tuple1, intersection.ElementAt(0));
            Assert.AreEqual(_tuple2, intersection.ElementAt(0));


        }

        /*[TestMethod()]
        public void TupleSpaceXLTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ItemCountTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void readTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void takeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void writeTest()
        {
            Assert.Fail();
        }*/
    }
}