# syntax=docker/dockerfile:1

##################################
# Etapa 1: Build
##################################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 1) Copiar solução
COPY ["Movtech.sln", "./"]

# 2) Copiar só o .csproj
COPY ["PedidoClientManagement/src/PedidoClientManagement.API/PedidoClientManagement.API.csproj", \
     "PedidoClientManagement/src/PedidoClientManagement.API/"]

# 3) Restaurar dependências
RUN dotnet restore "PedidoClientManagement/src/PedidoClientManagement.API/PedidoClientManagement.API.csproj"

# 4) Copiar todo o restante do projeto
COPY ["PedidoClientManagement/", "PedidoClientManagement/"]

# 5) Publicar em Release
WORKDIR /app/PedidoClientManagement/src/PedidoClientManagement.API
RUN dotnet publish "PedidoClientManagement.API.csproj" -c Release -o /publish

##################################
# Etapa 2: Runtime
##################################
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 6) Trazer artefatos publicados
COPY --from=build /publish .

# 7) Configurar porta para o Render
ENV ASPNETCORE_URLS=http://0.0.0.0:80

# 8) Entry point
ENTRYPOINT ["dotnet", "PedidoClientManagement.API.dll"]
