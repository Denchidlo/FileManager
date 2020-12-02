using FileManager.Parsers.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileManager.Parsers
{
    class JsonParser
    {
        public static string Serialize(object obj)
        {
            return Serialize(obj, 0);
        }
        public static T Deserialize<T>(string json)
        {
            return Deserialize<T>(Parse(json));
        }
        static string Serialize(object obj, int depth)
        {
            if (obj == null)
                return "";
            Type type = obj.GetType();
            StringBuilder sb;
            if (type.GetCustomAttribute(typeof(JsonIgnore)) != null || type.GetCustomAttribute(typeof(ParseIgnore)) != null
                    || type.GetCustomAttribute(typeof(NonSerializedAttribute)) != null)
                return "";
            if (type.IsPrimitive || type.IsEnum)
                return $"{obj}";
            if (type == typeof(string))
                return $"\"{obj}\"";
            if (ParserUtils.IsEnumerable(type))
            {
                bool isComplex = false;
                sb = new StringBuilder("");
                int counter = 0;
                int collectionLength = ParserUtils.CollectionLength((IEnumerable)obj);
                foreach (var subObj in (IEnumerable)obj)
                {
                    if (subObj == null)
                        continue;
                    string value = $"{Serialize(subObj, depth + 1)}";
                    if (value.Trim(new char[] { '\t', '\n', ' ' }).First() == '{')
                        isComplex = true;
                    if (counter < collectionLength - 1)
                    {
                        value = value.TrimEnd('\n');
                        value += ',';
                    }
                    sb.Append(value);
                    counter++;
                }
                if (isComplex)
                {
                    sb.Insert(0, $"\n{new string('\t', depth)}[");
                    sb.AppendLine();
                    sb.Append($"{new string('\t', depth)}]");
                }
                else
                {
                    sb.Insert(0, "[");
                    sb.Append("]");
                }
            }
            else
            {
                sb = new StringBuilder($"\n{new string('\t', depth)}{{\n");
                MemberInfo[] members = type.GetProperties();
                members = members.Concat(type.GetFields()).ToArray();
                int counter = 0;
                int length = members.Length;
                foreach (var member in members)
                {
                    if (member.GetCustomAttribute(typeof(JsonIgnore)) != null || member.GetCustomAttribute(typeof(ParseIgnore)) != null
                            || member.GetCustomAttribute(typeof(NonSerializedAttribute)) != null || ParserUtils.GetMemberValue(obj, member.Name) == null)
                        continue;
                    string value = $"{Serialize(ParserUtils.GetMemberValue(obj, member.Name), depth + 1)}";
                    sb.Append($"{new string('\t', depth + 1)}\"{member.Name}\" : {value}".TrimEnd('\n'));  //           "name" : { field }
                    if (counter != length - 1)
                        sb.Append(',');
                    sb.AppendLine();
                    ++counter;
                }
                sb.Append($"{new string('\t', depth)}}}\n");
            }
            return sb.ToString();
        }
        static List<DeserializableObject> Parse(string json)
        {
            List<DeserializableObject> objects = new List<DeserializableObject>();
            List<string> values = new List<string>();
            string key = "", value = "";
            int braces = 0, squares = 0;
            bool InQuotes = false;
            bool InKey = true;
            Regex trimming = new Regex("^\\s*{(?<object>.*)}\\s*$", RegexOptions.Singleline);
            Match match = trimming.Match(json);
            if (match.Success)
            {
                json = match.Groups["object"].Value;
            }
            foreach (char ch in json)
            {
                if (char.IsPunctuation(ch) || char.IsLetterOrDigit(ch) || InQuotes)
                {
                    if (ch == '\"')
                    {
                        InQuotes = !InQuotes;
                    }
                    if (InQuotes)
                    {
                        if (InKey)
                        {
                            key += ch;
                        }
                        else
                        {
                            value += ch;
                        }
                        continue;
                    }

                    if (ch == '{')
                    {
                        braces++;
                    }
                    else if (ch == '}')
                    {
                        braces--;
                    }
                    else if (ch == '[' && braces == 0)
                    {
                        squares++;
                        if (squares == 1)
                        {
                            continue;
                        }
                    }
                    else if (ch == ']' && braces == 0)
                    {
                        squares--;
                        if (squares == 0)
                        {
                            continue;
                        }
                    }
                    else if (ch == ':' && braces == 0 && squares == 0)
                    {
                        InKey = false;
                        continue;
                    }
                    else if (ch == ',' && braces == 0)
                    {
                        if (InKey)
                        {
                            value = key;
                            key = "";
                        }
                        values.Add(value);
                        value = "";
                        if (squares == 0)
                        {
                            objects.Add(new DeserializableObject(key, values));
                            values = new List<string>();
                            key = "";
                            value = "";
                            InKey = true;
                        }
                        continue;
                    }
                    if (InKey)
                    {
                        key += ch;
                    }
                    else
                    {
                        value += ch;
                    }

                }
            }
            if (braces == 0 && squares == 0)
            {
                if (!(key == "" && value == ""))
                {
                    if (InKey)
                    {
                        values.Add(key);
                        key = "";
                    }
                    else
                    {
                        values.Add(value);
                    }
                    objects.Add(new DeserializableObject(key, values));
                }
            }
            else
            {
                throw new Exception("Json read failure");
            }
            return objects;
        }
        static T Deserialize<T>(List<DeserializableObject> objects)
        {
            T result;
            Type type = typeof(T);

            if (objects.Count == 1 && objects.First().Key == "")
            {
                if (objects.First().ObjType == DeserializableObject.ObjectType.Primitive)
                    return (T)Convert.ChangeType(objects.First().Value.Trim('\"'), type);
                else if (objects.First().ObjType == DeserializableObject.ObjectType.Collection)
                {
                    if (ParserUtils.IsEnumerable(type))
                    {
                        return (T)GetEnumerableInstance(objects.First(), type);
                    }
                }
            }
            result = (T)Activator.CreateInstance(type);
            foreach (var obj in objects)
            {
                string key = obj.Key.Trim('\"');
                string value = obj.Value.Trim('\"');
                Type memberType = ParserUtils.GetMemberType(type, key);
                if (obj.ObjType == DeserializableObject.ObjectType.Primitive)
                {
                    object converted;
                    if (memberType.IsEnum)
                        converted = Enum.Parse(memberType, value);
                    else
                        converted = Convert.ChangeType(value, memberType);
                    ParserUtils.SetMemberValue(result, key, converted);
                }
                else if (obj.ObjType == DeserializableObject.ObjectType.ComplexObject)
                {
                    object parsed = typeof(JsonParser)
                        .GetMethod("Deserialize")
                        .MakeGenericMethod(memberType)
                        .Invoke(null, new object[] { Parse(value) });
                    ParserUtils.SetMemberValue(result, key, parsed);
                }
                else
                {
                    if (ParserUtils.IsEnumerable(memberType))
                        ParserUtils.SetMemberValue(result, key, GetEnumerableInstance(obj, memberType));
                    else
                        throw new Exception("Invalid deserialization");
                }
            }
            return result;
        }
        static object GetEnumerableInstance(DeserializableObject obj, Type type)
        {
            Type genericArgument = type.GenericTypeArguments.Length == 0 ? type.GetElementType() : type.GenericTypeArguments[0];
            List<object> objects = new List<object>();
            Type listType = typeof(List<>).MakeGenericType(new Type[] { genericArgument });
            IList list = Activator.CreateInstance(listType) as IList;
            foreach (string subValue in obj.Values)
            {
                list.Add(typeof(JsonParser)
                    .GetMethod("Deserialize")
                    .MakeGenericMethod(new Type[] { genericArgument })
                    .Invoke(null, new object[] { subValue.Trim() }));
            }
            if (type.IsArray)
            {
                Array array = Array.CreateInstance(genericArgument, list.Count);
                list.CopyTo(array, 0);
                return array;
            }
            else
            {
                Type IEnumerableGenericType = typeof(IEnumerable<>).MakeGenericType(new Type[] { genericArgument });
                ConstructorInfo info = type.GetConstructor(new Type[] { IEnumerableGenericType });
                if (info != null)
                    return Activator.CreateInstance(type, new object[] { list });
                throw new Exception("Type is not a collection");
            }
        }
    }
}
