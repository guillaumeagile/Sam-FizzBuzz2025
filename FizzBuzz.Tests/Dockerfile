FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
USER $APP_UID
WORKDIR /app/build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FizzBuzz.Tests/FizzBuzz.Tests.csproj", "FizzBuzz.Tests/"]
COPY ["FizzBuzz.Engine/FizzBuzz.Engine.csproj", "FizzBuzz.Engine/"]
RUN dotnet restore "FizzBuzz.Tests/FizzBuzz.Tests.csproj"
COPY . .
WORKDIR "/src/FizzBuzz.Tests"
RUN dotnet build "FizzBuzz.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS final
WORKDIR /app
COPY --from=build /app/build .
WORKDIR "/app" 


# entry point to run tests
ENTRYPOINT ["dotnet", "test", "FizzBuzz.Tests.dll", "--collect", "XPlat Code Coverage"]

# command line to build container (and use it later to run tests), from root directory
# docker build -t fizzbuzztests -f FizzBuzz.Tests/Dockerfile .

# command line to run tests in container
# docker run -t fizzbuzztests

# run in interactive mode
# docker run -it fizzbuzztests