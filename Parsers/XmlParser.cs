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
    class XmlParser
    {
        public static string Serialize(object obj)
        {
            return Serialize(obj, 0, "");
        }
        public static T Deserialize<T>(string xml)
        {
            List<DeserializableObject> objects = Parse(xml, true);
            return Deserialize<T>(objects);
        }
        static T Deserialize<T>(List<DeserializableObject> objects)
        {
            T result;
            Type type = typeof(T);

            if (objects.First().ObjType == DeserializableObject.ObjectType.Primitive
                    && objects.Count == 1
                    && objects.First().Key == "")
                return (T)Convert.ChangeType(objects.First().Value.Trim('\"'), type);
            if (ParserUtils.IsEnumerable(type) && objects.Count > 0)
            {
                objects.First().ObjType = DeserializableObject.ObjectType.Collection;
                return (T)GetEnumerableInstance(objects.First(), type);
            }
            result = (T)Activator.CreateInstance(type);
            foreach (var obj in objects)
            {
                string key = obj.Key;
                string value = obj.Value.Trim('\"');
                Type memberType = ParserUtils.GetMemberType(type, key);
                if (obj.ObjType == DeserializableObject.ObjectType.Primitive)
                {
                    object converted;
                    if (memberType.IsEnum)
                    {
                        converted = Enum.Parse(memberType, value);
                    }
                    else
                    {
                        converted = Convert.ChangeType(value, memberType);
                    }
                    ParserUtils.SetMemberValue(result, key, converted);
                }
                else if (obj.ObjType == DeserializableObject.ObjectType.ComplexObject)
                {

                    if (ParserUtils.IsEnumerable(ParserUtils.GetMemberType(type, key)))
                    {
                        if (ParserUtils.IsEnumerable(memberType))
                            ParserUtils.SetMemberValue(result, key, GetEnumerableInstance(obj, memberType));
                    }
                    else
                    {
                        object parsed = typeof(XmlParser).GetMethod("Deserialize", BindingFlags.NonPublic | BindingFlags.Static)
                                                     .MakeGenericMethod(memberType)
                                                     .Invoke(null, new object[] { Parse(value, false) });
                        ParserUtils.SetMemberValue(result, key, parsed);
                    }

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
        static string Serialize(object obj, int depth, string key)
        {
            if (obj == null)
                return "";
            Type type = obj.GetType();
            StringBuilder sb;
            if (type.GetCustomAttribute(typeof(NonSerializedAttribute)) != null
                    || type.GetCustomAttribute(typeof(XmlIgnore)) != null
                    || type.GetCustomAttribute(typeof(ParseIgnore)) != null)
                return "";
            if (type.IsPrimitive
                || type.IsEnum
                || type == typeof(string))
            {
                key = (key == "") ? type.Name : key;
                return (new StringBuilder($"{new string('\t', depth)}<{key}>{obj}</{key}>\n")).ToString();
            }
            if (ParserUtils.IsEnumerable(type))
            {
                Type genericArgument = (type.GenericTypeArguments.Length == 0) ? type.GetElementType() : type.GenericTypeArguments[0];
                if (key == "")
                    key = $"{type.Name}_{genericArgument.Name}";
                sb = new StringBuilder($"{new string('\t', depth)}<{key}>\n");
                foreach (object el in (IEnumerable)obj)
                    sb.Append(Serialize(el, depth + 1, genericArgument.Name));
                sb.AppendLine($"{new string('\t', depth)}</{key}>");
            }
            else
            {
                key = (key == "") ? type.Name : key;
                sb = new StringBuilder($"{new string('\t', depth)}<{key}>\n");
                MemberInfo[] members = type.GetFields();
                members = members.Concat(type.GetProperties()).ToArray();
                int counter = 0;
                foreach (var member in members)
                {
                    if (member.GetCustomAttribute(typeof(XmlIgnore)) != null || member.GetCustomAttribute(typeof(ParseIgnore)) != null
                            || member.GetCustomAttribute(typeof(NonSerializedAttribute)) != null || ParserUtils.GetMemberValue(obj, member.Name) == null)
                        continue;
                    string value = Serialize(ParserUtils.GetMemberValue(obj, member.Name), depth + 1, member.Name);
                    if (counter == members.Length - 1)
                        value = value.TrimEnd(new char[] { '\t', '\n', ' ' });
                    sb.Append(value);
                    counter++;
                }
                sb.AppendLine($"\n{new string('\t', depth)}</{key}>");
            }
            return sb.ToString();
        }
        static List<DeserializableObject> Parse(string xml, bool trim)
        {
            xml = xml.Trim(new char[] { '\n', '\t', '\r', ' ' });
            List<DeserializableObject> objects = new List<DeserializableObject>();
            List<string> values = new List<string>();
            string tagName;
            Match match;
            try
            {
                tagName = GetNextTag(xml, 0);
                if (trim)
                {
                    Regex trimming = new Regex($"^<{tagName}>(.*)</{tagName}>$", RegexOptions.Singleline);
                    match = trimming.Match(xml);
                    if (match.Success)
                    {
                        xml = match.Groups[1].Value;
                    }
                }
            }
            catch
            {
                return new List<DeserializableObject>() { new DeserializableObject("", new List<string>() { xml }, DeserializableObject.ObjectType.Primitive) };
            }

            Regex Tag = new Regex(@"<(/?.*)>");

            Dictionary<string, List<string>> keyValues = new Dictionary<string, List<string>>();
            string mainTag = "", tag = "";
            int deep = 0;
            bool isMainTag = true, isValue = false;
            string value = "";
            bool quotes = false;
            foreach (char c in xml)
            {
                if ((c != '\t' && c != '\r' && c != '\n') || quotes)
                {
                    if (c == '\"')
                    {
                        quotes = !quotes;
                    }
                    if (quotes)
                    {
                        value += c;
                        continue;
                    }
                    if (c == '<')
                    {
                        isValue = false;
                        if (!isMainTag)
                        {
                            tag += c;
                        }
                    }
                    else if (c == '>')
                    {
                        if (isMainTag)
                        {
                            isMainTag = false;
                            isValue = true;
                            deep++;
                        }
                        else
                        {
                            tag += c;
                            match = Tag.Match(tag);

                            if (match.Success)
                            {
                                tagName = match.Groups[1].Value;
                                if (tagName[0] == '/')
                                {
                                    if ('/' + mainTag == tagName && deep == 1)
                                    {
                                        if (keyValues.ContainsKey(mainTag))
                                            keyValues[mainTag].Add(value);
                                        else
                                            keyValues.Add(mainTag, new List<string>() { value });
                                        mainTag = "";
                                        tag = "";
                                        isMainTag = true;
                                        isValue = false;
                                        value = "";
                                    }
                                    else
                                    {
                                        value += tag;
                                        tag = "";
                                        isValue = true;
                                    }
                                    deep--;
                                }
                                else
                                {
                                    deep++;
                                    isValue = true;
                                    value += tag;
                                    tag = "";
                                }
                            }
                            else
                            {
                                throw new Exception("XML file was damaged");
                            }
                        }
                    }
                    else
                    {
                        if (isValue)
                        {
                            value += c;
                        }
                        else if (isMainTag)
                        {
                            mainTag += c;
                        }
                        else
                        {
                            tag += c;
                        }
                    }
                }
            }
            if (mainTag != "")
                return new List<DeserializableObject>() { new DeserializableObject("", new List<string>() { mainTag }, DeserializableObject.ObjectType.Primitive) };
            foreach (KeyValuePair<string, List<string>> pair in keyValues)
            {
                DeserializableObject.ObjectType type;
                if (pair.Value.First().Length > 0 && pair.Value.First()[0] == '<')
                    type = DeserializableObject.ObjectType.ComplexObject;
                else
                    type = DeserializableObject.ObjectType.Primitive;
                objects.Add(new DeserializableObject(pair.Key, pair.Value, type));
            }
            return objects;
        }
        static string GetNextTag(string xml, int startIndex)
        {
            StringBuilder tag = new StringBuilder("");
            bool isTag = false;
            for (int i = startIndex; i < xml.Length; i++)
            {
                char ch = xml[i];
                if (ch == '<')
                {
                    isTag = true;
                    continue;
                }
                else if ((ch == '>' || ch == ' ') && isTag)
                {
                    return tag.ToString();
                }
                else if (isTag)
                {
                    tag.Append(ch);
                }
            }
            throw new Exception($"No tags are left from current position {startIndex}");
        }
        static object GetEnumerableInstance(DeserializableObject obj, Type type)
        {
            Type genericArgument = type.GenericTypeArguments.Length == 0 ? type.GetElementType() : type.GenericTypeArguments[0];
            List<object> objects = new List<object>();
            Type listType = typeof(List<>).MakeGenericType(new Type[] { genericArgument });
            IList list = Activator.CreateInstance(listType) as IList;
            if (obj.ObjType == DeserializableObject.ObjectType.ComplexObject)
                obj = Parse(obj.Value, false).First();
            foreach (string subVal in obj.Values)
            {
                list.Add(typeof(XmlParser).GetMethod("Deserialize")
                                          .MakeGenericMethod(new Type[] { genericArgument })
                                          .Invoke(null, new object[] { subVal.Trim() }));
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
