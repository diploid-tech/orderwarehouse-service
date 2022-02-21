FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
RUN apk update && apk add --no-cache curl

COPY output/ /app/
# Create non-root user
RUN adduser -u 1000 -S appuser
RUN chown appuser /app /app/* && chmod +x /app/start_service.sh

USER appuser
ENV ASPNETCORE_URLS http://*:5000
ENV ASPNETCORE_ENVIRONMENT Production

# See: https://rehansaeed.com/securing-asp-net-core-in-docker/
ENV COMPlus_EnableDiagnostics=0
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

EXPOSE 5000
WORKDIR /app/
ENTRYPOINT ["./start_service.sh"]

