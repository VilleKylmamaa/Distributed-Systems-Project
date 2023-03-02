
# Distributed Systems Project

Work in progress.


---

The whole project can be run with a single `docker compose up` command.

The images pulled will be `.NET 6.0`, `Redis 7.0.8`, and `Node:18.14.0`.

1. Install and run [Docker](https://www.docker.com/) if not already installed or running.

2. Open command prompt in the **root folder**

3. Create a Docker network: `docker network create --subnet=177.17.0.0/16 distr_network`

4. Run: `docker compose up`





<!--
`docker network create --subnet=177.17.0.0/16 distr_network`

**3. Pull Redis Docker image:**

`docker pull redis`


**4. Run Redis Cluster container:**

`docker compose --file ./Backplane/Cluster/docker-compose.yml up`


**5. Build application server Docker image:**

`docker build -t distr_chat:1 "./Application Server"`


**6. Run two application server containers:**

`docker run -d --hostname distr_server_1 --net distr_network --ip 177.17.0.5 --add-host redis:177.17.0.255 -p 5000:80 --name distr_server_1 distr_chat:1`

`docker run -d --hostname distr_server_2 --net distr_network --ip 177.17.0.6 --add-host redis:177.17.0.255 -p 5001:80 --name distr_server_2 distr_chat:1`


**7. Build frontend (will pull [node image](https://hub.docker.com/_/node/)) Docker image:**

`docker build -t distr_frontend:1 ./Frontend`

**8. Run frontend Docker container:**

`docker run -d --hostname distr_frontend --net distr_network -p 3000:3000 --name distr_frontend distr_frontend:1`
 -->
