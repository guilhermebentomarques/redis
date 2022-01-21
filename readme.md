# Pre requirements

## Have local docker

	https://docs.docker.com/docker-for-windows/install/

# 1 - Pull the docker

docker pull redis

# 2 - Create docker container to run Redis
# Now create a Redis Container and assign ports , Redis run from 6379 port so assign the inner container port to 6379 so that we will be able to communicate with Redis Instance.
# docker run --name <instance-name> -p  5001:6379 -d redis

docker run --name my-redis -p  5001:6379 -d redis

# List of containers

docker ps

# To Check if Redis is up or not follow below steps,

docker exec -it my-redis sh

# Run redis-cli and you should have an IP

redis-cli

# Run ping and expect the response pong

ping

select 0
scan 0

# After running the solution do the same

select 0
scan 0

# Command to get the keys

MGET e1
MGET TestConsole