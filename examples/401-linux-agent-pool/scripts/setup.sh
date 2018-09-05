
function check_rc() {
  if [ $? -ne 0 ]
  then
    echo $1
    exit -1
  fi
}

#/bin/bash -e
while [[ $# -gt 0 ]]; do
  case $1 in
    -t|--token) token="$2" ; shift ;;
    *) POSITIONAL+=("$1") ;;
  esac
  shift
done
set -- "${POSITIONAL[@]}" # restore positional parameters

sudo apt-get update
sudo apt-get upgrade -y

# https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-apt?view=azure-cli-latest
echo "Installing Azure CLI 2.0"
AZ_REPO=$(lsb_release -cs)
echo "deb [arch=amd64] https://packages.microsoft.com/repos/azure-cli/ $AZ_REPO main" | \
sudo tee /etc/apt/sources.list.d/azure-cli.list
curl -L https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
sudo apt-get install -y apt-transport-https
sudo apt-get update
sudo apt-get install -y azure-cli  # TODO: upgrade if old. requires a bugfix in 2.0.38-1
check_rc "Failed to install azure-cli"

# https://www.microsoft.com/net/download/linux-package-manager/ubuntu16-04/sdk-current
echo "Installing .NET SDK 2.1"
wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get install -y apt-transport-https
sudo apt-get update 
sudo apt-get install -y dotnet-sdk-2.1
check_rc "Failed to install dotnet-sdk-2.1"

# https://www.mono-project.com/download/stable/#download-lin
echo "Installing Mono-Complete"
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
sudo apt-get install -y apt-transport-https
echo "deb https://download.mono-project.com/repo/ubuntu stable-xenial main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
sudo apt-get update
sudo apt-get install -y mono-complete
check_rc "Failed to install mono-complete"


mkdir /var/lib/vstsagent && chown azureuser:azureuser /var/lib/vstsagent && cd /var/lib/vstsagent
check_rc "Failed to create /var/lib/vstsagent"

curl {{ pool.agentDownloadUrl }} --output vsts-agent.tar.gz
check_rc "Failed to download {{ pool.agentDownloadUrl }}"

for NUM in `seq 1 1 {{ pool.agentCount }}`
do
  sudo -u azureuser mkdir /var/lib/vstsagent/${NUM} && cd /var/lib/vstsagent/${NUM}
  check_rc "Failed to create /var/lib/vstsagent/${NUM}"

  sudo -u azureuser tar -zxf /var/lib/vstsagent/vsts-agent.tar.gz
  check_rc "Failed to unpack /var/lib/vstsagent/vsts-agent.tar.gz"

  ./bin/installdependencies.sh
  check_rc "Failed to run vstsagent installdependencies.sh"

  sudo -u azureuser ./config.sh --url https://{{ vsts.account }}.visualstudio.com --auth pat --token ${token} --unattended --pool {{ pool.name }} --agent `uname -n`-${NUM} --replace --work /var/lib/vstsagent/${NUM}/_work --acceptTeeEula
  check_rc "Failed to run vstsagent config.sh"

  ./svc.sh install azureuser
  check_rc "Failed to install service for vstsagent"

  ./svc.sh start
  check_rc "Failed to start service for vstsagent"
done
