using Microsoft.VisualStudio.TestTools.UnitTesting;
using DIDA_TUPLE_XL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
        TupleSpaceXL _tupleSpace;

        [TestInitialize]
        public void TestInitialize()
        {
            _frontEnd = new FrontEndXL(1);
            _fields = new List<Object>();
            _fields2 = new List<Object>();
            _fields3 = new List<Object>();
            _fields4 = new List<Object>();
            _tupleSpace = new TupleSpaceXL();

        }

        [TestMethod()]
        public void testBasicIntersection()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _tuple1 = new Tuple(_fields);

            _fields2.Add("cat");
            _fields2.Add("white");
            _tuple2 = new Tuple(_fields2);

            List<Tuple> list1 = new List<Tuple>();
            List<Tuple> list2 = new List<Tuple>();

            list1.Add(_tuple1);
            list2.Add(_tuple2);

            Assert.AreEqual(1, list1.Count);
            Assert.AreEqual(1, list2.Count);

            List<List<Tuple>> listOfListsOfTuples = new List<List<Tuple>>();
            listOfListsOfTuples.Add(list1);
            listOfListsOfTuples.Add(list2);

            Assert.AreEqual(2, listOfListsOfTuples.Count);

            IEnumerable<Tuple> intersection = FrontEndXL.Intersection(listOfListsOfTuples);

            Assert.AreEqual(1, intersection.Count());

            //tuple1 or tuple2 must be equals to the intersection
            Assert.AreEqual(_tuple1, intersection.ElementAt(0));
            Assert.AreEqual(_tuple2, intersection.ElementAt(0));


        }

        [TestMethod()]
        public void testIntersection()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _tuple1 = new Tuple(_fields);

            _fields2.Add("cat");
            _fields2.Add("gray");
            _tuple2 = new Tuple(_fields2);

            _fields3.Add("cat");
            _fields3.Add("white");
            _tuple3 = new Tuple(_fields3);

            List<Tuple> list1 = new List<Tuple>();
            List<Tuple> list2 = new List<Tuple>();
            List<Tuple> list3 = new List<Tuple>();

            //list1: [<cat, white>]
            list1.Add(_tuple1);

            Assert.AreEqual(1, list1.Count);

            //list2: [<cat, gray>, <cat, white>]
            list2.Add(_tuple2);
            list2.Add(_tuple3);

            Assert.AreEqual(2, list2.Count);

            //list3: [<cat, white>]
            list3.Add(_tuple3);

            Assert.AreEqual(1, list3.Count);

            List<List<Tuple>> listOfListsOfTuples = new List<List<Tuple>>();
            listOfListsOfTuples.Add(list1);
            listOfListsOfTuples.Add(list2);
            listOfListsOfTuples.Add(list3);

            Assert.AreEqual(3, listOfListsOfTuples.Count);

            IEnumerable<Tuple> intersection = FrontEndXL.Intersection(listOfListsOfTuples);

            Assert.AreEqual(1, intersection.Count());

            //tuple1 or tuple3 must be equals to the intersection
            Assert.AreEqual(_tuple1, intersection.ElementAt(0));
            Assert.AreEqual(_tuple3, intersection.ElementAt(0));


        }

        [TestMethod()]
        public void testIntersectionWithDuplicates()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _tuple1 = new Tuple(_fields);

            _fields2.Add("cat");
            _fields2.Add("gray");
            _tuple2 = new Tuple(_fields2);

            _fields3.Add("dog");
            _fields3.Add("brown");
            _tuple3 = new Tuple(_fields3);

            List<Tuple> list1 = new List<Tuple>();
            List<Tuple> list2 = new List<Tuple>();
            List<Tuple> list3 = new List<Tuple>();

            //list1: [<cat, white>, <cat, gray>]
            list1.Add(_tuple1);
            list1.Add(_tuple2);

            Assert.AreEqual(2, list1.Count);

            //list2: [<cat, gray>, <dog, brown>]
            list2.Add(_tuple2);
            list2.Add(_tuple3);

            Assert.AreEqual(2, list2.Count);

            //list3: [<dog, brown>, <dog, brown>, <dog, brown>, <cat, gray>]
            list3.Add(_tuple3);
            list3.Add(_tuple3);
            list3.Add(_tuple3);
            list3.Add(_tuple2);

            Assert.AreEqual(4, list3.Count);

            List<List<Tuple>> listOfListsOfTuples = new List<List<Tuple>>();
            listOfListsOfTuples.Add(list1);
            listOfListsOfTuples.Add(list2);
            listOfListsOfTuples.Add(list3);

            Assert.AreEqual(3, listOfListsOfTuples.Count);

            IEnumerable<Tuple> intersection = FrontEndXL.Intersection(listOfListsOfTuples);

            Assert.AreEqual(1, intersection.Count());

            Assert.AreEqual(_tuple2, intersection.ElementAt(0));


        }

        [TestMethod()]
        public void SimplewriteTest()
        {
            Assert.AreEqual(0, _tupleSpace.ItemCount());

            _fields.Add("cat");
            _fields.Add("white");
            _tuple1 = new Tuple(_fields);

            _tupleSpace.write(1, 1, _tuple1);

            Assert.AreEqual(1, _tupleSpace.ItemCount());

        }

        [TestMethod()]
        public void writeDuplicatesTest()
        {
            Assert.AreEqual(0, _tupleSpace.ItemCount());

            _fields.Add("cat");
            _fields.Add("white");
            _tuple1 = new Tuple(_fields);

            _fields2.Add("cat");
            _fields2.Add("white");
            _tuple2 = new Tuple(_fields2);

            _tupleSpace.write(1, 1, _tuple1);

            Assert.AreEqual(1, _tupleSpace.ItemCount());

            _tupleSpace.write(1, 2, _tuple2);

            Assert.AreEqual(2, _tupleSpace.ItemCount());

        }

        [TestMethod()]
        public void readTest()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _tuple1 = new Tuple(_fields);

            _fields2.Add("cat");
            _fields2.Add("white");
            _tuple2 = new Tuple(_fields2);

            _tupleSpace.write(1, 1, _tuple1);

            Tuple readReturn = null;
            readReturn = _tupleSpace.read(_tuple2);

            Assert.IsNotNull(readReturn);

            Assert.AreEqual(_tuple1, readReturn);
        }

        [TestMethod()]
        public void readUsingWildCardsTest()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _tuple1 = new Tuple(_fields);

            _fields2.Add("cat");
            _fields2.Add("*");
            _tuple2 = new Tuple(_fields2);

            _tupleSpace.write(1, 1, _tuple1);

            Tuple readReturn = null;
            readReturn = _tupleSpace.read(_tuple2);

            Assert.IsNotNull(readReturn);

            Assert.AreEqual(_tuple1, readReturn);
        }

        [TestMethod()]
        public void readIntensiveTest()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _tuple1 = new Tuple(_fields);

            _fields2.Add("cat");
            _fields2.Add("gray");
            _tuple2 = new Tuple(_fields2);

            _fields3.Add("dog");
            _fields3.Add("brown");
            _tuple3 = new Tuple(_fields3);

            _fields4.Add("dog");
            _fields4.Add("gray");
            _tuple4 = new Tuple(_fields4);

            _tupleSpace.write(1, 1, _tuple1);
            Tuple readReturn = null;

            readReturn = _tupleSpace.read(_tuple1);
            Assert.IsNotNull(readReturn);
            Assert.AreEqual(_tuple1, readReturn);

            _tupleSpace.write(1, 2, _tuple2);

            readReturn = _tupleSpace.read(_tuple2);
            Assert.IsNotNull(readReturn);
            Assert.AreEqual(_tuple2, readReturn);

            //Two writes in a row
            _tupleSpace.write(1, 3, _tuple3);
            _tupleSpace.write(1, 4, _tuple4);

            //This is not a mistake, first read 4 and then read 3
            readReturn = _tupleSpace.read(_tuple4);
            Assert.IsNotNull(readReturn);
            Assert.AreEqual(_tuple4, readReturn);

            readReturn = _tupleSpace.read(_tuple3);
            Assert.IsNotNull(readReturn);
            Assert.AreEqual(_tuple3, readReturn);
        }

        /**
         * Caution: This only test Take 1 Phase!
         * So it is not checked if the item is removed from the tuple space!
         * */
        [TestMethod()]
        public void take1PhaseTest()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _tuple1 = new Tuple(_fields);

            _fields2.Add("cat");
            _fields2.Add("gray");
            _tuple2 = new Tuple(_fields2);

            _tupleSpace.write(1, 1, _tuple1);
            _tupleSpace.write(1, 2, _tuple2);

            //Check if the tuple space has the 2 elements we added right now
            Assert.AreEqual(2, _tupleSpace.ItemCount());

            List<Tuple> takeReturn = null;

            takeReturn = _tupleSpace.take(1, 3, _tuple1);
            Assert.IsNotNull(takeReturn);

            //Only one tuple should be returned
            Assert.AreEqual(1, takeReturn.Count);

            Assert.AreEqual(_tuple1, takeReturn[0]);
        }

        /// <summary>
        /// A test to check for thread waitness of take.
        /// This is a multithreading test.
        /// Caution: This only test Take 1 Phase!
        /// </summary>
        [TestMethod()]
        public void takeNotAvailable1PhaseTest()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _fields2.Add("dog");
            _fields2.Add("brown");
            _tuple1 = new Tuple(_fields);
            _tuple2 = new Tuple(_fields2);

            Assert.AreEqual(0, _tupleSpace.ItemCount());
            //write <cat,white>
            _tupleSpace.write(1,1,_tuple1);
            Assert.AreEqual(1, _tupleSpace.ItemCount());

            Task.Run(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //lets delay the write in 1 second
                Thread.Sleep(1000);
                _tupleSpace.write(2,1,_tuple2);
                return; //just to ensure that we stop the thread
            });

            Assert.AreEqual(1, _tupleSpace.ItemCount());
            //take <dog, brown> which will only exists 1 sec ahead!
            List<Tuple> takeResult = null;

            takeResult = _tupleSpace.take(1, 2, _tuple2);

            //Should exists 2 items <cat, white> and <dog, brown>
            //Caution: This is a 1 phase take test
            Assert.AreEqual(2, _tupleSpace.ItemCount());

            Assert.IsNotNull(takeResult);

            Assert.AreEqual(1, takeResult.Count);

            Assert.AreEqual(_tuple2, takeResult[0]);

        }

        /// <summary>
        /// A test to check for potencial deadlocks on take
        /// This is a multithreading test.
        /// Caution: This only test Take 1 Phase!
        /// </summary>
        [TestMethod(), Timeout(10000)]
        public void TwoTakeOnThreadsTest()
        {
            _fields.Add("cat");
            _fields.Add("white");
            _fields2.Add("dog");
            _fields2.Add("brown");
            _tuple1 = new Tuple(_fields);
            _tuple2 = new Tuple(_fields2);

            Assert.AreEqual(0, _tupleSpace.ItemCount());
            //write <cat,white>
            _tupleSpace.write(1, 1, _tuple1);
            Assert.AreEqual(1, _tupleSpace.ItemCount());

            Task.Run(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //lets delay the write in 1 second
                Thread.Sleep(1000);
                //write <dog, brown>
                _tupleSpace.write(2, 1, _tuple2);
                return; //just to ensure that we stop the thread
            });

            Task.Run(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //performs a take concurrently
                _tupleSpace.take(3, 1, _tuple2);

                return; //just to ensure that we stop the thread
            });

            Task.Run(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                //performs a take concurrently
                _tupleSpace.take(4, 1, _tuple2);

                return; //just to ensure that we stop the thread
            });

            Assert.AreEqual(1, _tupleSpace.ItemCount());
            //take <dog, brown> which will only exists 1 sec ahead!
            List<Tuple> takeResult = null;
            /*takeResult = _tupleSpace.take(1, 2, _tuple2);

            //Should exists 2 items <cat, white> and <dog, brown>
            //Caution: This is a 1 phase take test
            Assert.AreEqual(2, _tupleSpace.ItemCount());

            Assert.IsNotNull(takeResult);

            Assert.AreEqual(1, takeResult.Count);

            Assert.AreEqual(_tuple2, takeResult[0]);*/
            Thread.Sleep(4000);
        }
    }
}