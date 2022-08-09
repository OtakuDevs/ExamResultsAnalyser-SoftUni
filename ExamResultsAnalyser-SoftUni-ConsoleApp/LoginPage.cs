using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;

namespace ExamResultsAnalyser_SoftUni_ConsoleApp
{
    class LoginPage
    {
        private IWebDriver driver;

        public LoginPage(IWebDriver browser)
        {
            driver = browser;
        }

        private By username = By.Id("UserName");
        private IWebElement Username => driver.FindElement(username);

        private By password = By.Id("Password");
        private IWebElement Password => driver.FindElement(password);

        private By loginClick = By.CssSelector("input[class='btn btn-default']");
        private IWebElement BtnLoginClick => driver.FindElement(loginClick);

        public void LoginApplication(string username, string password)
        {
            Username.SendKeys(username);
            Password.SendKeys(password);

            BtnLoginClick.Click();
        }
    }
}
