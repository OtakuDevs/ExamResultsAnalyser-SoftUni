using System;
using System.Collections.Generic;
using System.Text;

namespace ExamResultsAnalyser_SoftUni_ConsoleApp
{
    class Input
    {

        public static int ReadUserInputInteger()
        {
            int input;

            if (int.TryParse(Console.ReadLine(), out input))
                return input;
            else
                Console.WriteLine($"Wrong input. The value is not a whole number (int). Please try again: ");

            return ReadUserInputInteger();
        }
    }
}
