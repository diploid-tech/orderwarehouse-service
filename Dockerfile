FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
RUN apk update && apk add --no-cache curl
COPY output/ /root/
ENV ASPNETCORE_URLS http://*:5000
ENV ASPNETCORE_ENVIRONMENT Production

EXPOSE 5000
WORKDIR /root/
RUN chmod +x ./start_service.sh
ENTRYPOINT ["./start_service.sh"]
