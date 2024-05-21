// Recursively find all csprojs inside a working directory
using Autovers.Core;
using System.CommandLine;
using LibGit2Sharp;
using Autovers.ConsoleApp.Commands;
using System.Reflection;

internal class Program
{
	private static async Task<int> Main(string[] args)
	{
		string workingDir = "C:\\Users\\Thiago\\source\\repos\\Autovers";
		if (!Versioning.LoadConfig(workingDir))
			return -1;

		var root = new RootCommand("A CLI tool to automatically version your .NET projects based on git history and enforce Conventional Commits guidelines");

		root.SetHandler(Versionize);

		var commitcommand = new CommitCommand();
		commitcommand.Setup(root);

		return await root.InvokeAsync(args);
	}

	public static void Versionize()
	{

		string workingDir = "C:\\Users\\Thiago\\source\\repos\\Autovers";

		var projects = Versioning.GetVersionableProjects(workingDir);

		// Get Autovers.json config file from working directory

		foreach (var project in projects)
		{
			Logger.LogSuccess($"Found versionable project: {Path.GetFileName(project.csprojPath)}");
		}


		var commits = new string[0];

		try
		{
			commits = Versioning.GetCommits().Reverse().ToArray();
		}
		catch (RepositoryNotFoundException)
		{
			Logger.LogError("No git repository found in " + workingDir);
			return;
		}

		if (commits.Length == 0)
		{
			Logger.LogWarning("No commits found in repository");
			return;
		}

		Versioning.UpdateVersionWithTags(projects, commits);


		Logger.LogSuccess("Done!");
	}

}