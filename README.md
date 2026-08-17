# BarberShopAgenda 💈
### Sistema de Agendamento para Barbearia

Este é o repositório do Projeto Integrador da disciplina **PJI240 - Projeto Integrador em Computação II - Turma A2026S2N3** da **UNIVESP**. O tema abordado é *"Desenvolvimento de um software com framework web que utilize banco de dados, inclua script web (Javascript), nuvem, uso de API, acessibilidade, controle de versão e testes"*.

---

## 👥 Integrantes

- Antonio Pedro Da Silva — RA 24206532
- Denis Araujo — RA 24204415
- Gisele Cunha Da Silva Braga — RA 1713908
- Guilherme Augusto Benfica Da Costa — RA 24205141
- Zildineia Conceicao Magri — RA 24228400
- *(a definir)* — RA *(a definir)*
- *(a definir)* — RA *(a definir)*
- *(a definir)* — RA *(a definir)*

**Polos:** Cabreúva, Iperó e *(a definir)*
**Orientadora:** Ana Carla de Paula Leite Almeida

---

## 📋 Descrição do Projeto

O **BarberShopAgenda** nasceu da necessidade real identificada em barbearias de pequeno e médio porte que enfrentam dificuldades no gerenciamento de horários e agendamentos, frequentemente resultando em conflitos de horários, perda de clientes e falta de organização no atendimento.

O sistema tem como objetivo **facilitar o agendamento de serviços**, tornando a gestão da barbearia mais eficiente e acessível, tanto para os barbeiros quanto para os clientes.

---

## ✅ Funcionalidades Principais

**Cliente (público, sem cadastro obrigatório)**
- Agendamento online em poucos passos: escolhe serviço, profissional, data e horário — sem precisar criar conta.
- Grade de horários calculada automaticamente a partir da agenda de cada barbeiro (manhã/tarde), já excluindo horários ocupados, passados e períodos de férias/ausência.
- Confirmação por e-mail com código de 6 dígitos, usado para consultar os próprios agendamentos depois (sem senha).
- Conta opcional (`criar-conta.html`) com verificação de e-mail e recuperação de senha, pra quem preferir ter um histórico permanente sem depender do código.

**Barbeiro**
- Login com e-mail e senha (conta criada pelo admin no cadastro).
- Agenda pessoal com confirmação, cancelamento e conclusão de atendimentos.
- Troca da própria senha a qualquer momento.

**Administrador**
- Dashboard com resumo do dia (total de agendamentos, status e receita prevista).
- CRUD completo de clientes, barbeiros e serviços.
- Cadastro de barbeiro já cria a conta de login (e-mail + senha inicial).
- Controle independente de **agenda** (aparece pra cliente agendar) e **conta** (consegue logar) por barbeiro — inativar a conta remove o barbeiro completamente da visão de clientes e de outros usuários, sem apagar o histórico.
- Período de férias/ausência por barbeiro: sem horário disponível só durante o intervalo definido, com retomada automática depois.
- Visão de todos os agendamentos, com filtro por barbeiro e por data.

**Transversal**
- Autenticação JWT com 3 papéis (Admin, Barbeiro, Cliente), cada um com seu próprio nível de acesso.
- Verificação de conflito de horário por barbeiro **e** por cliente (impede que o mesmo cliente marque dois horários que se sobrepõem, mesmo com barbeiros diferentes).
- E-mail transacional (confirmação de agendamento, verificação de conta, redefinição de senha) via API HTTP da Brevo — gratuito, 300 e-mails/dia.
- API REST documentada via Swagger/OpenAPI.
- Interface seguindo diretrizes de acessibilidade WCAG 2.1.

---

## 🛠️ Tecnologias Utilizadas

- **Linguagem:** C# (.NET 8 — ASP.NET Core Web API)
- **Frontend:** HTML, CSS e JavaScript puro
- **Banco de Dados:** MySQL 8.0
- **ORM:** Entity Framework Core 8 com Pomelo (MySQL)
- **Autenticação:** JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`) + hash de senha via `PasswordHasher`
- **E-mail:** API HTTP transacional da Brevo
- **Testes:** xUnit + Moq
- **Documentação da API:** Swagger / OpenAPI
- **Controle de versão:** Git + GitHub
- **Nuvem:** Microsoft Azure (App Service + Azure Database for MySQL)
- **Containerização:** Docker + Docker Compose

---

## ⚙️ Instalação e Uso

### Opção 1 — Rodando com Docker (Recomendado)

#### Pré-requisitos
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/download/win)

#### Passos

**1. Clone o repositório**
```bash
git clone https://github.com/Guilherme-Benfica/PJI240-A2026S2N3-Grupo12-BarberShopAgenda.git
cd PJI240-A2026S2N3-Grupo12-BarberShopAgenda
```

**2. Suba o banco de dados**
```bash
docker-compose up -d mysql
```

Isso cria o container MySQL 8.0, o banco `barbershop_agenda` e aplica `database/schema.sql` automaticamente (tabelas + seeds: 3 barbeiros, 5 serviços e as contas iniciais de admin/barbeiro).

**3. Rode a API**
```bash
cd BarberShopAgenda.API
dotnet run
```
A API sobe em `https://localhost:7001` (Swagger em `/swagger`). Veja a variável `BARBERSHOP_CONNECTION_STRING` na seção [2. Configurar a connection string](#2-configurar-a-connection-string) — ela tem prioridade sobre o `appsettings.json`.

**4. Sirva o frontend**

O frontend é estático (HTML/CSS/JS puro), sem build. Sirva a pasta `frontend/` com qualquer servidor local (evita bloqueios de CORS/file://):
```bash
cd frontend
python -m http.server 5500
# ou: npx serve frontend
```

| Serviço | URL |
|---|---|
| Frontend | `http://localhost:5500` |
| API | `https://localhost:7001` |
| Swagger | `https://localhost:7001/swagger` |
| MySQL | `localhost:3306` |

> 💡 Também é possível subir a API dentro do próprio Docker Compose (`docker-compose up -d`, sobe API + MySQL juntos, API em `http://localhost:5000`) — útil para não precisar do SDK .NET instalado localmente.

Para parar:
```bash
docker-compose down          # mantém os dados
docker-compose down -v       # apaga também os dados do banco
```

Por padrão a senha do MySQL é `barbershop123` (definida no `docker-compose.yml`). Para customizar, crie um `.env` na raiz do projeto:
```env
MYSQL_ROOT_PASSWORD=uma_senha_forte
MYSQL_DATABASE=barbershop_agenda
```

---

### Opção 2 — Rodando sem Docker

#### Pré-requisitos
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [MySQL Server 8.0](https://dev.mysql.com/downloads/mysql/) local, ou acesso a uma instância MySQL
- Um servidor HTTP simples para o frontend (ex.: extensão "Live Server" do VS Code, ou `python -m http.server`)

## 1. Configurar o banco de dados

```bash
mysql -u root -p < database/schema.sql
```

Isso cria as tabelas e os dados iniciais (3 barbeiros, 5 serviços, contas de admin e barbeiro — veja a seção [Autenticação](#-autenticação)). Alternativamente, o schema também pode ser criado via **migrations do EF Core** (passo 3).

## 2. Configurar a connection string

Em desenvolvimento, edite `BarberShopAgenda.API/appsettings.Development.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=barbershop_agenda;User=root;Password=SUA_SENHA;"
}
```

> Em produção, a connection string é lida da variável de ambiente `BARBERSHOP_CONNECTION_STRING` (tem prioridade sobre o `appsettings.json`) — veja a seção [Deploy](#deploy).

Alternativa mais segura em desenvolvimento, com [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):
```bash
cd BarberShopAgenda.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=barbershop_agenda;User=root;Password=SUA_SENHA;"
```

## 3. Rodar as migrations do EF Core (alternativa ao schema.sql)

```bash
dotnet tool install --global dotnet-ef   # se ainda não tiver o dotnet-ef instalado
dotnet ef database update -p BarberShopAgenda.Infrastructure -s BarberShopAgenda.API
```

As migrations aplicam o schema e os seeds (`HasData`) definidos em `BarberShopContext`. **Não misture os dois caminhos** na mesma instância de banco: ou usa `schema.sql`, ou usa migrations — se o banco já existir por um dos dois, o EF pode reclamar que as tabelas já existem ao tentar aplicar migrations do zero.

## 4. Restaurar dependências e compilar

Na raiz do repositório:
```bash
dotnet restore
dotnet build
```

## 5. Executar a API

```bash
cd BarberShopAgenda.API
dotnet run
```

A API sobe em `https://localhost:7001` e `http://localhost:5080` (ajustável via `ASPNETCORE_URLS`). Swagger em desenvolvimento:
```
https://localhost:7001/swagger
```

## 6. Executar o frontend

```bash
cd frontend
python -m http.server 5500
# ou: npx serve frontend
```

Se a URL/porta da API não for `https://localhost:7001/api`, ajuste antes de carregar os scripts, definindo a variável global no HTML (antes de `<script src="js/api.js">`):
```html
<script>window.BARBERSHOP_API_URL = "https://localhost:7001/api";</script>
```

## 7. Executar os testes

```bash
dotnet test
```

Cobrem: criação de agendamento válido, conflito de horário por barbeiro e por cliente, cancelamento, listagem por data, cálculo de horários disponíveis (incluindo período de férias), autenticação (login válido/inválido/e-mail não confirmado), troca de senha, e o ciclo completo de conta de cliente (registro, vínculo ao histórico de convidado, confirmação de e-mail, redefinição de senha).

---

### ❗ Problemas comuns

| Erro | Causa | Solução |
|---|---|---|
| `Unable to connect to any of the specified MySQL hosts` | MySQL não está rodando, ou connection string errada | Confirme que o MySQL (local ou `docker-compose up -d mysql`) está ativo e que `appsettings.Development.json`/`BARBERSHOP_CONNECTION_STRING` apontam pro lugar certo |
| `Access denied for user 'root'` | Senha incorreta na connection string | Corrija a senha em `appsettings.Development.json` ou na variável de ambiente |
| `Table 'X' already exists` ao rodar `dotnet ef database update` | O banco já foi criado via `schema.sql` e agora você está tentando aplicar migrations do zero nele | Use só um dos dois caminhos (schema.sql *ou* migrations) por banco; veja o passo 3 acima |
| Acentos aparecendo corrompidos (`Ã©`, `Ã£`) depois de recriar o banco | Cliente MySQL usando charset diferente de utf8mb4 ao carregar `schema.sql` | Já corrigido no próprio `schema.sql` (`SET NAMES utf8mb4;` no início do script) — se persistir, confirme que está usando a versão atual do arquivo |
| `Chave JWT não configurada` / `Connection string não configurada` ao rodar a API | Faltou configurar `Jwt:Key` / `ConnectionStrings:DefaultConnection` (ou as variáveis de ambiente equivalentes) | Veja as seções [2](#2-configurar-a-connection-string) e [Autenticação](#-autenticação) |
| Tela de admin/barbeiro fica "piscando" entre páginas | Sessão de um papel tentando acessar página de outro papel (bug corrigido) | Dê um hard refresh (`Ctrl+Shift+R`) pra garantir que está com os arquivos JS mais recentes |
| Mudança no frontend não aparece no navegador | Cache do navegador | Hard refresh (`Ctrl+Shift+R`) ou aba anônima |

---

## 📁 Estrutura do Projeto

```
BarberShopAgenda/
├── BarberShopAgenda.API/            Web API ASP.NET Core
│   ├── Controllers/                 Auth, Agendamentos, Barbeiros, Clientes, Servicos, Horarios, Dashboard
│   ├── DTOs/                        Objetos de entrada/saída da API
│   ├── Middleware/                  Tratamento global de exceções
│   ├── Program.cs                   Inicialização, DI, JWT, CORS
│   ├── appsettings.json             Configuração base (versionado)
│   └── appsettings.Development.json Configuração local de desenvolvimento
├── BarberShopAgenda.Domain/         Entidades e interfaces (contratos)
│   ├── Entities/                    Cliente, Barbeiro, Servico, Agendamento, Usuario, PapelUsuario...
│   └── Interfaces/                  Contratos de repositórios e serviços
├── BarberShopAgenda.Infrastructure/ Implementação: EF Core, repositórios, serviços de negócio
│   ├── Data/                        BarberShopContext (DbContext)
│   ├── Repositories/                Implementação dos repositórios
│   ├── Services/                    Auth, ClienteConta, Agendamento, HorarioDisponivel, Email (SMTP)...
│   └── Migrations/                  Migrations do EF Core
├── BarberShopAgenda.Tests/          Testes xUnit
│   └── Services/                    Testes dos serviços de domínio
├── frontend/                        Interface web (HTML/CSS/JS puro, sem build)
│   ├── index.html                   Dashboard (Admin)
│   ├── agendamentos.html            Agenda (Admin vê tudo; Barbeiro só a própria)
│   ├── clientes.html                CRUD de clientes (Admin)
│   ├── barbeiros.html               CRUD de barbeiros + conta + férias (Admin)
│   ├── servicos.html                CRUD de serviços (Admin)
│   ├── agendar.html                 Fluxo público de agendamento (Cliente)
│   ├── meus-agendamentos.html       Consulta por telefone + código (Cliente, sem conta)
│   ├── login.html                   Login (Admin, Barbeiro ou Cliente)
│   ├── criar-conta.html             Criação de conta de cliente
│   ├── confirmar-email.html         Confirmação de e-mail (link enviado por e-mail)
│   ├── esqueci-senha.html           Solicitação de redefinição de senha
│   ├── redefinir-senha.html         Redefinição de senha (link enviado por e-mail)
│   ├── minha-conta.html             Histórico de agendamentos (Cliente autenticado)
│   ├── trocar-senha.html            Troca de senha (qualquer papel autenticado)
│   ├── css/                         style.css (tema/base) + agendar.css (fluxo do cliente)
│   └── js/                          config.js (URL da API) + api.js (fetch) + auth.js (sessão/JWT) + 1 script por página
├── database/schema.sql              Script SQL de criação e seeds (alternativa às migrations)
├── Dockerfile                       Build multi-stage da API
├── docker-compose.yml               Orquestração API + MySQL
├── .github/workflows/deploy-pages.yml  Publica frontend/ no GitHub Pages a cada push
├── .dockerignore
├── .gitignore
└── README.md                        Este arquivo
```

---

## 🔐 Autenticação

A API usa **JWT**, com 3 papéis: `Admin`, `Barbeiro` e `Cliente`. O token é obtido em `POST /api/auth/login` e enviado nas rotas protegidas via header `Authorization: Bearer <token>`.

### Contas padrão (seed)

`schema.sql` e as migrations já criam os usuários abaixo. **Troque essas senhas antes de qualquer uso além de desenvolvimento local.**

| Papel | E-mail | Senha padrão |
|---|---|---|
| Admin | `admin@barbershop.com` | `Admin@123` |
| Barbeiro (Carlos Silva) | `carlos.silva@barbershop.com` | `Barbeiro@123` |
| Barbeiro (João Pereira) | `joao.pereira@barbershop.com` | `Barbeiro@123` |
| Barbeiro (Marcos Souza) | `marcos.souza@barbershop.com` | `Barbeiro@123` |

Qualquer usuário autenticado pode trocar a própria senha em `PUT /api/auth/senha` (tela `trocar-senha.html`).

### Conta de cliente (opcional)

O cliente não precisa de conta pra agendar — o fluxo público (`agendar.html`) funciona só com nome/telefone/e-mail. Quem quiser conta permanente pode criar em `criar-conta.html`: o cadastro fica vinculado automaticamente ao histórico de agendamentos que já existir com aquele telefone, exige confirmação por e-mail antes do primeiro login, e tem recuperação de senha própria (`esqueci-senha.html` → `redefinir-senha.html`).

Em produção, defina a chave de assinatura do JWT via variável de ambiente `BARBERSHOP_JWT_KEY` (mesmo padrão da connection string) em vez de deixá-la no `appsettings.json`.

---

## 📧 E-mail transacional (opcional)

A API envia e-mails automaticamente (confirmação de agendamento com código, verificação de conta, redefinição de senha) pela **API HTTP transacional da Brevo** — sem custo (300 e-mails/dia grátis). Se a chave não estiver configurada, tudo continua funcionando normalmente, só não envia o e-mail (fica um aviso no log).

> Optamos pela API HTTP (porta 443) em vez de SMTP (porta 587/465) porque hosts como o Render bloqueiam portas SMTP na camada de rede do plano gratuito — toda tentativa de conexão SMTP travava por ~100s até estourar timeout e falhar.

Configurado em `appsettings.json`/`appsettings.Development.json`:

```json
"Email": {
  "RemetenteEmail": "barbershopagenda90@gmail.com",
  "RemetenteNome": "BarberShop Agenda"
},
"Brevo": {
  "ApiKey": ""
},
"Frontend": {
  "BaseUrl": "http://localhost:5500"
}
```

`Frontend:BaseUrl` é usado para montar os links de confirmação/redefinição enviados por e-mail. A chave da API **não** deve ir no `appsettings.json` — defina via variável de ambiente `BARBERSHOP_BREVO_API_KEY` (mesmo padrão de `BARBERSHOP_CONNECTION_STRING`/`BARBERSHOP_JWT_KEY`), gerada em [app.brevo.com/settings/keys/api](https://app.brevo.com/settings/keys/api).

---

## 🔌 Endpoints da API

| Recurso | Método | Rota | Acesso |
|---|---|---|---|
| Auth | POST | `/api/auth/login` | Público |
| Auth | POST | `/api/auth/registrar` | Público (cria conta de cliente) |
| Auth | POST | `/api/auth/confirmar-email` | Público |
| Auth | POST | `/api/auth/esqueci-senha` | Público |
| Auth | POST | `/api/auth/redefinir-senha` | Público |
| Auth | PUT | `/api/auth/senha` | Qualquer autenticado (troca a própria senha) |
| Clientes | GET | `/api/clientes` | Admin |
| Clientes | GET | `/api/clientes/{id}` | Admin |
| Clientes | POST | `/api/clientes` | Público (autocadastro no agendamento) |
| Clientes | PUT | `/api/clientes/{id}` | Admin |
| Clientes | DELETE | `/api/clientes/{id}` | Admin |
| Barbeiros | GET | `/api/barbeiros` | Público (catálogo — só quem tem conta ativa ou não tem conta) |
| Barbeiros | GET | `/api/barbeiros/todos` | Admin (todos, inclusive conta inativa) |
| Barbeiros | GET | `/api/barbeiros/{id}` | Público |
| Barbeiros | POST | `/api/barbeiros` | Admin (já cria a conta de login) |
| Barbeiros | PUT | `/api/barbeiros/{id}` | Admin (agenda, especialidade, férias) |
| Barbeiros | PUT | `/api/barbeiros/{id}/conta/ativar` | Admin |
| Barbeiros | PUT | `/api/barbeiros/{id}/conta/inativar` | Admin |
| Serviços | GET | `/api/servicos` | Público (catálogo) |
| Serviços | POST | `/api/servicos` | Admin |
| Serviços | PUT | `/api/servicos/{id}` | Admin |
| Horários | GET | `/api/horarios/disponiveis?barbeiroId=&data=&servicoId=` | Público |
| Agendamentos | GET | `/api/agendamentos` | Admin, Barbeiro |
| Agendamentos | GET | `/api/agendamentos/{id}` | Admin, Barbeiro |
| Agendamentos | GET | `/api/agendamentos/barbeiro/{barbeiroId}` | Admin, Barbeiro (só a própria agenda) |
| Agendamentos | GET | `/api/agendamentos/cliente?telefone=&codigo=` | Público (telefone + código de confirmação) |
| Agendamentos | GET | `/api/agendamentos/me` | Cliente autenticado |
| Agendamentos | GET | `/api/agendamentos/data/{data}` (formato `yyyy-MM-dd`) | Admin, Barbeiro |
| Agendamentos | POST | `/api/agendamentos` | Público (fluxo de agendamento do cliente) |
| Agendamentos | PUT | `/api/agendamentos/{id}/confirmar` | Admin, Barbeiro |
| Agendamentos | PUT | `/api/agendamentos/{id}/cancelar` | Admin, Barbeiro |
| Agendamentos | PUT | `/api/agendamentos/{id}/concluir` | Admin, Barbeiro |
| Dashboard | GET | `/api/dashboard/hoje` | Admin |

---

## ☁️ Deploy

- **API:** [Render](https://render.com) (Web Service, camada gratuita, build via `Dockerfile`) — https://pji240-a2026s2n3-grupo12-barbershopagenda.onrender.com
- **Banco de Dados:** [Aiven](https://aiven.io) (MySQL, camada always-free)
- **Frontend:** [GitHub Pages](https://pages.github.com) (publicado automaticamente a cada push em `frontend/**` via `.github/workflows/deploy-pages.yml`) — https://guilherme-benfica.github.io/PJI240-A2026S2N3-Grupo12-BarberShopAgenda/

Variáveis de ambiente configuradas no Render (nunca commitadas):

| Variável | Descrição |
|---|---|
| `BARBERSHOP_CONNECTION_STRING` | Connection string do MySQL (Aiven), com `SslMode=Required` |
| `BARBERSHOP_JWT_KEY` | Chave de assinatura dos tokens JWT em produção |
| `BARBERSHOP_BREVO_API_KEY` | Chave da API HTTP transacional da Brevo |

> A instância gratuita do Render "dorme" após 15 minutos de inatividade — a primeira requisição depois disso pode levar até ~50s pra responder.

---

## ♿ Acessibilidade (WCAG 2.1)

- Contraste mínimo 4.5:1 entre texto e fundo (tema escuro preto/dourado)
- Navegação por teclado com `:focus-visible` destacado em todos os elementos interativos
- `aria-label`, `aria-required` e `aria-current` em formulários e navegação
- Link "Pular para o conteúdo principal" em todas as páginas
- Mensagens de status/erro com `role="status"`/`role="alert"` e `aria-live="polite"`

---

*Projeto desenvolvido para fins acadêmicos — UNIVESP 2026*
