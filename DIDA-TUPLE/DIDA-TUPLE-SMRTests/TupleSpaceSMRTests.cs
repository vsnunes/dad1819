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
            _fields.Add("dog");
            _fields.Add("brown");
            tuple1 = new Tuple(_fields);
            ts1 = new TupleSpaceSMR();
            

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
            ts1.write(tuple1);
            List<Tuple> tlist = ts1.GetTuples();


            List<Tuple> testtlist = new List<Tuple>();
            testtlist.Add(tuple1);
            List<Object> testolist;
            List<Object> test;

            for(int i = 0; i < tlist.Count(); i++)
            {
                test = tlist.ElementAt(i).GetAllFields();
                testolist = testtlist.ElementAt(i).GetAllFields();
                if(test.Count() == testolist.Count())
                {
                    for(int k = 0; k < test.Count(); k++)
                    {
                        Assert.IsTrue(test.ElementAt(k).Equals(testolist.ElementAt(k)));
                    }
                }
                
            }
        }
    }
}