using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIDA_LIBRARY
{
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
            return _fields[n];
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
                        if (!(this.GetFieldByNumber(i).Equals(tuple.GetFieldByNumber(i))))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
