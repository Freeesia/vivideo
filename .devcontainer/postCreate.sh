sudo chown vscode:vscode /workspace/node_modules/
sudo apt-get update
sudo apt-get install --no-install-recommends -y ffmpeg
mkdir -p ~/.ssh && cp -r ~/.ssh-localhost/* ~/.ssh && chmod 700 ~/.ssh && chmod 600 ~/.ssh/*
bash -i -c 'nvm install && yarn'
