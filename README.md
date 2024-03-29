# README

## Introduction
As part of one of my 3rd year modules we had to develop a Universal Windows App. I developed an app called Commute.
Commute is a real time app for Bus Eirrean services in the Co. Galway area. It provides real time departure information and
bus stop locations using a an interactive map.

## Project Spec Guidelines

- Well designed UI that is fit for purpose and provides a good user experience.
- Uses local/roaming storage for storing data and/or settings that are necessary for or enhance this user experience.
- Demonstrates appropriate use of the sensors/hardware available on UWP capable devices, i.e Accelerometer, gyroscope, location services,
sound, network service (connect to server for data), camera, multi touch gestures
- The app must be more than a simple information app.  It must have interactivity as part of the design.

## Running Commute

To run Commute you can either download the app throught the Microsoft App Store
Or alternatively you can clone the git repository. To complete the next step GIT is also required on you PC. If you don't have GIT installed go to the link below and download and install GIT

    https://git-scm.com/

Once both has been installed you are ready to clone the go application
To do this open a console window and navigate to the folder you which to clone the application into.
Type the following command into the console window to clone the repository
```sh
$ git clone https://github.com/kevind992/Universal-Windows-App-Project.git
```  
Once the repository has been cloned open Visual Studio and open the open the solution. Depending on the version of visual studio you are running, you may need to rebuild the solution. 

To run Commute press the run Local Machine button on the top middle of the Visual Studio screen. 

Once the applcation is up and running you will be prompted to select two bus stops in the Co. Galway area and click the submit button.
You will be redirected to a new page which will show you three pivots. The first pivot "Bus Times", will show you information on the two bus stops selected. The second pivot "Nearby" will show you a map with your location and all the Galway stops. When you select the stop a windows pops up showing you the information for the selected stop. The final pivot is "Settings". You are able to change your two selected stops and turn location on and off for the application.
## Research and Development
Before I attempted to code out the application I researched to see with there were publicly available API's which I could use to get live information Irish bus information. I found that this was available throught the data.gov.ie website. 
### API Call Example
https://data.smartdublin.ie/cgi-bin/rtpi/busstopinformation?stopid=184&format=json

![capture](https://user-images.githubusercontent.com/31921534/38560180-f3b2a688-3ccc-11e8-9ee2-eed43c1c1d72.PNG)

I decided I wanted to create a two page application. One page is for setting up the application and the second page would be the main page. It contains 3 pivots. The first pivot contains information on the two selected bus stops. The second pivot contains a map and shows all the  bus stops in the Co. Galway area. The main reason I wanted to include a map was to show all the bus stops and the user location. I created two icons using Gimp 2 and assigned one to the user location and the other icon to all the bus stops. 
The third pivit contains all the settings. The user has an option to change selected stops and turn off location services. I tried to keep the UI as close as I could to the UI on windows settings.
## UWP Services Used
- Microsoft Azure Service. This is used for the Tile Noticiation. The applcation sends the latitude and longtude of the user to the Azure service. The service then calculates which bus stop is nearest to the user and sends this back to the main application which then is displayed as a tile notification.
- Map Services. This is used for displaying a map and icons. 
- HttpClient Services. This was used for making get requests to a restful API (Link below). Without this I wouldn't have been able to get all the bus stop information. 
Code snippet for a Http get request:
```
// Url which is used for making the get request
string url = "http://data.dublinked.ie/cgi-bin/rtpi/realtimebusinformation?stopid=" + stop + "&format=json";
//Creating a Http client
HttpClient client = new HttpClient();
// Making the request and putting the response into responce
string response = await client.GetStringAsync(url);
// Parsing the data using NewtonJson package
var data = JsonConvert.DeserializeObject<RootBusStopTimeObject>(response);
```
- Location Services. This was used to get the users location. By getting the users location is was able to plot there location on the map with an icon. I was also able to use there location to calculate which bus stop they were closest too.
Code snippet for getting user location:
```
// Creata a new geolocator with a desired accuracy of what ever is possible
Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 0 };
geolocator.StatusChanged += Geolocator_StatusChanged;
// Getting position of user
Geoposition pos = await geolocator.GetGeopositionAsync();

// Creating a Basic Geoposition
BasicGeoposition snPosition = new BasicGeoposition();

// setting snPosition latitude and longitude with the pos coodinates of the user
snPosition.Latitude = (float)pos.Coordinate.Point.Position.Latitude;
snPosition.Longitude = (float)pos.Coordinate.Point.Position.Longitude;
```
- Local Storage. I used local storage to store the users two selected bus stops. I also was able to store certain application variables to stop re-access to certain pages after submission and also to improve user expierence.
Code snippet for Local Storage:
```
// Getting local settings
ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

//If localSettings - IsFirstTime is null, i.e first time app has been run
if (localSettings.Values["IsFirstTime"] == null)
{
    // set to true
    localSettings.Values["IsFirstTime"] = true;
}
//if contains a bool, i.e. not null
if ((bool)localSettings.Values["IsFirstTime"])
{
    // Set to false
    localSettings.Values["IsFirstTime"] = false;
    // Navigate to MainPage
    this.Frame.Navigate(typeof(MainPage));
}
```
## Ideas for Future Development
For future development,
- I would like to improve the tile notification service. I would like to set a system that sends the user a notification when there next bus is due. The user could specify times in the day that they wish to recieve notification ie. if the user commutes in the morning at 9 times and returns home around 5, the user would only recieve notifications around that time.
- I would like to add more modes of transports, ie Train, Tram and other bus companies. I would also like to add more cities available. The user would be able to select what city and what more of transport they commute with.
- I would also like to improve the first time user page. I would like to make the page more visually pleasing.
## Technolegy Used
For developing this application, I did all my coding using Visual Studio 2017.
For my .net service application, I hosted it on Azure and coded on Visual Studio 2017.
For my Logo's and Map Icon's, I used Gimp 2.
## Known Bugs
There may be a bug in the application if the PC you are running does not have GPS. May cause application to crash. I am currently working on a fix for this issue.
## References
- https://data.gov.ie/dataset/real-time-passenger-information-rtpi-for-dublin-bus-bus-eireann-luas-and-irish-rail
- https://docs.microsoft.com/en-us/windows/uwp/design/basics/navigate-between-two-pages
- https://stackoverflow.com/questions/37685214/show-page-only-once-when-app-is-installed-in-uwp
- https://docs.microsoft.com/en-us/windows/uwp/maps-and-location/display-poi
- https://www.youtube.com/watch?v=UMQ2JVOE_xE
- https://channel9.msdn.com/Series/Windows-10-development-for-absolute-beginners/UWP-061-UWP-Weather-Updating-the-Tile-with-Periodic-Notifications
- https://docs.microsoft.com/en-us/windows/uwp/networking/httpclient
