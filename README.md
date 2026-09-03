# 🏛️ Sistema de Agendamento de Salões

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white)
![Cloudflare](https://img.shields.io/badge/Cloudflare_Tunnel-F38020?style=for-the-badge&logo=cloudflare&logoColor=white)
![CI/CD Pipeline](https://img.shields.io/badge/GitHub_Actions-CI%2FCD-2088FF?style=for-the-badge&logo=github-actions&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

Sistema web fullstack moderno, performático e desacoplado para gestão e agendamento de locais e salões de reuniões, com validações em tempo real, bloqueio inteligente de datas ocupadas, persistência em PostgreSQL e disparo automatizado de notificações por e-mail (SMTP) para o Administrador e Solicitante.

---

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura e Padrões de Engenharia](#-arquitetura-e-padrões-de-engenharia)
- [Regras de Negócio](#-regras-de-negócio)
- [Estrutura do Repositório](#-estrutura-do-repositório)
- [Endpoints da API](#-endpoints-da-api)
- [Tecnologias Utilizadas](#-tecnologias-utilizadas)
- [Instalação e Execução Local](#-instalação-e-execução-local)
- [Testes Automatizados](#-testes-automatizados)
- [Pipeline de CI/CD](#-pipeline-de-cicd)
- [Licença](#-licença)

---

## 🎯 Visão Geral

O projeto foi concebido para resolver de forma elegante o conflito de reservas entre congregações em salões compartilhados:
1. **Interface Reativa:** Carregamento instantâneo via HTML5, Tailwind CSS e Flatpickr.
2. **Separação de Responsabilidades:** Frontend desacoplado servido estaticamente pelo backend C# ASP.NET Core Minimal API.
3. **Persistência Segura:** PostgreSQL estruturado com índices e restrições únicas para prevenir *double booking* em nível de banco de dados.
4. **Comprovante Automático:** Disparo assíncrono de comprovante em HTML estilizado para o e-mail do solicitante e notificação para a administração.

---

## 🏗️ Arquitetura e Padrões de Engenharia

O projeto adota princípios rigorosos de **Clean Architecture**, **SOLID**, **KISS** e **DRY**:

```
                                  ┌───────────────────────────────┐
                                  │      Cloudflare Tunnel        │
                                  │    (HTTPS Edge Encryption)    │
                                  └──────────────┬────────────────┘
                                                 │
                                                 ▼
┌─────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                       AgendamentoApp (.NET 10)                                  │
│                                                                                                 │
│  ┌─────────────────────────┐               ┌─────────────────────────────────────────────────┐  │
│  │   wwwroot (Frontend)    │               │            Presentation / API Endpoints         │  │
│  │  - index.html           │ ──(HTTP/JSON)─► - GET  /api/agendamentos/ocupados               │  │
│  │  - css/style.css        │               │ - POST /api/agendamentos                        │  │
│  │  - js/app.js            │               │ - GET  /health                                  │  │
│  └─────────────────────────┘               └────────────────────────┬────────────────────────┘  │
│                                                                     │                           │
│                                            ┌────────────────────────┴────────────────────────┐  │
│                                            │               Services / Business Logic         │  │
│                                            │  - DatabaseService (Npgsql / Connection Pool)   │  │
│                                            │  - EmailService (MailKit / Async Worker)        │  │
│                                            └──────────────┬──────────────────┬───────────────┘  │
└───────────────────────────────────────────────────────────┼──────────────────┼──────────────────┘
                                                            │                  │
                                                            ▼                  ▼
                                                  ┌──────────────────┐  ┌─────────────┐
                                                  │ PostgreSQL 16+   │  │ SMTP Server │
                                                  │ (agendamentos_db)│  │ (Gmail TLS) │
                                                  └──────────────────┘  └─────────────┘
```

---

## ⚖️ Regras de Negócio

| Salão / Local | Dias Permitidos | Comportamento no Calendário |
|---|---|---|
| **Salão Nova Lima** | Segunda a Sexta-feira | Sábados e Domingos são desabilitados automaticamente. |
| **Salão Marques Herval** | Apenas Segunda-feira | Terça a Domingo são desabilitados automaticamente. |

* **Bloqueio Dinâmico:** Datas que já possuem agendamento confirmado no banco são marcadas como indisponíveis.
* **Categorias Permitidas:** `Confidencial`, `Ancião`, `Treinamento` e `Troca / Visita`.
* **Idempotência:** A restrição `UNIQUE (salao, data_reserva)` impede qualquer colisão de concorrência.

---

## 📂 Estrutura do Repositório

```text
agendamento_saloes/
├── .editorconfig                # Padronização de formatação de código (C#, JS, CSS, JSON)
├── .gitignore                   # Arquivos ignorados pelo Git
├── README.md                    # Documentação do projeto
├── SaaS.sln                     # Solution contendo os projetos de App e Testes
│
├── src/
│   └── AgendamentoApp/          # Projeto Principal C# ASP.NET Core
│       ├── AgendamentoApp.csproj
│       ├── Program.cs           # Inicialização e Endpoints Minimal API
│       ├── appsettings.json     # Configurações de Banco e SMTP
│       ├── Models/
│       │   └── Agendamento.cs   # Entidade e DTO com JsonPropertyName
│       ├── Services/
│       │   ├── DatabaseService.cs # Acesso ao banco com Npgsql
│       │   └── EmailService.cs    # Montagem e envio de e-mails via MailKit
│       └── wwwroot/             # Frontend Desacoplado
│           ├── index.html       # Estrutura semântica
│           ├── css/
│           │   └── style.css    # Estilos isolados e tema customizado
│           └── js/
│               └── app.js       # Reatividade e integração com a API
│
├── tests/
│   ├── AgendamentoApp.UnitTests/        # Testes Unitários (xUnit + FluentAssertions)
│   └── AgendamentoApp.IntegrationTests/ # Testes de Integração (WebApplicationFactory)
│
└── .github/
    └── workflows/
        └── ci.yml               # Pipeline de CI (Build, Testes e Lint)
```

---

## 🔌 Endpoints da API

### 1. `GET /health`
Verifica a saúde do serviço.
* **Resposta `200 OK`:** `{"status": "healthy", "timestamp": "2026-09-02T22:00:00Z"}`

### 2. `GET /api/agendamentos/ocupados?salao={nome}`
Retorna a lista de datas indisponíveis para o salão solicitado.
* **Resposta `200 OK`:** `["2026-09-07", "2026-09-14", "2026-09-28"]`

### 3. `POST /api/agendamentos`
Registra uma nova reserva e dispara os e-mails de confirmação.
* **Payload:**
```json
{
  "salao": "Salão Nova Lima",
  "data_reserva": "2026-09-15",
  "congregacao": "Nova Lima",
  "categoria": "Ancião",
  "nome_solicitante": "João da Silva",
  "email_solicitante": "joao.silva@exemplo.com"
}
```
* **Respostas:**
  * `201 Created`: Reserva confirmada com sucesso.
  * `400 Bad Request`: Campos ausentes ou dia da semana inválido para o salão.
  * `409 Conflict`: Data já reservada para este salão.

---

## 🛠️ Tecnologias Utilizadas

* **Linguagem & Framework:** C# 12 / .NET 10 (ASP.NET Core Minimal API)
* **Banco de Dados:** PostgreSQL (Driver `Npgsql`)
* **Envio de E-mail:** `MailKit` & `MimeKit` via SMTP TLS (Porta 587)
* **Frontend:** HTML5, Tailwind CSS, Vanilla JavaScript e Flatpickr (l10n PT-BR)
* **Testes:** xUnit, FluentAssertions, Moq
* **Túnel Seguro & HTTPS:** Cloudflare Tunnel (`cloudflared`)

---

## 🚀 Instalação e Execução Local

### Pré-requisitos
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [PostgreSQL 14+](https://www.postgresql.org/)
* [Cloudflare CLI (`cloudflared`)](https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/)

### 1. Clonar o Repositório
```bash
git clone git@github.com:GeovaneParedes/agendamento_saloes.git
cd agendamento_saloes
```

### 2. Criar o Banco de Dados
```sql
CREATE DATABASE agendamentos_db;

\c agendamentos_db;

CREATE TABLE IF NOT EXISTS agendamentos (
    id SERIAL PRIMARY KEY,
    salao VARCHAR(50) NOT NULL,
    data_reserva DATE NOT NULL,
    congregacao VARCHAR(100) NOT NULL,
    categoria VARCHAR(50) NOT NULL,
    nome_solicitante VARCHAR(150),
    email_solicitante VARCHAR(150),
    criado_em TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT unique_salao_data UNIQUE (salao, data_reserva)
);
```

### 3. Configurar Credenciais
No arquivo `src/AgendamentoApp/appsettings.json`, configure a string de conexão e as credenciais de SMTP:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=agendamentos_db;Username=seu_usuario;Password=sua_senha;"
  },
  "ADMIN_EMAIL": "seu_email@dominio.com",
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "User": "seu_email@gmail.com",
    "Pass": "sua_senha_de_app_16_digitos"
  }
}
```

### 4. Executar a Aplicação
```bash
dotnet run --project src/AgendamentoApp --urls "http://localhost:5555"
```

### 5. Expor com HTTPS via Cloudflare Tunnel
Em outro terminal:
```bash
cloudflared tunnel --config /dev/null --url http://127.0.0.1:5555 --http-host-header localhost
```

---

## 🧪 Testes Automatizados

Executar todos os testes unitários e de integração:

```bash
dotnet test SaaS.sln --logger "console;verbosity=detailed"
```

---

## 🔄 Pipeline de CI/CD

O repositório conta com um workflow automatizado no **GitHub Actions** (`.github/workflows/ci.yml`) que executa a cada `push` ou `pull_request` para a branch `main`:
1. **Setup .NET 10**
2. **Restauração de dependências (`dotnet restore`)**
3. **Build em modo Release (`dotnet build -c Release`)**
4. **Execução de Testes Unitários e Integração (`dotnet test`)**
5. **Verificação de Formatação e Linter (`dotnet format --verify-no-changes`)**

---

## 📄 Licença

Distribuído sob a licença MIT. Veja `LICENSE` para mais informações.
