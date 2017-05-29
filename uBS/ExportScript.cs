using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace uBS
{
	public partial class MainClass
	{
		protected static List<string> DumpText (FileInfo file, char[] code_page)
		{
			List<string> script = new List<string> ();  
			BinaryReader reader = new BinaryReader ((Stream)new FileStream (file.FullName, FileMode.Open));
			string str = "";
			string strsq = "";

			while (reader.BaseStream.Position < reader.BaseStream.Length) {
				ushort num1 = reader.ReadUInt16 ();
				if ((int)num1 < 32768) {//ordinary char from "font" 
					if (code_page.Length > num1)
						str += code_page [num1].ToString ();
					else
						str += "<" + num1.ToString () + ">";
				} else if ((int)num1 == 38147)
					str = str + "<[" + (reader.ReadUInt16 ()).ToString () + "," + (reader.ReadUInt16 ()).ToString () + "," + (reader.ReadUInt16 ()).ToString () + "]";
				else if ((int)num1 == 32769)
					str = str + "●" + "[" + (reader.ReadUInt16 ()).ToString () + "]";
				else if ((int)num1 == 38144)
					str += ">";
				else if ((int)num1 == 33536)
					str += "\r\n";
				else if ((int)num1 == 44544)
					str += "▲";
				else if ((int)num1 == 33024) { // window end
					script.Add (str);
					script.Add ("-----------------------");
					str = "";
				} else if ((int)num1 == 35074) {
					str += "■";
					reader.BaseStream.Position -= 2;
					strsq += "[";
					for (int i = 0; i < 6; i++)
						strsq += reader.ReadByte () + ".";
					strsq += "]";
				} else if ((int)num1 == 32024 || (int)num1 == 39168 || (int)num1 == 39424 || (int)num1 == 33280 || (int)num1 == 39936 || (int)num1 == 47104) { //some strange chars
					str += "■";
					reader.BaseStream.Position -= 2;
					strsq += "[" + reader.ReadByte () + "." + reader.ReadByte () + ".]";
				} else { 
					str += "■";
					reader.BaseStream.Position -= 2;
					strsq += "[";
					for (int i = 0; i < 4; i++)
						if (reader.BaseStream.Position < reader.BaseStream.Length) {
							strsq += reader.ReadByte () + ".";
						}
					strsq += "]";
				}
			}
			if (str != "")
				script.Add (str);
			if (strsq != "") {
				StreamWriter sqwriter = new StreamWriter (file.Directory.ToString () + System.IO.Path.DirectorySeparatorChar + "Squares.txt", true);
				sqwriter.WriteLine ("<" + file.Name + ">");
				sqwriter.WriteLine (strsq);
				sqwriter.WriteLine ();
				sqwriter.Flush ();
				sqwriter.Close ();
			}
			reader.Close ();
			return script;
		}

		protected static void ScriptMaker (string script_dir, string out_txt)
		{
			StreamWriter writer = new StreamWriter ((Stream)new FileStream (out_txt, FileMode.Create));
			DirectoryInfo info = new DirectoryInfo (script_dir);
			FileInfo[] files = info.GetFiles ().OrderBy (p => p.CreationTime).ToArray ();
			string[] num = script_dir.Split (System.IO.Path.DirectorySeparatorChar);
			char[] code_page;
			if (num.Length == 3)
				code_page = GetCodePage (Int32.Parse (num [1]), num [2]);
			else
				code_page = GetCodePage (Int32.Parse (num [1]));
			int index = 0;
			foreach (FileInfo file in files) {
				index++;
				if (file.Extension == "") {
					List<string> script_list = DumpText (file, code_page);
					string line = "#### File " + index + ": " + file.Name + " ####";
					if (script_list.Count > 0) {
						if (script_list.Count == 1 && script_list [0] == "\r\n")
							continue;
						writer.WriteLine (line);
						for (int index2 = 0; index2 < script_list.Count; ++index2) {
							writer.Write (script_list [index2]);
							writer.WriteLine ();
						}
						writer.WriteLine ("###### End ######");
						writer.WriteLine ();
					}
				}
			}
			writer.Flush ();
			writer.Close ();
		}
	}
}