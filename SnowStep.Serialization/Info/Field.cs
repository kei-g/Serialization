using System;
using System.Reflection;

namespace SnowStep.Serialization.Info
{
    internal class Field : Member
    {
        private readonly FieldInfo field;

        public Field(FieldInfo field) : base(field) => this.field = field;

        public override Type Type => this.field.FieldType;

        public override object GetValue(object obj) => this.field.GetValue(obj);

        public override void SetValue(object obj, object value) => this.field.SetValue(obj, value);
    }
}
