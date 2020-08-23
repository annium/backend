FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
COPY . /code
RUN dotnet publish -c Release -o /dist /code

FROM nginx:alpine
COPY --from=build /dist/wwwroot/ /usr/share/nginx/html/
COPY ./nginx.conf /etc/nginx/nginx.conf