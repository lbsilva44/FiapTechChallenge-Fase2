
# Fiap.Cloud.Games

Plataforma de venda de jogos digitais com autenticação, aquisição e gerenciamento de biblioteca — desenvolvido como parte do Tech Challenge FIAP - **Fase 2**.

---

## 📋 Sumário

- [Descrição](#descrição)  
- [Arquitetura](#arquitetura)  
- [Pré-requisitos](#pré-requisitos)  
- [Instalação & Configuração](#instalação--configuração)  
- [Migrações & Banco de Dados](#migrações--banco-de-dados)  
- [Execução Local e via Docker](#execução-local-e-via-docker)  
- [CI/CD e Deploy na Cloud](#cicd-e-deploy-na-cloud)  
- [Monitoramento](#monitoramento)  
- [Documentação da API](#documentação-da-api)  
- [Endpoints Principais](#endpoints-principais)  
- [Usuários Seed](#usuários-seed)  
- [Testes Automatizados](#testes-automatizados)  
- [Links Úteis](#links-úteis)  

---

## 📝 Descrição

Projeto que contempla:

- Cadastro e autenticação de usuários com JWT  
- Controle de acesso com roles (Usuário/Admin)  
- Cadastro, publicação e aquisição de jogos  
- Criação e ativação de promoções para jogos  
- Compra de jogos e promoções com saldo  
- Armazenamento e auditoria com EF Core + Serilog  
- Testes automatizados (xUnit + FluentAssertions + Moq)  
- Deploy automatizado via Azure DevOps Pipeline  
- Monitoramento via **Application Insights (Azure)**  
- Container Docker com push para Docker Hub  

---

## 🏗️ Arquitetura

```
Fiap.Cloud.Games.sln
├─ Fiap.Cloud.Games.API            → Camada de apresentação (Controllers, Middlewares, Swagger)
├─ Fiap.Cloud.Games.Application    → Casos de uso, DTOs e Interfaces de Serviço
├─ Fiap.Cloud.Games.Domain         → Entidades, VOs, Policies e Enums
├─ Fiap.Cloud.Games.Infrastructure → DbContext, Repositórios, Migrations
└─ Fiap.Cloud.Games.Tests          → Testes unitários (Domain e Controller)
```

---

## ⚙️ Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local ou Azure)
- Docker (para execução containerizada)

---

## 🚀 Instalação & Configuração

1. Clone o repositório:
```bash
git clone https://github.com/lbsilva44/Fiap.Cloud.Games.git
```

2. Configure `appsettings.json` em `Fiap.Cloud.Games.API`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:fcg-sql-server.database.windows.net,1433;Database=fcg-db;User Id=adminsql;Password=Fcg@2025;Encrypt=True;"
  },
  "Jwt": {
    "Key": "chave-super-secreta",
    "Issuer": "Fiap.Cloud.Games",
    "Audience": "Fiap.Cloud.Games.API",
    "ExpiresInHours": 2
  }
}
```

---

## 🧱 Migrações & Banco de Dados

1. Acesse o projeto Infrastructure:
```bash
cd Fiap.Cloud.Games.Infrastructure
```

2. Aplique as migrations:
```bash
dotnet ef database update --startup-project ../Fiap.Cloud.Games.API
```

O banco será criado com tabelas de usuários, jogos, promoções, biblioteca e logs.

---

## ▶️ Execução Local e via Docker

Local:
```bash
cd Fiap.Cloud.Games.API
dotnet run
```

Docker:
```bash
docker build -t fcg-app .
docker run -p 8080:80 fcg-app
```

---

## 🔁 CI/CD e Deploy na Cloud

- CI/CD via **Azure DevOps Pipeline**
- Imagem Docker publicada no **Docker Hub**  
  [https://hub.docker.com/r/lbsilva44/fcg-app](https://hub.docker.com/r/lbsilva44/fcg-app)
- Deploy realizado no **Azure App Service Linux** com Web App for Containers
- Variáveis de ambiente configuradas no portal

---

## 📊 Monitoramento

- Stack usada: **Application Insights (Azure)**
- Métricas monitoradas:
  - Requisições com erro (4xx / 5xx)
  - Tempo médio de resposta
  - Requests por segundo
- Visualização via Azure Portal

---

## 📖 Documentação da API

Swagger UI:  
https://fcg-tech-challenge-eac7hheqfne6bkbn.brazilsouth-01.azurewebsites.net/swagger/index.html

ReDoc:  
https://fcg-tech-challenge-eac7hheqfne6bkbn.brazilsouth-01.azurewebsites.net/docs/index.html

---

## 📡 Endpoints Principais

### 🎮 Gerenciamento de Jogos

| Método | Rota                                  | Descrição            |
|--------|----------------------------------------|-----------------------|
| POST   | /api/Jogo                             | Cadastrar jogo        |
| GET    | /api/Jogo                             | Listar jogos          |
| PATCH  | /api/Jogo/{idJogo}/publicar         | Publicar jogo         |
| PATCH  | /api/Jogo/{idJogo}/ativar           | Ativar jogo           |
| PATCH  | /api/Jogo/{idJogo}/desativar        | Desativar jogo        |
| GET    | /api/Jogo/tipos                       | Listar tipos de jogo  |

### 🎯 Gerenciamento de Promoções

| Método | Rota                                                    | Descrição                  |
|--------|----------------------------------------------------------|-----------------------------|
| POST   | /api/Promocao                                           | Criar promoção              |
| PATCH  | /api/Promocao/{idPromocao}/ativar                     | Ativar promoção             |
| POST   | /api/Promocao/{idPromocao}/{jogoId}/Adicina         | Adicionar jogo à promoção   |
| PATCH  | /api/Promocao/{idPromocao}/exclui                     | Excluir promoção            |

### 📣 Promoções Disponíveis

| Método | Rota                | Descrição              |
|--------|---------------------|-------------------------|
| GET    | /api/Promocao       | Listar promoções        |

### 👤 Conta do Usuário

| Método | Rota                                 | Descrição               |
|--------|--------------------------------------|--------------------------|
| POST   | /api/Usuario/{id}/deposito         | Depositar saldo          |
| GET    | /api/Usuario/{id}                  | Detalhes do usuário      |

### 🔐 Autenticação e Registro

| Método | Rota                              | Descrição                  |
|--------|-----------------------------------|-----------------------------|
| POST   | /api/Usuario                      | Registrar usuário           |
| POST   | /api/Usuario/login                | Login                       |
| POST   | /api/Usuario/senha                | Alterar senha               |
| POST   | /api/Usuario/senha/redefinir      | Redefinir senha             |

### 🛠️ Administração de Usuários

| Método | Rota                                          | Descrição              |
|--------|-----------------------------------------------|-------------------------|
| GET    | /api/Usuario                                 | Listar usuários         |
| PATCH  | /api/Usuario/{idUsuario}/acesso            | Alterar tipo de acesso  |
| GET    | /api/Usuario/roles                           | Listar roles            |
| PATCH  | /api/Usuario/{idUsuario}/desativar         | Desativar usuário       |
| PATCH  | /api/Usuario/{idUsuario}/ativar            | Ativar usuário          |

### 🛍️ Aquisição de Jogos

| Método | Rota                                  | Descrição         |
|--------|----------------------------------------|--------------------|
| POST   | /api/Jogo/{idJogo}/adquirir         | Adquirir jogo      |
| GET    | /api/Jogo/biblioteca                  | Ver biblioteca     |

### 🛍️ Aquisição de Promoções

| Método | Rota                                        | Descrição                 |
|--------|---------------------------------------------|----------------------------|
| POST   | /api/Promocao/{idPromocao}/comprar        | Adquirir promoção          |

---

## 👥 Usuários Seed

Criados automaticamente no primeiro `Run()`:

### Admin  
- **Email**: `admin@fcg.com`  
- **Senha**: `Admin@123!`

### Usuário  
- **Email**: `user@fcg.com`  
- **Senha**: `Senha@123!`

---

## ✅ Testes Automatizados

Rodar na raiz do projeto:
```bash
dotnet test --logger "console;verbosity=detailed"
```

- Cobertura de `UsuarioTests` e `UsuarioControllerTests`
- Validação de regras de negócio (Domínio) e retorno HTTP (API)

---

## 🔗 Links Úteis

- **Miro (Event Storming + Diagramas):**  
  https://miro.com/app/board/uXjVIFs8CKc=/
  
- **Docker Hub (imagem do projeto):**  
  https://hub.docker.com/r/lbsilva44/fcg-app

- **Azure Web App (Deploy da API):**  
  https://fcg-tech-challenge-eac7hheqfne6bkbn.brazilsouth-01.azurewebsites.net/swagger

- **Repositório GitHub:**  
  https://github.com/lbsilva44/FiapTechChallenge-Fase2.git

---

**Autor:** Leonardo Silva  
**Data de Entrega:** 04/08/2025  
**Desafio:** Tech Challenge FIAP - Fase 2  
**Grupo:** Fase 2 - Cloud Games

---
