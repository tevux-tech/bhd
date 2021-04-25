# Warning! Make sure images for each architecture is pushed to docker hub before running this.
# This will only work if experimental docker features are enabled in ~/.docker/config.json ("experimental": "enabled").

docker manifest create girdauskas/bhd:latest --amend girdauskas/bhd:latest-arm32 --amend girdauskas/bhd:latest-arm64 --amend girdauskas/bhd:latest-amd64
docker manifest push girdauskas/bhd:latest