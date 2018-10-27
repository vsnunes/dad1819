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
        Tuple tuple1;
        Tuple tuple2;
        Tuple tuple3;
        TupleSpaceSMR ts1;

        [TestInitialize]
        public void TestInitialize()
        {
            _fields = new List<object>();

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
            
        }
    }
}