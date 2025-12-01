# TestProject

Small ASP.NET Core Web API playground for school assignments. Use it to implement and validate authentication/login stories before moving the code into a larger production solution.

## Quick Start

```bash
dotnet build
dotnet run --project MyWebApi
```

The API hosts a simple authentication controller:
- `POST /api/authentication/register` – create a user (file-based store) and get a bearer token.
- `POST /api/authentication/login` – verify credentials and receive a token.

Update `appsettings.json` or `appsettings.Development.json` to tweak JWT issuer/audience/secrets while experimenting.

## Frontend (dev/test)

```
cd Frontend
npm install      # first time only
npm start        # launches React dev server at http://localhost:3000
npm test         # optional: run UI tests if/when added
```

Run the backend (`dotnet run --project MyWebApi`) in parallel so the UI can hit the API. Adjust API URLs in the frontend as needed once wiring is added.

## Study Project Links

This playground lives inside our Software Project Management (Scrum) study project, so keep the following references nearby:

- API for data ingestion – https://apispm.eppoi.io/swagger/index.html (used by the classroom exercises).
- Google Classroom – https://classroom.google.com/c/ODA5NzkwMTMwMzUx for announcements and sprint reviews.
- GitHub backlog – https://github.com/suderco/SPM hosts the user stories/issues we estimate and pull into each sprint.
- ZenHub workspace – open https://app.zenhub.com/ with the GitHub repo above to view the Kanban board and burndown chart we report on in class.
