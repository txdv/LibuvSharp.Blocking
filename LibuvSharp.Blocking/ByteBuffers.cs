using System;
using System.Collections.Generic;
using System.Linq;
using LibuvSharp;

namespace LibuvSharp.Blocking
{
	class ByteBuffers
	{
		List<ByteBuffer> buffers = new List<ByteBuffer>();
		
		public ByteBuffers()
		{
		}
		
		public void AddCopy(ByteBuffer buffer)
		{
			byte[] bytes = new byte[buffer.Length];
			Buffer.BlockCopy(buffer.Bytes, buffer.Position, bytes, 0, bytes.Length);
			Add(new ByteBuffer(bytes, 0, bytes.Length));
		}
		
		public void Add(ByteBuffer buffer)
		{
			buffers.Add(buffer);
		}
		
		List<ByteBuffer> Clone()
		{
			List<ByteBuffer> r = new List<ByteBuffer>();
			foreach (var buffer in buffers) {
				r.Add(buffer);
			}
			return r;
		}
		
		public void Skip(int restLength)
		{
			foreach (var buffer in Clone()) {
				int r = restLength - buffer.Length;
				if (r >= 0) {
					buffer.Skip(buffer.Length);
					buffers.Remove(buffer);
					restLength = r;
				} else {
					// it is the last buffer we need to skip
					// break afterwards
					buffer.Skip(restLength);
					return;
				}
			}
		}
		
		public bool HasLength(int length)
		{
			foreach (var buffer in buffers) {
				if (length < buffer.Length) {
					return true;
				}
				length -= buffer.Length;
			}
			return false;
		}
		
		public int Length {
			get {
				int length = 0;
				foreach (var buffer in buffers) {
					length += buffer.Length;
				}
				return length;
			}
		}
		
		public byte this[int index] {
			get {
				int position = index;
				foreach (var buffer in buffers) {
					if (position < buffer.Length) {
						return buffer.Bytes[buffer.Position + position];
					} else {
						position -= buffer.Length; 
					}
				}
				throw new Exception();
			}
		}
		
		public void CopyTo(byte[] destination, int length)
		{
			int startPos = 0;
			foreach (var buffer in buffers) {
				int rest = length - buffer.Length;
				if (rest <= 0) {
					Buffer.BlockCopy(buffer.Bytes, buffer.Position, destination, startPos, length);
					break;
				} else {
					Buffer.BlockCopy(buffer.Bytes, buffer.Position, destination, startPos, buffer.Length);
					startPos += buffer.Length;
					length = rest;
				}
			}
		}
		
		public int FirstByte(byte val)
		{
			int pos = 0;
			foreach (var buffer in buffers) {
				for (int i = 0; i < buffer.Length; i++) {
					if (buffer.Bytes[i + buffer.Position] == val) {
						return pos;
					} else {
						pos++;
					}
				}
			}
			return -1;
		}
		
		public byte CurrentByte {
			get {
				var buffer = buffers.First();
				return buffer.CurrentByte;
			}
		}
		
		public bool ReadLong(int size, out long result)
		{
			if (size > sizeof(long) || !HasLength(size)) {
				result = 0;
				return false;
			}
			
			result = this[size - 1];

			for (int i = size - 2;i >= 0; i--) {
				result <<= 8;
				result |= this[i];
			}
			
			return true;
		}
	}
}
