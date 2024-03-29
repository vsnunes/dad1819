﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public void EqualsWc()
        {

            List<object> _list = new List<object>();
            _list.Add("cat");
            _list.Add("white");
            Tuple _tup = new Tuple(_list);

            List<Object> _f1 = new List<object>();
            _f1.Add("cat");
            _f1.Add("*");
            Tuple _t1 = new Tuple(_f1);
            Assert.AreEqual(_tup, _t1);
           

            _f1 = new List<object>();
            _f1.Add("cat");
            _f1.Add("w*");
            _t1 = new Tuple(_f1);
            Assert.AreEqual(_tup, _t1);

            _f1 = new List<object>();
            _f1.Add("cat");
            _f1.Add("c*");
            _t1 = new Tuple(_f1);
            Assert.AreNotEqual(_tup, _t1);

            _f1 = new List<object>();
            _f1.Add("cat");
            _f1.Add("*c");
            _t1 = new Tuple(_f1);
            Assert.AreNotEqual(_tup, _t1);

        }

        [TestMethod()]
        public void EqualsWc2()
        {

            List<object> _list = new List<object>();
            _list.Add("white");
            _list.Add("cat");
            Tuple _tup = new Tuple(_list);

            List<Object> _f1 = new List<object>();
            _f1.Add("white");
            _f1.Add("*");
            Tuple _t1 = new Tuple(_f1);
            Assert.AreEqual(_tup, _t1);


            _f1 = new List<object>();
            _f1.Add("white");
            _f1.Add("ca*");
            _t1 = new Tuple(_f1);
            Assert.AreEqual(_tup, _t1);

            _f1 = new List<object>();
            _f1.Add("white");
            _f1.Add("*t");
            _t1 = new Tuple(_f1);
            Assert.AreEqual(_tup, _t1);

            _f1 = new List<object>();
            _f1.Add("white");
            _f1.Add("*ate");
            _t1 = new Tuple(_f1);
            Assert.AreNotEqual(_tup, _t1);

        }

        [TestMethod()]
        public void TupleTest()
        {
            Assert.AreEqual(tuple1.GetType(), typeof(Tuple));
        }

        [TestMethod()]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void InvalidNumberUnderGetFieldByNumber()
        {
            tuple1.GetFieldByNumber(-1);
        }

        [TestMethod()]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void InvalidNumberOverGetFieldByNumber()
        {
            tuple1.GetFieldByNumber(3);
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
        public void SimpleDistributeProp1Equals()
        {
            List<object> fields = new List<object>();
            fields.Add("1");
            Tuple t1 = new Tuple(fields);

            List<object> fields2 = new List<object>();
            fields2.Add("2");
            Tuple t2 = new Tuple(fields2);

            Assert.AreNotEqual(t1, t2);
            Assert.AreNotEqual(t2, t1);

        }

        [TestMethod()]
        public void SimpleDistributeProp2Equals()
        {
            List<object> fields = new List<object>();
            fields.Add("1");
            Tuple t1 = new Tuple(fields);

            List<object> fields2 = new List<object>();
            fields2.Add("1");
            Tuple t2 = new Tuple(fields2);

            Assert.AreEqual(t1, t2);
            Assert.AreEqual(t2, t1);

        }

        [TestMethod()]
        public void GetNumberOfFieldsTest()
        {
            Assert.AreEqual(tuple1.GetNumberOfFields(), 3);
            Assert.AreEqual(tuple2.GetNumberOfFields(), 1);
        }

        [TestMethod()]
        public void WildCardsUsingObjects1Test()
        {
            List<object> _list = new List<object>();
            _list.Add("DADTest*");
            _list.Add("cat");
            Tuple _tup = new Tuple(_list);

            List<object> _list2 = new List<object>();
            _list2.Add(new DADTestA(1,"ola"));
            _list2.Add("cat");
            Tuple _tup2 = new Tuple(_list2);

            Assert.AreEqual(_tup, _tup2);

        }

        [TestMethod()]
        public void WildCardsUsingObjects2Test()
        {
            List<object> _list = new List<object>();
            _list.Add("*TestA");
            _list.Add("cat");
            Tuple _tup = new Tuple(_list);

            List<object> _list2 = new List<object>();
            _list2.Add(new DADTestA(1, "ola"));
            _list2.Add("cat");
            Tuple _tup2 = new Tuple(_list2);

            Assert.AreEqual(_tup, _tup2);

        }

        [TestMethod()]
        public void WildCardsUsingObjects3Test()
        {
            List<object> _list = new List<object>();
            _list.Add("DADTestA");
            _list.Add("cat");
            Tuple _tup = new Tuple(_list);

            List<object> _list2 = new List<object>();
            _list2.Add(new DADTestA(1, "ola"));
            _list2.Add("cat");
            Tuple _tup2 = new Tuple(_list2);

            Assert.AreEqual(_tup, _tup2);

        }

        [TestMethod()]
        public void WildCardsUsingObjectsStressTest()
        {
            List<object> _list = new List<object>();
            _list.Add("DADTestA");
            _list.Add("*Test*");
            Tuple _tup = new Tuple(_list);

            List<object> _list2 = new List<object>();
            _list2.Add(new DADTestA(1, "ola"));
            _list2.Add(new DADTestB(2, "Sodade", 4));
            Tuple _tup2 = new Tuple(_list2);

            Assert.AreEqual(_tup, _tup2);

            _list = new List<object>();
            _list.Add("DAD*");
            _list.Add("*Test*");
            _list.Add("Sodade");
            _tup = new Tuple(_list);

            _list2 = new List<object>();
            _list2.Add(new DADTestA(1, "ola"));
            _list2.Add(new DADTestB(2, "Sodade", 4));
            _list2.Add("Sodade");
            _tup2 = new Tuple(_list2);

            Assert.AreEqual(_tup, _tup2);

        }

        [TestMethod()]
        public void WildCardsUsingObjectsDiferentTest()
        {
            List<object> _list = new List<object>();
            _list.Add("*TestA");
            _list.Add("*TestA");
            Tuple _tup = new Tuple(_list);

            List<object> _list2 = new List<object>();
            _list2.Add(new DADTestA(1, "ola"));
            _list2.Add(new DADTestB(2, "Sodade", 4));
            Tuple _tup2 = new Tuple(_list2);

            Assert.AreNotEqual(_tup, _tup2);
            
        }

        [TestMethod()]
        public void WildCardsAtDiferentPositionTest()
        {
            List<object> _list = new List<object>();
            _list.Add("c*");
            _list.Add("*TestB");
            Tuple _tup = new Tuple(_list);

            List<object> _list2 = new List<object>();
            _list2.Add("cat");
            _list2.Add(new DADTestB(2, "Sodade", 4));
            Tuple _tup2 = new Tuple(_list2);

            Assert.AreEqual(_tup, _tup2);

            _list = new List<object>();
            _list.Add("*og");
            _list.Add("brown");
            _list.Add("*gly");
            _list.Add("*Test*");
            _list.Add("*mar*");
            _tup = new Tuple(_list);

             _list2 = new List<object>();
            _list2.Add("dog");
            _list2.Add("brown");
            _list2.Add("ugly");
            _list2.Add(new DADTestB(2, "Sodade", 4));
            _list2.Add("smart");
            _tup2 = new Tuple(_list2);

            Assert.AreEqual(_tup, _tup2);


        }
    }
}