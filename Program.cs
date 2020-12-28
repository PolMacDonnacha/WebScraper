using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;
using static System.Console;
using System.Web;
using System.Data;
using System.Threading;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.IO;


    public class Program
    {
        
        static void Main(string[] args)
        {
            List<Rocket> rocketList = new List<Rocket>();
            List<Rocket> DataforAPI = new List<Rocket>();
            List<Mission> missionList = new List<Mission>();
            List<Rocket> jsonRockets;
            List<Mission> jsonMissions;
            int rocketIndex = 0;           //used to count number of unique rockets
            int missionIndex = 0;           //used to count number of unique missions
            int duplicate = 0;             //used to count number of duplicate rockets
            ChromeOptions options = new ChromeOptions(); 
		    options.AddArguments("headless"); 
            int myindex = 1; // counts total number of mission records
            int prevDateUsed = 0;
            string[] urls = 
            {
                "http://spaceflightnow.com/launch-log-2004-2008/",
                "http://spaceflightnow.com/launch-log-2009-2011/",
                "http://spaceflightnow.com/launch-log-2012-2014/",
                "https://spaceflightnow.com/launch-log-2015-2016/",
                "https://spaceflightnow.com/launch-log/",
                "https://www.Spaceflightnow.com/launch-schedule/"
            };
            string missionName;
            string[] launchdate = new string[3];
            string manufacturer = "";
            string launchSite = "";
            string time;
            string missionDescription;
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
                    missionName = missionSplt[1].Trim();
                    WriteLine("\n{0} Mission Name: {1}",myindex, missionName);
                    WriteLine(" Launch Vehicle: " + vehicle);
                    string timeAndSite = IWmissionData.Text.ToString();
                    string objectDate = IWlaunchdate.Text.ToString().ToUpper();
                    WriteLine($"Date before processing: {objectDate}");
                    string missDesc = IWDescription.Text.ToString();
                    switch(missDesc)
                    {
                        case var read when missDesc.Contains("Read our"):
                            missionDescription = missDesc.Replace("Read our", "*");
                            break;
                        case var read when missDesc.Contains("See our"):
                            missionDescription = missDesc.Replace("See our", "*");
                            break;
                            default:
                            missionDescription = missDesc;
                            break;
                    }
                    if (missionDescription.Contains("*"))
                    {
                        missionDescription = missionDescription.Remove(missionDescription.IndexOf("*"));
                    }
                    
                    if (objectDate.Contains("/")) //if two days are in the date, example: Dec. 22/23, 2008
                    {
                        switch (objectDate)
                        {
                            case var febmar when objectDate.Contains("FEB") & objectDate.Contains("MAR"):
                                objectDate = objectDate.Remove(objectDate.IndexOf("/"), 8); 
                                break;
                            case var novdec when objectDate.Contains("NOV") & objectDate.Contains("DEC"):
                                objectDate = objectDate.Remove(objectDate.IndexOf("/"), 7); 
                                break;
                            case var janfeb when objectDate.Contains("JAN") & objectDate.Contains("FEB"):
                                objectDate = objectDate.Remove(objectDate.IndexOf("/"), 7); 
                                break;
                            default:
                                objectDate = objectDate.Remove(objectDate.IndexOf("/") -3, 3); //Removes the first Day date from the Date item 
                                break;
                        }

                        WriteLine("Launch Date after day removal: " + objectDate);  
                    }
                    if (objectDate.Contains("."))
                    {
                        if(missionName == "TJS 5")
                        {
                            objectDate = objectDate.Remove(objectDate.IndexOf("7") + 1,2);
                            WriteLine("After second . removal: " + objectDate);
                            objectDate = objectDate.Insert(objectDate.IndexOf("7") + 1, ",");
                            WriteLine("After , insertion: " + objectDate);
                        }
                        launchdate[0] = objectDate.Substring(objectDate.IndexOf(".")+ 2).Trim();
                        if (launchdate[0].Contains(","))
                        {
                            launchdate[0] = launchdate[0].Remove(launchdate[0].IndexOf(","));
                        }
                        if(launchdate[0].Length == 1)
                        {
                            launchdate[0].Insert(0,"0");
                        }
                        WriteLine("Launch Date after '.' removal: " + launchdate[0]);
                    }
                    else
                    {
                        if (objectDate.Contains(","))
                        {
                            launchdate[0] = objectDate.Substring(objectDate.IndexOf(",")- 2);
                            WriteLine("Index of ,: " + objectDate.IndexOf(","));
                            launchdate[0] = launchdate[0].Remove(launchdate[0].IndexOf(",")).Trim();
                            if (launchdate[0].Contains("/"))
                            {
                                launchdate[0] = launchdate[0].Remove(launchdate[0].IndexOf("/"),1).Trim();
                            }
                            WriteLine("Substring of date day without . but with ,: " + launchdate[0]);
                        }
                        else
                        {
                            if (objectDate.Contains(" "))
                            {
                                string[] timeSplt = objectDate.Split(' ',StringSplitOptions.RemoveEmptyEntries);
                                launchdate[0] = timeSplt[1];
                            }
                            else
                            {
                                launchdate[0] = prevMissionDate.AddDays(3).Day.ToString();
                            }
                             
                        }      
                    }
                    if (!objectDate.Contains(",")) //Adding year if it is missing
                    {
                        if(prevMissionDate.Month == 12)
                        {
                            WriteLine(prevMissionDate.Month);
                             if(prevMissionDate.Day !> 27)
                            {
                                WriteLine(prevMissionDate.Day);
                                launchdate[2] = prevMissionDate.Year.ToString();
                                prevDateUsed++;
                            }
                            else
                            {
                                launchdate[2] = (prevMissionDate.Year + 1).ToString();
                                prevDateUsed++;
                                
                            }
                        }
                        else
                        {
                            launchdate[2] = prevMissionDate.Year.ToString();
                            prevDateUsed++;
                            /*string dateInDiv = IWmissionYear.Text.ToString();//convert to string for separation         
                            String[] yearSplit = dateInDiv.Split(',',StringSplitOptions.RemoveEmptyEntries);
                            yearText = yearSplit[1];*/
                        }
                        
                        WriteLine("Launch Year added: " + launchdate[2]);  
                    }
                    if (objectDate.Contains(","))//getting year from date
                    {
                        string[] launchYearSplit = objectDate.Split(",",StringSplitOptions.RemoveEmptyEntries);
                        WriteLine("launchyear split: " + launchYearSplit[1].Trim());
                        launchdate[2] = launchYearSplit[1].Trim();
                      WriteLine("Launch year trimmed: " + launchdate[2]);
                    }
                    switch (objectDate)
                    {
                        case var jan when objectDate.Contains("JAN"):
                            launchdate[1] = "01";
                            break;
                        case var feb when objectDate.Contains("FEB"):
                            launchdate[1] = "02";
                            break;
                        case var mar when objectDate.Contains("MAR"):
                            launchdate[1] = "03";
                            break;
                        case var apr when objectDate.Contains("APR"):
                            launchdate[1] = "04";
                            break;
                        case var may when objectDate.Contains("MAY"):
                            launchdate[1] = "05";
                            break;
                        case var jun when objectDate.Contains("JUN"):
                            launchdate[1] = "06";
                            break;
                        case var jul when objectDate.Contains("JUL"):
                            launchdate[1] = "07";
                            break;
                        case var aug when objectDate.Contains("AUG"):
                            launchdate[1] = "08";
                            break;
                        case var sep when objectDate.Contains("SEP"):
                            launchdate[1] = "09";
                            break;
                        case var oct when objectDate.Contains("OCT"):
                            launchdate[1] = "10";
                            break;
                        case var nov when objectDate.Contains("NOV"):
                            launchdate[1] = "11";
                            break;
                        case var dec when objectDate.Contains("DEC"):
                            launchdate[1] = "12";
                            break;
                        default:
                            launchdate[0] = prevMissionDate.AddDays(3).Day.ToString();
                            if (prevMissionDate.Month == prevMissionDate.AddDays(3).Month)
                            {
                                launchdate[1] = prevMissionDate.Month.ToString();
                            }
                            else
                            {
                                launchdate[1] = prevMissionDate.AddMonths(1).Month.ToString();
                            }
                            prevDateUsed++;
                            break;
                    }

                    if(timeAndSite.Contains("\n")) //edge case on page 2012-2014 where launchsite doesn't go to a new line, in else statement find closing bracket after time to separate
                    {
                        String[] timeSiteSplt = timeAndSite.Split("\n" ,StringSplitOptions.None); //separates the launch time from the launch site
                        time = timeSiteSplt[0].Remove(timeSiteSplt[0].IndexOf("L"),12).Trim(); //removes the string "Launch Time: " from the time and site data and also any spaces at the ends of the data
                        launchSite = timeSiteSplt[1].Remove(timeSiteSplt[1].IndexOf("L"),12).Trim();
                        WriteLine("Date and Time: {0} {1}",objectDate,time);
                        WriteLine("Launch Site: " + launchSite);  
                    }
                    else
                    {
                        int separator = timeAndSite.IndexOf(")") + 1;// finds the space after the closing bracket of the time property
                        String[] timeSiteSplt = timeAndSite.Split(timeAndSite[separator] ,StringSplitOptions.None); //separates every string by spaces
                        time = string.Format("{0} {1} {2}{3} {4}",timeSiteSplt[2], timeSiteSplt[3], timeSiteSplt[4], timeSiteSplt[5], timeSiteSplt[6]); //concatenates the strings needed
                        launchSite = string.Format("{0} {1} {2} {3} {4} {5} {6}",timeSiteSplt[9], timeSiteSplt[10], timeSiteSplt[11], timeSiteSplt[12], timeSiteSplt[13], timeSiteSplt[14], timeSiteSplt[15]);
                        WriteLine("Date and Time: {0} {1}",objectDate,time); 
                        WriteLine("Launch Site: " + launchSite); 
                    
                    }
                    
                    string fulldate = String.Concat($"{launchdate[0]}/{launchdate[1]}/{launchdate[2]}");
                    WriteLine($"Full date: {fulldate}");
                    DateTime date = DateTime.Parse(fulldate).Date;
                    bool isReusable;
                    //Move most used rockets to top of statment to improve efficency
                    switch(vehicle) 
                    {
                        case  var fal when vehicle.Contains("Falcon")://SpaceX rockets
                         case  var sta when vehicle.Contains("Starship"):
                        manufacturer = "SpaceX";
                        break;

                        case var fal when vehicle.Contains("Long March")://Chinese rockets
                         case var sta when vehicle.Contains("CZ"):
                          case var jie when vehicle.Contains("Jielong"):
                           case var hyp when vehicle.Contains("Hyperbola"):
                            case var osx when vehicle.Contains("OS-"):
                             case var kt when vehicle.Contains("KT"):
                              case var cer when vehicle.Contains("Ceres"):
                               case var xhu when vehicle.Contains("Zhuque"):
                                case var kua when vehicle.Contains("Kuaizhou"):
                        manufacturer = "Chinese Space Agency";
                        break;

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
                        
                        case  var tau when vehicle.Contains("Taurus")://Northrop rockets
                         case  var min when vehicle.Contains("Minotaur"):
                          case  var peg when vehicle.Contains("Pegasus"):
                           case  var shu when vehicle.Contains("Shuttle"):
                            case  var ant when vehicle.Contains("Antares"):
                        manufacturer = "Northrop Grumman";
                        break;

                        case  var ttn when vehicle.Contains("Titan")://Glenn L rocket
                        manufacturer = "Glenn L. Martin Company";
                        break;
                        
                        case  var air when vehicle.Contains("Ariane")://ESA rockets
                         case  var veg when vehicle.Contains("Vega"):
                        manufacturer = "European Space Agency";
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

                        case  var h2 when vehicle.Contains("H-2")://Japanese rockets
                         case  var f20 when vehicle.Contains("520"):
                          case  var t10 when vehicle.Contains("310"):
                           case  var m5 when vehicle.Contains("M-5"):
                            case  var eps when vehicle.Contains("Epsilon"):
                            manufacturer = "Japan Aerospace Exploration Agency";
                            break;

                        case  var lau when vehicle.Contains("LauncherOne")://Virgin Orbit rockets
                         case  var spac when vehicle.Contains("SpaceShip"):
                          case  var cos when vehicle.Contains("Cosmic"):
                        manufacturer = "Virgin Orbit";
                        break;

                        case  var gsl when vehicle.Contains("GSLV")://Indian rockets
                         case  var psl when vehicle.Contains("PSLV"):
                          case  var ssl when vehicle.Contains("SSLV"):
                        manufacturer = "Indian Space Research Organisation";
                        break;
                        
                        case  var roc when vehicle.Contains("Rocket 3")://Astra rocket
                        manufacturer = "Astra";
                        break;

                          case  var uhn when vehicle.Contains("Unha")://North Korean rocket
                        manufacturer = "North Korea";
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
                    Mission m = new Mission(missionName,date.Date,time,launchSite,vehicle,missionDescription);
                   if (!missionList.Exists(x => x.missionName == missionName))
                    {
                        using (StreamReader missionReader = new StreamReader("../missions.json"))
                        {
                            string missionJson = missionReader.ReadToEnd();
                            jsonMissions = JsonConvert.DeserializeObject<List<Mission>>(missionJson);
                        }
                        if(jsonMissions == null)
                        {
                             missionList.Add(m);
                            missionIndex++;
                        }
                        else
                        {
                             if( !jsonMissions.Exists(x => x.missionName == missionName))
                            {
                                missionList.Add(m);
                                missionIndex++;
                            }
                        }
                       
                    }

                    if (!rocketList.Exists(x => x.name == vehicle))
                    {
                        using (StreamReader reader = new StreamReader("../rockets.json"))
                        {
                            string myJson = reader.ReadToEnd();
                            jsonRockets = JsonConvert.DeserializeObject<List<Rocket>>(myJson);
                        }
                        if(jsonRockets == null)
                        {
                            rocketList.Add(r);
                            rocketIndex++;
                        }
                        else
                        {
                            if( !jsonRockets.Exists(x => x.name == vehicle))
                            {
                                rocketList.Add(r);
                                rocketIndex++;
                            }
                        }
                        
                    }
                    else
                    {
                        WriteLine("Duplicate rocket found");
                        duplicate++;
                    }
                    prevMissionDate = date;
                    myindex++;
                }
                driver.Quit();
            }
             foreach (var obj in rocketList)
                {
                    string jsonObj = JsonConvert.SerializeObject(obj);
                    WriteLine(" Object in rocketList: " + jsonObj);
                }
               
                WriteLine("\n\n\n\nList of unique missions"); 
            missionIndex = 0;
            foreach (var missObj in missionList)
            {
               // WriteLine($"\n{rocketIndex} \n Name: {rocket.name}\n Manufacturer: {rocket.manufacturer}\nReusable: {rocket.reusable}");
               string missionJSON = JsonConvert.SerializeObject(missObj,Formatting.Indented);
               WriteLine(missionJSON);
                missionIndex++;
            }
           /* WriteLine("\n\n\n\nList of unique rockets"); 
            rocketIndex = 0;
            foreach (Rocket rocket in rocketList)
            {
               // WriteLine($"\n{rocketIndex} \n Name: {rocket.name}\n Manufacturer: {rocket.manufacturer}\nReusable: {rocket.reusable}");
               JsonConvert.SerializeObject(rocket,Formatting.Indented);
                rocketIndex++;
            }*/ 
            WriteLine("Number of unique rockets: " + rocketIndex);
            WriteLine("Number of unique missions: " + missionIndex);
            WriteLine($"Number of duplicate rockets on site: {duplicate}");
            WriteLine($"Number of times prevMissionDate was used: {prevDateUsed}");
             var rjson = JsonConvert.SerializeObject(rocketList.ToArray(), Formatting.Indented);
                    //write string to file
                    System.IO.File.WriteAllText("../rockets.json", rjson);
             var mjson = JsonConvert.SerializeObject(missionList.ToArray(), Formatting.Indented);
                    //write string to file
                    System.IO.File.WriteAllText("../missions.json", mjson);

        }
       
    }
public class Rocket
{

    public string manufacturer { get; set; }
    public string name { get; set; }
    public bool reusable { get; set; }
    public Rocket(string _name, string _manufacturer,bool _isreusable)
    {
        name = _name;
        manufacturer = _manufacturer;
        reusable = _isreusable;
    }
}
public class Mission
{

    public string missionName { get; set; }
    public DateTime launchDate { get; set; }
    public string launchTime {get; set;}
    public string launchSite { get; set; }
    public string rocket { get; set; }
    public string missionDescription { get; set; }
    public Mission(string _missionName, DateTime _fullDate,string _time, string _launchSite,string _rocket, string _description)
    {
        missionName = _missionName;
        launchDate = _fullDate.Date;
        launchTime = _time;
        launchSite = _launchSite;
        rocket = _rocket;
        missionDescription = _description;
    }
}
