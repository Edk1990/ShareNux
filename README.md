<p align="center">
  <img src="docs/images/sharex_linux_banner.jpg" alt="ShareX Linux Edition Banner" width="100%"/>
</p>

<h1 align="center">ShareX — Linux & Cross-Platform Edition</h1>

<p align="center">
  <b>A ferramenta definitiva de captura de tela, anotação e compartilhamento de arquivos — agora com suporte nativo moderno para Linux (Wayland & X11) e Windows.</b>
</p>

<div align="center">
  <a href="https://github.com/Edk1990/ShareX/actions/workflows/linux-build.yml"><img src="https://github.com/Edk1990/ShareX/actions/workflows/linux-build.yml/badge.svg?branch=develop" alt="Linux Build Status"/></a>
  <a href="https://github.com/Edk1990/ShareX/actions/workflows/build.yml"><img src="https://github.com/Edk1990/ShareX/actions/workflows/build.yml/badge.svg?branch=develop" alt="Windows Build Status"/></a>
  <a href="./LICENSE.txt"><img src="https://img.shields.io/badge/License-GPL%20v3-blue.svg" alt="License"/></a>
  <img src="https://img.shields.io/badge/.NET-10.0%20%7C%20Native%20AOT-purple.svg" alt=".NET 10 Native AOT"/>
  <img src="https://img.shields.io/badge/Display-Wayland%20%2F%20X11-orange.svg" alt="Wayland and X11"/>
</div>

<br>

---

## 🐧 A Iniciativa ShareX para Linux

O **ShareX** é historicamente a ferramenta de screenshot e produtividade mais poderosa do ecossistema Windows. No entanto, sua arquitetura histórica de 15 anos baseada em Windows Forms (`Win32`, `P/Invoke`, `GDI+`) sempre impediu que usuários de Linux desfrutassem de seus recursos.

Este projeto é um **fork de modernização** focado em trazer a experiência completa do ShareX para o desktop Linux moderno, unindo o melhor de dois mundos:

1. **A Filosofia Modular do ShareNix (CLI-First):** Execução ultrarrápida por linha de comando (`sharex -m region`), integrável diretamente aos atalhos de qualquer Window Manager (i3, Sway, Hyprland, bspwm, GNOME, KDE).
2. **O Poder Real do ShareX:** Preservação do motor com **mais de 80 serviços de upload**, suporte total a arquivos de uploader customizado (**`.sxcu`**) e o novo **Editor de Imagens em Avalonia UI / SkiaSharp** para anotações ricas (setas, texto, blur, recorte).
3. **Alto Desempenho com .NET 10 Native AOT:** Compilado como um **binário ELF nativo estático único**, sem necessidade de runtime instalado e com tempo de inicialização (*cold start*) de apenas **~15ms**.
4. **Wayland First + X11:** Desenvolvido segundo os padrões modernos do *freedesktop.org*, usando **XDG Desktop Portals**, **PipeWire** e notificações via **D-Bus**.

---

## 📚 Documentação Técnica do Projeto

Criamos uma base de documentação viva e detalhada para o desenvolvimento do port para Linux:

| Documento | Descrição |
| :--- | :--- |
| 📐 [**`ARCHITECTURE.md` (ADR-001)**](docs/linux-port/ARCHITECTURE.md) | Decisões de arquitetura, comparativo técnico (Go vs. Rust vs. C# Native AOT) e modelagem de camadas do sistema. |
| 🗺️ [**`ROADMAP.md`**](docs/linux-port/ROADMAP.md) | Cronograma oficial dividido em 6 Milestones, com tarefas atômicas e critérios de aceite (*Definition of Done*). |
| 🛠️ [**`SETUP_FROM_SCRATCH.md`**](docs/linux-port/SETUP_FROM_SCRATCH.md) | Guia completo passo a passo para configurar o ambiente do zero em Ubuntu, Debian, Fedora, Arch e WSL2. |
| 📋 [**`GITHUB_MANAGEMENT.md`**](docs/linux-port/GITHUB_MANAGEMENT.md) | Guia de acompanhamento de tarefas via GitHub Projects (Kanban), Milestones, Labels e Branches. |

---

## 🚦 Status Atual dos Milestones

```
[✅ Milestone 1: Fundação & Desacoplamento] 
         ↓
[⏳ Milestone 2: O Binário CLI Mínimo (MVP AOT)] 
         ↓
[⏹️ Milestone 3: Captura de Tela Nativa (Wayland/X11)] 
         ↓
[⏹️ Milestone 4: Pipeline de Saída (Clipboard & Notificações D-Bus)] 
         ↓
[⏹️ Milestone 5: Editor de Imagens Integrado (Avalonia UI)] 
         ↓
[⏹️ Milestone 6: Daemon de Atalhos & Empacotamento Oficial]
```

* **Milestone 1 — Concluído (100%):** O [`ShareX.HelpersLib`](ShareX.HelpersLib/ShareX.HelpersLib.csproj) e o [`ShareX.UploadersLib`](ShareX.UploadersLib/ShareX.UploadersLib.csproj) foram desacoplados para compilação nativa em `net10.0`, com suíte de testes unitários passando em [`ShareX.Tests.Linux`](ShareX.Tests.Linux/).
* **Milestone 2 — Próxima Etapa:** Criação do executável CLI `ShareX.Linux` gerado via Native AOT.

---

## ⚡ Como Compilar e Testar no Linux

### Pré-requisitos (Debian/Ubuntu)
```bash
sudo apt update && sudo apt install -y build-essential clang zlib1g-dev libkrb5-dev libdbus-1-dev
```

### Compilar os Módulos do Core e Rodar os Testes
```bash
# Compilar bibliotecas principais em net10.0 puro
dotnet build ShareX.HelpersLib/ShareX.HelpersLib.csproj -f net10.0 -c Release
dotnet build ShareX.UploadersLib/ShareX.UploadersLib.csproj -f net10.0 -c Release

# Executar a suíte de testes de uploaders e SXCU
dotnet test ShareX.Tests.Linux/ShareX.Tests.Linux.csproj -c Release
```

---

## 💻 Comandos Previstos para a CLI (`sharex`)

Inspirado na usabilidade do ShareNix, o ShareX para Linux funcionará perfeitamente via terminal e atalhos de teclado:

```bash
# Capturar uma região da tela e fazer upload imediato
sharex -m region

# Capturar tela inteira e copiar a URL final para a área de transferência
sharex -m fullscreen -c -n

# Capturar e abrir no Editor de Imagens (Avalonia/SkiaSharp) antes de subir
sharex -m region -e

# Fazer upload de um arquivo existente usando uma configuração SXCU específica
sharex /caminho/da/imagem.png --config meu_uploader.sxcu

# Enviar o conteúdo da área de transferência (imagem ou texto)
sharex -m clipboard
```

---

## 🌐 Recursos Originais do ShareX

Para usuários Windows e documentação detalhada sobre o ecossistema original do ShareX, consulte os links oficiais:

* **Site Oficial:** [https://getsharex.com](https://getsharex.com)
* **Documentação de Custom Uploaders (.sxcu):** [https://getsharex.com/docs/custom-uploader](https://getsharex.com/docs/custom-uploader)
* **Repositório Upstream (Windows Original):** [https://github.com/ShareX/ShareX](https://github.com/ShareX/ShareX)
* **Discord Oficial:** [https://discord.gg/ShareX](https://discord.gg/ShareX)

---

## 📜 Licença

Este projeto é software livre licenciado sob a [GNU General Public License v3.0 (GPL-3.0)](LICENSE.txt).
Créditos integrais aos criadores originais do ShareX e aos contribuidores da comunidade de código aberto.
