# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

COPY . ./
RUN dotnet restore
RUN dotnet publish "./PHC-HighCounsel-Bot/PHC-HighCounsel-Bot.csproj" -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /App
COPY --from=build-env /App/out .

# Install ffmpeg
RUN apt-get update \
    && apt-get install -y ffmpeg libopus0 opus-tools libopus-dev libsodium-dev \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "PHC-HighCounsel-Bot.dll"]
