using System;
using System.Reflection;

namespace SnowStep.Serialization.Info
{
    internal class Property : Member
    {
        private readonly PropertyInfo property;

        public Property(PropertyInfo property) : base(property) => this.property = property;

        public override Type Type => this.property.PropertyType;

        public override object GetValue(object obj) => this.property.GetValue(obj);

        public override void SetValue(object obj, object value) => this.property.SetValue(obj, value);
    }
}
