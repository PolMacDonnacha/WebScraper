using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using static System.Console;

    public class Program
    {
        
        static void Main(string[] args)
        {
            List<Rocket> rocketList = new List<Rocket>();
            int rocketIndex = 0;           //used to count number of unique rockets
            int duplicate = 0;             //used to count number of duplicate rockets
            ChromeOptions options = new ChromeOptions(); 
		    options.AddArguments("headless"); 
            int myindex = 1; // counts total number of mission records
            string[] urls = 
            {
                "http://spaceflightnow.com/launch-log-2004-2008/",
                "http://spaceflightnow.com/launch-log-2009-2011/",
                "http://spaceflightnow.com/launch-log-2012-2014/",
                "https://spaceflightnow.com/launch-log-2015-2016/",
                "https://spaceflightnow.com/launch-log/",
                "https://www.Spaceflightnow.com/launch-schedule/"
            };
            DateTime prevMissionDate = new DateTime();
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
                    IWebElement IWmissionYear;
                    if(url != "https://spaceflightnow.com/launch-log-2015-2016/" && url != "https://www.Spaceflightnow.com/launch-schedule/")
                    {
                        IWmissionYear = driver.FindElement(By.XPath("/html/body/div[1]/div[4]/div/div/article/div/p[2]/b/font"));//Contains the year so that it can be added onto datees that havent specified a year
                    }
                    if( url == "https://www.Spaceflightnow.com/launch-schedule/")
                    {
                        IWmissionYear = driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div[1]/div"));//Contains the year on another element and needs to  be split from the rest of the date
                    }
                    else
                    {
                        IWmissionYear = driver.FindElement(By.XPath("/html/body/div[1]/div[4]/div/div/article/div/p[2]/b"));
                    }
                    string VehicleAndTitle = IWVehicleTitle.Text.ToString();//convert to string for separation         
                    String[] missionSplt = VehicleAndTitle.Split('•',StringSplitOptions.RemoveEmptyEntries);
                    string vehicle = missionSplt[0].Trim();
                    string missionName = missionSplt[1].Trim();
                    WriteLine("\n{0} Mission Name: {1}",myindex, missionName);
                    WriteLine(" Launch Vehicle: " + vehicle);
                    string timeAndSite = IWmissionData.Text.ToString();
                    string launchDate = IWlaunchdate.Text.ToString().ToUpper();
                    string manufacturer = "";
                    string launchSite = "";
                    string[] monthnames = 
                    {
                        "JAN","FEB","MAR","APR","MAY","JUN",
                        "JUL","AUG","SEP","OCT","NOV","DEC"};                 
                    if (launchDate.Contains("/"))
                    {
                        launchDate = launchDate.Remove(launchDate.IndexOf("/") -2, 3); //Removes the first Day date from the Date item example: Dec. 22/23, 2008
                        WriteLine("Launch Date after day removal: " + launchDate);  
                    }
                    if (launchDate.Contains("."))
                    {
                        launchDate = launchDate.Remove(launchDate.IndexOf("."), 1);
                        WriteLine("Launch Date after '.' removal: " + launchDate);
                    }
                    if (!launchDate.Contains(",")) //Adding year if it is missing
                    {
                         string yearText;
                        if (url != "https://www.Spaceflightnow.com/launch-schedule/")
                        {
                            yearText = IWmissionYear.Text.ToString();
                        }
                        else
                        {
                            string dateInDiv = IWmissionYear.Text.ToString();//convert to string for separation         
                            String[] yearSplit = dateInDiv.Split(',',StringSplitOptions.RemoveEmptyEntries);
                            yearText = yearSplit[1];
                        }
                        string launchYear = string.Concat(", {0}",yearText);
                        launchDate = string.Concat(launchDate,launchYear);
                        WriteLine("Launch Date with year added: " + launchDate);  
                    }
                    if (launchDate.Contains(","))//getting year from date
                    {
                        string[] launchYearSplit = launchDate.Split(",",StringSplitOptions.RemoveEmptyEntries);
                        string launchYear = launchYearSplit[1].Trim();
                      WriteLine("Launch year trimmed: " + launchDate);
                    }
                    foreach(string month in monthnames)
                    {
                    /* if (launchDate.Contains(month)) //getting month from date
                        {
                            DateTime dateMonth = DateTime.Parse(month);
                            WriteLine(dateMonth);
                            break;
                        }
                        else if(launchDate == "TBD"|| launchDate.Contains("NET"))
                        {
                            launchDate = prevMissionDate.AddDays(5).ToString();// If date does not include a month name, set date to five days after the last mission
                            WriteLine("Date after adding 5 days");
                        }*/
                    }
                // DateTime date = DateTime.Parse(launchDate);//, "MMM dd, yyyy", CultureInfo.InvariantCulture);
                    if(timeAndSite.Contains("\n")) //edge case on page 2012-2014 where launchsite doesn't go to a new line, in else statement find closing bracket after time to separate
                    {
                        String[] timeSiteSplt = timeAndSite.Split("\n" ,StringSplitOptions.None); //separates the launch time from the launch site
                        string time = timeSiteSplt[0].Remove(timeSiteSplt[0].IndexOf("L"),12).Trim(); //removes the string "Launch Time: " from the time and site data and also any spaces at the ends of the data
                        launchSite = timeSiteSplt[1].Remove(timeSiteSplt[1].IndexOf("L"),12).Trim();
                        WriteLine("Date and Time: {0} {1}",launchDate,time);
                        WriteLine("Launch Site: " + launchSite);  
                    }
                    else
                    {
                        int separator = timeAndSite.IndexOf(")") + 1;// finds the space after the closing bracket of the time property
                        String[] timeSiteSplt = timeAndSite.Split(timeAndSite[separator] ,StringSplitOptions.None); //separates every string by spaces
                        string time = string.Format("{0} {1} {2}{3} {4}",timeSiteSplt[2], timeSiteSplt[3], timeSiteSplt[4], timeSiteSplt[5], timeSiteSplt[6]); //concatenates the strings needed
                        launchSite = string.Format("{0} {1} {2} {3} {4} {5} {6}",timeSiteSplt[9], timeSiteSplt[10], timeSiteSplt[11], timeSiteSplt[12], timeSiteSplt[13], timeSiteSplt[14], timeSiteSplt[15]);
                        WriteLine("Date and Time: {0} {1}",launchDate,time); 
                        WriteLine("Launch Site: " + launchSite); 
                    
                    }
                    bool isReusable;
                    //Move most used rockets to top of statment to improve efficency
                    switch(vehicle) 
                    {
                        case  var soy when vehicle.Contains("Soyuz")://Russian Rockets
                         case  var ang when vehicle.Contains("Angara"):
                          case  var vol when vehicle.Contains("Volna"):
                           case  var pro when vehicle.Contains("Proton"):
                            case  var mol when vehicle.Contains("Molniya"):
                             case  var rok when vehicle.Contains("Rockot"):
                              case  var kos when vehicle.Contains("Kosmos"):
                               case  var sht when vehicle.Contains("Shtil"):
                                case  var str when vehicle.Contains("Strela"):
                        manufacturer = "Russian Space Agency";
                        break;

                        case  var atl when vehicle.Contains("Atlas")://ULA rockets
                         case  var del when vehicle.Contains("Delta"):
                          case  var vul when vehicle.Contains("Vulcan"):
                        manufacturer = "United Launch Alliance";
                        break;

                        case  var roc when vehicle.Contains("Rocket 3")://Astra rocket
                        manufacturer = "Astra";
                        break;

                        case  var ttn when vehicle.Contains("Titan")://Glenn L rocket
                        manufacturer = "Glenn L. Martin Company";
                        break;

                        case  var ele when vehicle.Contains("Electron")://Rocket Labs rocket
                        manufacturer = "Rocket Labs";
                        break;

                        case  var saf when vehicle.Contains("Safir")://Iranian Space Agency rockets
                         case  var qsd when vehicle.Contains("Qased"):
                          case  var wim when vehicle.Contains("Simorgh"):
                        manufacturer = "Iranian Space Agency";
                        break;

                        case  var ksl when vehicle.Contains("KSLV")://South Korean Space Agency rockets
                        manufacturer = "Korea Aerospace Research Institute";
                        break;

                        case  var sha when vehicle.Contains("Shavit")://Israeli Space Agency rockets
                        manufacturer = "Israel Aerospace Industries";
                        break;

                        case  var ksl when vehicle.Contains("Zenit")://Yuzhmash rockets
                         case  var sea when vehicle.Contains("Sea Launch"):
                          case  var tsi when vehicle.Contains("Tsiklon"):
                           case  var tsy when vehicle.Contains("Tsyklon"):
                            case  var dne when vehicle.Contains("Dnepr"):
                        manufacturer = "Yuzhmash";
                        break;

                        case  var are when vehicle.Contains("Ares")://Beoing rockets
                         case  var sls when vehicle.Contains("SLS"):
                        manufacturer = "Beoing";
                        break;

                        case  var sup when vehicle.Contains("Super Strypi")://Aerojet rockets
                         case  var spa when vehicle.Contains("SPARK"):
                        manufacturer = "Aerojet Rocketdyne";
                        break;
                        
                        case  var fal when vehicle.Contains("Falcon")://SpaceX rockets
                         case  var sta when vehicle.Contains("Starship"):
                        manufacturer = "SpaceX";
                        break;

                        case  var fal when vehicle.Contains("Long March")://Chinese rockets
                         case  var sta when vehicle.Contains("CZ"):
                          case  var jie when vehicle.Contains("Jielong"):
                           case  var hyp when vehicle.Contains("Hyperbola"):
                            case  var osx when vehicle.Contains("OS-"):
                             case  var kt when vehicle.Contains("KT"):
                              case  var xhu when vehicle.Contains("Zhuque"):
                               case  var kua when vehicle.Contains("Kuaizhou"):
                        manufacturer = "Chinese Space Agency";
                        break;

                        case  var h2 when vehicle.Contains("H-2")://Japanese rockets
                         case  var f20 when vehicle.Contains("520"):
                          case  var t10 when vehicle.Contains("310"):
                           case  var m5 when vehicle.Contains("M-5"):
                            case  var eps when vehicle.Contains("Epsilon"):
                            manufacturer = "Japan Aerospace Exploration Agency";
                            break;

                        case  var tau when vehicle.Contains("Taurus")://Northrop rockets
                         case  var min when vehicle.Contains("Minotaur"):
                          case  var peg when vehicle.Contains("Pegasus"):
                           case  var shu when vehicle.Contains("Shuttle"):
                            case  var ant when vehicle.Contains("Antares"):
                        manufacturer = "Northrop Grumman";
                        break;

                        case  var lau when vehicle.Contains("LauncherOne")://Virgin Orbit rockets
                         case  var spac when vehicle.Contains("SpaceShip"):
                          case  var cos when vehicle.Contains("Cosmic"):
                        manufacturer = "Virgin Orbit";
                        break;

                        case  var uhn when vehicle.Contains("Unha")://North Korean rocket
                        manufacturer = "North Korea";
                        break;

                        case  var air when vehicle.Contains("Ariane")://ESA rockets
                         case  var veg when vehicle.Contains("Vega"):
                        manufacturer = "European Space Agency";
                        break;
                    
                        case  var gsl when vehicle.Contains("GSLV")://Indian rockets
                         case  var psl when vehicle.Contains("PSLV"):
                          case  var ssl when vehicle.Contains("SSLV"):
                        manufacturer = "Indian Space Research Organisation";
                        break;

                        default:
                        Write($"Launch site for hint: {launchSite}, {vehicle} was manufacutred by: ");
                        manufacturer = ReadLine();
                        break;
                    }
               
                    if(manufacturer == "SpaceX" | vehicle.Contains("Shephard") | manufacturer.Contains("Virgin"))
                    {
                        isReusable = true;
                    }
                    else
                    {
                        isReusable = false;
                    }
                    Rocket r = new Rocket(vehicle,manufacturer,isReusable);
                    if (!rocketList.Contains(r))
                    {
                        rocketList.Add(r);
                        rocketIndex++;
                    }
                    else
                    {
                        WriteLine("Duplicate rocket found");
                        duplicate++;
                    }
                    myindex++;
                }
                driver.Quit();
            }
            
            WriteLine("List of unique rockets");
            foreach (Rocket rocket in rocketList)
            {
                WriteLine($"\n{rocketIndex} \n Name: {rocket.Name}\n Manufacturer: {rocket.Manufacturer}\nReusable: {rocket.Reusable}");
            }
            WriteLine($"Number of duplicate rockets on site: {duplicate}");
        }  
    }
public class Rocket
{
    public string Name { get; set; }
    public string Manufacturer { get; set; }
    public bool Reusable { get; set; }
    public Rocket(string name, string manufacturer,bool isreusable)
    {
        Name = name;
        Manufacturer = manufacturer;
        Reusable = isreusable;
    }
}

