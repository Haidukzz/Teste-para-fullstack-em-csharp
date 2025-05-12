# Movtech Client & Order Management API

## Descrição do Projeto

Este projeto é uma API web para gerenciamento de **clientes** e **pedidos**, desenvolvida como parte de um teste técnico para a vaga de desenvolvedor full stack na **Movtech**.  
A aplicação permite cadastrar, consultar, atualizar e remover clientes e pedidos, bem como gerenciar os itens de cada pedido.  
Trata-se de um serviço backend escrito em C# utilizando .NET, expondo endpoints RESTful para realizar operações de CRUD (Create, Read, Update, Delete) dos dados de clientes e seus pedidos.

## Tecnologias Utilizadas

- **ASP.NET Core (.NET 8)** – Framework principal para construção da API Web em C#.  
- **Entity Framework Core** – ORM para manipulação dos dados, com Migrations code-first.  
- **PostgreSQL** – Banco de dados relacional (instância hospedada no Render).  
- **Render** – Plataforma de hospedagem em nuvem usada para deploy da API e do banco.

## Organização do Projeto
```PedidoClientManagement.API/
├── Controllers/
│ ├── ClientesController.cs
│ └── PedidosController.cs
├── Models/
│ ├── Cliente.cs
│ ├── Pedido.cs
│ └── ItemPedido.cs
├── Data/
│ ├── AppDbContext.cs
│ └── Migrations/
│ ├── {timestamp}_InicialPostgres.cs
│ └── AppDbContextModelSnapshot.cs
├── appsettings.json
└── Program.cs```


- **Controllers/**  
  Definem os endpoints HTTP:  
  - `ClientesController.cs` → `/api/clientes` (CRUD de clientes)  
  - `PedidosController.cs` → `/api/pedidos` (CRUD de pedidos e itens)

- **Models/**  
  Representam as entidades de domínio:  
  - `Cliente.cs` (Nome, Email, CPF, DataNascimento, etc.)  
  - `Pedido.cs` (ClienteId, DataPedido, ValorTotal, Itens)  
  - `ItemPedido.cs` (Descrição, Quantidade, PrecoUnitario, PedidoId)

- **Data/**  
  Configuração do EF Core e acesso a dados:  
  - `AppDbContext.cs` (DbSet de Clientes, Pedidos e ItensPedido; regras de mapeamento)  
  - **Migrations/** (script C# para criação/atualização das tabelas)

- **appsettings.json**  
  Contém a connection string para PostgreSQL no Render.

- **Program.cs**  
  Configura serviços, middleware e inicia o servidor Kestrel, incluindo auto-migrations.

## Acesso à Aplicação Publicada

A API está implantada e disponível publicamente no Render.  
Para testar, use a **URL base**:

https://pedido-client-management.onrender.com
