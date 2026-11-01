"""Run profile regression tests using a disposable PostgreSQL container."""
import argparse
import os
from pathlib import Path
import secrets
import subprocess
import time


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--docker-host")
    args = parser.parse_args()
    docker = ["docker"]
    if args.docker_host:
        docker += ["--host", args.docker_host]
    project = str(Path(__file__).with_name("StudentProfile.Tests.csproj"))
    test_env = dict(os.environ)
    test_env["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"
    subprocess.run(
        [args.dotnet, "restore", project, "--source", "https://api.nuget.org/v3/index.json"],
        env=test_env, check=True)

    password = secrets.token_hex(24)
    container_env = dict(os.environ, POSTGRES_PASSWORD=password)
    container_id = subprocess.check_output(
        docker + ["run", "--rm", "--detach", "--publish", "127.0.0.1::5432",
                  "--env", "POSTGRES_PASSWORD", "--env", "POSTGRES_DB=profile_tests",
                  "postgres:16-bookworm"],
        env=container_env, text=True).strip()
    try:
        deadline = time.monotonic() + 45
        while subprocess.run(
            docker + ["exec", container_id, "pg_isready", "-U", "postgres", "-d", "profile_tests"],
            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL).returncode:
            if time.monotonic() >= deadline:
                raise RuntimeError("Test PostgreSQL did not become ready within 45 seconds")
            time.sleep(0.25)
        address = subprocess.check_output(
            docker + ["port", container_id, "5432/tcp"], text=True).strip()
        port = int(address.rsplit(":", 1)[1])
        test_env["PROFILE_TEST_CONNECTION"] = (
            f"Host=127.0.0.1;Port={port};Database=profile_tests;"
            f"Username=postgres;Password={password}")
        result = subprocess.run(
            [args.dotnet, "test", project, "--no-restore", "--logger", "console;verbosity=normal"],
            env=test_env)
        return result.returncode
    finally:
        subprocess.run(docker + ["rm", "--force", "--volumes", container_id],
                       stdout=subprocess.DEVNULL, check=True)


if __name__ == "__main__":
    raise SystemExit(main())
