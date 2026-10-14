# ShareX Linux Port Initiative

> **Projeto:** Port e Modernização do ShareX para Linux  
> **Estratégia:** Arquitetura Greenfield / CLI-First com C# .NET 10 + Native AOT + Avalonia UI  
> **Status:** Planejamento e Estruturação Inicial  

---

## 📌 Visão Geral

Este diretório contém a documentação técnica oficial, decisões de arquitetura e o planejamento operacional para a criação da versão Linux do **ShareX**.

Em vez de tentar portar a base legada em Windows Forms (repleta de dependências profundas da API Win32 e GDI+), esta iniciativa adota a **filosofia modular do ShareNix** combinada com o **poder das bibliotecas modernas do ShareX** (Uploaders, Avalonia e SkiaSharp), compiladas para binário nativo de alto desempenho via **.NET Native AOT**.

---

## 📚 Índice de Documentos

1. [**`ARCHITECTURE.md`**](file:///c:/ShareX/docs/linux-port/ARCHITECTURE.md)  
   *Architecture Decision Record (ADR)* completo. Detalha a análise do ShareNix, o comparativo de tecnologias (Go vs. Rust vs. C# Native AOT) e a modelagem das camadas do sistema (Wayland/X11, D-Bus, Portals, Core e UI).

2. [**`ROADMAP.md`**](file:///c:/ShareX/docs/linux-port/ROADMAP.md)  
   Cronograma detalhado em 6 Milestones, com tarefas atômicas numeradas, critérios de aceite e *Definition of Done (DoD)* prontos para serem importados no GitHub Projects.

3. [**`SETUP_FROM_SCRATCH.md`**](file:///c:/ShareX/docs/linux-port/SETUP_FROM_SCRATCH.md)  
   Guia passo a passo para configurar o ambiente de desenvolvimento do zero em qualquer distribuição Linux (Ubuntu/Debian, Fedora, Arch, WSL2), incluindo requisitos de compilação Native AOT.

4. [**`GITHUB_MANAGEMENT.md`**](file:///c:/ShareX/docs/linux-port/GITHUB_MANAGEMENT.md)  
   Guia prático para configurar o GitHub Projects (Kanban), Milestones, Labels e fluxo de branches/PRs para acompanhar o desenvolvimento com rastreabilidade total.

---

## 🎯 Pilares do Projeto

* **Velocidade Extrema (Cold Start < 20ms):** Binário ELF estático gerado por Native AOT, sem dependência de runtime ou JIT instalado na máquina.
* **Wayland & X11 Nativos:** Integração com os padrões modernos do freedesktop.org via **XDG Desktop Portals** e D-Bus.
* **Poder do ShareX Mantido:** Suporte nativo ao motor de uploads com mais de 80 serviços, arquivos `.sxcu` e ao editor de anotações em Avalonia/SkiaSharp.
* **Modularidade Unix (CLI-First):** Toda funcionalidade pode ser executada via linha de comando ou atalhos de Window Managers (i3, Sway, Hyprland, GNOME, KDE).
