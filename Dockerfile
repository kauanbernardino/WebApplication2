# Estágio 1: Build (Compilação)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo de projeto e baixa as dependências
COPY ["WebApplication1.csproj", "./"]
RUN dotnet restore "WebApplication1.csproj"

# Copia todo o resto do código e gera a versão final
COPY . .
RUN dotnet publish "WebApplication1.csproj" -c Release -o /app/publish

# Estágio 2: Runtime (Para rodar leve)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080

# Copia os arquivos compilados do estágio anterior
COPY --from=build /app/publish .

# Comandos Especiais para SQLite no Docker (Evita erro de permissão)
USER root
RUN chmod 777 .

# Comando que inicia o site
ENTRYPOINT ["dotnet", "WebApplication1.dll"]