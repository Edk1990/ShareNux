# Guia de Configuração do Ambiente do Zero (Linux / WSL2)

Este documento contém o passo a passo completo para configurar o ambiente de desenvolvimento, compilação e teste do **ShareX para Linux** a partir do zero em qualquer distribuição ou no WSL2.

---

## 1. Pré-requisitos de Sistema

O ShareX para Linux utiliza **.NET 10** e compilação **Native AOT**, gerando um binário ELF estático nativo. Para isso, o sistema precisa dos compiladores de C/C++ (`clang`) e bibliotecas de desenvolvimento de sistema.

### 1.1. Ubuntu / Debian / Pop!_OS / Linux Mint

```bash
# 1. Atualizar repositórios
sudo apt update && sudo apt upgrade -y

# 2. Instalar ferramentas de compilação essenciais e Native AOT
sudo apt install -y build-essential clang zlib1g-dev libkrb5-dev libdbus-1-dev

# 3. Ferramentas auxiliares de desktop (Wayland/X11 e Clipboard)
sudo apt install -y xdg-desktop-portal wl-clipboard xclip libnotify-bin
```

### 1.2. Fedora / RHEL

```bash
# 1. Ferramentas de compilação e Native AOT
sudo dnf install -y clang zlib-devel krb5-devel dbus-devel glibc-devel gcc

# 2. Ferramentas auxiliares de desktop
sudo dnf install -y xdg-desktop-portal wl-clipboard xclip libnotify
```

### 1.3. Arch Linux / Manjaro

```bash
# 1. Ferramentas de compilação e Native AOT
sudo pacman -Syu --needed base-devel clang zlib krb5 dbus

# 2. Ferramentas auxiliares de desktop
sudo pacman -S --needed xdg-desktop-portal wl-clipboard xclip libnotify grim slurp
```

---

## 2. Instalação do .NET 10 SDK

Verifique se o SDK do .NET 10 está instalado:

```bash
dotnet --version
```

Caso não esteja instalado, utilize o script oficial da Microsoft:

```bash
# Baixar o script oficial de instalação
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh

# Instalar o SDK do .NET 10
./dotnet-install.sh --channel 10.0

# Adicionar ao PATH (~/.bashrc ou ~/.zshrc)
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
source ~/.bashrc
```

---

## 3. Clonando e Compilando o Repositório

### 3.1. Clone do Repositório

```bash
git clone https://github.com/ShareX/ShareX.git
cd ShareX
```

### 3.2. Restaurando Dependências

```bash
dotnet restore ShareX.sln
```

### 3.3. Compilando as Bibliotecas do Core

Para validar que o core agnóstico compila sem erros:

```bash
# Compilar HelpersLib em net10.0 puro
dotnet build ShareX.HelpersLib/ShareX.HelpersLib.csproj -f net10.0 -c Release

# Compilar UploadersLib em net10.0 puro
dotnet build ShareX.UploadersLib/ShareX.UploadersLib.csproj -f net10.0 -c Release

# Compilar o Editor de Imagens (Avalonia)
dotnet build ShareX.ImageEditor.App/ShareX.ImageEditor.App.csproj -c Release
```

### 3.4. Compilando o Executável Linux com Native AOT

Para gerar o binário único de alta velocidade:

```bash
dotnet publish ShareX.Linux/ShareX.Linux.csproj \
  -r linux-x64 \
  -c Release \
  -p:PublishAot=true \
  -o ./bin/publish
```

O arquivo gerado em `./bin/publish/sharex` é um binário nativo executável sem dependências externas.

---

## 4. Testando a Execução

### 4.1. Verificando a Versão e Ajuda
```bash
./bin/publish/sharex --version
./bin/publish/sharex --help
```

### 4.2. Testando Upload de Arquivo com SXCU
```bash
./bin/publish/sharex /caminho/para/uma/imagem.png --config meu_uploader.sxcu
```

### 4.3. Testando Captura de Tela
```bash
# Captura de tela inteira
./bin/publish/sharex -m fullscreen -n -c

# Captura de região com edição de imagem
./bin/publish/sharex -m region -e
```

---

## 5. Resolução de Problemas Comuns (Troubleshooting)

### Erro: `clang: error: linker command failed with exit code 1 (cannot find -lz)`
* **Causa:** Falta a biblioteca `zlib` de desenvolvimento.
* **Solução:**
  * Debian/Ubuntu: `sudo apt install zlib1g-dev`
  * Fedora: `sudo dnf install zlib-devel`
  * Arch: `sudo pacman -S zlib`

### Erro: `org.freedesktop.portal.Error.NotFound` ao tentar capturar tela
* **Causa:** O backend do portal XDG não está em execução no desktop environment.
* **Solução:**
  * Instale o backend correspondente ao seu ambiente:
    * GNOME: `xdg-desktop-portal-gnome`
    * KDE Plasma: `xdg-desktop-portal-kde`
    * Wlroots / Hyprland / Sway: `xdg-desktop-portal-wlr` ou `xdg-desktop-portal-hyprland`
  * O ShareX também possui fallback automático para `grim`/`slurp` caso o portal esteja desativado.
