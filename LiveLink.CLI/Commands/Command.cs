using System;

namespace COM3D2.LiveLink.CLI.Commands
{
    public abstract class Command
    {
        public abstract string Name { get; }

        protected readonly CommandConsole Console;
		protected readonly CommandConsole DebugConsole;
		protected readonly CommandConsole ErrorConsole;


        public bool IsSilent = false;

		protected Command()
        {
			Console      = new CommandConsole(this,   System.Console.Write,   System.Console.WriteLine);
            DebugConsole = new CommandConsole(this, CLI.DebugConsole.Write, CLI.DebugConsole.WriteLine);
			ErrorConsole = new CommandConsole(
                this,
                System.Console.Error.Write, 
                (x) => System.Console.Error.WriteLine($"{Name} error: {x}")
            );
		}

        public bool Match(in string commandline)
        {
            return Match(Parse.StringToArray(commandline));
        }

        public bool Match(in string[] args)
        {
            if (args.Length == 0) return false;
            if (args[0].ToLower() == Name.ToLower()) return true;
            return false;
        }

        public int Run(in string commandline)
        {
            return Run(Parse.StringToArray(commandline));
        }

        public abstract int Run(in string[] args);
    



        protected class CommandConsole
        {
            private Command m_Command;
            private Action<object> m_OnWrite;
			private Action<object> m_OnWriteLine;
			public CommandConsole(Command command, Action<object> onWrite, Action<object> onWriteLine)
            {
                m_Command = command;
                m_OnWrite = onWrite;
                m_OnWriteLine = onWriteLine;
            }

			public void Write(object message)
            {
                if (m_Command.IsSilent) return;
				m_OnWrite.Invoke(message);
			}
			public void WriteLine(object message)
			{
				if (m_Command.IsSilent) return;
				m_OnWriteLine.Invoke(message);
			}
		}
	}


    public class LambdaCommand : Command
    {
        public override string Name => _name;
        private readonly string _name;

        private readonly Func<string[], int> m_Function;


        public LambdaCommand(in string name, Func<string[], int> func)
        {
            _name = name;
            m_Function = func;
        }

        public LambdaCommand(in string name, Func<int> action) :
            this(name, (x) => action.Invoke())
        { }

        public LambdaCommand(in string name, Action<string[]> action) :
            this(name, (x) => { action.Invoke(x); return 0; })
        { }

        public LambdaCommand(in string name, Action action) :
            this(name, (x) => action.Invoke())
        { }

        public override int Run(in string[] args)
        {
            return m_Function.Invoke(args);
        }
    }
}
