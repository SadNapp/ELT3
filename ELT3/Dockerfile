FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копіюємо файл проекту
COPY ["ELT3/ELT3.csproj", "ELT3/"]
RUN dotnet restore "ELT3/ELT3.csproj"

# Копіюємо весь інший код і збираємо
COPY ["ELT3/", "ELT3/"]
WORKDIR "/src/ELT3"
RUN dotnet publish "ELT3.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ELT3.dll"]