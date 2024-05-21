using System.Text.RegularExpressions;

namespace Autovers.Core.Types
{
	public class ProjectVersion
	{
		private int major, minor, patch;
		public string csprojPath { get; init; }

		public ProjectVersion(string csprojPath)
		{
			this.csprojPath = csprojPath;
			major = 1;
			minor = 0;
			patch = 0;
		}

		public ProjectVersion(string csprojPath, int major, int minor, int patch)
		{
			this.csprojPath = csprojPath;
			this.major = major;
			this.minor = minor;
			this.patch = patch;

		}
		public bool HasVersion => major > 0;

		public void Update(UpdateType updateType)
		{
			if (major <= 0)
				return;
			switch (updateType)
			{
				case UpdateType.Major:
					major++;
					minor = 0;
					patch = 0;
					break;
				case UpdateType.Minor:
					minor++;
					patch = 0;
					break;
				case UpdateType.Patch:
					patch++;
					break;
			}
		}

		public static ProjectVersion FromCsproj(string path)
		{
			string text = File.ReadAllText(path);
			string version = Regex.Match(text, "<Version>(.*?)</Version>").Groups[1].Value;

			if (string.IsNullOrEmpty(version))
			{
				return new(path, -1, 0, 0);
			}

			string[] versionParts = version.Split('.');
			return new ProjectVersion(path, int.Parse(versionParts[0]), int.Parse(versionParts[1]), int.Parse(versionParts[2]));
		}

		public string FormattedVersion => $"{major}.{minor}.{patch}";
		public override string ToString()
		{
			return $"{major}.{minor}.{patch}";
		}
	}
}
