using System;
using System.Collections.Generic;
using System.Reflection;

namespace SnowStep.Serialization.Info
{
    internal abstract class Member
    {
        private readonly MemberInfo memberInfo;
        public string Name { get => this.memberInfo.Name; }
        public abstract Type Type { get; }
        public IEnumerable<T> GetCustomAttributes<T>() where T : Attribute => this.memberInfo.GetCustomAttributes<T>();
        public abstract object GetValue(object obj);
        public abstract void SetValue(object obj, object value);
        protected Member(MemberInfo memberInfo) => this.memberInfo = memberInfo;
    }
}
