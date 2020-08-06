using System;


namespace PNUDispatcher
{
    class Program
    {


        static void Main(string[] args)
        {
            Watcher wWatcher = new Watcher();

            // Wait for the user to quit the program.
            Console.WriteLine("Press 'q' to quit the sample.");
            while (Console.Read() != 'q') ;


        }
    }
}
