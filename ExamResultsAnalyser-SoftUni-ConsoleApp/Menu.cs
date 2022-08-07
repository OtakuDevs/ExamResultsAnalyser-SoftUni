using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.CompilerServices;

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
                            for (int i = 1; i <= totalResultsPages; i++)
                            {
                                //string htmlCode = BuildHTML(client, i, contestNumber);
                                string regexPatternTasks = @"(0[1-6]{1}\.) [ ]?[^\W].+";

                                string regextPatternIndividualResults = @"\B<td>([0-9]+){3}\b</td>|<td>([\-])</td>";

                                List<string> taskNames = new List<string>();
                                Dictionary<string, List<string>> individualResults =
                                    new Dictionary<string, List<string>>();

                                List<string> tempList = new List<string>();

                                using (StreamReader r = new StreamReader("test.html"))
                                {
                                    string line;
                                    while ((line = r.ReadLine()) != null)
                                    {
                                        // X.
                                        // Try to match each line against the Regex.
                                        Match m = Regex.Match(line, regexPatternTasks);
                                        if (m.Success)
                                        {
                                            // Y.
                                            // Write original line and the value.
                                            string v = m.Value;
                                            taskNames.Add(v);
                                        }

                                        Match mk = Regex.Match(line, regextPatternIndividualResults);
                                        if (mk.Success)
                                        {
                                            // Y.
                                            // Write original line and the value.
                                            string v = mk.Value;

                                            v = v.TrimStart(new char[] { '<', 't', 'd', '>' });
                                            v = v.TrimEnd(new char[] { '<', '/', 't', 'd', '>' });
                                            tempList.Add(v);
                                        }
                                    }
                                }

                                for (int j = 0; j < taskNames.Count; j++)
                                {
                                    individualResults.Add(taskNames[j], new List<string>());
                                }

                                Dictionary<int, List<string>> studentDictionary = new Dictionary<int, List<string>>();

                                for (int j = 0; j < 100; j++)
                                {
                                    studentDictionary.Add(j + 1, new List<string>());

                                }


                                int counter = 1;
                                for (int j = 1; j <= tempList.Count; j++)
                                {
                                    studentDictionary[counter].Add(tempList[j - 1]);

                                    if (j % 17 == 0)
                                    {
                                        counter++;
                                    }

                                    if (counter > 100)
                                    {
                                        studentDictionary[counter - 1].Add(tempList[j - 1]);
                                        break;
                                    }
                                }


                                for (int j = 0; j < studentDictionary.Count; j++)
                                {
                                    studentDictionary[j + 1].RemoveAt(16);

                                    if (j == 99)
                                    {
                                        studentDictionary[j + 1].RemoveAt(0);
                                    }
                                }

                                foreach (var student in studentDictionary)
                                {
                                    int index = 0;
                                    foreach (var taskResult in student.Value)
                                    {
                                        string currentResult = taskResult;

                                        string currentKey = individualResults.ElementAt(index).Key;

                                        individualResults[currentKey].Add(currentResult);
                                        index++;
                                    }
                                }

                                Console.WriteLine($"Unique tasks: {taskNames.Count}");
                                Console.WriteLine(string.Join(Environment.NewLine, taskNames));

                                Dictionary<string, List<double>> averagePerTask = new Dictionary<string, List<double>>();

                                foreach (var result in individualResults)
                                {
                                    double averagePoints = 0;
                                    int counter2 = 0;
                                    foreach (var rr in result.Value)
                                    {
                                        double a;
                                        bool testParse = double.TryParse(rr, out a);

                                        if (testParse)
                                        {
                                            averagePoints += a;
                                            counter2++;
                                        }
                                    }

                                    averagePoints /= counter2;

                                    if (!averagePerTask.ContainsKey(result.Key))
                                    {
                                        averagePerTask.Add(result.Key, new List<double>());
                                    }

                                    averagePerTask[result.Key].Add(averagePoints);
                                }



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
            Console.WriteLine(" URL Builder and Get Results                    :1 ");
            Console.WriteLine("                                                :2 ");
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

