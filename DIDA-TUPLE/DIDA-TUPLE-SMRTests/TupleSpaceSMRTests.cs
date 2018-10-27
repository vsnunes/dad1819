using Microsoft.VisualStudio.TestTools.UnitTesting;
using DIDA_TUPLE_SMR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
            _tupleSpaceSMR = new TupleSpaceSMR();
        }
        [TestMethod()]
        public void readTest()
        {
            //Assert.Fail();
        }

        [TestMethod()]
        public void takeTest()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _tuple1 = new Tuple(_fields);

            Assert.AreEqual(0, _tupleSpaceSMR.ItemCount());

            _tupleSpaceSMR.write(_tuple1);

            Assert.AreEqual(1, _tupleSpaceSMR.ItemCount());

            _tupleSpaceSMR.take(_tuple1);

            Assert.AreEqual(0, _tupleSpaceSMR.ItemCount());
        }

        /// <summary>
        /// A test to check for thread waitness of take.
        /// This is a multithreading test.
        /// </summary>
        [TestMethod()]
        public void takeNotAvailableTest()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _fields2.Add("dog");
            _fields2.Add("brown");
            _tuple1 = new Tuple(_fields);
            _tuple2 = new Tuple(_fields2);

            Assert.AreEqual(0, _tupleSpaceSMR.ItemCount());
            //write <cat,white>
            _tupleSpaceSMR.write(_tuple1);
            Assert.AreEqual(1, _tupleSpaceSMR.ItemCount());

            Task.Run(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //lets delay the write in 1 second
                Thread.Sleep(1000);
                _tupleSpaceSMR.write(_tuple2);
                return; //just to ensure that we stop the thread
            });

            Assert.AreEqual(1, _tupleSpaceSMR.ItemCount());
            //take <dog, brown> which will only exists 1 sec ahead!
            _tupleSpaceSMR.take(_tuple2);

            //Even if we add <dog, brown> take operation will remove it so
            //only <cat,white> should exists!
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
            _fields = new List<object>();
            _fields.Add("dog");
            _fields.Add("brown");
            Tuple tuple1 = new Tuple(_fields);
            _tupleSpaceSMR.write(tuple1);
            List<Tuple> tlist = _tupleSpaceSMR.GetTuples();


            List<Tuple> testtlist = new List<Tuple>();
            testtlist.Add(tuple1);
            List<Object> testolist;
            List<Object> test;

            for (int i = 0; i < tlist.Count(); i++)
            {
                test = tlist.ElementAt(i).GetAllFields();
                testolist = testtlist.ElementAt(i).GetAllFields();
                if (test.Count() == testolist.Count())
                {
                    for (int k = 0; k < test.Count(); k++)
                    {
                        Assert.IsTrue(test.ElementAt(k).Equals(testolist.ElementAt(k)));
                    }
                }

            }
        }

        /// <summary>
        /// A test to check for thread waitness of read.
        /// This is a multithreading test.
        /// </summary>
        [TestMethod()]
        public void readNotAvailableTest()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _tuple1 = new Tuple(_fields);
            Tuple readTuple = null;

            Assert.AreEqual(0, _tupleSpaceSMR.ItemCount());
            Assert.AreEqual(null, readTuple);
            Task.Run(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //lets delay the write in 1 second
                Thread.Sleep(1000);
                _tupleSpaceSMR.write(_tuple1);
                return; //just to ensure that we stop the thread
            });

            Assert.AreEqual(0, _tupleSpaceSMR.ItemCount());

            //current thread will be blocked here 1 second until write.
            readTuple = _tupleSpaceSMR.read(_tuple1);

            Assert.AreEqual(1, _tupleSpaceSMR.ItemCount());
            Assert.AreEqual(_tuple1, readTuple);
        }
    }
}