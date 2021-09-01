using Java.IO;
using Java.Lang;

namespace GoogleVisionBarCodeScanner
{
    public class SerializationContainer<T> : Object, ISerializable
    {
        public SerializationContainer(T content)
        {
            Content = content;
        }

        public T Content { get; }
    }
}