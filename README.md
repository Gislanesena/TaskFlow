# TaskFlow

Sistema web para gerenciamento de tarefas, projetos e equipes.

## Visão geral

O TaskFlow foi pensado para permitir que usuários:

- criem contas e façam login;
- gerenciem projetos;
- criem, editem e acompanhem tarefas;
- atualizem o status por pendente, em andamento e concluída;
- acompanhem indicadores no dashboard;
- visualizem o histórico de alterações.

## Stack

- Backend: ASP.NET Core Web API
- Frontend: React + TypeScript
- Banco de dados: SQL Server / EF Core
- Autenticação: ASP.NET Core Identity + JWT

## Estrutura do projeto

- `TaskFlow.API/` — backend API em ASP.NET Core
- `taskflow.frontend/` — aplicação React + TypeScript

## Pré-requisitos

- .NET 10 SDK
- Node.js 20+
- npm
- SQL Server (opcional para ambiente local real)

## Executando o backend

```bash
cd TaskFlow.API
 dotnet restore
 dotnet run
```

A API expõe endpoints em `https://localhost:5001` e `http://localhost:5000` por padrão.

## Executando o frontend

```bash
cd taskflow.frontend
npm install
npm run dev
```

A aplicação fica disponível em `http://localhost:5173`.

## Usuário padrão de teste

- Email: `admin@taskflow.local`
- Senha: `Admin@123`

## Funcionalidades principais

- Cadastro e login
- Dashboard com indicadores
- Criação de projetos
- Criação e atualização de tarefas
- Controle de status
- Histórico de atividades
- Autorização por perfil

## Observação

Caso queira conectar a um SQL Server real, ajuste a string de conexão em `TaskFlow.API/appsettings.json`.
