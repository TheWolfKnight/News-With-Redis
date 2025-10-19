FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /App

COPY ./NewsWithRedis.Api ./NewsWithRedis.Api/
COPY ./NewsWithRedis.Infrastructure ./NewsWithRedis.Infrastructure/
COPY ./NewsWithRedis.Common ./NewsWithRedis.Common/

RUN dotnet restore ./NewsWithRedis.Api
RUN dotnet restore ./NewsWithRedis.Infrastructure

RUN dotnet publish -o out ./NewsWithRedis.Api

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App
EXPOSE 8080
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "NewsWithRedis.Api.dll" ]

