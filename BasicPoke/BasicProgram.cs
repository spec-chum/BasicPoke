using System.Collections.Generic;

namespace BasicPoke
{
	public class BasicProgram
	{
		public const byte Flag = 0xff;

		public List<byte> program = new List<byte>();

		private readonly List<Line> lines = new List<Line>();

		public int ProgramLength { get => program.Count; }

		public static byte CalcChecksum(List<byte> list)
		{
			byte result = 0;

			list.ForEach(element => result = (byte)(result ^ element));

			return result;
		}

		public void AddLine(Line line) => lines.Add(line);

		public void Compile()
		{
			program.Add(0xff);  // flag

			foreach (var line in lines)
			{
				program.Add((byte)(line.LineNumber >> 8 & 0xff));
				program.Add((byte)(line.LineNumber & 0xff));
				program.Add((byte)(line.LineLength & 0xff));
				program.Add((byte)(line.LineLength >> 8 & 0xff));
				program.AddRange(line.line);
			}

			program.Add(CalcChecksum(program));
		}
	}
}