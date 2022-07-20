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
        private CommandCollection(object source)
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

        public static CommandCollection Build(object source) => new CommandCollection(source);

        private static Command BuildCommand(MethodInfo method, object source)
        {
            var name = method.GetCustomAttribute<CommandAttribute>()!.Command;
            var description = GetDescription(method);

            var arguments = GetArguments(method);
            var options = GetOptions(method);

            var command = new Command(name, description)
            {
                Handler = method.IsStatic ? CommandHandler.Create(method) : CommandHandler.Create(method, source)
            };

            foreach (var argument in arguments)
                command.AddArgument(argument);

            foreach (var option in options)
                command.AddOption(option);

            return command;
        }

        private static IEnumerable<Argument> GetArguments(MethodInfo method)
        {
            return from parameter in method.GetParameters()
                where parameter.IsIn
                where !parameter.IsOptional
                select BuildArgument(parameter);
        }

        private static IEnumerable<Option> GetOptions(MethodInfo method)
        {
            return from parameter in method.GetParameters()
                where parameter.IsIn
                where parameter.IsOptional
                select BuildOption(parameter);
        }

        private static Argument BuildArgument(ParameterInfo parameter)
        {
            return new Argument(parameter.Name)
            {
                Description = GetDescription(parameter),
                ArgumentType = parameter.ParameterType
            };
        }

        private static Option BuildOption(ParameterInfo parameter)
        {
            return new Option(parameter.Name)
            {
                Description = GetDescription(parameter),
                Argument = BuildArgument(parameter),
                Required = false
            };
        }

        private static string? GetDescription(ICustomAttributeProvider member)
        {
            return member.GetCustomAttribute<CommandAttribute>()?.Description;
        }
    }
}
