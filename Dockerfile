FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Src/OrderFlow.Integrations/OrderFlow.Integrations.csproj", "Src/OrderFlow.Integrations/"]
RUN dotnet restore "Src/OrderFlow.Integrations/OrderFlow.Integrations.csproj"
COPY . .
WORKDIR "/src/Src/OrderFlow.Integrations"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderFlow.Integrations.dll"]
