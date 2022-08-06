using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ExamResultsAnalyser_SoftUni_ConsoleApp
{
    class Menu
    {
        public void StartProgram()
        {
            int choice = -1;

            while (choice != 0)
            {
                DisplayConsoleMenu();

                choice = Input.ReadUserInputInteger();

                switch (choice)
                {
                    case 0:
                        break;
                    case 1:
                        string contestNumber = GetContestNumber();
                        int totalResultsPages = GetPagesFromContestants();

                        using (WebClient client = new WebClient { Encoding = System.Text.Encoding.UTF8 })
                        {
                            for (int i = 1; i < totalResultsPages; i++)
                            {
                                string htmlCode = BuildHTML(client, i, contestNumber);

                                //Finish calculations and REGEX


                            }

                        }

                        break;
                    case 2:
                        break;
                    default:
                        Console.WriteLine(); //Blank line - formatting
                        Console.WriteLine("Invalid choice, please select and existing option!\n");
                        break;
                }

            }
        }

        private void DisplayConsoleMenu()
        {
            //Menu formatting

            Console.WriteLine(" +++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("\n                        MENU                        ");
            Console.WriteLine("\n +++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine(" URL Builder                                    :1 ");
            Console.WriteLine(" Get Results                                    :2 ");
            Console.WriteLine("                                                :3 ");
            Console.WriteLine("                                                :4 ");
            Console.WriteLine("                                                :5 ");
            Console.WriteLine(" Exit the program                               :0 ");
            Console.WriteLine(" +++++++++++++++++++++++++++++++++++++++++++++++++++\n");
            Console.Write(" Which program would you like to access?: ");
        }

        private static string GetContestNumber()
        {
            Console.Write(" Which Contest/Exam ID you wish to check?: ");

            string contestNumber = Console.ReadLine();

            return contestNumber;
        }

        private static int GetPagesFromContestants()
        {
            Console.Write(" How many contestants participated?: ");

            int numberOfContestants = 0;

            numberOfContestants = Input.ReadUserInputInteger();

            double totalPages = Math.Ceiling(numberOfContestants / 100.0);

            int maxPageIndex = (int)totalPages;

            return maxPageIndex;
        }

        private static string BuildHTML(WebClient client, int pageIndex, string contestNumber)
        {
            string indexToString = pageIndex.ToString();

            byte[] content =
                client.DownloadData(
                    @$"https://judge.softuni.org/Contests/Compete/Results/Simple/{contestNumber}?page={indexToString}");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding1251 = Encoding.GetEncoding("windows-1251");
            var convertedBytes = Encoding.Convert(encoding1251, Encoding.UTF8, content);

            string htmlCode = Encoding.UTF8.GetString(convertedBytes);

            return htmlCode;
        }
    }
}

