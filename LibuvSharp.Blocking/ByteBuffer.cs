using System;

namespace LibuvSharp.Blocking
{
	/// <summary>
	/// Byte buffers are used avoid expensive array copy operations when
	/// only parts of an array are valid.
	/// </summary>
	sealed class ByteBuffer
	{
		int position;
		int length;
		byte [] bytes;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Manos.IO.ByteBuffer"/> class with the exact
		/// size of the byte buffer.
		/// </summary>
		public ByteBuffer (byte [] bytes)
			: this (bytes, 0, bytes.Length)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Manos.IO.ByteBuffer"/> class.
		/// </summary>
		public ByteBuffer (byte [] bytes, int position, int length)
		{
			this.bytes = bytes;
			this.position = position;
			this.length = length;
		}
		
		/// <summary>
		/// Gets the byte at position <see cref="Position"/> in
		/// <see cref="Bytes"/>.
		/// </summary>
		public byte CurrentByte {
			get { return bytes [position]; }
		}
		
		/// <summary>
		/// Gets the byte array wrapped by this instance.
		/// </summary>
		public byte [] Bytes {
			get { return bytes; }
		}
		
		/// <summary>
		/// Gets the length of the segment of valid data within <see cref="Bytes"/>.
		/// This length may be <c>0</c>.
		/// </summary>
		public int Length {
			get { return length; }
		}
		
		/// <summary>
		/// Gets the position at which the segment of valid data within <see cref="Bytes"/>
		/// starts, i.e. the first valid byte.
		/// </summary>
		public int Position {
			get { return position; }
		}
		
		/// <summary>
		/// Reads and consumes one byte from the buffer. This method is mostly equivalent to
		/// copying <see cref="CurrentByte"/> and skipping one byte with <see cref="Skip"/>.
		/// </summary>
		/// <returns>
		/// The byte.
		/// </returns>
		/// <exception cref='InvalidOperationException'>
		/// Is thrown when no more bytes are left to read.
		/// </exception>
		public byte ReadByte ()
		{
			if (length == 0)
				throw new InvalidOperationException ("Read past end of ByteBuffer.");
			length--;
			return bytes [position++];
		}
		
		/// <summary>
		/// Consumes <see cref="bytes"/> bytes of valid data by advancing <see cref="Position"/>
		/// and decreasing <see cref="Length"/>.
		/// </summary>
		/// <param name='bytes'>
		/// Number of bytes to skip.
		/// </param>
		/// <exception cref='ArgumentException'>
		/// When <see cref="bytes"/> is less than <c>0</c>, or greater than the remaining amount
		/// of data in the valid segment.
		/// </exception>
		public void Skip (int bytes)
		{
			if (bytes < 0)
				throw new ArgumentException ("Can't move backwards in buffer.");
			if (bytes > length)
				throw new ArgumentException ("Can't move past end of buffer.");
			position += bytes;
			length -= bytes;
		}
	}
}

