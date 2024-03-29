using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Modix.Services.Utilities;
using NUnit.Framework;
using Shouldly;

namespace Modix.Services.Test.UtilityTests
{
    [TestFixture]
    public class FormatCodeForEmbedTests
    {
        [Test]
        public void TestCSharp()
        {
            var source =
@"#nullable enable

var c = new C();
c.M(c = null, $"");
c.ToString();

class C
{
    public void M(object? o1, [InterpolatedStringHandlerArgument("")] CustomHandler c) => throw null!;
}

[InterpolatedStringHandler]
struct CustomHandler
{
    public CustomHandler(int literalLength, int formattedCount, [NotNull] C? o){}
}
";

            var expected =
@"```cs
#nullable enable
var c = new C();
c.M(c = null, $"");
c.ToString();
class C {
    public void M(object? o1, [InterpolatedStringHandlerAr...
}
[InterpolatedStringHandler]
struct CustomHandler {
    public CustomHandler(int literalLength, int formattedC...
}
```";

            Verify("cs", source, expected);
        }

        [Test]
        public void TestVisualBasic()
        {
            var source =
@"Imports System
Imports System.Threading.Tasks
Public Class C
    Public Sub M()
        Console.WriteLine(""something"")
        Console.WriteLine(""Hello this is a long line of test to test the truncation feature"")
        Console.WriteLine(""something"")
    End Sub


    Public Async Function SomeFunction(arg As Double) As Task
        Console.WriteLine(""something"")
        Await Task.Delay(arg)
        Console.WriteLine(""something"")
    End Function
End Class
";

            var expected =
@"```vb
Imports System
Imports System.Threading.Tasks
Public Class C
    Public Sub M()
        Console.WriteLine(""something"")
        Console.WriteLine(""Hello this is a long line of te...
        Console.WriteLine(""something"")
    End Sub
    Public Async Function SomeFunction(arg As Double) As T...
        Console.WriteLine(""something"")
' 4 more lines. Follow the link to view.
```";

            Verify("vb", source, expected);
        }

        [Test]
        public void TestFSharp()
        {
            var source =
@"open System

let printMessage name =
    printfn $""Hello there, {name}!""

let printNames names =
    for name in names do
        printMessage name

let names = [ ""Ana""; ""Felipe""; ""Emillia"" ]
printNames names
";

            var expected =
@"```fs
open System
let printMessage name =
    printfn $""Hello there, {name}!""
let printNames names =
    for name in names do
        printMessage name
let names = [ ""Ana""; ""Felipe""; ""Emillia"" ]
printNames names
```";

            Verify("fs", source, expected);
        }

        [Test]
        public void TestIL()
        {
            var source =
@".assembly A
{
}

.class public auto ansi abstract sealed beforefieldinit C
    extends System.Object
{
    .method public hidebysig static
        void M () cil managed
    {
        .maxstack 8

        ret
    }
}
";

            var expected =
@"```il
.assembly A {
}
.class public auto ansi abstract sealed beforefieldinit C
    extends System.Object {
    .method public hidebysig static
        void M () cil managed {
        .maxstack 8
        ret
    }
}
```";

            Verify("il", source, expected);
        }

        [Test]
        public void TestCSharpWithMaxLength()
        {
            var source =
@"
public class C {
    uint[] s_crcTable = new uint[256];
        
    public uint Crc32C(uint crc, uint data)
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(bytes), data);

        foreach (byte b in bytes)
        {
            int tableIndex = (int)((crc ^ b) & 0xFF);
            crc = s_crcTable[tableIndex] ^ (crc >> 8);
        }

        return crc;
    }
    
    public uint Crc32C2(uint crc, uint data)
    {
        if (!BitConverter.IsLittleEndian)
            data = BinaryPrimitives.ReverseEndianness(data);

        ref uint lut = ref MemoryMarshal.GetArrayDataReference(s_crcTable);
        
        for (int i = 0; i < sizeof(uint); i++)
        {
            crc = Unsafe.Add(ref lut, (nuint)(byte)(crc ^ (byte)data)) ^ (crc >> 8);
            data >>= 8;
        }

        return crc;
    }

    public uint Crc32C3(uint crc, uint data)
    {
        if (!BitConverter.IsLittleEndian)
            data = BinaryPrimitives.ReverseEndianness(data);

        ref uint lut = ref MemoryMarshal.GetArrayDataReference(s_crcTable);
        
        crc = Unsafe.Add(ref lut, (nuint)(byte)(crc ^ (byte)data)) ^ (crc >> 8);
        data >>= 8;
        crc = Unsafe.Add(ref lut, (nuint)(byte)(crc ^ (byte)data)) ^ (crc >> 8);
        data >>= 8;
        crc = Unsafe.Add(ref lut, (nuint)(byte)(crc ^ (byte)data)) ^ (crc >> 8);
        data >>= 8;
        crc = Unsafe.Add(ref lut, (nuint)(byte)(crc ^ data)) ^ (crc >> 8);

        return crc;
    }

    public uint Crc32C4(uint crc, uint data)
    {
        if (!BitConverter.IsLittleEndian)
            data = BinaryPrimitives.ReverseEndianness(data);

        ref uint lut = ref MemoryMarshal.GetArrayDataReference(s_crcTable);
        
        return Crc32CImpl(ref lut, crc, data);
    }

    public uint Crc32C4(uint crc, ulong data)
    {
        if (!BitConverter.IsLittleEndian)
            data = BinaryPrimitives.ReverseEndianness(data);

        ref uint lut = ref MemoryMarshal.GetArrayDataReference(s_crcTable);
        
        crc = Crc32CImpl(ref lut, crc, (uint)data);
        data >>= 32;
        crc = Crc32CImpl(ref lut, crc, (uint)data);
        
        return crc;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Crc32CImpl(ref uint lut, uint crc, uint data)
    {
        crc = Unsafe.Add(ref lut, (nuint)(byte)(crc ^ (byte)data)) ^ (crc >> 8);
        data >>= 8;
        crc = Unsafe.Add(ref lut, (nuint)(byte)(crc ^ (byte)data)) ^ (crc >> 8);
        data >>= 8;
        crc = Unsafe.Add(ref lut, (nuint)(byte)(crc ^ (byte)data)) ^ (crc >> 8);
        data >>= 8;
        crc = Unsafe.Add(ref lut, (nuint)(byte)(crc ^ data)) ^ (crc >> 8);
        
        return crc;
    }
}
";

            var expected =
@"```cs
public class C {
    uint[] s_crcTable = new uint[256];
    public uint Crc32C(uint crc, uint data) {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReferen...
// 63 more lines. Follow the link to view.
```";

            VerifyWithLength("cs", 293, source, expected);
        }

        private void Verify(string language, string source, string expected)
        {
            var actual = FormatUtilities.FormatCodeForEmbed(language, source, 2048);
            actual.ShouldBe(expected.Replace("\r", string.Empty));
        }

        private void VerifyWithLength(string language, int length, string source, string expected)
        {
            var actual = FormatUtilities.FormatCodeForEmbed(language, source, length);
            actual.ShouldBe(expected.Replace("\r", string.Empty));
        }
    }
}
