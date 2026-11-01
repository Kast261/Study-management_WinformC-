FROM debian:bookworm-slim
RUN apt-get update && apt-get install -y --no-install-recommends \
    mono-runtime libmono-system-windows-forms4.0-cil libmono-system-data4.0-cil \
    libmono-system-core4.0-cil libgdiplus xvfb xauth fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*
CMD ["timeout", "40s", "xvfb-run", "-a", "mono", "/tests/bin/Debug/Pagination.SmokeTests.exe"]
