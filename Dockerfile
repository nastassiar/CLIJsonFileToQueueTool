FROM microsoft/dotnet:2.1-sdk As Build-Env

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c release -o out

# Build runtime image
FROM microsoft/dotnet:runtime
WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "hii.dll"]
CMD ["C:\SourceFiles\HyperSpectral\automation\project-med.json"]