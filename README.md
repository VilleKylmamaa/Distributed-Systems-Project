
# Distributed Systems Project

Work in progress.


---

1. Create a Docker network:

`docker network create --subnet=177.17.0.0/16 distr_network`


2. Pull Redis Docker image:

`docker pull redis`


3. Run Redis container:

`docker run -d --hostname redis --name redis_backplane --net distr_network --ip 177.17.0.255 --publish 6379:6379 redis`


4. Build application server Docker image:

`docker build -t distr_chat:1 .`


5. Run two application server containers:

`docker run -d --hostname distr_server_1 --net distr_network --ip 177.17.0.5 --add-host redis:177.17.0.255 -p 5000:80 --name distr_server_1 distr_chat:1`

`docker run -d --hostname distr_server_2 --net distr_network --ip 177.17.0.6 --add-host redis:177.17.0.255 -p 5001:80 --name distr_server_2 distr_chat:1`

