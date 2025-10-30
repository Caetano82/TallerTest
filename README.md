# Real-Time Task Management App (Backend: .NET Minimal API + SignalR, Frontend: React + Vite)

A simple task manager with real-time updates via SignalR and AI-powered task summarization.

## Stack
- Backend: .NET 9, Minimal APIs, Entity Framework Core (InMemory), SignalR, CORS
- Frontend: React + Vite + TypeScript, @microsoft/signalr
- AI: OpenAI (optional) with mock fallback

## Structure
```
TallerTest/
├─ TaskApi/            # .NET Minimal API backend
├─ frontend/           # React + Vite (TS) frontend
└─ TaskApp.sln         # Solution
```

## Run locally
1) Backend
```bash
cd TaskApi
# optional: enable real AI summary
# PowerShell
# $env:OPENAI_API_KEY = "your_key"
# CMD
# set OPENAI_API_KEY=your_key

dotnet run
# runs at http://localhost:5000
```

2) Frontend
```bash
cd frontend
npm install
npm run dev
# typically at http://localhost:5173
```

## Backend endpoints
- GET `/api/tasks` → list tasks
- POST `/api/tasks` → create task
  - JSON body: `{ "title": string, "description"?: string }`
- POST `/api/summarize` → returns `{ summary }` (uses OpenAI if `OPENAI_API_KEY` is set; otherwise mock)
- SignalR Hub: `/hubs/tasks` (broadcasts `TaskAdded` after creation)

## Important settings
- CORS allows `http://localhost:5173` and local variants
- Backend URL is fixed to `http://localhost:5000`
- Frontend calls the backend at `http://localhost:5000` by default

## AI summary
- Set `OPENAI_API_KEY` to use OpenAI (model `gpt-3.5-turbo`)
- Without a key, the backend returns a short mock summary

## Troubleshooting
- Blank page: open browser Console (F12) and check errors
- JSX/TSX: enabled via `tsconfig.json` (`jsx: react-jsx`)
- SignalR not connecting:
  - Ensure backend is running at `http://localhost:5000`
  - Check `GET /hubs/tasks/negotiate` in Network tab (should be 200)
  - In development, React StrictMode is disabled to avoid double-start of HubConnection
- CORS: ensure the frontend is served from `http://localhost:5173` (not https)

## Useful scripts
- Backend
```bash
cd TaskApi
 dotnet build
 dotnet run
```
- Frontend
```bash
cd frontend
 npm run dev
 npm run build
 npm run preview
```

## License
Free for educational/demo use.


