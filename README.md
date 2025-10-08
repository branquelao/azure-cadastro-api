# Sistema de RH na Nuvem (Desafio .NET + Azure)

Este projeto é um **sistema de Recursos Humanos** simples, desenvolvido como parte de um desafio da trilha **.NET + Azure da DIO**. O objetivo é criar uma **API REST** para gerenciar funcionários e registrar logs de todas as ações realizadas.

---

## ✨ Funcionalidades

* **Cadastrar Funcionários** (`POST /Funcionario`)
* **Consultar Funcionário por ID** (`GET /Funcionario/{id}`)
* **Atualizar Funcionário** (`PUT /Funcionario/{id}`)
* **Deletar Funcionário** (`DELETE /Funcionario/{id}`)
* **Gerar Logs Automáticos** em cada operação (criação, atualização e exclusão).

---

## 🏗️ Arquitetura

O sistema foi construído em **.NET 6 (Web API)** e utiliza os seguintes recursos do **Microsoft Azure**:

* **App Service** → Hospeda a API.
* **Azure SQL Database** → Armazena os dados principais de Funcionários.
* **Azure Table Storage** → Armazena os logs de alterações.

### Fluxo resumido:

```
[Usuário] → [API no App Service] → [SQL Database]
                                 ↘ [Table Storage - Logs]
```

---

## 📦 Estrutura de Classes

### Funcionario

```json
{
  "nome": "Nome funcionario",
  "endereco": "Rua 1234",
  "ramal": "1234",
  "emailProfissional": "email@email.com",
  "departamento": "TI",
  "salario": 1000,
  "dataAdmissao": "2022-06-23T02:58:36.345Z"
}
```

### FuncionarioLog

Herdada de **Funcionario**, adiciona as informações de log:

* `TipoAcao` (Inclusão, Atualização, Remoção)
* `PartitionKey` e `RowKey` para organização na Table Storage

### TipoAcao

Enum para identificar a operação:

* Inclusao
* Atualizacao
* Remocao

---

## 🔧 Configuração

### `appsettings.json`

```json
{
  "ConnectionStrings": {
    "ConexaoPadrao": "<CONNECTION_STRING_SQL_SERVER>",
    "SAConnectionString": "<AZURE_STORAGE_ACCOUNT_CONNECTION_STRING>",
    "AzureTableName": "FuncionarioLogs"
  }
}
```

No Azure App Service, configure os mesmos valores em **Application Settings**.

---

## 🗄️ Banco de Dados

O projeto utiliza **Entity Framework Core**. A migration inicial já cria a tabela de Funcionários.

### Criar/atualizar o banco local:

```bash
dotnet ef database update
```

### Nova migration (se necessário):

```bash
dotnet ef migrations add NomeMigration
dotnet ef database update
```

---

## 🚀 Publicação no Azure

1. Criar recursos no portal Azure:

   * SQL Database
   * Storage Account (Table)
   * App Service

2. Configurar connection strings no App Service.

3. Publicar a aplicação:

   * Via Visual Studio (Publish → Azure App Service)
   * Ou via CLI:

     ```bash
     dotnet publish -c Release
     ```

4. Acessar a API publicada:
   `https://<nome-app>.azurewebsites.net/swagger`

---

## 🧪 Testando os Endpoints

### Criar Funcionário

```bash
curl -X POST "https://<seu-app>.azurewebsites.net/Funcionario" \
  -H "Content-Type: application/json" \
  -d '{
    "nome":"Joao",
    "endereco":"Rua A, 123",
    "ramal":"100",
    "emailProfissional":"joao@empresa.com",
    "departamento":"TI",
    "salario":2500,
    "dataAdmissao":"2022-06-23T02:58:36.345Z"
  }'
```

### Atualizar Funcionário

```bash
curl -X PUT "https://<seu-app>.azurewebsites.net/Funcionario/1" \
  -H "Content-Type: application/json" \
  -d '{ ... }'
```

### Obter Funcionário

```bash
curl "https://<seu-app>.azurewebsites.net/Funcionario/1"
```

### Deletar Funcionário

```bash
curl -X DELETE "https://<seu-app>.azurewebsites.net/Funcionario/1"
```

---

## 📌 Resumo

Esse projeto entrega um **sistema de RH simples na nuvem**, com API em .NET hospedada no Azure, persistindo dados em SQL Database e guardando logs em Table Storage. Assim, todas as operações ficam registradas e o sistema é escalável e confiável para uso em produção ou aprendizado.
