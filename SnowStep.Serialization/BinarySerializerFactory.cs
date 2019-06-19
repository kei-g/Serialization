using SnowStep.Serialization.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SnowStep.Serialization
{
    public class BinarySerializerFactory
    {
        private readonly Dictionary<Type, object> serializers = new Dictionary<Type, object>();

        internal Func<BinarySource, object> GetDeserializer(Type type)
        {
            var instance = GetInstance(type, out var serializerType);
            var method = serializerType.GetMethod("Deserialize");
            return source => method.Invoke(instance, new object[] { source });
        }

        internal Func<object, byte[]> GetSerializer(Type type)
        {
            var instance = GetInstance(type, out var serializerType);
            var method = serializerType.GetMethod("Serialize");
            return obj => (byte[])method.Invoke(instance, new object[] { obj });
        }

        private object GetInstance(Type type, out Type serializerType)
        {
            serializerType = typeof(BinarySerializer<>).MakeGenericType(type);
            if (this.serializers.TryGetValue(type, out var s))
                return s;
            var ctor = serializerType.GetConstructors(BinaryMembers.BindingFlags).SingleOrDefault();
            var instance = ctor.Invoke(new object[] { this });
            this.serializers.Add(type, instance);
            return instance;
        }

        public BinarySerializer<T> GetInstance<T>()
        {
            return (BinarySerializer<T>)GetInstance(typeof(T), out var _);
        }
    }
}
