namespace Autovers.Core.Types
{
	public class AutoversConfig
	{
		public string WorkingDir { get; set; } = "";
		public Keyword[] Keywords { get; set; } =
		[
			new("feat", UpdateType.Minor, true),
			new("fix", UpdateType.Patch, true),
			new("chore", UpdateType.None, false),
			new("docs", UpdateType.None, false),
			new("style", UpdateType.None, false),
			new("refactor", UpdateType.None, false),
			new("perf", UpdateType.None, false),
			new("test", UpdateType.None, false),
			new("build", UpdateType.None, false),
			new("ci", UpdateType.None, false),
		];
	}
}
