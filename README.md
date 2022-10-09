Create an administrative panel with static login form (login/password).
After signing in the user should get access to list of users from https://dummyapi.io/data/v1/user. When clicking on user from the list their posts are getting available (from https://dummyapi.io/data/v1/post).
All API's data must be cached on the client side. Data refreshing requests must be sent not often then once per 5 minutes. In case if new data is requested (for instance, list of user's posts that wasn't requested before) direct API calls are allowed.

Technologies:
Blazor.Server, ASP.NET Core, C#, .NET 6
