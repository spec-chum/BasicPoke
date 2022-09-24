namespace BasicPoke;

public static class ExtensionMethods
{
	public static byte[] GetAsciiBytes(this string str)
	{
		return System.Text.Encoding.ASCII.GetBytes(str);
	}

	public static byte GetLowByte(this int value)
	{
		return (byte)value;
	}

	public static byte GetHighByte(this int value)
	{
		return (byte)(value >> 8);
	}
}
