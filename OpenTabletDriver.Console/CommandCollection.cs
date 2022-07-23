using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Console.Attributes;

namespace OpenTabletDriver.Console
{
    public class CommandCollection : Collection<Command>
    {
        public CommandCollection(object source)
        {
            var module = source.GetType();
            var commandMethods = from method in module.GetMethods()
                where method.GetCustomAttribute<CommandAttribute>() != null
                select method;

            foreach (var method in commandMethods)
            {
                var command = BuildCommand(method, source);
                Add(command);
            }
        }

        private static Command BuildCommand(MethodInfo method, object source)
        {
            var name = method.GetCustomAttribute<CommandAttribute>()!.Command;
            var description = GetDescription(method);

            var command = new Command(name, description)
            {
                Handler = method.IsStatic ? CommandHandler.Create(method) : CommandHandler.Create(method, source)
            };

            foreach (var symbol in GetSymbols(method))
                command.Add(symbol);

            return command;
        }

        private static IEnumerable<Symbol> GetSymbols(MethodBase method)
        {
            return from parameter in method.GetParameters()
                where parameter.IsIn
                select parameter.IsOptional ? BuildOption(parameter) : BuildArgument(parameter);
        }

        private static Symbol BuildArgument(ParameterInfo parameter)
        {
            return new Argument(parameter.Name!)
            {
                Description = GetDescription(parameter),
                ArgumentType = parameter.ParameterType
            };
        }

        private static Symbol BuildOption(ParameterInfo parameter)
        {
            return new Option(parameter.Name!)
            {
                Description = GetDescription(parameter)
            };
        }

        private static string? GetDescription(ICustomAttributeProvider member)
        {
            return member.GetCustomAttribute<CommandAttribute>()?.Description;
        }
    }
}
