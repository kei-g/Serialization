using SnowStep.Serialization.Info;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SnowStep.Serialization
{
    internal class BinaryMembers : IEnumerable<Info.Member>
    {
        public static readonly BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

        private static Info.Member Cast(MemberInfo memberInfo)
        {
            return memberInfo is FieldInfo ? (Info.Member)new Info.Field(memberInfo as FieldInfo) : new Info.Property(memberInfo as PropertyInfo);
        }

        private readonly Info.Member[] members;

        private BinaryMembers(IEnumerable<Info.Member> members) => this.members = members.ToArray();

        private BinaryMembers(IEnumerable<MemberInfo> members)
            : this(members.Where(m => m is FieldInfo || m is PropertyInfo).Select(Cast))
        {
        }

        public BinaryMembers(Type type)
            : this(type.GetMembers(BinaryMembers.BindingFlags))
        {
        }

        #region IEnumerable<Info.Member>

        public IEnumerator<Member> GetEnumerator()
        {
            return ((IEnumerable<Member>)this.members).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Member>)this.members).GetEnumerator();
        }

        #endregion
    }
}
