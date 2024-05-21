using Autovers.Core;
using LibGit2Sharp;
using System.CommandLine;
using System.Reflection;

namespace Autovers.ConsoleApp.Commands
{
	public class CommitCommand : BaseCommand
	{
		public override void Setup(RootCommand root)
		{
			var command = new Command("commit", "Creates a new commit enforcing conventional commit guidelines");

			var messageOption = new Option<string>(new string[] { "--message", "-m" }, "The commit message")
			{
				IsRequired = true
			};

			command.AddOption(messageOption);

			command.SetHandler(Commit, messageOption);

			root.AddCommand(command);

		}

		private void Commit(string message)
		{
			if(Versioning.AllowComitting(message, Directory.GetCurrentDirectory()))
			{
				using (var repo = new Repository(Directory.GetCurrentDirectory()))
				{
					// Retrieve the committer's information from the configuration
					var config = repo.Config;
					string name = config.Get<string>("user.name")?.Value ?? throw new InvalidOperationException("User name not configured");
					string email = config.Get<string>("user.email")?.Value ?? throw new InvalidOperationException("User email not configured");

					// Create the committer's signature and commit
					var author = new Signature(name, email, DateTime.Now);
					var committer = author;

					// Commit the changes
					Commit commit = repo.Commit(message, author, committer);

					// Output the commit info
					Logger.LogSuccess($"Commit {commit.Id} created: {commit.MessageShort}");

				}
			}
			else
			{
				if(!Versioning.IsConventionalCommit(message))
					Logger.LogError("Commit message does not follow Conventional Commits guidelines");
				else
					Logger.LogError("No git repository found in " + AppDomain.CurrentDomain.BaseDirectory);
			}
		}
	}
}
