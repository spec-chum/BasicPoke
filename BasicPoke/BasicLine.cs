namespace BasicPoke;

public class BasicLine
{
	private readonly List<byte> line = new();

	public BasicLine(int lineNumber)
	{
		LineNumber = lineNumber;
	}

	public enum Token : byte
	{
		RND = 165,
		INKEYS = 166,
		PI = 167,
		FN = 168,
		POINT = 169,
		SCREENS = 170,
		ATTR = 171,
		AT = 172,
		TAB = 173,
		VALS = 174,
		CODE = 175,
		VAL = 176,
		LEN = 177,
		SIN = 178,
		COS = 179,
		TAN = 180,
		ASN = 181,
		ACS = 182,
		ATN = 183,
		LN = 184,
		EXP = 185,
		INT = 186,
		SQR = 187,
		SGN = 188,
		ABS = 189,
		PEEK = 190,
		IN = 191,
		USR = 192,
		STRS = 193,
		CHRS = 194,
		NOT = 195,
		BIN = 196,
		OR = 197,
		AND = 198,
		LESSOREQUAL = 199,
		MOREOREQUAL = 200,
		NOTEQUAL = 201,
		LINE = 202,
		THEN = 203,
		TO = 204,
		STEP = 205,
		DEFFN = 206,
		CAT = 207,
		FORMAT = 208,
		MOVE = 209,
		ERASE = 210,
		OPEN = 211,
		CLOSE = 212,
		MERGE = 213,
		VERIFY = 214,
		BEEP = 215,
		CIRCLE = 216,
		INK = 217,
		PAPER = 218,
		FLASH = 219,
		BRIGHT = 220,
		INVERSE = 221,
		OVER = 222,
		OUT = 223,
		LPRINT = 224,
		LLIST = 225,
		STOP = 226,
		READ = 227,
		DATA = 228,
		RESTORE = 229,
		NEW = 230,
		BORDER = 231,
		CONTINUE = 232,
		DIM = 233,
		REM = 234,
		FOR = 235,
		GOTO = 236,
		GOSUB = 237,
		INPUT = 238,
		LOAD = 239,
		LIST = 240,
		LET = 241,
		PAUSE = 242,
		NEXT = 243,
		POKE = 244,
		PRINT = 245,
		PLOT = 246,
		RUN = 247,
		SAVE = 248,
		RANDOMIZE = 249,
		IF = 250,
		CLS = 251,
		DRAW = 252,
		CLEAR = 253,
		RETURN = 254,
		COPY = 255
	}

	public int Length => line.Count;

	public int LineNumber { get; }

	public List<byte> Line => line;

	public void AddCode(IEnumerable<byte> code) => line.AddRange(code);

	public void AddNumber(int number)
	{
		line.AddRange(number.ToString().GetAsciiBytes());
		line.Add(14);
		line.Add(0);
		line.Add(0);
		line.Add(number.GetLowByte());
		line.Add(number.GetHighByte());
		line.Add(0);
	}

	public void AddText(string text, bool quotes = false)
	{
		if (quotes)
		{
			line.Add(34);
			line.AddRange(text.GetAsciiBytes());
			line.Add(34);

			return;
		}

		line.AddRange(text.GetAsciiBytes());
	}

	public void AddToken(Token token) => line.Add((byte)token);

	public void EndLine() => line.Add(13);

	public void GenerateFor(string variable, int start, int length)
	{
		AddToken(Token.FOR);
		AddText(variable + "=");
		AddNumber(start);
		AddToken(Token.TO);
		AddNumber(start + length - 1);
	}
}