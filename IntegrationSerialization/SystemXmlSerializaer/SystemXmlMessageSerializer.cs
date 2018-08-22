namespace NServiceBus.Serializers.SystemXml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;
    using Serialization;

    class SystemXmlMessageSerializer : IMessageSerializer
    {
        public void Serialize(object message, Stream stream)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings{Encoding = Encoding.Default}))
            {
                var serializer = XmlSerializerCache.GetSerializer(message.GetType());
                serializer.Serialize(writer, message);
            }
        }

        public object[] Deserialize(Stream stream, IList<Type> messageTypes)
        {
            if (stream == null)
            {
                return null;
            }

            var messages = new List<object>();
            using (var streamReader = new StreamReader(stream))
            {
                using (var reader = XmlReader.Create(streamReader, new XmlReaderSettings{CloseInput = false}))
                {
                    foreach (var messageType in messageTypes)
                    {
                        var serializer = XmlSerializerCache.GetSerializer(messageType);

                        if (serializer.CanDeserialize(reader))
                        {
                            messages.Add(serializer.Deserialize(reader));
                        }
                    }
                }
            }

            return messages.ToArray();
        }

        public string ContentType => SystemXmlSerializer.ContentType;
    }
}