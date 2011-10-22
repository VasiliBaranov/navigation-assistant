Architectural decisions:

//////////////////////
1. General architecture
It's a standard layered architecture, common in Domain Driven Design
(see, e.g. Domain Driven Design Quickly Online by Avram and Marinescu
http://www.infoq.com/minibooks/domain-driven-design-quickly ;
Patterns of Enterprise Architecture by Fowler).
UI layers are implemented with MVP pattern.
1. UI (NavigationAssistant assembly)
	a. UI (Views and ViewModels)
	b. Presenters
	c. Presentation Services
2. Domain Model
	a. Application Services
	b. Domain Data Objects (usually plain Data Transfer Objects)
3. No Data Access Layer :-)

Unit tests are implemented with nunit and moq.

//////////////////////
2. UI

UI layers are implemented with MVP pattern (not MVVM, common to WPF) for the following reason
(actually, I switched to MVP after MVVM became insufficient):
1. MVP adds another layer of abstraction (presenters); otherwise, a huge amount of logic would go to View Models and
2. this logic is not conceptually belonging to view models
3. view models become too heavy
4. not possible to use interface-driven development for views, consequently
5. UI layer becomes a mess

Also, Tray Icon can not be implemented through MVVM (as is based on WinForms code).

So currently I'm using MVP, but bind data to UI through view models:
1. either pass view models to IView interface (methods or properties) (see INavigationView and NvaigationWindow)
2. instantiate view models in views internally (see ISettingsView and SettingsWindow)
