FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY Dos.ReplayService/Dos.ReplayService.csproj Dos.ReplayService/
COPY Dos.Game/Dos.Game.csproj Dos.Game/
COPY Dos.Database/Dos.Database.csproj Dos.Database/
COPY Dos.Utils/Dos.Utils.csproj Dos.Utils/
RUN dotnet restore Dos.ReplayService/Dos.ReplayService.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out ./Dos.ReplayService/Dos.ReplayService.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
COPY --from=build-env /app/Dos.ReplayService/front ./front/
ENTRYPOINT ["dotnet", "Dos.ReplayService.dll"]
