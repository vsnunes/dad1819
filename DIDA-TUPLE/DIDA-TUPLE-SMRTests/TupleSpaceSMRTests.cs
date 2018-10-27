using Microsoft.VisualStudio.TestTools.UnitTesting;
using DIDA_TUPLE_SMR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tuple = DIDA_LIBRARY.Tuple;

namespace DIDA_TUPLE_SMR.Tests
{
    [TestClass()]
    public class TupleSpaceSMRTests
    {
        List<Object> _fields;
        List<Object> _fields2;
        List<Object> _fields3;
        List<Object> _fields4;
        Tuple _tuple1;
        Tuple _tuple2;
        Tuple _tuple3;
        Tuple _tuple4;
        TupleSpaceSMR _tupleSpaceSMR;

        [TestInitialize]
        public void TestInitialize()
        {
            _fields = new List<Object>();
            _fields2 = new List<Object>();
            _fields3 = new List<Object>();
            _fields4 = new List<Object>();
        }
        [TestMethod()]
        public void readTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void takeTest()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _tuple1 = new Tuple(_fields);

            _tupleSpaceSMR = new TupleSpaceSMR();

            Assert.AreEqual(0, _tupleSpaceSMR.ItemCount());

            _tupleSpaceSMR.write(_tuple1);

            Assert.AreEqual(1, _tupleSpaceSMR.ItemCount());

            _tupleSpaceSMR.take(_tuple1);

            Assert.AreEqual(0, _tupleSpaceSMR.ItemCount());
        }

        [TestMethod()]
        public void takeInexistantTupleTest()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _fields2.Add("dog");
            _fields2.Add("brown");
            _tuple1 = new Tuple(_fields);
            _tuple2 = new Tuple(_fields2);

            _tupleSpaceSMR = new TupleSpaceSMR();

            Assert.AreEqual(0, _tupleSpaceSMR.ItemCount());
            //write <cat,white>
            _tupleSpaceSMR.write(_tuple1);
            Assert.AreEqual(1, _tupleSpaceSMR.ItemCount());
            //take <dog, brown> which will not exists!
            _tupleSpaceSMR.take(_tuple2);

            Assert.AreEqual(1, _tupleSpaceSMR.ItemCount());
        }

        [TestMethod()]
        public void takeUsingWildCardsTest()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _fields2.Add("cat");
            _fields2.Add("*");
            _tuple1 = new Tuple(_fields);
            _tuple2 = new Tuple(_fields2);

            _tupleSpaceSMR = new TupleSpaceSMR();

            Assert.AreEqual(0, _tupleSpaceSMR.ItemCount());
            //write <cat,white>
            _tupleSpaceSMR.write(_tuple1);
            Assert.AreEqual(1, _tupleSpaceSMR.ItemCount());
            //take <cat, *>
            _tupleSpaceSMR.take(_tuple2);
            Assert.AreEqual(0, _tupleSpaceSMR.ItemCount());
        }

        [TestMethod()]
        public void takeUsingWildCards2Test()
        {
            _fields.Add("cat");
            _fields.Add("white");

            _fields2.Add("cat");
            _fields2.Add("gray");

            _fields3.Add("dog");
            _fields3.Add("white");

            _fields4.Add("*");
            _fields4.Add("white");
            _tuple1 = new Tuple(_fields);
            _tuple2 = new Tuple(_fields2);
            _tuple3 = new Tuple(_fields3);
            _tuple4 = new Tuple(_fields4);

            _tupleSpaceSMR = new TupleSpaceSMR();

            //write <cat,white>
            _tupleSpaceSMR.write(_tuple1);
            //write <cat,gray>
            _tupleSpaceSMR.write(_tuple2);
            //write <dog,white>
            _tupleSpaceSMR.write(_tuple3);

            Assert.AreEqual(3, _tupleSpaceSMR.ItemCount());

            //take <*, white>
            _tupleSpaceSMR.take(_tuple4);
            Assert.AreEqual(2, _tupleSpaceSMR.ItemCount());

            //take <*, white> again and remove the second one
            _tupleSpaceSMR.take(_tuple4);
            Assert.AreEqual(1, _tupleSpaceSMR.ItemCount());
        }

        [TestMethod()]
        public void writeTest()
        {
            Assert.Fail();
        }
    }
}