using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DIDA_LIBRARY
{
    [Serializable]
    public class Tuple
    {
        private List<Object> _fields;

        public Tuple(List<Object> fields)
        {
            _fields = new List<object>();

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

        private bool WildComparator(string s1, string s2)
        {
            
            string regex;

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

        public override bool Equals(object obj)
        {
            if (obj.GetType() == this.GetType())
            {

                Tuple tuple = obj as Tuple;
                int numberOfFields = this.GetNumberOfFields();
                if (tuple.GetNumberOfFields() == numberOfFields)
                {
                    List<Object> otherObjects = tuple.GetAllFields();
                    for (int i = 0; i < numberOfFields; i++)
                    {
                        //When tuple fields are string instead of calling equals method call the wildcomparator
                        //to check for wildcards in strings
                        if (this.GetFieldByNumber(i).GetType() == typeof(string))
                        {
                            if (WildComparator(this.GetFieldByNumber(i) as string, tuple.GetFieldByNumber(i) as string) == false)
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
