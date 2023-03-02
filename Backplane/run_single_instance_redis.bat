:: Pull Redis image from Docker Hub if not already pulled
docker pull redis

:: Run Redis container in distr_network ip 177.17.0.255 in port 6379
docker run -d --hostname redis --name redis_backplane --net distr_network --ip 177.17.0.255 --publish 6371:6371 redis

:: Enter container and execute "redis-cli MONITOR" to see Redis output in the command prompt
docker exec -it redis_backplane bash -c "redis-cli MONITOR"
