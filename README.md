# Real-Time Task Management App (Backend: .NET Minimal API + SignalR, Frontend: React + Vite)

Aplicação simples de gerenciamento de tarefas com atualização em tempo real via SignalR e um resumo por IA das tarefas.

## Stack
- Backend: .NET 9, Minimal APIs, Entity Framework Core (InMemory), SignalR, CORS
- Frontend: React + Vite + TypeScript, @microsoft/signalr
- AI: OpenAI (opcional) com fallback de resumo mock

## Estrutura
```
TallerTest/
├─ TaskApi/            # Backend .NET Minimal API
├─ frontend/           # Frontend React + Vite (TS)
└─ TaskApp.sln         # Solution
```

## Como rodar
1) Backend
```bash
cd TaskApi
# opcional: exporte sua chave para resumo por IA real
# Windows PowerShell
# $env:OPENAI_API_KEY = "sua_chave"
# CMD
# set OPENAI_API_KEY=sua_chave

# Executar
dotnet run
# Sobe em http://localhost:5000
```

2) Frontend
```bash
cd frontend
npm install
npm run dev
# Normalmente em http://localhost:5173
```

## Endpoints Backend
- GET `/api/tasks` → Lista tarefas
- POST `/api/tasks` → Cria tarefa
  - Body JSON: `{ "title": string, "description"?: string }`
- POST `/api/summarize` → Retorna `{ summary }` (usa OpenAI se `OPENAI_API_KEY` setado; caso contrário, resumo mock)
- SignalR Hub: `/hubs/tasks` (evento `TaskAdded` broadcast após criação)

## Configurações importantes
- CORS permite `http://localhost:5173` e variantes locais
- URL do backend fixada: `http://localhost:5000`
- O frontend aponta para `http://localhost:5000` por padrão

## Resumo por IA
- Configure `OPENAI_API_KEY` no ambiente para usar OpenAI (modelo `gpt-3.5-turbo`)
- Sem chave ou em caso de erro, o backend retorna um resumo mock curto

## Dicas e Solução de Problemas
- Tela branca no frontend: abra o Console do navegador (F12) e verifique erros
- JSX/TSX: garantido via `tsconfig.json` com `jsx: react-jsx`
- SignalR não conecta:
  - Verifique que o backend está rodando em `http://localhost:5000`
  - Verifique a chamada `GET /hubs/tasks/negotiate` no Network (deve retornar 200)
  - Em desenvolvimento, desabilitamos o React StrictMode para evitar duplo start do HubConnection
- CORS: confirme que o frontend acessa via `http://localhost:5173` (não https)

## Scripts úteis
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

## Licença
Livre para uso educacional/demonstração.


