Have you ever thought of an idea that navigation in Windows could be much more convenient? Why wouldn't one employ the technique similar to the "Navigate to class" in most common IDEs, when relevant folder matches are displayed in the popup window, brought to the foreground by a predefined key combination, like that:

![https://navigation-assistant.googlecode.com/svn/wiki/Images/MatchesList.png](https://navigation-assistant.googlecode.com/svn/wiki/Images/MatchesList.png)

# Why needed at all #
Here are Windows Explorer and Total Commander disadvantages which may frustrate most of the users:

  1. one needs to perform a huge amount of mouse clicks to access a deeply nested folder
  1. search in Windows Explorer is rather slow, even with indexing turned on
  1. low usability of Windows Explorer search: need to open an Explorer, eyefocus on the search field (not centered on the screen), click it, type a (preferrably complete) folder name, and wait. If you've made a mistake in the folder name, need to repeat it all over.
  1. low usability of address bar hints in Windows Explorer and Total Commander (subfolder names which appear after pressing Tab in the address bar). You need to type the full path anyway and press the Tab button for each subfolder, and search for the correct hint for each subfolder. And if you are expecting a wrong folder path (e.g. "Program Files (x32)" instead of "Program Files") you need to perform all these actions again.

For these particular reasons I've written "Navigation Assistant", whose operation is equivalent to "Navigate to class" or "Navigate to File" features of most common IDEs: combinations Ctrl-N and Ctrl-Shift-N in JetBrains products (ReSharper, IDEA, PhpStorm, WebStorm), Ctrl-Shift-T in Eclipse.

# Why it's so good #
Besides eliminating the aforementioned disadvantages the following benefits are obtained:
  1. no need to know the exact folder path
  1. no need to know the exact folder name
  1. instant search results preview at each keypress (so you can quickly amend the search query)
  1. less typing, navigation speed, great usability

Better folder structure comes as a free bonus with the usage of Navigation Assistant. Indeed, many people (especially developers familiar with a brilliant Code Complete book) are aware of a [rule of 7+-2](http://en.wikipedia.org/wiki/The_Magical_Number_Seven,_Plus_or_Minus_Two) (the most efficient number of elements in the brain active memory). Therefore, it's a good strategy to restrict the number of subfolders in a given folder up to 7.

Unfortunately, such an approach would be utilized at a cost of inconvenient folder navigation. Navigation Assistant will let you forget about concerning this disadvantage.

# How to use it #

To navigate quickly to the needed folder simply press a predefined key combination (Ctrl-Shift-M by default) in a Windows Explorer or Total Commander application (or at any other application, which will imply a new Windows Explorer instantiation for navigation). The main application window will appear:

![https://navigation-assistant.googlecode.com/svn/wiki/Images/NavigationWindow.png](https://navigation-assistant.googlecode.com/svn/wiki/Images/NavigationWindow.png)

Start typing a destination folder name.

**Killer feature**: it's not necessary to type a complete folder name; e.g. it's sufficient to type "documents and" to reach "Documents and Settings". Moreover, it's not even necessary to type the word "documents" completely, as "doc and" will also suit (best wishes, JetBrains!); also, it's not mandatory to type folder name from the start, "and settings" will be sufficient. Pascal/camel case in folder names is also recognized for developers' sake: search query "nav assist" will match the folder "NavigationAssistant".

A list of folder matches will appear due to these manipulations:

![https://navigation-assistant.googlecode.com/svn/wiki/Images/MatchesList.png](https://navigation-assistant.googlecode.com/svn/wiki/Images/MatchesList.png)

All you have to do is to click a suitable element with mouse or use Up/Down keys to select a required path and press Enter.

If you are dissatisfied for some reason with the navigation window you can quickly minimize it to tray with the Escape button.

If you bring the program to front outside Windows Explorer or Total Commander, a new Window Explorer application will be launched on selecting a folder match (with the needed folder already open); it's possible to change the default navigator (e.g. use Total Commander) in settings.

Just fast folder navigation is currently supported, no files navigation is implemented.

# What else in there #
There is a cute settings window, supporting the following customizations:
  1. list of programs with enhanced navigation support (e.g. turn off Total Commander support)
  1. default application for navigation
  1. key combination to bring Navigation Assistant to front
  1. restrict search to certain folders (e.g. to only C:\Users\ or D:\)
  1. exclude certain folder names from matching, e.g. bin, obj, .svn; more precisely, any path containing the “bin” folder will be excluded; each entry is actually a regular expression, so you may even specify something like bin\S`*`
  1. enable launching on Windows start up (recommended, as it would eliminate the necessity to re-parse folder system at the launch)

Each option is enhanced with a tooltip. To reach the settings one just needs to right-click a tray icon.

Best Wishes and Happy Exploring!
