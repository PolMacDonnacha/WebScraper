using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Linq;
using System.Collections.Generic;
using static System.Console;
    class Program
    {
        static void Main(string[] args)
        {
            ChromeOptions options = new ChromeOptions(); 
		    options.AddArguments("headless"); 
            int myindex = 0; // counts total number of mission records
            string[] urls = {
                "http://spaceflightnow.com/launch-log-2004-2008/",
                 "http://spaceflightnow.com/launch-log-2009-2011/",
                  "http://spaceflightnow.com/launch-log-2012-2014/",
                   "https://spaceflightnow.com/launch-log-2015-2016/",
                   "https://spaceflightnow.com/launch-log/",
                   "https://www.Spaceflightnow.com/launch-schedule/"};
            foreach(string url in urls)
            {
            IWebDriver driver = new ChromeDriver(@"D:\000College Work\Year3\Semester1\WebProgramming2\TestingScrapers\bin\Debug\netcoreapp3.1\",options);
            driver.Url = url;

            WriteLine("Page Title: " + driver.Title);
            IReadOnlyCollection<IWebElement> IWdatename = driver.FindElements(By.ClassName("datename"));//div holding mission records


             List<IWebElement> dateNameList = new List<IWebElement>();
             if (url == "https://www.Spaceflightnow.com/launch-schedule/")
            {
                dateNameList = IWdatename.ToList();
            }
            else
            {
                dateNameList = IWdatename.Reverse().ToList(); //inverts the order of the objects since previous pages have latest missions first
            }

           
            foreach(var datename in dateNameList.Select(value => new {value}))
            {          
            IWebElement IWlaunchdate = datename.value.FindElement(By.ClassName("launchdate"));
            IWebElement IWVehicleTitle = datename.value.FindElement(By.ClassName("mission"));//includes vehicle and name 
            IWebElement IWmissionData = datename.value.FindElement(By.XPath("following-sibling::*[1]"));//launchtime and site
            IWebElement IWDescription = datename.value.FindElement(By.XPath("following-sibling::*[2]"));//Mission Description
             string VehicleAndTitle = IWVehicleTitle.Text.ToString();//convert to string for separation         
                String[] missionSplt = VehicleAndTitle.Split('•',StringSplitOptions.RemoveEmptyEntries);
                string vehicle = missionSplt[0];
                string missionName = missionSplt[1];
                WriteLine("\n{0} Mission Name: {1}",myindex, missionName);
                WriteLine("Launch Vehicle: " + vehicle);

            string timeAndSite = IWmissionData.Text.ToString();
            string launchDate = IWlaunchdate.Text.ToString();
            if(timeAndSite.Contains("\n")) //edge case on page 2012-2014 where launchsite doesn't go to a new line, in else statement find closing bracket after time to separate
            {
                String[] timeSiteSplt = timeAndSite.Split("\n" ,StringSplitOptions.None); //separates the launch time from the launch site
                string time = timeSiteSplt[0].Remove(timeSiteSplt[0].IndexOf("L"),12).Trim(); //removes the string "Launch Time: " from the time and site data and also any spaces at the ends of the data
                string launchSite = timeSiteSplt[1].Remove(timeSiteSplt[1].IndexOf("L"),12).Trim();
                WriteLine("Date and Time: {0} {1}",launchDate,time);
                WriteLine("Launch Site: " + launchSite);
            }
            else
            {
                int separator = timeAndSite.IndexOf(")") + 1;// finds the space after the closing bracket of the time property
                String[] timeSiteSplt = timeAndSite.Split(timeAndSite[separator] ,StringSplitOptions.None); //separates every string by spaces
                string time = string.Format("{0} {1} {2}{3} {4}",timeSiteSplt[2], timeSiteSplt[3], timeSiteSplt[4], timeSiteSplt[5], timeSiteSplt[6]); //concatenates the strings needed
                string launchSite = string.Format("{0} {1} {2} {3} {4} {5} {6}",timeSiteSplt[9], timeSiteSplt[10], timeSiteSplt[11], timeSiteSplt[12], timeSiteSplt[13], timeSiteSplt[14], timeSiteSplt[15]);
                WriteLine("Date and Time: {0} {1}",launchDate,time);
                WriteLine("Launch Site: " + launchSite);
            }
            myindex++;
            }

            driver.Quit();
            }
        }
    }
