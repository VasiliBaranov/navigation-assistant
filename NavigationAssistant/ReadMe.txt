Architectural decisions:

//////////////////////
1. General architecture
It's a standard layered architecture, common in Domain Driven Design
(see, e.g. Domain Driven Design Quickly Online by Avram and Marinescu
http://www.infoq.com/minibooks/domain-driven-design-quickly;
Patterns of Enterprise Architecture by Fowler).
UI layer is implemented with MVP pattern.
1. UI layer
	1. Views
2. Presenters layer
	1. Presenters
	2. Presentation Services
	3. View Model Mappers
	4. View Models
3. Domain Model aka Business Layer
	1. Application Services
	2. Domain Data Objects (usually plain Data Transfer Objects)
4. No Data Access Layer :-)

Unit tests are implemented with nunit and moq.

//////////////////////
2. UI

I tried to use Model-View-ViewModel pattern at first (common to WPF), but then switched to MVP for the following reasons:
1. MVP adds another layer of abstraction (presenters); otherwise, a huge amount of logic would go to View Models and
2. this logic is not conceptually belonging to view models (see single responsibility principle)
3. view models become too heavy
4. not possible to use interface-driven development for views, consequently
5. UI layer becomes a mess (no separation of concerns, modularity, clear interfaces)

Also, Tray Icon can not be implemented through MVVM (as is based on WinForms code).

So currently I'm using MVP, but bind data to UI through view models:
1. either pass view models to IView interface (methods or properties) (see INavigationView and NavigationWindow)
2. instantiate view models in views internally (see ISettingsView and SettingsWindow)
