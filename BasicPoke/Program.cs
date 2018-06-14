using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static BasicPoke.Line;

namespace BasicPoke
{
	public static class Program
	{
		private static bool clear;
		private static int clearAddress;
		private static int codeAddress;
		private static string inputFile;
		private static bool loadCode;
		private static int startAddress;
		private static int usrAddress;

		private static void CreateTap(BasicProgram program)
		{
			int programSize = program.Length - 2;

			const string Filename = "taptest";
			var header = new List<byte>();
			header.Add(0);                                  // flag
			header.Add(0);                                  // type
			header.AddRange(Encoding.ASCII.GetBytes(Filename.PadRight(10)));
			header.Add((byte)(programSize & 0xff));         // data size
			header.Add((byte)((programSize >> 8) & 0xff));  // data size
			header.Add(10);                                 // autostart number
			header.Add(0);                                  // autostart number
			header.Add((byte)(programSize & 0xff));         // variables
			header.Add((byte)((programSize >> 8) & 0xff));  // variables
			header.Add(BasicProgram.CalcChecksum(header));

			using (var tapFile = new BinaryWriter(File.Open("poke.tap", FileMode.Create)))
			{
				tapFile.Write((byte)19);
				tapFile.Write((byte)0);
				tapFile.Write(header.ToArray());

				tapFile.Write((byte)(program.Length & 0xff));
				tapFile.Write((byte)((program.Length >> 8) & 0xff));
				tapFile.Write(program.program.ToArray());
			}
		}

		private static int GenerateDataStatements(byte[] pokeCode, BasicProgram program, int lineNumber)
		{
			Line line;
			int lineIndex = 0;
			const int lineLength = 5;
			line = new Line(lineNumber);

			for (int i = 0; i < pokeCode.Length; i++)
			{
				if (lineIndex == 0)
				{
					line.AddToken(Token.DATA);
					line.AddNumber(pokeCode[i]);
					lineIndex++;
				}
				else
				{
					line.AddText(",");
					line.AddNumber(pokeCode[i]);

					if (++lineIndex == lineLength)
					{
						line.EndLine();
						program.AddLine(line);
						lineNumber += 10;
						lineIndex = 0;
						line = new Line(lineNumber);
					}
				}
			}
			if (lineIndex > 0)
			{
				line.EndLine();
				program.AddLine(line);
			}

			return lineNumber;
		}

		private static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Usage: BasicPoke inputfile.bin address [clear:address | loadcode:[address] | [usr:address]");
				return;
			}

			foreach (var arg in args)
			{
				if (!ProcessArg(arg))
				{
					return;
				}
			}

			var pokeCode = File.ReadAllBytes(inputFile);
			var program = new BasicProgram();
			int lineNumber = GenerateBoilerPlate(pokeCode, program);

			GenerateDataStatements(pokeCode, program, lineNumber);

			program.Compile();

			CreateTap(program);
		}

		private static int GenerateBoilerPlate(byte[] pokeCode, BasicProgram program)
		{
			int lineNumber = 10;
			var line = new Line(lineNumber);

			if (clear)
			{
				line = new Line(lineNumber);
				line.AddToken(Token.CLEAR);
				line.AddNumber(clearAddress);
				line.EndLine();
				program.AddLine(line);

				lineNumber += 10;
			}

			// FOR F=xxxx TO yyyy
			line = new Line(lineNumber);
			line.GenerateFor("F", startAddress, pokeCode.Length);
			line.EndLine();
			program.AddLine(line);

			lineNumber += 10;

			// READ A: POKE F, A: NEXT F
			line = new Line(lineNumber);
			line.AddToken(Token.READ);
			line.AddText("A:");
			line.AddToken(Token.POKE);
			line.AddText("F,A:");
			line.AddToken(Token.NEXT);
			line.AddText("F");
			line.EndLine();
			program.AddLine(line);

			lineNumber += 10;

			// LOAD ""CODE xxxx
			if (loadCode)
			{
				line = new Line(lineNumber);
				line.AddToken(Token.LOAD);
				line.AddText("", true);
				line.AddToken(Token.CODE);

				if (codeAddress > 0)
				{
					line.AddNumber(codeAddress);
				}

				line.EndLine();
				program.AddLine(line);

				lineNumber += 10;
			}

			// RANDOMIZE USR xxxx
			line = new Line(lineNumber);
			line.AddToken(Token.RANDOMIZE);
			line.AddToken(Token.USR);
			line.AddNumber(usrAddress == 0 ? startAddress : usrAddress);
			line.EndLine();
			program.AddLine(line);

			lineNumber += 10;

			return lineNumber;
		}

		private static bool ProcessArg(string arg)
		{
			if (arg == null)
			{
				return false;
			}

			if (arg.StartsWith("--"))
			{
				string argName = arg.Substring(2);
				string value = null;

				int valuePos = argName.IndexOf(":");
				if (valuePos > 0)
				{
					value = argName.Substring(valuePos + 1);
					argName = argName.Substring(0, valuePos);
				}

				switch (argName)
				{
					case "clear":
						clear = true;
						clearAddress = Int32.Parse(value);
						break;

					case "loadcode":
						loadCode = true;
						Int32.TryParse(value, out codeAddress);
						break;

					case "usr":
						usrAddress = Int32.Parse(value);
						break;
				}
			}
			else
			{
				if (inputFile == null)
				{
					inputFile = arg;
				}
				else if (startAddress == 0)
				{
					startAddress = Int32.Parse(arg);
				}
			}

			return true;
		}
	}
}