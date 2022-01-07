using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace OpenTabletDriver.Console
{
    static class CommandTools
    {
        public static Command CreateCommand(Func<Task> action, string description, params string[] aliases)
        {
            var command = new Command(action.Method.Name.ToLower(), description)
            {
                Handler = CommandHandler.Create(action)
            };
            foreach (var alias in aliases)
                command.AddAlias(alias);
            return command;
        }

        public static Command CreateCommand<T1>(Func<T1, Task> action, string description, params string[] aliases)
        {
            var parameters = action.Method.GetParameters();
            var command = new Command(action.Method.Name.ToLower(), description)
            {
                new Argument<T1>(parameters[0].Name.ToLower())
            };
            command.Handler = CommandHandler.Create<T1>(action);
            foreach (var alias in aliases)
                command.AddAlias(alias);
            return command;
        }

        public static Command CreateCommand<T1, T2>(Func<T1, T2, Task> action, string description, params string[] aliases)
        {
            var parameters = action.Method.GetParameters();
            var command = new Command(action.Method.Name.ToLower(), description)
            {
                new Argument<T1>(parameters[0].Name.ToLower()),
                new Argument<T2>(parameters[1].Name.ToLower())
            };
            command.Handler = CommandHandler.Create<T1, T2>(action);
            foreach (var alias in aliases)
                command.AddAlias(alias);
            return command;
        }

        public static Command CreateCommand<T1, T2, T3>(Func<T1, T2, T3, Task> action, string description, params string[] aliases)
        {
            var parameters = action.Method.GetParameters();
            var command = new Command(action.Method.Name.ToLower(), description)
            {
                new Argument<T1>(parameters[0].Name.ToLower()),
                new Argument<T2>(parameters[1].Name.ToLower()),
                new Argument<T3>(parameters[2].Name.ToLower())
            };
            command.Handler = CommandHandler.Create<T1, T2, T3>(action);
            foreach (var alias in aliases)
                command.AddAlias(alias);
            return command;
        }

        public static Command CreateCommand<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> action, string description, params string[] aliases)
        {
            var parameters = action.Method.GetParameters();
            var command = new Command(action.Method.Name.ToLower(), description)
            {
                new Argument<T1>(parameters[0].Name.ToLower()),
                new Argument<T2>(parameters[1].Name.ToLower()),
                new Argument<T3>(parameters[2].Name.ToLower()),
                new Argument<T4>(parameters[3].Name.ToLower())
            };
            command.Handler = CommandHandler.Create<T1, T2, T3, T4>(action);
            foreach (var alias in aliases)
                command.AddAlias(alias);
            return command;
        }

        public static Command CreateCommand<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> action, string description, params string[] aliases)
        {
            var parameters = action.Method.GetParameters();
            var command = new Command(action.Method.Name.ToLower(), description)
            {
                new Argument<T1>(parameters[0].Name.ToLower()),
                new Argument<T2>(parameters[1].Name.ToLower()),
                new Argument<T3>(parameters[2].Name.ToLower()),
                new Argument<T4>(parameters[3].Name.ToLower()),
                new Argument<T5>(parameters[4].Name.ToLower())
            };
            command.Handler = CommandHandler.Create<T1, T2, T3, T4, T5>(action);
            return command;
        }

        public static Command CreateCommand<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, Task> action, string description, params string[] aliases)
        {
            var parameters = action.Method.GetParameters();
            var command = new Command(action.Method.Name.ToLower(), description)
            {
                new Argument<T1>(parameters[0].Name.ToLower()),
                new Argument<T2>(parameters[1].Name.ToLower()),
                new Argument<T3>(parameters[2].Name.ToLower()),
                new Argument<T4>(parameters[3].Name.ToLower()),
                new Argument<T5>(parameters[4].Name.ToLower()),
                new Argument<T6>(parameters[5].Name.ToLower())
            };
            command.Handler = CommandHandler.Create<T1, T2, T3, T4, T5, T6>(action);
            return command;
        }
    }
}
