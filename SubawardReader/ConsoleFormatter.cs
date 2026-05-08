using Spectre.Console;

namespace SubawardReader
{
    public static class ConsoleFormatter
    {
        public static void PrintError(string message)
        {
            // Print error messages in red using Spectre.Console
            AnsiConsole.MarkupLine($"[red]{message}[/]");
        }
        public static void PrintSubheading(string subheading, decimal amount)
        {
            // Plain white subheading label with green money value
            AnsiConsole.MarkupLine($"    {subheading}: {Money(amount)}");
        }
        public static string FileName(string fileName)
        {
            // Yellow text
            return $"[yellow]{fileName}[/]";
        }

        public static string RecipientName(string recipientName)
        {
            // Bold text
            return $"[bold]{recipientName}[/]";
        }

        public static string Money(decimal amount)
        {
            // Green text, currency formatted
            return $"[green]{amount:C}[/]";
        }

        public static void PrintFileName(string fileName)
        {
            AnsiConsole.MarkupLine(FileName(fileName));
        }

        public static void PrintRecipientName(string recipientName)
        {
            AnsiConsole.MarkupLine($"  {RecipientName(recipientName)}");
        }

        public static void PrintMoney(decimal amount)
        {
            AnsiConsole.MarkupLine(Money(amount));
        }
    }
}
