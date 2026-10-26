FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SmartProd.Server/SmartProd.Server.csproj SmartProd.Server/
RUN dotnet restore SmartProd.Server/SmartProd.Server.csproj
COPY SmartProd.Server/ SmartProd.Server/
WORKDIR /src/SmartProd.Server
RUN dotnet publish SmartProd.Server.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartProd.Server.dll"]
