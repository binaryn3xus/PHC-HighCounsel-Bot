FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

COPY . ./
RUN dotnet restore
RUN dotnet publish "./PHC-HighCounsel-Bot/PHC-HighCounsel-Bot.csproj" -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /App
COPY --from=build-env /App/out .

# Install ffmpeg
RUN apt-get update \
    && apt-get install -y ffmpeg \
    && rm -rf /var/lib/apt/lists/*

# Copy ffmpeg to /App
RUN cp /usr/bin/ffmpeg /App/ffmpeg \
    && chmod +x /App/ffmpeg

ENTRYPOINT ["dotnet", "PHC-HighCounsel-Bot.dll"]
