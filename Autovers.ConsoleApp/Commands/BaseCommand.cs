using System.CommandLine;

namespace Autovers.ConsoleApp.Commands
{
	public abstract class BaseCommand
	{
		public abstract void Setup(RootCommand root);
	}
}
