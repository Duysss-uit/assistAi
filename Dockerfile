FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AssistAi.Api/AssistAi.Api.csproj", "AssistAi.Api/"]
RUN dotnet restore "AssistAi.Api/AssistAi.Api.csproj"
COPY . .
WORKDIR "/src/AssistAi.Api"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AssistAi.Api.dll"]
