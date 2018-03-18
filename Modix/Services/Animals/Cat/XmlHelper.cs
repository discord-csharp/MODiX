namespace Modix.Services.Cat
{
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    internal static class XmlHelper
    {
        public static Stream ToStream(this string @this)
        {
            var stream = new MemoryStream();
            var write = new StreamWriter(stream);

            write.Write(@this);
            write.Flush();
            stream.Position = 0;
            return stream;
        }

        public static T DeserializeXML<T>(this string @this) where T : class
        {
            var reader = XmlReader.Create(@this.Trim().ToStream(),
                new XmlReaderSettings {ConformanceLevel = ConformanceLevel.Document});
            return new XmlSerializer(typeof(T)).Deserialize(reader) as T;
        }
    }
}
