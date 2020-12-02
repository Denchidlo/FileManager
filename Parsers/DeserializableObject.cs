using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Parsers
{
    public class DeserializableObject
    {
        public enum ObjectType
        {
            Primitive,
            ComplexObject,
            Collection
        }
        public ObjectType ObjType { get; set; }
        public string Key { get; }
        public string Value { get => Values.First(); }
        public List<string> Values { get; set; }
        public DeserializableObject(string key, List<string> values, ObjectType type)
        {
            Key = key;
            Values = values;
            ObjType = type;
        }
        public DeserializableObject(string key, List<string> values)
        {
            Key = key;
            Values = values;
            if (Values.Count > 1)
            {
                ObjType = ObjectType.Collection;
            }
            else if (Value.Contains('{'))
            {
                ObjType = ObjectType.ComplexObject;
            }
            else
            {
                ObjType = ObjectType.Primitive;
            }
        }
    }
}
