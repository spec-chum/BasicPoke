namespace BasicPoke;

public class BasicProgram
{
	private const byte Flag = 0xff;
	private readonly List<BasicLine> lines = new();

	public List<byte> Basic { get; } = new();

	public int Length => Basic.Count;

	public void AddLine(BasicLine line) => lines.Add(line);

	public void Compile()
	{
		Basic.Add(Flag);

		foreach (var line in lines)
		{
			Basic.Add(line.LineNumber.GetHighByte());
			Basic.Add(line.LineNumber.GetLowByte());
			Basic.Add(line.Length.GetLowByte());
			Basic.Add(line.Length.GetHighByte());
			Basic.AddRange(line.Line);
		}

		Basic.Add(Program.CalcChecksum(Basic));
	}
}