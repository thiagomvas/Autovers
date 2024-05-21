using Autovers.Core.Types;
using LibGit2Sharp;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Autovers.Core
{
	public static class Versioning
	{
		private static AutoversConfig _config = new();

		public static void UpdateVersionWithTags(ProjectVersion[] versions, string[] commits)
		{
			// Check if repo is dirty
			using (var repo = new Repository(_config.WorkingDir))
			{
				if (repo.RetrieveStatus().IsDirty)
				{
					Logger.LogError("Repository is dirty, please commit or stash your changes before updating the version");
					return;
				}
			}

			var sorted = commits.GroupBy(c => c.Split(":").First()).ToArray();

			// Get the highest update level from the commits
			var highest = sorted.Select(g => _config.Keywords.FirstOrDefault(k => k.Key == g.Key).Type).Max();

			// Update the version
			foreach (var version in versions)
			{
				version.Update(highest);
				// Update the csproj file
				string text = File.ReadAllText(version.csprojPath);
				text = Regex.Replace(text, "<Version>(.*?)</Version>", $"<Version>{version}</Version>");

				File.WriteAllText(version.csprojPath, text);
			}

			// Commit the changes
			using (var repo = new Repository(_config.WorkingDir))
			{
				// Retrieve the committer's information from the configuration
				var config = repo.Config;
				string name = config.Get<string>("user.name")?.Value ?? throw new InvalidOperationException("User name not configured");
				string email = config.Get<string>("user.email")?.Value ?? throw new InvalidOperationException("User email not configured");

				// Create the committer's signature and commit
				var author = new Signature(name, email, DateTime.Now);
				var committer = author;

				// Stage the changes
				Commands.Stage(repo, "*");

				// Commit the changes
				Commit commit = repo.Commit($"chore: update version to {versions.First()}", author, committer);

				// Apply a new version tag
				var tag = repo.ApplyTag(versions.First().ToString(), commit.Sha);

				// Output the tag info
				Logger.LogSuccess($"Tag {tag.FriendlyName} created: {tag.Target.Id.ToString().Substring(0, 6)}");
				Logger.LogInfo("Don't forget to push the tags to the remote repository with 'git push --tags'");

			}
		}
		public static void UpdateVersion(ProjectVersion version, string[] commits)
		{

			// Check if repo is dirty
			using (var repo = new Repository(_config.WorkingDir))
			{
				if (repo.RetrieveStatus().IsDirty)
				{
					Logger.LogError("Repository is dirty, please commit or stash your changes before updating the version");
					return;
				}
			}



			var sorted = commits.GroupBy(c => c.Split(":").First()).ToArray();
			
			// Get the highest update level from the commits
			var highest = sorted.Select(g => _config.Keywords.FirstOrDefault(k => k.Key == g.Key).Type).Max();

			// Update the version
			version.Update(highest);

			// Update the csproj file
			string text = File.ReadAllText(version.csprojPath);
			text = Regex.Replace(text, "<Version>(.*?)</Version>", $"<Version>{version}</Version>");

			File.WriteAllText(version.csprojPath, text);
		}

		public static string[] GetCommits()
		{
			using(var repo = new Repository(_config.WorkingDir))
			{
				var commits = repo.Commits.Select(c => c.Message).ToArray();
				return commits;
			}
		}

		public static ProjectVersion[] GetVersionableProjects(string workingDir, string extension = "*.csproj")
		{
			string[] csprojs = Directory.GetFiles(workingDir, extension, SearchOption.AllDirectories);
			var projects = new List<ProjectVersion>();
			foreach (var csproj in csprojs)
			{
				var result = ProjectVersion.FromCsproj(csproj);
				if(result.HasVersion)
					projects.Add(result);
			}
			return projects.ToArray();
		}

		public static bool LoadConfig(string dir)
		{
			if(!Repository.IsValid(dir))
			{
				Logger.LogError("No git repository found in " + dir);
				return false;
			}
			// Look for a autovers.json file in the working directory
			string configPath = Path.Combine(dir, "autovers.json");
			if (File.Exists(configPath))
			{
				string json = File.ReadAllText(configPath);
				var result = JsonConvert.DeserializeObject<AutoversConfig>(json);

				if (result is null)
				{
					_config = new();
					_config.WorkingDir = dir;
					// Save the default config if invalid
					Logger.LogWarning("Invalid config file, using default");
				}
				else
				{
					_config = result;
					Logger.LogSuccess($"Successfully loaded config file: {configPath}");
				}
				return true;
			}

			_config = new();
			_config.WorkingDir = dir;
			Logger.LogWarning("Could not find config file, using default, creating default config file...");
			string defaultConfig = JsonConvert.SerializeObject(_config, Formatting.Indented);
			File.WriteAllText(configPath, defaultConfig);

			return true;
		}

		public static bool IsConventionalCommit(string commit)
		{
			return _config.Keywords.Any(k => commit.ToLowerInvariant().Contains(k.Key));
		}

		public static bool AllowComitting(string message, string dir)
		{
			return IsConventionalCommit(message) && Repository.IsValid(dir);
		}
	}
}
