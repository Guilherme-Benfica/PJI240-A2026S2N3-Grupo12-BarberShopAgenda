# Deploy no Azure — BarberShop Agenda

Guia passo a passo para publicar a API no **Azure App Service** (camada gratuita/F1 ou B1) e o banco de dados no **Azure Database for MySQL Flexible Server**.

Pré-requisitos: [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) instalada e autenticada (`az login`).

## 1. Criar o resource group

```bash
az group create --name rg-barbershop-agenda --location brazilsouth
```

## 2. Criar o Azure Database for MySQL Flexible Server

```bash
az mysql flexible-server create \
  --resource-group rg-barbershop-agenda \
  --name barbershop-mysql-server \
  --location brazilsouth \
  --admin-user barbershopadmin \
  --admin-password "DEFINA-UMA-SENHA-FORTE-AQUI" \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --storage-size 32 \
  --version 8.0.21 \
  --public-access 0.0.0.0-255.255.255.255
```

> O parâmetro `--public-access` acima libera acesso público para simplificar o setup inicial. Em produção, restrinja a regras de firewall específicas (IP do App Service) ou use VNet Integration / Private Link.

Criar o banco de dados de aplicação:

```bash
az mysql flexible-server db create \
  --resource-group rg-barbershop-agenda \
  --server-name barbershop-mysql-server \
  --database-name barbershop_agenda
```

Liberar acesso a partir de serviços do Azure (necessário para o App Service alcançar o MySQL):

```bash
az mysql flexible-server firewall-rule create \
  --resource-group rg-barbershop-agenda \
  --name barbershop-mysql-server \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

Aplicar o schema e os seeds (a partir da sua máquina, apontando para o servidor Azure):

```bash
mysql -h barbershop-mysql-server.mysql.database.azure.com \
  -u barbershopadmin -p barbershop_agenda < database/schema.sql
```

(Alternativa: rodar `dotnet ef database update` apontando a connection string para o servidor Azure — veja o README.)

## 3. Criar o App Service Plan (camada gratuita) e o Web App

```bash
az appservice plan create \
  --resource-group rg-barbershop-agenda \
  --name plan-barbershop-agenda \
  --sku F1 \
  --is-linux

az webapp create \
  --resource-group rg-barbershop-agenda \
  --plan plan-barbershop-agenda \
  --name barbershop-agenda-api \
  --runtime "DOTNETCORE:8.0"
```

> O nome do Web App (`--name`) precisa ser globalmente único no Azure — ajuste conforme necessário. A URL final será `https://<nome-escolhido>.azurewebsites.net`.

## 4. Configurar variáveis de ambiente (connection string) no App Service

A API lê a connection string da variável de ambiente `BARBERSHOP_CONNECTION_STRING` (veja `Program.cs`), com prioridade sobre o `appsettings.json`:

```bash
az webapp config appsettings set \
  --resource-group rg-barbershop-agenda \
  --name barbershop-agenda-api \
  --settings BARBERSHOP_CONNECTION_STRING="Server=barbershop-mysql-server.mysql.database.azure.com;Port=3306;Database=barbershop_agenda;User=barbershopadmin;Password=DEFINA-UMA-SENHA-FORTE-AQUI;SslMode=Required;"
```

Configurar também o ambiente ASP.NET Core e origens de CORS liberadas para o frontend publicado:

```bash
az webapp config appsettings set \
  --resource-group rg-barbershop-agenda \
  --name barbershop-agenda-api \
  --settings ASPNETCORE_ENVIRONMENT="Production" \
             Cors__AllowedOrigins__0="https://SEU-FRONTEND-PUBLICADO"
```

## 5. Publicar a API

Na raiz do repositório:

```bash
cd BarberShopAgenda.API
dotnet publish -c Release -o ./publish

cd publish
zip -r ../publish.zip .
cd ..

az webapp deploy \
  --resource-group rg-barbershop-agenda \
  --name barbershop-agenda-api \
  --src-path publish.zip \
  --type zip
```

Alternativa via GitHub Actions/CI: configure `az webapp deployment source config-zip` no seu pipeline, ou conecte o repositório diretamente pelo portal do Azure (**Deployment Center**).

## 6. Verificar o deploy

```bash
az webapp browse --resource-group rg-barbershop-agenda --name barbershop-agenda-api
```

Teste o Swagger (se `ASPNETCORE_ENVIRONMENT` estiver como `Development`) ou um endpoint direto:

```
https://barbershop-agenda-api.azurewebsites.net/api/servicos
```

## 7. Publicar o frontend

O frontend é estático (HTML/CSS/JS) e pode ser hospedado separadamente, por exemplo em **Azure Static Web Apps**:

```bash
az staticwebapp create \
  --resource-group rg-barbershop-agenda \
  --name barbershop-agenda-frontend \
  --source frontend \
  --location brazilsouth \
  --sku Free
```

Antes de publicar, ajuste `window.BARBERSHOP_API_URL` nas páginas HTML (ou centralize em um arquivo de configuração carregado antes de `js/api.js`) para apontar para a URL definitiva da API publicada no App Service.

## Resumo das variáveis de ambiente do App Service

| Variável | Descrição |
|---|---|
| `BARBERSHOP_CONNECTION_STRING` | Connection string do MySQL Flexible Server (tem prioridade sobre `appsettings.json`) |
| `ASPNETCORE_ENVIRONMENT` | `Production` em produção (desabilita o Swagger, conforme configurado em `Program.cs`) |
| `Cors__AllowedOrigins__0`, `__1`, ... | Origens permitidas para CORS (URL do frontend publicado) |

## Custos e limites da camada gratuita

- **App Service F1 (Free):** 60 minutos de CPU/dia, sem domínio customizado com SSL próprio, sem "always on" — adequado para demonstração/portfólio, não para produção real.
- **Azure Database for MySQL Flexible Server (Burstable B1ms):** camada paga mais econômica; verifique o [Azure Free Account](https://azure.microsoft.com/free/) para créditos iniciais e elegibilidade de camada gratuita por tempo limitado.
