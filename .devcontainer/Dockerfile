FROM mcr.microsoft.com/vscode/devcontainers/dotnet:5.0

# [Option] Install Node.js
ARG INSTALL_NODE="false"
ARG NODE_VERSION="lts/*"
RUN if [ "${INSTALL_NODE}" = "true" ]; then su vscode -c "umask 0002 && . /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

# [Option] Install Azure CLI
ARG INSTALL_AZURE_CLI="false"
COPY library-scripts/azcli-debian.sh /tmp/library-scripts/
RUN if [ "$INSTALL_AZURE_CLI" = "true" ]; then bash /tmp/library-scripts/azcli-debian.sh; fi \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/* /tmp/library-scripts

# [Optional] Uncomment this section to install additional OS packages.
# RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
#     && apt-get -y install --no-install-recommends <your-package-list-here>

# Update the list of packages
RUN sudo apt-get update
# Install pre-requisite packages.
RUN sudo apt-get install -y wget apt-transport-https software-properties-common
# Download the Microsoft repository GPG keys
RUN wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
# Register the Microsoft repository GPG keys
RUN sudo dpkg -i packages-microsoft-prod.deb
# Update the list of products
RUN sudo apt-get update
# Enable the "universe" repositories
RUN sudo add-apt-repository universe
# Install PowerShell
RUN sudo apt-get install -y powershell
# Install Pester module
RUN pwsh -c Install-Module -Name Pester -Force -AcceptLicense -Scope AllUsers
