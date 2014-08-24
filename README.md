PunchIn - Simple Time Tracker
=============================

Initially a very simple Time Tracker utility I created to reconnect with WPF, I now use this at work to track my time.

The idea is to have this app running in the system tray which provides a quick and easy way to start and stop a timer for the current task you are working.

##Usage
Download, Clone or Fork, open solution in Visual Studio, hit F5 and away you go.
**Note:** You'll probably need to update packages before you can build.

##How it works
Once it is running you will see a new icon in the system tray. Right click icon to display a menu with the following top level items:

* PunchIn
  - The action changes to PunchOut if you have a current timer running i.e. already Punched In.
* Work Items
  - A list of your previous work items. Work Items can have many tasks (PunchIn/PunchOut).
* Options
  - Bad naming here but it basically opens the main window.
* Shortcuts
  - Similar to the old favorites menu in windows explorer of old aka XP.
* Exit

###Main Window
Shows tracking activity detail, reports and user settings.

####Title Links

* Settings
  - Theme
    - Select theme and accent colour
  - File Locations
    - Shortcuts folder location
    - SharePoint list location and name
* Messages
  - Playing around with Lync ;)

####Tracker
Displays the list of work items and their individual time entries.

####Reports
View weekly reports from last week and this week. I will add user filtering when I get around to it.
Export weekly timesheet to SharePoint. If the list doesn't exist, we try to create it.

##Credit where credit is due
NotifyIcon for WPF from [hardcodet.net][4] by Philipp Sumi was used for all NotifyIcon activities.
I also wanted to play around with theming and give the app a modern windows look. Turns out there are plenty of projects out there doing this so I decided to learn from them. The UI project is basically a fork of an awesome project hosted on [CodePlex][2] called [ModernUI][1]...they are only starting out but go check it out anywayz. Another project I have taken ideas from is the good ol' [WPF Toolkit][3].
I didn't want to invest too much time learning how these toolkits did their thing so I simply "borrowed" what I needed and modified to fit which was all I needed to get started with templates and themes etc.

[1]:https://mui.codeplex.com/
[2]:https://www.codeplex.com/
[3]:https://wpftoolkit.codeplex.com/
[4]:http://www.hardcodet.net