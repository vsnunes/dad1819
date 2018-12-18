using System;
using System.Collections.Generic;
using System.Linq;

using System.Text.RegularExpressions;

namespace DIDA_LIBRARY
{
    /// <summary>
    /// A Class for describing the Tuple Item.
    /// </summary>
    [Serializable]
    public class Tuple
    {
        /// <summary>
        /// List of tuple fields
        /// </summary>
        private List<Object> _fields;

        /// <summary>
        /// The status of a tuple.
        /// It can be locked because of some tuple space operation.
        /// </summary>
        public bool Locker;


        public Tuple(List<Object> fields)
        {
            _fields = new List<object>();
            Locker = false;

            foreach (Object obj in fields)
            {
                _fields.Add(obj);
            }
        }

        public Object GetFieldByNumber(int n)
        {
            if (n >= 0 & n <= this.GetNumberOfFields() - 1)
            {
                return _fields[n];
            }
            else throw new IndexOutOfRangeException();
        }

        public int GetNumberOfFields()
        {
            return _fields.Count;
        }


        public List<Object> GetAllFields()
        {
            List<Object> _objects = new List<Object>();

            foreach (Object obj in _fields)
            {
                _objects.Add(obj);
            }

            return _objects;
        }

        public List<Type> GetTypeOfFields()
        {
            List<Type> _types = new List<Type>();

            foreach (Object obj in _fields)
            {
                _types.Add(obj.GetType());
            }

            return _types;
        }

        /// <summary>
        /// Allow comparation using String WildCards Only.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns>True if the two string matches the wildcards criteria.</returns>
        private bool WildComparator(string s1, string s2)
        {
            
            string regex;
            s1 = s1.TrimStart('"').TrimEnd('"');
            s2 = s2.TrimStart('"').TrimEnd('"');
            if (s1.StartsWith("*") || s1.EndsWith("*"))
            {
                regex = s1.Replace("*", ".*");
                return Regex.IsMatch(s2, regex);
            }

            if (s2.StartsWith("*") || s2.EndsWith("*"))
            {
                regex = s2.Replace("*", ".*");
                return Regex.IsMatch(s1, regex);
            }

            return s1.Equals(s2);
        }

        /// <summary>
        /// Allow comparation using String WildCards with object elements.
        /// </summary>
        /// <param name="s1">The Wildcard</param>
        /// <param name="s2">The object to be compared</param>
        /// <returns>True if the object matches the wildcard string criteria.</returns>
        private bool WildComparator(string s1, object s2)
        {
            string s2Name = s2.GetType().Name;
            return s1 == s2Name; //WildComparator(s1.TrimStart('"').TrimEnd('"'), s2Name);
        }

        /// <summary>
        /// Given two tuples check if they are equals.
        /// Be aware that Equals support all WildCards operations. So <"dog", "*"> is equal to <"dog", "brown">.
        /// </summary>
        /// <param name="obj">An object of type Tuple to compare</param>
        /// <returns>True if the two objects are the same (or wild-identical). False otherwise. </returns>
        public override bool Equals(object obj)
        {
            //If other than Tuple object is given return false immediately
            if (obj.GetType() == this.GetType())
            {

                Tuple tuple = obj as Tuple;
                // 1 -> Starts by comparing the number of fields of two tuples
                int numberOfFields = this.GetNumberOfFields();
                if (tuple.GetNumberOfFields() == numberOfFields)
                {
                    List<Object> otherObjects = tuple.GetAllFields();
                    for (int i = 0; i < numberOfFields; i++)
                    {
                        // 2 -> Compare the types of the fields.
                        //When tuple fields are string instead of calling equals method call the wildcomparator
                        //to check for wildcards in strings
                        if (tuple.GetFieldByNumber(i).GetType() == typeof(string))
                        {
                            if (this.GetFieldByNumber(i) == null) return false;

                            if (this.GetFieldByNumber(i).GetType() != typeof(string))
                            {
                                //Wildcard support of class name using objects
                                if (WildComparator(tuple.GetFieldByNumber(i) as string, this.GetFieldByNumber(i)) == false)
                                    return false;
                            }
                            else if (WildComparator(tuple.GetFieldByNumber(i) as string, this.GetFieldByNumber(i) as string) == false)
                                return false;
                        }
                        // 3 -> Cross comparation
                        // Wild card can be on this object or on obj argument
                        else if (this.GetFieldByNumber(i).GetType() == typeof(string))
                        {
                            if (tuple.GetFieldByNumber(i) == null) return false;

                            if (tuple.GetFieldByNumber(i).GetType() != typeof(string))
                            {
                                //Wildcard support of class name using objects
                                if (WildComparator(this.GetFieldByNumber(i) as string, tuple.GetFieldByNumber(i)) == false)
                                    return false;
                            }
                            else if (WildComparator(this.GetFieldByNumber(i) as string, tuple.GetFieldByNumber(i) as string) == false)
                                return false;
                            
                        }

                        else if (!(this.GetFieldByNumber(i).Equals(tuple.GetFieldByNumber(i))))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    
        /// <summary>
        /// Textual representation of a Tuple
        /// </summary>
        /// <returns>A string like: <"Field1", "Field2", "..." ></returns>
        public override string ToString()
        {
            string repr = "<" ;
            List<Object> fields = this.GetAllFields();
            for (int i = 0; i < fields.Count() - 1; i++)
            {
                repr += fields[i].ToString() + ", ";
            }
            if (fields.Count() != 0)
            {
                repr += fields[fields.Count() - 1];   
            }

            repr += ">";
            return repr;
        }

        public override int GetHashCode()
        {
            return _fields.Count();
        }
    }
}
