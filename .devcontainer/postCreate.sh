sudo chown vscode:vscode /workspace/node_modules/
mkdir -p ~/.ssh && cp -r ~/.ssh-localhost/* ~/.ssh && chmod 700 ~/.ssh && chmod 600 ~/.ssh/*
bash -i -c 'nvm install && yarn'

curl https://dl.min.io/client/mc/release/linux-amd64/mc \
  --create-dirs \
  -o $HOME/minio-binaries/mc

chmod +x $HOME/minio-binaries/mc
export PATH=$PATH:$HOME/minio-binaries/

mc alias set myminio http://file:9000 minioadmin minioadmin
mc admin prometheus generate myminio
