using static BasicPoke.BasicLine;

namespace BasicPoke;

public static class Program
{
	private static bool clear;
	private static bool loadCode;
	private static bool rem;
	private static int clearAddress;
	private static int codeAddress;
	private static int startAddress;
	private static int usrAddress;
	private static string? inputFile;

	private static void Main(string[] args)
	{
		if (args.Length < 2)
		{
			Console.WriteLine("Usage: BasicPoke input.bin address [--clear:address] [--loadcode[:address]] [--usr:address] [--rem]");
			return;
		}

		ProcessArgs(args);

		var pokeCode = File.ReadAllBytes(inputFile!);
		var program = new BasicProgram();

		GenerateBasic(pokeCode, program);
		program.Compile();
		CreateTap(program);
	}

	public static byte CalcChecksum(IEnumerable<byte> list)
	{
		byte result = 0;

		foreach (var element in list)
		{
			result ^= element;
		}

		return result;
	}

	private static void CreateTap(BasicProgram program)
	{
		const string Filename = "poke";

		int programSize = program.Length - 2;

		List<byte> header = new(17);
		header.Add(0);							// type
		header.AddRange(Filename.PadRight(10).GetAsciiBytes());
		header.Add(programSize.GetLowByte());	// data size
		header.Add(programSize.GetHighByte());  // data size
		header.Add(10);                         // autostart number
		header.Add(0);                          // autostart number
		header.Add(programSize.GetLowByte());	// variables
		header.Add(programSize.GetHighByte());	// variables

		using var tapFile = new BinaryWriter(File.Open("poke.tap", FileMode.Create));
		tapFile.Write((byte)19);				// sizeof header + flag + checksum
		tapFile.Write((byte)0);
		tapFile.Write((byte)0);					// flag 0 = header
		tapFile.Write(header.ToArray());
		tapFile.Write(CalcChecksum(header));
		tapFile.Write(program.Length.GetLowByte());
		tapFile.Write(program.Length.GetHighByte());
		tapFile.Write(program.Basic.ToArray());
	}

	private static int GenerateDataStatements(IEnumerable<byte> pokeCode, BasicProgram program, int lineNumber)
	{
		const int LineLength = 5;

		var line = new BasicLine(lineNumber);
		int lineIndex = 0;

		foreach (var data in pokeCode)
		{
			if (lineIndex == 0)
			{
				line.AddToken(Token.DATA);
				line.AddNumber(data);
				lineIndex++;
			}
			else
			{
				line.AddText(",");
				line.AddNumber(data);

				if (++lineIndex == LineLength)
				{
					line.EndLine();
					program.AddLine(line);
					lineNumber += 10;
					lineIndex = 0;
					line = new BasicLine(lineNumber);
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

	private static void GenerateBasic(byte[] pokeCode, BasicProgram program)
	{
		int lineNumber = 0;
		var line = new BasicLine(lineNumber);

		if (rem)
		{
			var ldirCode = new byte[]
			{
				33, 222, 92,													// ld hl, 23774
				17, startAddress.GetLowByte(), startAddress.GetHighByte(),		// ld de, startAddress
				1, pokeCode.Length.GetLowByte(), pokeCode.Length.GetHighByte(),	// ld bc, pokeCode.Length
				237, 176,														// ldir
				195, startAddress.GetLowByte(), startAddress.GetHighByte()		// jp startAddress
			};

			line.AddToken(Token.REM);

			line.AddCode(ldirCode);
			line.AddCode(pokeCode);

			line.EndLine();
			program.AddLine(line);

			startAddress = 23760;
		}

		lineNumber += 10;

		if (clear)
		{
			line = new BasicLine(lineNumber);
			line.AddToken(Token.CLEAR);
			line.AddNumber(clearAddress);
			line.EndLine();
			program.AddLine(line);

			lineNumber += 10;
		}

		if (!rem)
		{
			// FOR F=xxxx TO yyyy
			line = new BasicLine(lineNumber);
			line.GenerateFor("F", startAddress, pokeCode.Length);
			line.EndLine();
			program.AddLine(line);

			lineNumber += 10;

			// READ A: POKE F, A: NEXT F
			line = new BasicLine(lineNumber);
			line.AddToken(Token.READ);
			line.AddText("A:");
			line.AddToken(Token.POKE);
			line.AddText("F,A:");
			line.AddToken(Token.NEXT);
			line.AddText("F");
			line.EndLine();
			program.AddLine(line);

			lineNumber += 10;
		}

		// LOAD ""CODE xxxx
		if (loadCode)
		{
			line = new BasicLine(lineNumber);
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
		line = new BasicLine(lineNumber);
		line.AddToken(Token.RANDOMIZE);
		line.AddToken(Token.USR);
		line.AddNumber(usrAddress == 0 ? startAddress : usrAddress);
		line.EndLine();
		program.AddLine(line);

		lineNumber += 10;

		if (!rem)
		{
			GenerateDataStatements(pokeCode, program, lineNumber);
		}
	}

	private static void ProcessArgs(string[] args)
	{
		inputFile = args[0];
		startAddress = int.Parse(args[1]);

		if (args.Length < 3)
		{
			return;
		}

		for (int i = 2; i < args.Length; i++)
		{
			ReadOnlySpan<char> arg = args[i];
			Console.WriteLine($"Processing arg[{i}]: {args[i]}");

			if (arg.StartsWith("--"))
			{
				ReadOnlySpan<char> argName = arg[2..];
				ReadOnlySpan<char> value = default;

				int valuePos = argName.IndexOf(":");
				if (valuePos > 0)
				{
					value = argName[(valuePos + 1)..];
					argName = argName[..valuePos];
				}

				// TODO: ToString() not needed in C# 11
				switch (argName.ToString())
				{
					case "clear":
						clear = true;
						clearAddress = int.Parse(value);
						break;

					case "loadcode":
						loadCode = true;
						// Value is optional, so might be null
						codeAddress = value == null ? 0 : int.Parse(value);
						break;

					case "usr":
						usrAddress = int.Parse(value);
						break;

					case "rem":
						rem = true;
						break;
				}
			}
		}
	}
}