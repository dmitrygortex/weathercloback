FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["weatherCloChase.Api/weatherCloChase.Api.csproj", "weatherCloChase.Api/"]
COPY ["weatherCloChase.Core/weatherCloChase.Core.csproj", "weatherCloChase.Core/"]
COPY ["weatherCloChase.Infrastructure/weatherCloChase.Infrastructure.csproj", "weatherCloChase.Infrastructure/"]
COPY ["weatherCloChase.ML/weatherCloChase.ML.csproj", "weatherCloChase.ML/"]
RUN dotnet restore "weatherCloChase.Api/weatherCloChase.Api.csproj"
COPY . .
WORKDIR "/src/weatherCloChase.Api"
RUN dotnet build "weatherCloChase.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "weatherCloChase.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Копируем ONNX модель
RUN mkdir -p /app/OnnxModels
# Копируем ONNX модель
COPY --from=build /src/weatherCloChase.ML/OnnxModels/clothing_classifier.onnx /app/OnnxModels/
ENTRYPOINT ["dotnet", "weatherCloChase.Api.dll"]