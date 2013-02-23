namespace CP.Azure.Samples.MultiComponentRole.Engine
{
    using System;
    using System.ServiceProcess;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                var engine = new ProcessingEngine();

                Console.WriteLine("Starting Processing interactively. Enter 'q' to quit");

                engine.StartProcessing(args);

                // Work unit not provided an option to quit
                while (Console.ReadLine() != "q")
                {
                    Console.WriteLine("Please, enter 'q' to stop");
                }

                engine.EndProcessing();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                    {
                        new ProcessingEngine()
                    };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
