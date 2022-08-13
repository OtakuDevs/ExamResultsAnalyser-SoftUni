using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;



namespace ExamResultsAnalyser_SoftUni_ConsoleApp
{
    public class Menu
    {
        private int browserSelection = -1;

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
                        Console.Clear();
                        Console.WriteLine("How to use Results Analyser for SoftUni Exams:");
                        Console.WriteLine();
                        Console.WriteLine("1.First you you need to pick you preferred browser.");
                        Console.WriteLine();
                        Console.WriteLine("2.Locate the \"Exam Contest Number\" and the \"Number of Contestants\".");
                        Console.WriteLine();
                        Console.WriteLine("3.Prepare your username and password!");
                        Console.WriteLine();
                        Console.WriteLine("4.Go to Option 3 from the Main Menu and follow the questions/prompts.");
                        Console.WriteLine();
                        Console.WriteLine("5.After the scraper has finished, results will show on the Console, but will also be saved on a local file.");
                        Console.WriteLine();
                        Console.WriteLine("Press any key to go back!");
                        System.Console.ReadKey(true);
                        Console.Clear();

                        break;
                    case 2:
                        Console.Clear();
                        Console.Write(" Pick your preferred browser: ");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("(Note: You must have the browser installed for this to work!)");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(" Type 1 for Google Chrome ");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(" Type 2 for Mozilla Firefox ");
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine(" Type 3 for Microsoft Edge ");
                        Console.ForegroundColor = ConsoleColor.White;

                        Console.Write($" Insert your selection here: ");
                        browserSelection = int.Parse(Console.ReadLine());
                        Console.Clear();

                        break;
                    case 3:
                        Console.Clear();
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

                        BrowserSetup(browserSelection);
                        LoginSetup(username, password);

                        bool stop = false;
                        //HTML Source scraping
                        using (WebClient client = new WebClient { Encoding = System.Text.Encoding.UTF8 })
                        {
                            List<string> taskNames = new List<string>();

                            SortedDictionary<string, SortedDictionary<double, int>> averagePerTask = new SortedDictionary<string, SortedDictionary<double, int>>();

                            Dictionary<string, List<string>> TaskResults = new Dictionary<string, List<string>>();

                            List<Student> students = new List<Student>();

                            for (int pageIndex = 1; pageIndex <= totalResultsPages; pageIndex++)
                            {
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

                                FileInfo fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\tempHTML.txt");
                                FileStream fs = fi.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);


                                int taskIndex = 0;

                                using (StreamReader r = new StreamReader(fs))
                                {
                                    string line;
                                    int success = -1;
                                    while ((line = r.ReadLine()) != null)
                                    {
                                        //If page does not load properly - reload
                                        if (line.Contains("error"))
                                        {
                                            goto repeat;
                                        }

                                        if (pageIndex == 1)
                                        {
                                            Match m = Regex.Match(line, regexPatternTasks);
                                            if (m.Success)
                                            {
                                                // Y.
                                                // Write original line and the value.
                                                string v = m.Value;
                                                if (v[4] == ' ')
                                                    v = v.Remove(4, 1);
                                                taskNames.Add(v);
                                            }
                                        }

                                        // X.
                                        // Try to match each line against the Regex.

                                        Match mk = Regex.Match(line, regextPatternIndividualResults);

                                        if (mk.Success)
                                        {
                                            success++;
                                            if (success == 0 || success == taskNames.Count + 1)
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

                            Console.WriteLine();
                            Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++");
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

                        Console.WriteLine();
                        Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++");
                        Console.WriteLine("Press any key to go back!");
                        System.Console.ReadKey(true);
                        Console.Clear();

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
            Console.WriteLine(" How to use the Exam Results Analyser           :1 ");
            Console.WriteLine(" Select your preferred browser                  :2 ");
            Console.WriteLine(" URL Builder and Get Results                    :3 ");
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


        private void BrowserSetup(int browserSelection)
        {
            switch (browserSelection)
            {
                case 1:
                    ChromeDriverService serviceChrome = ChromeDriverService.CreateDefaultService(AppDomain.CurrentDomain.BaseDirectory); // location of the chromedriver.exe file
                    driver = new ChromeDriver(serviceChrome);
                    break;
                case 2:
                    FirefoxDriverService serviceFirefox = FirefoxDriverService.CreateDefaultService(AppDomain.CurrentDomain.BaseDirectory); // location of the geckodriver.exe file
                    driver = new FirefoxDriver(serviceFirefox);
                    break;
                case 3:
                    EdgeDriverService serviceEdge = EdgeDriverService.CreateDefaultService(AppDomain.CurrentDomain.BaseDirectory); // location of the msedgedriver.exe file
                    driver = new EdgeDriver(serviceEdge);
                    break;
            }

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

