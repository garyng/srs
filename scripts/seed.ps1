$env:SrsConfig__ShouldSeedDatabase="true"
docker compose -f "$PSScriptRoot/../src/docker-compose.yml" up srs.api
$env:SrsConfig__ShouldSeedDatabase="false"

docker compose -f "$PSScriptRoot/../src/docker-compose.yml" up srs.api -d