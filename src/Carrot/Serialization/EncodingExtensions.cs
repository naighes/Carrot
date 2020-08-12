using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Carrot.Serialization
{
    public static class EncodingExtensions
    {
        public static string GetString(this Encoding encoding, ReadOnlyMemory<byte> memory)
        {
            var arraySegment = GetArray(memory);
            return encoding.GetString(arraySegment.Array ?? throw new InvalidOperationException(), 
                                 arraySegment.Offset, arraySegment.Count);
        }

        private static ArraySegment<byte> GetArray(ReadOnlyMemory<byte> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
                throw new InvalidOperationException("Buffer backed by array was expected");
            return result;
        }
    }
}