using System;
using System.IO;
using System.Drawing;

namespace uBS
{
	public class PaletteBGR //BGR555
	{
		const int palette_size = 512;
		ushort[] paletteBGR = new ushort[palette_size / 2];

		public PaletteBGR (string file)
		{
			BinaryReader reader = new BinaryReader ((Stream)new FileStream (file, FileMode.Open, FileAccess.Read, FileShare.Read));
			reader.BaseStream.Position = 32;
			ushort[] raw_paletteBGR = new ushort[palette_size / 2];
			for (int i = 0; i < palette_size / 2; i++)
				raw_paletteBGR [i] = reader.ReadUInt16 ();
			reader.Close ();

			int[] palette_order = new int[] {0, 2, 1, 3, 4, 6, 5, 7, 8, 10, 9, 11, 12, 14, 13, 15, 16, 18, 17, 
				19, 20, 22, 21, 23, 24, 26, 25, 27, 28, 30, 29, 31
			}; // about pallete rearrange see: http://izumobridge.blog.fc2.com/blog-entry-103.html
			paletteBGR = new ushort[palette_size / 2];
			for (int i = 0; i < palette_size / 2 / 8; i++)
				for (int j = 0; j < 8; j++)
					paletteBGR [i * 8 + j] = raw_paletteBGR [8 * palette_order [i] + j];
		}

		private Color BGRToColor(ushort color) //from BGR555
		{
			int a = color & 0x8000;
			int b = (color & 0x7C00) >> 10;
			int g = (color & 0x03E0) >> 5;
			int r = color & 0x1F;
			b = b << 3;
			g = g << 3;
			r = r << 3;	
			if (a == 0x8000)
				a = 0xFF;
			else
				a = 0xE8;

			return Color.FromArgb (a, r, g, b);
		}

		public byte GetIndex (Color color)
		{
			byte index = 0;
			int maxdev = 1024;
			if (color.A < 0xFA)
				for (int i = 0; i < palette_size / 2; i++)
					if (BGRToColor (paletteBGR [i]).A == 0xE8)
						return (byte)i;
			for (int i = 0; i < palette_size / 2; i++) {
				Color palette = BGRToColor (paletteBGR [i]);
				int dev = Math.Abs (palette.B - color.B) + Math.Abs (palette.G - color.G) + Math.Abs (palette.R - color.R);
				if (maxdev > dev) {
					index = (byte)i;
					maxdev = dev;
				}
				if (maxdev == 0)
					break;
			}
			return index;
		}

		public Color GetColor(byte index)
		{
			return BGRToColor (paletteBGR [index]);
		}
	}

	abstract public class PaletteS
	{
		abstract public Color GetColor (byte index);
	}

	public class PaletteBGRS : PaletteS //BGR555 in Separate file
	{
		const int palette_size = 512;
		ushort[] paletteBGR = new ushort[palette_size / 2];

		public PaletteBGRS (string file)
		{
			BinaryReader reader = new BinaryReader ((Stream)new FileStream (file, FileMode.Open, FileAccess.Read, FileShare.Read));
			ushort[] raw_paletteBGR = new ushort[palette_size / 2];
			for (int i = 0; i < palette_size / 2; i++)
				raw_paletteBGR [i] = reader.ReadUInt16 ();
			reader.Close ();

			int[] palette_order = new int[] {0, 2, 1, 3, 4, 6, 5, 7, 8, 10, 9, 11, 12, 14, 13, 15, 16, 18, 17, 
				19, 20, 22, 21, 23, 24, 26, 25, 27, 28, 30, 29, 31
			}; // about pallete rearrange see: http://izumobridge.blog.fc2.com/blog-entry-103.html
			paletteBGR = new ushort[palette_size / 2];
			for (int i = 0; i < palette_size / 2 / 8; i++)
				for (int j = 0; j < 8; j++)
					paletteBGR [i * 8 + j] = raw_paletteBGR [8 * palette_order [i] + j];
		}

		private Color BGRToColor(ushort color) //from BGR555
		{
			int a = color & 0x8000;
			int b = (color & 0x7C00) >> 10;
			int g = (color & 0x03E0) >> 5;
			int r = color & 0x1F;
			b = b << 3;
			g = g << 3;
			r = r << 3;	
			if (a == 0x8000)
				a = 0xFF;
			else
				a = 0x53;

			return Color.FromArgb (a, r, g, b);
		}

		public byte GetIndex (Color color)
		{
			byte index = 0;
			int maxdev = 1024;
			if (color.A < 0xFA)
				for (int i = 0; i < palette_size / 2; i++)
					if (BGRToColor (paletteBGR [i]).A == 0xE8)
						return (byte)i;
			for (int i = 0; i < palette_size / 2; i++) {
				Color palette = BGRToColor (paletteBGR [i]);
				int dev = Math.Abs (palette.B - color.B) + Math.Abs (palette.G - color.G) + Math.Abs (palette.R - color.R);
				if (maxdev > dev) {
					index = (byte)i;
					maxdev = dev;
				}
				if (maxdev == 0)
					break;
			}
			return index;
		}

		public override Color GetColor(byte index)
		{
			return BGRToColor (paletteBGR [index]);
		}
	}

	public class PaletteRGB8S : PaletteS
	{
		const int palette_size = 1024;

		int[] paletteA = new int[palette_size / 4];
		int[] paletteR = new int[palette_size / 4];
		int[] paletteG = new int[palette_size / 4];
		int[] paletteB = new int[palette_size / 4];

		public PaletteRGB8S (string file)
		{
			int alpha_mult = 2;
			BinaryReader reader = new BinaryReader ((Stream)new FileStream (file, FileMode.Open, FileAccess.Read, FileShare.Read));
			int[] raw_paletteA = new int[palette_size / 4];
			int[] raw_paletteR = new int[palette_size / 4]; 
			int[] raw_paletteG = new int[palette_size / 4]; 
			int[] raw_paletteB = new int[palette_size / 4]; 
			for (int i = 0; i < palette_size / 4; i++) {
				raw_paletteR [i] = reader.ReadByte ();
				raw_paletteG [i] = reader.ReadByte ();
				raw_paletteB [i] = reader.ReadByte ();
				raw_paletteA [i] = reader.ReadByte ();
				if (raw_paletteA [i] > 127)
					alpha_mult = 1;
			}
			for (int i = 0; i < palette_size / 4; i++) 
				raw_paletteA [i] *= alpha_mult;
			reader.Close ();

			int[] palette_order = new int[] {0, 2, 1, 3, 4, 6, 5, 7, 8, 10, 9, 11, 12, 14, 13, 15, 16, 18, 17, 
				19, 20, 22, 21, 23, 24, 26, 25, 27, 28, 30, 29, 31
			}; // about pallete rearrange see: http://izumobridge.blog.fc2.com/blog-entry-103.html
			for (int i = 0; i < palette_size / 4 / 8; i++) {
				for (int j = 0; j < 8; j++) {
					paletteA [i * 8 + j] = raw_paletteA [8 * palette_order [i] + j];
					paletteR [i * 8 + j] = raw_paletteR [8 * palette_order [i] + j];
					paletteG [i * 8 + j] = raw_paletteG [8 * palette_order [i] + j];
					paletteB [i * 8 + j] = raw_paletteB [8 * palette_order [i] + j];
				}
			}
		}

		public byte GetIndex (Color color)
		{
			byte index = 0;
			int maxdev = 1024;

			for (int i = 0; i < palette_size / 4; i++) {
				int dev = Math.Abs (paletteA [i] - color.A) + Math.Abs (paletteR [i] - color.R) + Math.Abs (paletteG [i] - color.G) + Math.Abs (paletteG [i] - color.B);
				if (paletteA [i] == 0 & color.A == 0) {
					index = (byte)i;
					break;
				}
				if (maxdev > dev) {
					index = (byte)i;
					maxdev = dev;
				}
				if (maxdev == 0)
					break;
			}
			return index;
		}

		public override Color GetColor (byte index)
		{
			return Color.FromArgb (paletteA[index], paletteR[index], paletteG[index], paletteB[index]);
		}
	}

	public class PaletteRGBA8
	{
		const int palette_size = 1024;

		int[] paletteA = new int[palette_size / 4];
		int[] paletteR = new int[palette_size / 4];
		int[] paletteG = new int[palette_size / 4];
		int[] paletteB = new int[palette_size / 4];

		public PaletteRGBA8 (string file)
		{
			int alpha_mult = 2;
			BinaryReader reader = new BinaryReader ((Stream)new FileStream (file, FileMode.Open, FileAccess.Read, FileShare.Read));
			reader.BaseStream.Position = 32;
			int[] raw_paletteA = new int[palette_size / 4];
			int[] raw_paletteR = new int[palette_size / 4]; 
			int[] raw_paletteG = new int[palette_size / 4]; 
			int[] raw_paletteB = new int[palette_size / 4]; 
			for (int i = 0; i < palette_size / 4; i++) {
				raw_paletteR [i] = reader.ReadByte ();
				raw_paletteG [i] = reader.ReadByte ();
				raw_paletteB [i] = reader.ReadByte ();
				raw_paletteA [i] = reader.ReadByte ();
				if (raw_paletteA [i] > 127)
					alpha_mult = 1;
			}
			for (int i = 0; i < palette_size / 4; i++) 
				raw_paletteA [i] *= alpha_mult;
			reader.Close ();

			int[] palette_order = new int[] {0, 2, 1, 3, 4, 6, 5, 7, 8, 10, 9, 11, 12, 14, 13, 15, 16, 18, 17, 
				19, 20, 22, 21, 23, 24, 26, 25, 27, 28, 30, 29, 31
			}; // about pallete rearrange see: http://izumobridge.blog.fc2.com/blog-entry-103.html
			for (int i = 0; i < palette_size / 4 / 8; i++) {
				for (int j = 0; j < 8; j++) {
					paletteA [i * 8 + j] = raw_paletteA [8 * palette_order [i] + j];
					paletteR [i * 8 + j] = raw_paletteR [8 * palette_order [i] + j];
					paletteG [i * 8 + j] = raw_paletteG [8 * palette_order [i] + j];
					paletteB [i * 8 + j] = raw_paletteB [8 * palette_order [i] + j];
				}
			}
		}

		public byte GetIndex (Color color)
		{
			byte index = 0;
			int maxdev = 1024;

			for (int i = 0; i < palette_size / 4; i++) {
				int dev = Math.Abs (paletteA [i] - color.A) + Math.Abs (paletteR [i] - color.R) + Math.Abs (paletteG [i] - color.G) + Math.Abs (paletteG [i] - color.B);
				if (paletteA [i] == 0 & color.A == 0) {
					index = (byte)i;
					break;
				}
				if (maxdev > dev) {
					index = (byte)i;
					maxdev = dev;
				}
				if (maxdev == 0)
					break;
			}
			return index;
		}

		public Color GetColor (byte index)
		{
			return Color.FromArgb (paletteA[index], paletteR[index], paletteG[index], paletteB[index]);
		}
	}

	public class PaletteRGBA4
	{
		int[] paletteA = new int[16];
		int[] paletteR = new int[16];
		int[] paletteG = new int[16];
		int[] paletteB = new int[16];

		public PaletteRGBA4 (string file)
		{
			int alpha_mult = 2;
			BinaryReader reader = new BinaryReader ((Stream)new FileStream (file, FileMode.Open, FileAccess.Read, FileShare.Read));
			reader.BaseStream.Position = 32;
			for (int i = 0; i < 16; i++) {
				paletteR [i] = reader.ReadByte ();
				paletteG [i] = reader.ReadByte ();
				paletteB [i] = reader.ReadByte ();
				paletteA [i] = reader.ReadByte ();
				if (paletteA [i] > 127)
					alpha_mult = 1;
			}
			for (int i = 0; i < 16; i++) 
				paletteA [i] *= alpha_mult;
			reader.Close ();
		}

		public byte GetIndex (Color color)
		{
			byte index = 0;
			int maxdev = 1024;

			for (int i = 0; i < 16; i++) {
				int dev = Math.Abs (paletteA [i] - color.A) + Math.Abs (paletteR [i] - color.R) + Math.Abs (paletteG [i] - color.G) + Math.Abs (paletteG [i] - color.B);
				if (paletteA [i] == 0 & color.A == 0) {
					index = (byte)i;
					break;
				}
				if (maxdev > dev) {
					index = (byte)i;
					maxdev = dev;
				}
				if (maxdev == 0)
					break;
			}
			return index;
		}

		public Color GetColor (byte index)
		{
			return Color.FromArgb (paletteA[index], paletteR[index], paletteG[index], paletteB[index]);
		}
	}
}

