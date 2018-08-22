namespace NServiceBus.Serializers.SystemXml
{
    using System;
    using System.Collections.Concurrent;
    using System.Xml.Serialization;

    static class XmlSerializerCache
    {
        static readonly ConcurrentDictionary<Type, XmlSerializer> xmlSerializers =
            new ConcurrentDictionary<Type, XmlSerializer>();

        public static XmlSerializer GetSerializer(Type type)
        {
            // Be careful about how many XmlSerializers we construct;
            // statically store 1 per type that we encounter.
            if (!xmlSerializers.ContainsKey(type))
            {
                xmlSerializers.GetOrAdd(type, new XmlSerializer(type));
            }

            return xmlSerializers[type];
        }
    }
}
