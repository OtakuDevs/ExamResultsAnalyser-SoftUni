using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;



namespace ExamResultsAnalyser_SoftUni_ConsoleApp
{
    public class Menu
    {

        FirefoxDriverService service = FirefoxDriverService.CreateDefaultService(AppDomain.CurrentDomain.BaseDirectory); // location of the geckodriver.exe file

        private IWebDriver driver;
        private LoginPage login;

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
                        //Obtain contest number and number of contestants to calculate the pages and build the url
                        string contestNumber = GetContestNumber();
                        int totalResultsPages = GetPagesFromContestants();

                        //Login for Judge
                        Console.Write(" Enter your username: ");
                        string username = Console.ReadLine();

                        Console.Write(" Enter is your password: ");
                        string password = null;
                        while (true)
                        {
                            var key = System.Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Enter)
                                break;
                            password += key.KeyChar;
                        }

                        BrowserSetup();
                        LoginSetup(username, password);

                        bool stop = false;
                        //HTML Source scraping
                        using (WebClient client = new WebClient { Encoding = System.Text.Encoding.UTF8 })
                        {
                            List<string> taskNames = new List<string>();
                            //Dictionary<string, List<string>> individualResults = new Dictionary<string, List<string>>();

                            SortedDictionary<string, SortedDictionary<double, int>> averagePerTask = new SortedDictionary<string, SortedDictionary<double, int>>();

                            Dictionary<string, List<string>> TaskResults = new Dictionary<string, List<string>>();

                            List<Student> students = new List<Student>();

                            //int counterTest = 0;

                            for (int pageIndex = 1; pageIndex <= totalResultsPages; pageIndex++)
                            {
                                //if (pageIndex == totalResultsPages)
                                //{
                                //    counterTest++;
                                //}

                                //if (counterTest > 1)
                                //{
                                //    break;
                                //}

                            //if the page does not load
                            repeat:
                                string indexToString = pageIndex.ToString();

                                string currentPageUrl =
                                    $"https://judge.softuni.org/Contests/Compete/Results/Simple/{contestNumber}?page={indexToString}";



                                string htmlCode = GoToContest(currentPageUrl);

                                string regexPatternTasks = @"(0[1-6]{1}\.) [ ]?[^\W].+";

                                string regextPatternIndividualResults = @"\B<td>([0-9]+){1,3}\b</td>|<td>([\-])</td>";

                                System.IO.StreamWriter htmlFile = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\tempHTML.txt");
                                htmlFile.BaseStream.Seek(0, SeekOrigin.End);
                                htmlFile.Write(htmlCode);
                                htmlFile.Flush();
                                htmlFile.Close();

                                //List<string> tempList = new List<string>();

                                FileInfo fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\tempHTML.txt");
                                FileStream fs = fi.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);


                                int taskIndex = 0;

                                using (StreamReader r = new StreamReader(fs))
                                {
                                    string line;
                                    int success = -1;
                                    while ((line = r.ReadLine()) != null)
                                    {

                                        if (pageIndex == 1)
                                        {
                                            Match m = Regex.Match(line, regexPatternTasks);
                                            if (m.Success)
                                            {
                                                // Y.
                                                // Write original line and the value.
                                                string v = m.Value;
                                                if(v[4] == ' ') 
                                                    v = v.Remove(4,1);
                                                taskNames.Add(v);
                                            }
                                        }

                                        // X.
                                        // Try to match each line against the Regex.

                                        Match mk = Regex.Match(line, regextPatternIndividualResults);
                                        
                                        if (mk.Success)
                                        {
                                            success++;
                                            if(success == 0 || success == taskNames.Count + 1)
                                            {
                                                if (success == taskNames.Count + 1)
                                                {
                                                    success = -1;
                                                    if (taskIndex == taskNames.Count)
                                                    {
                                                        taskIndex = 0;
                                                    }
                                                }
                                                continue;
                                            }
                                            // Y.
                                            // Write original line and the value.
                                            string v = mk.Value;

                                            v = v.TrimStart(new char[] { '<', 't', 'd', '>' });
                                            v = v.TrimEnd(new char[] { '<', '/', 't', 'd', '>' });
                                            //tempList.Add(v);
                                            
                                            if (!TaskResults.ContainsKey(taskNames[taskIndex]))
                                            {
                                                TaskResults.Add(taskNames[taskIndex], new List<string>());
                                            }
                                            TaskResults[taskNames[taskIndex]].Add(v);
                                            taskIndex++;

                                        }
                                    }
                                }
                            }

                            
                            foreach (var result in TaskResults)
                            {
                                
                                double averagePoints = 0;
                                int counterAttempts = 0;

                                foreach (var rr in result.Value)
                                {
                                    double a;
                                    bool testParse = double.TryParse(rr, out a);

                                    if (testParse)
                                    {
                                        averagePoints += a;
                                        counterAttempts++;
                                    }
                                }
                                
                                averagePoints /= counterAttempts;

                                if (!averagePerTask.ContainsKey(result.Key))
                                {
                                    averagePerTask.Add(result.Key, new SortedDictionary<double, int>());
                                }

                                averagePerTask[result.Key].Add(averagePoints, counterAttempts);
                            }
                            

                            Console.WriteLine($"Unique tasks: {taskNames.Count}");

                            foreach (var taskAverage in averagePerTask)
                            {
                                double taskAvg = taskAverage.Value.FirstOrDefault().Key;
                                int taskAtt = taskAverage.Value.FirstOrDefault().Value;

                                Console.Write($"{taskAverage.Key}");

                                Console.WriteLine(string.Join(Environment.NewLine, taskAverage.Value.OrderBy(x => x.Key).Select(y => $" -> {y.Key:f1} / 100.0, total task attempts {y.Value}")));
                            }
                        }
                        BrowserClose();
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

        private void BrowserSetup()
        {
            driver = new FirefoxDriver(service);
            driver.Navigate().GoToUrl("https://judge.softuni.org/Account/Login");
        }

        private void LoginSetup(string username, string password)
        {
            login = new LoginPage(driver);

            login.LoginApplication(username, password);
        }

        private string GoToContest(string currentUrl)
        {
            string currentHtml = String.Empty;

            driver.Navigate().GoToUrl(currentUrl);

            currentHtml = driver.PageSource;

            return currentHtml;
        }

        private void BrowserClose()
        {
            driver.Close();
            driver.Quit(); 
        }
    }
}

