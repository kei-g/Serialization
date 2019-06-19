using System;
using SnowStep.Serialization;
using SnowStep.Serialization.IO;

namespace ConsoleApp
{
    class Foo
    {
        [BinarySerializableText(Order = 0, EncodingName = "iso-2022-jp")]
        public string Name { get; set; } = "hello";

        [BinarySerializable(Order = 1)]
        public int[] Values = new int[] { 1, 2, 3 };
    }

    class Program
    {
        [BinarySerializable(Order = 1)]
        private Foo Foo { get; set; } = new Foo();

        [BinarySerializable(Order = 2)]
        private int Value { get; set; } = 551;

        static void Main(string[] args)
        {
            var prog = new Program { Foo = new Foo { Name = "あいうえお" }, Value = 114514 };
            var f = new BinarySerializerFactory();
            var s = f.GetInstance<Program>();
            var buf = s.Serialize(prog);
            Console.WriteLine($"{buf.LongLength} bytes");
            var source = new BinarySource(buf);
            var p = s.Deserialize(source);
            Console.WriteLine($"{p.Foo.Name}, {p.Value}");
            Console.ReadLine();
        }
    }
}
