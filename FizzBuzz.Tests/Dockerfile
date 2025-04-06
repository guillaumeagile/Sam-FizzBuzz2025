# Stage 1: Build and Test
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app


ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FizzBuzz.Tests/FizzBuzz.Tests.csproj", "FizzBuzz.Tests/"]
COPY ["FizzBuzz.Engine/FizzBuzz.Engine.csproj", "FizzBuzz.Engine/"]
RUN dotnet restore "FizzBuzz.Tests/FizzBuzz.Tests.csproj"
COPY . .
WORKDIR "/src/FizzBuzz.Tests"
RUN dotnet build "FizzBuzz.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app
#RUN dotnet publish -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS test
WORKDIR /app
COPY --from=build /app/build ./


# entry point to run tests
ENTRYPOINT ["dotnet", "test", "FizzBuzz.Tests.dll", "--collect", "XPlat Code Coverage"]

# command line to build container (and use it later to run tests), from root directory
# docker build -t fizzbuzztests -f FizzBuzz.Tests/Dockerfile .

# command line to run tests in container
# docker run -t fizzbuzztests

# run in interactive mode
# docker run -it fizzbuzztests