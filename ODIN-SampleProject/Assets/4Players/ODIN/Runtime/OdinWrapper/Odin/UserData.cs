using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OdinNative.Odin
{
    public interface IUserData
    {
        byte[] ToBytes();
    }

    /// <summary>
    /// Odin UserData helper for marshal byte arrays
    /// </summary>
    public class UserData : IUserData
    {
        public Encoding Encoding { get; set; }
        public byte[] Buffer { get => _buffer; set => _buffer = value; }
        private byte[] _buffer;

        public static explicit operator UserData(string text) => new UserData(text);
        public static implicit operator string(UserData userdata) => userdata?.ToString() ?? string.Empty;
        public static explicit operator UserData(byte[] data) => new UserData(data);
        public static implicit operator byte[](UserData userdata) => userdata?.ToBytes() ?? new byte[0];

        internal UserData() : this(new byte[0]) { }
        public UserData(string text) : this(text, Encoding.UTF8) { }
        public UserData(string text, Encoding encoding) : this(encoding.GetBytes(text), encoding) { }
        public UserData(byte[] data) : this(data, null) { }
        public UserData(byte[] data, Encoding encoding)
        {
            Encoding = encoding ?? Encoding.UTF8;
            Buffer = data;
        }

        public virtual void CopyFrom(IntPtr ptr, ulong size)
        {
            Buffer = new byte[size];
            if (ptr == IntPtr.Zero) return;

            Marshal.Copy(ptr, Buffer, 0, Buffer.Length);
        }

        public virtual bool IsEmpty()
        {
            return Buffer.Length == 0 || string.IsNullOrEmpty(this.ToString());
        }

        public virtual bool Contains(string value)
        {
            return this.ToString()
                .Contains(value);
        }

        public virtual bool Contains(byte value)
        {
            return this.Buffer
                .Contains(value);
        }

        public virtual IEnumerable<int> PatternAt(byte[] pattern)
        {
            for (int i = 0; i < Buffer.Length; i++)
                if (Buffer.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                    yield return i;
        }

        public virtual UserData Clone()
        {
            return new UserData((byte[])Buffer.Clone()) { Encoding = this.Encoding };
        }

        public virtual byte[] ToBytes()
        {
            return Buffer.ToArray();
        }

        public override string ToString()
        {
            return Encoding.GetString(Buffer);
        }
    }
}
