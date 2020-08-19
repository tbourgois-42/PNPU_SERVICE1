using System;


namespace PNUDispatcher
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Watcher wWatcher = new Watcher();
            wWatcher.launchWatcher();

            // Wait for the user to quit the program.
            Console.WriteLine("Press 'q' to quit the sample.");
            while (Console.Read() != 'q')
            {
                ;
            }
        }
    }
}
