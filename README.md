SandBox
=======

## Compose development container

This repository includes a Compose file that works with either Docker Compose or
Podman Compose. The service uses a fully qualified image name so Podman does not
prompt for a registry when it pulls the Mono image.

Start the development container with Podman:

```sh
podman compose up
```

If your Podman installation uses the standalone Compose wrapper, run:

```sh
podman-compose up
```

Or with Docker:

```sh
docker compose up
```

Open a shell in the running container:

```sh
podman compose exec dev sh
```

The repository is bind-mounted at `/workspace` inside the container.
