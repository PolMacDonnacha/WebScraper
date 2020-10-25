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
            int myindex = 0;
            //List<IWebElement> IWdatenames = new List<IWebElement>();
            string[] urls = {"http://spaceflightnow.com/launch-log-2004-2008/", "http://spaceflightnow.com/launch-log-2009-2011/", "http://spaceflightnow.com/launch-log-2012-2014/", "https://spaceflightnow.com/launch-log-2015-2016/","https://www.Spaceflightnow.com/launch-schedule/"};
            foreach(string url in urls)
            {

            
            IWebDriver driver = new ChromeDriver(@"D:\000College Work\Year3\Semester1\WebProgramming2\TestingScrapers\bin\Debug\netcoreapp3.1\");
            driver.Url = url;

            WriteLine("This is the title of the viewed page" + driver.Title);
            IReadOnlyCollection<IWebElement> IWdatename = driver.FindElements(By.ClassName("datename"));//includes date, vehicle and mission name
            foreach(var datename in IWdatename.Select((value, index) => new {value, index})){
                WriteLine("This is IWDateName {0}: {1}",myindex,datename.value.Text.ToString());
                myindex++;
            }
            
            IWebElement IWlaunchdate = driver.FindElement(By.ClassName("launchdate"));//date of launch
            string launchdate = IWlaunchdate.Text.ToString();
            WriteLine("This is the date of the most recent mission on the site" + launchdate);
            IWebElement IWmission = driver.FindElement(By.ClassName("mission"));//includes vehicle and name
            string VehicleAndTitle = IWmission.Text.ToString();//convert to string for separation
            String[] missionSplt = VehicleAndTitle.Split('•',StringSplitOptions.RemoveEmptyEntries);
            string vehicle = missionSplt[0];
            WriteLine("Launch Vehicle: " + vehicle);
            string missionName = missionSplt[1];
            WriteLine("Mission Name: " + missionName);
            IWebElement IWmissionData = driver.FindElement(By.ClassName("missiondata"));//launchtime and site
            string timeAndSite = IWmissionData.Text.ToString();
            String[] timeSiteSplt = timeAndSite.Split("\n" ,StringSplitOptions.None); //separates the launch time from the launch site
            foreach(var item in timeSiteSplt)
            {
                WriteLine("Time or Launch Site: " + item);
            }
            string time = timeSiteSplt[0];
            WriteLine("Time of launch: " + time);
            string launchSite = timeSiteSplt[1];
            WriteLine("Launch Site: " + launchSite);            
           

        
            driver.Quit();
            }
        }
    }

/* foreach(var driver.FindElement(By.ClassName("datename")) in driver.Url){         foreach date on the page, create new object with data

            }*/