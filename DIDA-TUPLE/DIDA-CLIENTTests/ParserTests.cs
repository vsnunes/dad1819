using Microsoft.VisualStudio.TestTools.UnitTesting;
using DIDA_CLIENT;
using DIDA_LIBRARY;
using Tuple = DIDA_LIBRARY.Tuple;
using System;
using System.Collections.Generic;

namespace DIDA_CLIENT.Tests
{
    [TestClass()]
    public class ParserTests
    {

        private List<Object> _fields;
        private List<Object> _fields2;
        private Tuple _tuple1;
        private Tuple _tuple2;
        private Tuple _tuple3;

        private Scanner _scanner;
        private Parser _parser;

        private const string INST_1 = "add <\"dog\", \"brown\">";
        private const string INST_2 = "add <\"dog\", \"*\">";

        private const string INST_3 = "add <\"dog\", DADTestA(1, \"Cat\")>";

        private const string INST_4 = "read <\"1\">";
        private const string INST_5 = "read <\"2\">";

        [TestInitialize]
        public void TestInitialize()
        {
            _fields = new List<object>();
            _fields2 = new List<object>();
            _fields.Add("dog");
            _fields.Add("brown");

            _tuple1 = new Tuple(_fields);

            _scanner = new Scanner();

            _parser = new Parser(_scanner);
        }

        [TestMethod()]
        public void SimpleParserTest()
        {
            ParseTree tree = _parser.Parse(INST_1);

            Assert.IsNotNull(tree);

            Tuple t = (Tuple) tree.Eval(null);

            Assert.IsNotNull(t);

            Assert.AreEqual(_tuple1, t);


        }

        [TestMethod()]
        public void SimpleParserWithWildCardsTest()
        {
            ParseTree tree = _parser.Parse(INST_2);

            Assert.IsNotNull(tree);

            Tuple t = (Tuple)tree.Eval(null);

            Assert.IsNotNull(t);

            Assert.AreEqual(_tuple1, t);
        }

        [TestMethod()]
        public void ParseStringAndObjectTest()
        {
            ParseTree tree = _parser.Parse(INST_3);

            Assert.IsNotNull(tree);

            Tuple t = (Tuple)tree.Eval(null);

            Assert.IsNotNull(t);

            _fields2.Add("dog");
            _fields2.Add(new DADTestA(1, "Cat"));

            _tuple2 = new Tuple(_fields2);

            Assert.AreEqual(_tuple2.GetNumberOfFields(), t.GetNumberOfFields());

            Assert.AreEqual(_tuple2, t);
        }

        [TestMethod()]
        public void ParseEqualsTest()
        {
            ParseTree tree1 = _parser.Parse(INST_4);

            Assert.IsNotNull(tree1);

            Tuple t1 = (Tuple)tree1.Eval(null);

            Assert.IsNotNull(t1);

            ParseTree tree2 = _parser.Parse(INST_5);

            Assert.IsNotNull(tree2);

            Tuple t2 = (Tuple)tree2.Eval(null);

            Assert.IsNotNull(t2);

            Assert.AreNotEqual(t1, t2);

            Assert.AreNotEqual(t2, t1);


        }

      
    }
}