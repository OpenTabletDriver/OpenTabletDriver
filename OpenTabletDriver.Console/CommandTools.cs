using System;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace OpenTabletDriver.Console
{
    static class CommandTools
    {
        private static Command SetupCommand<T>(T action, string description, params string[] aliases)
            where T : Delegate
        {
            var command = new Command(action.Method.Name.ToLower(), description);
            foreach (var alias in aliases.Skip(1))
                command.Aliases.Add(alias);
            return command;
        }

        private static Argument<T> SetupArgument<T>(Command command, string name)
        {
            var arg = new Argument<T>(name.ToLower());
            command.Arguments.Add(arg);
            return arg;
        }

        public static Command CreateCommand(Func<Task> action, string description, params string[] aliases)
        {
            var command = SetupCommand(action, description, aliases);
            command.SetAction(_ => action());
            return command;
        }

        public static Command CreateCommand<T1>(Func<T1, Task> action, string description, params string[] aliases)
        {
            var parameters = action.Method.GetParameters();
            var command = SetupCommand(action, description, aliases);

            var arg1 = SetupArgument<T1>(command, parameters[0].Name.ToLower());
            command.SetAction(pResults => action(pResults.GetRequiredValue(arg1)));

            return command;
        }

        public static Command CreateCommand<T1, T2>(Func<T1, T2, Task> action, string description, params string[] aliases)
        {
            var parameters = action.Method.GetParameters();
            var command = SetupCommand(action, description, aliases);

            var arg1 = SetupArgument<T1>(command, parameters[0].Name.ToLower());
            var arg2 = SetupArgument<T2>(command, parameters[1].Name.ToLower());
            command.SetAction(pResults =>
                action(
                    pResults.GetRequiredValue(arg1),
                    pResults.GetRequiredValue(arg2)
                    ));

            return command;
        }

        public static Command CreateCommand<T1, T2, T3>(Func<T1, T2, T3, Task> action, string description, params string[] aliases)
        {
            var parameters = action.Method.GetParameters();
            var command = SetupCommand(action, description, aliases);

            var arg1 = SetupArgument<T1>(command, parameters[0].Name.ToLower());
            var arg2 = SetupArgument<T2>(command, parameters[1].Name.ToLower());
            var arg3 = SetupArgument<T3>(command, parameters[2].Name.ToLower());
            command.SetAction(pResults =>
                action(
                    pResults.GetRequiredValue(arg1),
                    pResults.GetRequiredValue(arg2),
                    pResults.GetRequiredValue(arg3)
                    ));

            return command;
        }

        public static Command CreateCommand<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> action, string description, params string[] aliases)
        {
            var parameters = action.Method.GetParameters();
            var command = SetupCommand(action, description, aliases);

            var arg1 = SetupArgument<T1>(command, parameters[0].Name.ToLower());
            var arg2 = SetupArgument<T2>(command, parameters[1].Name.ToLower());
            var arg3 = SetupArgument<T3>(command, parameters[2].Name.ToLower());
            var arg4 = SetupArgument<T4>(command, parameters[3].Name.ToLower());
            command.SetAction(pResults =>
                action(
                    pResults.GetRequiredValue(arg1),
                    pResults.GetRequiredValue(arg2),
                    pResults.GetRequiredValue(arg3),
                    pResults.GetRequiredValue(arg4)
                    ));

            return command;
        }

        public static Command CreateCommand<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> action, string description, params string[] aliases)
        {
            var parameters = action.Method.GetParameters();
            var command = SetupCommand(action, description, aliases);

            var arg1 = SetupArgument<T1>(command, parameters[0].Name.ToLower());
            var arg2 = SetupArgument<T2>(command, parameters[1].Name.ToLower());
            var arg3 = SetupArgument<T3>(command, parameters[2].Name.ToLower());
            var arg4 = SetupArgument<T4>(command, parameters[3].Name.ToLower());
            var arg5 = SetupArgument<T5>(command, parameters[4].Name.ToLower());
            command.SetAction(pResults =>
                action(
                    pResults.GetRequiredValue(arg1),
                    pResults.GetRequiredValue(arg2),
                    pResults.GetRequiredValue(arg3),
                    pResults.GetRequiredValue(arg4),
                    pResults.GetRequiredValue(arg5)
                    ));

            return command;
        }

        public static Command CreateCommand<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, Task> action, string description, params string[] aliases)
        {
            var parameters = action.Method.GetParameters();
            var command = SetupCommand(action, description, aliases);

            var arg1 = SetupArgument<T1>(command, parameters[0].Name.ToLower());
            var arg2 = SetupArgument<T2>(command, parameters[1].Name.ToLower());
            var arg3 = SetupArgument<T3>(command, parameters[2].Name.ToLower());
            var arg4 = SetupArgument<T4>(command, parameters[3].Name.ToLower());
            var arg5 = SetupArgument<T5>(command, parameters[4].Name.ToLower());
            var arg6 = SetupArgument<T6>(command, parameters[5].Name.ToLower());
            command.SetAction(pResults =>
                action(
                    pResults.GetRequiredValue(arg1),
                    pResults.GetRequiredValue(arg2),
                    pResults.GetRequiredValue(arg3),
                    pResults.GetRequiredValue(arg4),
                    pResults.GetRequiredValue(arg5),
                    pResults.GetRequiredValue(arg6)
                    ));

            return command;
        }
    }
}
