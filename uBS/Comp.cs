using System;
using System.IO;

namespace uBS
{
	//этот код существует по приципу "работает, и не трогай".
	//Я нашёл его на китайском форуме, он был написан на C++. 
	//Вставил сюда и исправлял пока не заработает.
	static class Comp
	{
		static byte[] buffer;
		static int controlpos, datapos;
		static int readflag = 8;

		private static bool readbit()
		{
			if(readflag == 0)
			{
				controlpos = datapos++;
				readflag = 8;
			}
			readflag--;
			return (buffer[controlpos] & (1 << readflag)) != 0;
		}

		public static void uncompr(string file)
		{
			BinaryReader reader = new BinaryReader (new FileStream (file, FileMode.Open));
			int size = reader.ReadInt32 ();
			reader.ReadInt32 ();
			buffer = new byte[reader.BaseStream.Length - 8];
			byte[] outbuf = new byte[size];
			for(int i = 0; reader.BaseStream.Position < reader.BaseStream.Length; i++)
				buffer[i] = reader.ReadByte (); 
			reader.Close ();

			controlpos = 0;
			datapos = 1;
			readflag = 8;
			int epos = 0;
			while (epos < size) {
				if (readbit ()) //1 uncompressed
					outbuf [epos++] = buffer [datapos++];
				else {
					int len = 0;
					if(readbit()) {
						//01 len from 3 to 9, pos from -1 to -8192
						int t1 = (buffer[datapos] << 8) + buffer[datapos + 1];
						//1111 1111 1001 0         110
						datapos += 2;
						len = t1 & 7;
						t1 >>= 3;
						t1 |= -0x2000;

						//01 len from 1 to 256, pos from -1 to -8192
						if(len != 0)
							len += 2;
						else
							len = buffer[datapos++] + 1;
						for(int i = 0; i < len; ++i) {
							outbuf[epos] = outbuf[epos + t1];
							++epos;
						}
					} else {
						//00xx len from 2 to 6, pos from -1 to -256
						len = Convert.ToInt32(readbit()) * 2 + Convert.ToInt32(readbit()) + 2;
						int tpos = 256 - buffer[datapos++];
						for(int i = 0; i < len; ++i) {
							outbuf[epos] = outbuf[epos - tpos];
							++epos;
						}
					}
				}
			}
			File.WriteAllBytes (file, outbuf);
		}
	}
}


