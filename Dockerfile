FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Src/OrderFlow.Api/OrderFlow.Api.csproj", "Src/OrderFlow.Api/"]
COPY ["Src/OrderFlow.Application/OrderFlow.Application.csproj", "Src/OrderFlow.Application/"]
COPY ["Src/OrderFlow.Domain/OrderFlow.Domain.csproj", "Src/OrderFlow.Domain/"]
COPY ["Src/OrderFlow.Infrastructure/OrderFlow.Infrastructure.csproj", "Src/OrderFlow.Infrastructure/"]
RUN dotnet restore "Src/OrderFlow.Api/OrderFlow.Api.csproj"
COPY . .
WORKDIR "/src/Src/OrderFlow.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderFlow.Api.dll"]
