# Etapa 1: Build de Angular
FROM node:20 AS client-build
WORKDIR /app/client
COPY ClientApp/ ./
RUN npm install && npm run build --prod

# Etapa 2: Build de .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Etapa 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=client-build /app/client/dist/ /app/wwwroot/
ENTRYPOINT ["dotnet", "TuProyecto.dll"]