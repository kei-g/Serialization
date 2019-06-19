using SnowStep.Serialization.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SnowStep.Serialization
{
    public class BinarySerializer<T>
    {
        private static readonly Dictionary<Type, Func<BinarySource, object>> PrimitiveDeserializers = new Dictionary<Type, Func<BinarySource, object>>
        {
            { typeof(int), r => r.ReadInt32() },
            { typeof(uint), r => r.ReadUInt32() },
            { typeof(long), r => r.ReadInt64() },
            { typeof(ulong), r => r.ReadUInt64() },
        };

        private static readonly Dictionary<Type, Func<object, byte[]>> PrimitiveSerializers = new Dictionary<Type, Func<object, byte[]>>
        {
            { typeof(int), v => BitConverter.GetBytes((int)v) },
            { typeof(uint), v => BitConverter.GetBytes((uint)v) },
            { typeof(long), v => BitConverter.GetBytes((long)v) },
            { typeof(ulong), v => BitConverter.GetBytes((ulong)v) },
        };

        private readonly ConstructorInfo ctor = typeof(T).GetConstructor(new Type[] { });
        private readonly Dictionary<string, Encoding> encodings = new Dictionary<string, Encoding>();
        private readonly BinarySerializerFactory factory;
        private readonly Dictionary<string, Info.Member> members = new Dictionary<string, Info.Member>();
        private readonly SortedDictionary<int, string> order = new SortedDictionary<int, string>();
        private readonly HashSet<string> values = new HashSet<string>();

        private void Process(BinarySerializableAttribute attr, string memberName)
        {
            if (this.order.ContainsKey(attr.Order))
                throw new BinarySerializerException();
            this.order.Add(attr.Order, memberName);
            attr.Visit(this.encodings, memberName);
        }

        internal BinarySerializer(BinarySerializerFactory factory)
        {
            this.factory = factory;
            foreach (var member in new BinaryMembers(typeof(T)))
            {
                var attr = member.GetCustomAttributes<BinarySerializableAttribute>().SingleOrDefault();
                if (attr is null)
                    continue;
                Process(attr, member.Name);
                this.members.Add(member.Name, member);
                if (member.Type.IsValueType)
                    this.values.Add(member.Name);
            }
        }

        public T Deserialize(BinarySource source)
        {
            var obj = this.ctor.Invoke(new object[] { });
            foreach (var order in this.order.Keys)
            {
                var name = this.order[order];
                var member = this.members[name];
                if (!this.values.Contains(name))
                {
                    var b = source.ReadByte();
                    if (b == 0)
                    {
                        member.SetValue(obj, null);
                        continue;
                    }
                    Trace.Assert(b == 255);
                }
                if (PrimitiveDeserializers.TryGetValue(member.Type, out var func))
                    member.SetValue(obj, func(source));
                else if (typeof(string) == member.Type)
                    if (this.encodings.TryGetValue(name, out var encoding))
                        member.SetValue(obj, source.ReadString(encoding));
                    else
                        member.SetValue(obj, source.ReadString(Encoding.UTF8));
                else if (member.Type.IsArray)
                {
                    // XXX: TODO
                }
                else
                {
                    var deserialize = this.factory.GetDeserializer(member.Type);
                    var value = deserialize(source);
                    member.SetValue(obj, value);
                }
            }
            return (T)obj;
        }

        public byte[] Serialize(T obj)
        {
            var list = new List<byte[]>();
            foreach (var order in this.order.Keys)
            {
                var name = this.order[order];
                var member = this.members[name];
                var value = member.GetValue(obj);
                if (!this.values.Contains(name))
                    if (value == null)
                    {
                        list.Add(new byte[] { 0 });
                        continue;
                    }
                    else
                        list.Add(new byte[] { 255 });
                if (PrimitiveSerializers.TryGetValue(member.Type, out var func))
                    list.Add(func(value));
                else if (typeof(string) == member.Type)
                {
                    if (this.encodings.TryGetValue(name, out var encoding))
                        list.Add(encoding.GetBytes((string)value));
                    else
                        list.Add(Encoding.UTF8.GetBytes((string)value));
                    list.Add(new byte[] { 0 });
                }
                else if (member.Type.IsArray)
                {
                    var elementType = member.Type.GetElementType();
                    Console.WriteLine($"{elementType}");
                    // XXX: TODO
                }
                else
                {
                    var serialize = this.factory.GetSerializer(member.Type);
                    list.Add(serialize(value));
                }
            }
            return list.Aggregate((a, b) => a.Concat(b).ToArray());
        }
    }
}
