FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

COPY stmchat_backend/stmchat_backend.csproj /app/stmchat/
WORKDIR /app/stmchat/
RUN dotnet restore

COPY stmchat_backend/* /app/stmchat/
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/stmchat/out .
COPY stmchat_backend/appsettings.json /app/
ENTRYPOINT ["dotnet", "stmchat_backend.dll"]
