# -------- BUILD STAGE --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy toàn bộ source code
COPY . ./

# Khôi phục packages
RUN dotnet restore

# Build và publish ra thư mục /publish
RUN dotnet publish -c Release -o /publish

# -------- RUNTIME STAGE --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy từ stage build
COPY --from=build /publish ./

# Expose port HTTP
EXPOSE 80

# Chạy app
ENTRYPOINT ["dotnet", "PdfGeneratorApi.dll"]
