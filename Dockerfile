# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["VentoryApi.csproj", "."]
RUN dotnet restore "./VentoryApi.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/."
RUN dotnet build "VentoryApi.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish "VentoryApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final, small runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VentoryApi.dll"]