#!/usr/bin/env bash

application_name=$1
container_id=$(docker inspect "registry.heroku.com/$application_name/web" --format="{{.Id}}")
curl \
    --request PATCH "https://api.heroku.com/apps/$application_name/formation" \
    --netrc \
    --data "{\"updates\": [{ \"type\": \"web\", \"docker_image\": \"$container_id\" }]}" \
    --header "Content-Type: application/json" \
    --header "Accept: application/vnd.heroku+json; version=3.docker-releases" \
    --header "Authorization: Bearer $HEROKU_TOKEN"
