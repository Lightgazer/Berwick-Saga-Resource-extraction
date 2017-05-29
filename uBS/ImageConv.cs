using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace uBS
{
	public class ImageConv
	{
		public static void TbToPNG (string image, string pal, string info, string output)
		{
			BinaryReader reader = new BinaryReader ((Stream)new FileStream (info, FileMode.Open));
			int width = reader.ReadInt16();
			int height = reader.ReadInt16();
			reader.Close();
			PaletteS palette;
			if(new System.IO.FileInfo(pal).Length == 1024)
				palette = new PaletteRGB8S(pal);
			else 
				palette = new PaletteBGRS(pal);
			reader = new BinaryReader ((Stream)new FileStream (image, FileMode.Open));
			Bitmap bmp = new Bitmap (width, height);
			for (int i = 0; i < bmp.Height; i++)
				for (int j = 0; j < bmp.Width; j++) {
					byte index = reader.ReadByte ();
					bmp.SetPixel (j, i, palette.GetColor(index));
				}
			bmp.Save (output, ImageFormat.Png); 
			reader.Close ();
		}

		public static void TTXToPNG (string input, string output)
		{
			BinaryReader reader = new BinaryReader ((Stream)new FileStream (input, FileMode.Open, FileAccess.Read, FileShare.Read));
			if (reader.ReadInt32 () != 811095124) //TTX0
				return;
			reader.ReadInt32 (); //zeros
			int bpp = reader.ReadInt32 (); 
			reader.ReadInt32 (); //image size
			int width = reader.ReadInt32 ();
			int height = reader.ReadInt32 ();
			int palette_type = reader.ReadInt32 (); //2 - BGR555, 0 - RGBA
			int palette_size = reader.ReadInt32 ();
			reader.BaseStream.Position = 32 + palette_size;

			Bitmap bmp = new Bitmap (width, height);
			if (palette_type == 2) {  //BGR555
				PaletteBGR palette = new PaletteBGR(input);
				for (int i = 0; i < bmp.Height; i++)
					for (int j = 0; j < bmp.Width; j++) {
						byte index = reader.ReadByte ();
						bmp.SetPixel (j, i, palette.GetColor(index));
					}
			} else {  //RGBA
				if (bpp == 0x14) {    //4bpp
					PaletteRGBA4 palette = new PaletteRGBA4(input);
					for (int i = 0; i < bmp.Height; i++)
						for (int j = 0; j < bmp.Width; j++) {
							byte pixel = reader.ReadByte ();
							byte index = (byte)(pixel & 0x0F);
							bmp.SetPixel (j, i, palette.GetColor(index));
							j++;
							index = (byte)(pixel >> 4);
							bmp.SetPixel (j, i, palette.GetColor(index));
						}
				} else if (bpp == 0x13) { //8bpp
					PaletteRGBA8 palette = new PaletteRGBA8(input);
					for (int i = 0; i < bmp.Height; i++)
						for (int j = 0; j < bmp.Width; j++) {
							byte index = reader.ReadByte ();
							bmp.SetPixel (j, i, palette.GetColor(index));
						}
				}
			}
			bmp.Save (output, ImageFormat.Png); 
			reader.Close ();
		}
	}
}


