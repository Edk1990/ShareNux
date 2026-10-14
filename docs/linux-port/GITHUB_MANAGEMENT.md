# Guia de Gestão e Rastreabilidade no GitHub

Este documento orienta como configurar e utilizar o **GitHub Projects**, **Issues** e **Milestones** para acompanhar a evolução do ShareX Linux com padrões de engenharia de software profissional.

---

## 1. Configurando o GitHub Projects

O **GitHub Projects** (v2) é gratuito e permite visualizar o progresso tanto em visão de **Quadro Kanban** quanto em **Tabela / Roadmap**.

### 1.1. Criar o Projeto
1. Acesse o seu repositório no GitHub: `https://github.com/<seu-usuario>/ShareX`.
2. Clique na aba superior **Projects** $\rightarrow$ **New project**.
3. Selecione o template **Board** (Kanban) e dê o nome: `ShareX Linux Port`.

### 1.2. Configurar as Colunas do Quadro
Crie as seguintes colunas de fluxo de trabalho:
* 📥 **Backlog:** Tarefas futuras mapeadas no roadmap que ainda não foram iniciadas.
* 📋 **Todo (A Fazer):** Tarefas do Milestone atual prontas para execução.
* ⚙️ **In Progress (Em Andamento):** Tarefa que você está desenvolvendo no momento (mantenha no máximo 1 ou 2 simultâneas).
* 🔍 **Review / Test:** Código escrito aguardando validação ou teste de compilação.
* ✅ **Done (Concluído):** Tarefas com testes e Definition of Done atendidos.

---

## 2. Configurando os Milestones no GitHub

Os Milestones agrupam as tarefas por grandes entregas de versão.

Acesse a aba **Issues** $\rightarrow$ **Milestones** $\rightarrow$ **New milestone** e crie os 6 marcos definidos no [`ROADMAP.md`](file:///c:/ShareX/docs/linux-port/ROADMAP.md):

1. **`M1: Fundação & Desacoplamento`** — Suporte a `net10.0` puro em HelpersLib e UploadersLib.
2. **`M2: CLI Mínimo & Native AOT`** — Executável ELF `sharex` com upload funcional via linha de comando.
3. **`M3: Captura de Tela Nativa`** — Integração de screenshots com Wayland Portals e X11.
4. **`M4: Pipeline de Saída`** — Notificações D-Bus e cópia para clipboard nativo (`wl-copy`/`xclip`).
5. **`M5: Editor de Imagens (Avalonia)`** — Conexão com o `ShareX.ImageEditor.App` para anotações.
6. **`M6: Daemon & Empacotamento`** — Atalhos globais em background e pacotes AUR / Flatpak.

---

## 3. Configurando as Labels (Etiquetas)

Crie as seguintes etiquetas no repositório (**Issues** $\rightarrow$ **Labels** $\rightarrow$ **New label**) para categorizar o trabalho:

### 🏷️ Área / Módulo
* `area:core` — Lógica compartilhada do ShareX (`HelpersLib`, `UploadersLib`).
* `area:cli` — Interface de linha de comando (`System.CommandLine`).
* `area:capture` — Subsistema de captura de tela (XDG Portal, PipeWire, X11).
* `area:ui` — Interface gráfica Avalonia / Editor de imagem.
* `area:packaging` — Scripts de empacotamento (`PKGBUILD`, Flatpak, CI/CD).

### 🏷️ Plataforma
* `platform:wayland` — Específico para servidores Wayland.
* `platform:x11` — Específico para sessões X11.
* `platform:linux` — Genérico para Linux.

### 🏷️ Tipo
* `feat` — Nova funcionalidade.
* `fix` — Correção de bug.
* `refactor` — Desacoplamento ou melhoria estrutural sem alterar comportamento.
* `docs` — Documentação técnica.

---

## 4. Cadastrando as Primeiras Issues (Milestone 1)

Você pode criar as primeiras issues diretamente no GitHub copiando os modelos abaixo:

### Issue 1: Desacoplar ShareX.HelpersLib para net10.0
* **Título:** `[M1-T1/T2] Desacoplar ShareX.HelpersLib para suporte a net10.0 puro`
* **Milestone:** `M1: Fundação & Desacoplamento`
* **Labels:** `area:core`, `platform:linux`, `refactor`
* **Descrição:**
  ```markdown
  ### Descrição
  Ajustar o `ShareX.HelpersLib.csproj` para suportar multi-targeting (`net10.0;net10.0-windows`) e condicionar a referência de `Microsoft.WindowsDesktop.App` apenas para builds Windows.

  ### Tarefas
  - [ ] Atualizar TargetFrameworks no .csproj
  - [ ] Isolar `Microsoft.WindowsDesktop.App` com condição de SO
  - [ ] Isolar chamadas de System.Drawing com `#if WINDOWS`

  ### Critério de Aceite
  `dotnet build ShareX.HelpersLib/ShareX.HelpersLib.csproj -f net10.0` compila sem erros.
  ```

### Issue 2: Desacoplar ShareX.UploadersLib para net10.0
* **Título:** `[M1-T4/T5] Permitir compilação de ShareX.UploadersLib em net10.0`
* **Milestone:** `M1: Fundação & Desacoplamento`
* **Labels:** `area:core`, `platform:linux`, `refactor`
* **Descrição:**
  ```markdown
  ### Descrição
  Habilitar `net10.0` no `ShareX.UploadersLib.csproj` e garantir que o motor de upload via HTTP (incluindo SXCU) funcione no Linux.

  ### Tarefas
  - [ ] Atualizar TargetFrameworks no .csproj
  - [ ] Isolar diálogos WinForms legados de OAuth com `#if WINDOWS`
  - [ ] Validar compilação do projeto no alvo net10.0

  ### Critério de Aceite
  `dotnet build ShareX.UploadersLib/ShareX.UploadersLib.csproj -f net10.0` compila com 0 erros.
  ```

---

## 5. Fluxo de Trabalho com Git (Branching & Commits)

Para garantir que o código seja rastreável e você possa recomeçar ou reverter qualquer passo:

### 5.1. Convenção de Branches
* **Branch principal:** `develop` ou `main`.
* **Branches de funcionalidade:** `feature/<milestone>-<nome-da-tarefa>`
  * Exemplo: `feature/m1-helperslib-net10`
  * Exemplo: `feature/m2-cli-skeleton`

### 5.2. Convenção de Commits
Adote o padrão *Conventional Commits*:
* `feat(linux): adiciona suporte a captura de tela via XDG portal`
* `refactor(helpers): isola chamadas de WindowsDesktop.App para net10.0`
* `docs(roadmap): atualiza critérios de aceite do Milestone 2`

### 5.3. Vinculando Pull Requests a Issues
Ao abrir um Pull Request para a branch principal, inclua no comentário:
```
Closes #1
```
Isso fará com que o GitHub feche a Issue automaticamente e mova o card no Kanban para **Done** assim que o merge for concluído!
