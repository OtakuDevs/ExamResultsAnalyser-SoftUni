using System;

namespace ExamResultsAnalyser_SoftUni_ConsoleApp
{
    class MainProgram
    {
        static void Main(string[] args)
        {
            //Console Formatting

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = "Exam Results Analyser - SoftUni";
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Clear();

            Menu menu = new Menu();

            menu.StartProgram();
        }
    }
}
