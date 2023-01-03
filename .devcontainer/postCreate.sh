sudo chown vscode:vscode /workspace/node_modules/
bash -i -c 'nvm install && yarn'
mkdir -p ~/.ssh && cp -r ~/.ssh-localhost/* ~/.ssh && chmod 700 ~/.ssh && chmod 600 ~/.ssh/*