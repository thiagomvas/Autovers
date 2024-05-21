namespace Autovers.Core
{
	public static class Logger
	{
		public static void LogSuccess(string message)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("V ");
			Console.ResetColor();
			Console.WriteLine(message);
			Console.ResetColor();
		}

		public static void LogError(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("X ");
			Console.ResetColor();
			Console.WriteLine(message);
			Console.ResetColor();
		}

		public static void LogWarning(string message)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("! ");
			Console.ResetColor();
			Console.WriteLine(message);
			Console.ResetColor();
		}

		public static void LogInfo(string message)
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write("> ");
			Console.ResetColor();
			Console.WriteLine(message);
			Console.ResetColor();
		}
	}
}
