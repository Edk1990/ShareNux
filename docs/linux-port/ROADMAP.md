# Roadmap Oficial: ShareX Linux Port

> Este documento detalha todas as fases, épicos e tarefas atômicas da iniciativa de port do ShareX para Linux.  
> Cada tarefa possui uma etiqueta identificadora (ex: `[M1-T1]`) para permitir rastreamento direto no **GitHub Projects** e **GitHub Issues**.

---

## 🗺️ Visão Geral dos Milestones

```
[M1: Fundação & Desacoplamento]
         ↓
[M2: CLI Mínimo & Native AOT]
         ↓
[M3: Captura de Tela Nativa (Wayland/X11)]
         ↓
[M4: Pipeline de Saída (Clipboard & Notificações)]
         ↓
[M5: Integração do Editor de Imagens (Avalonia)]
         ↓
[M6: Daemon de Atalhos & Empacotamento Oficial]
```

---

## 📌 Milestone 1: Fundação & Desacoplamento do Core

**Objetivo:** Garantir que as bibliotecas de lógica de negócio e upload do ShareX compilem de forma limpa em ambiente Linux (`net10.0`), isolando qualquer chamada legada do Windows sob diretivas de compilação.

### Tarefas
- [x] **[M1-T1]** Auditar e ajustar [`ShareX.HelpersLib.csproj`](file:///c:/ShareX/ShareX.HelpersLib/ShareX.HelpersLib.csproj) para multi-targeting (`<TargetFrameworks>net10.0;net10.0-windows</TargetFrameworks>`).
- [x] **[M1-T2]** Condicionar a dependência de `Microsoft.WindowsDesktop.App` no `HelpersLib` apenas para a plataforma Windows (`Condition="$(TargetFramework.EndsWith('-windows'))"`).
- [x] **[M1-T3]** Isolar métodos legados de `System.Drawing.Common` (impressão e ícones shell) no `HelpersLib` com `#if WINDOWS`.
- [x] **[M1-T4]** Ajustar [`ShareX.UploadersLib.csproj`](file:///c:/ShareX/ShareX.UploadersLib/ShareX.UploadersLib.csproj) para multi-targeting (`net10.0;net10.0-windows`).
- [x] **[M1-T5]** Isolar classes de uploaders dependentes de WinForms (ex: diálogo OAuth antigo do Google Drive/Dropbox) para usar URLs diretas no Linux.
- [x] **[M1-T6]** Criar projeto de teste unitário `ShareX.Tests.Linux` executando em `net10.0` para validar o carregamento e parsing de arquivos `.sxcu`.

### Critérios de Aceite (Definition of Done)
1. `dotnet build ShareX.HelpersLib -f net10.0` compila com **0 erros**.
2. `dotnet build ShareX.UploadersLib -f net10.0` compila com **0 erros**.
3. O build padrão do Windows (`net10.0-windows`) permanece 100% funcional sem quebras.

---

## 📌 Milestone 2: O Binário CLI Mínimo Viável (MVP)

**Objetivo:** Criar o novo executável da aplicação Linux (`ShareX.Linux`), capaz de rodar via terminal com suporte a Native AOT para inicialização instantânea.

### Tarefas
- [ ] **[M2-T1]** Criar o projeto `ShareX.Linux/ShareX.Linux.csproj` com `<TargetFramework>net10.0</TargetFramework>` e `<OutputType>Exe</OutputType>`.
- [ ] **[M2-T2]** Adicionar suporte a **Native AOT** (`<PublishAot>true</PublishAot>`) e configurar o `rd.xml` para reflexão em serialização JSON caso necessário.
- [ ] **[M2-T3]** Integrar biblioteca de linha de comando (`System.CommandLine`) para suporte a argumentos robustos.
- [ ] **[M2-T4]** Implementar subcomando de upload de arquivo existente:
  * Exemplo: `sharex upload /caminho/do/arquivo.png` ou `sharex /caminho/do/arquivo.png`.
- [ ] **[M2-T5]** Implementar flags de controle: `--silent` / `-q`, `--uploader <nome>`, `--config <caminho.sxcu>`.
- [ ] **[M2-T6]** Implementar exibição de versão e ajuda com branding limpo (`sharex --version`, `sharex --help`).

### Critérios de Aceite (Definition of Done)
1. O comando `dotnet publish ShareX.Linux -r linux-x64 -c Release` gera um único arquivo executável ELF nativo (`sharex`).
2. Executar `./sharex /tmp/imagem.png --config custom.sxcu` realiza o upload para o servidor e imprime a URL resultante no stdout.
3. Tempo de inicialização do executável AOT inferior a **25 milissegundos**.

---

## 📌 Milestone 3: Captura de Tela Nativa (Wayland & X11)

**Objetivo:** Adicionar a capacidade de tirar screenshots no Linux tanto em ambientes modernos Wayland (via XDG Portals) quanto em sessões X11.

### Tarefas
- [ ] **[M3-T1]** Criar a abstração `IScreenCaptureHost` em `ShareX.Linux/Capture/`.
- [ ] **[M3-T2]** Implementar `WaylandPortalScreenCaptureHost` usando chamadas D-Bus para a interface `org.freedesktop.portal.Screenshot`.
- [ ] **[M3-T3]** Implementar suporte a seleção de tela cheia (`sharex -m fullscreen` / `-m fs`).
- [ ] **[M3-T4]** Implementar suporte a seleção de região (`sharex -m region` / `-m s`).
- [ ] **[M3-T5]** Implementar mecanismo de *fallback* resiliente para ambientes que não possuem portais ativos:
  * Wayland: `grim` + `slurp`.
  * X11: `maim` ou `scrot`.
- [ ] **[M3-T6]** Salvar a captura gerada na pasta padrão de histórico do usuário (`$XDG_DATA_HOME/ShareX/Screenshots/`).

### Critérios de Aceite (Definition of Done)
1. Em sessão GNOME/KDE Wayland, `sharex -m fs` captura toda a tela sem erros de autorização.
2. Em sessão Wayland (Sway/Hyprland), `sharex -m region` abre o seletor de área e grava o PNG.
3. Se o portal falhar, o fallback executa transparentemente.

---

## 📌 Milestone 4: Pipeline de Saída (Clipboard & Notificações D-Bus)

**Objetivo:** Prover integração perfeita com o desktop Linux sem carregar dependências pesadas de bibliotecas de interface como GTK.

### Tarefas
- [ ] **[M4-T1]** Criar serviço de notificação nativa `DBusNotificationService` via `org.freedesktop.Notifications`.
- [ ] **[M4-T2]** Exibir notificação de "Upload concluído" com:
  * Título do serviço de upload.
  * Botão de ação "Abrir no Navegador" (`xdg-open`).
  * Botão de ação "Copiar URL".
- [ ] **[M4-T3]** Criar serviço de área de transferência `LinuxClipboardService`:
  * Wayland: integração com protocolo de clipboard ou chamada a `wl-copy`.
  * X11: integração via `xclip` / `xsel`.
- [ ] **[M4-T4]** Suportar flag `-c` para copiar automaticamente a URL final para a área de transferência.
- [ ] **[M4-T5]** Suportar flag `-o` para abrir automaticamente o link no navegador padrão após o upload.

### Critérios de Aceite (Definition of Done)
1. Ao término do upload, o link aparece na área de transferência e pode ser colado com `Ctrl+V`.
2. A notificação de sistema do GNOME/KDE/Dunst exibe o preview do link e o clique abre a página no navegador.

---

## 📌 Milestone 5: Integração do Editor de Anotações (Avalonia UI)

**Objetivo:** Permitir que o usuário adicione setas, texto, blur e recorte na captura de tela antes de salvá-la ou enviá-la para a nuvem.

### Tarefas
- [ ] **[M5-T1]** Conectar o projeto [`ShareX.ImageEditor.App`](file:///c:/ShareX/ShareX.ImageEditor.App) (Avalonia/SkiaSharp) ao pipeline da CLI.
- [ ] **[M5-T2]** Implementar a flag `-e` / `--edit` no comando de captura (`sharex -m region -e`).
- [ ] **[M5-T3]** Passar o stream da imagem capturada para o editor em memória (sem I/O desnecessário de disco).
- [ ] **[M5-T4]** Ao clicar em "Salvar/Enviar" no editor, repassar a imagem final renderizada pelo SkiaSharp diretamente para o uploader.

### Critérios de Aceite (Definition of Done)
1. `sharex -m region -e` captura a área, abre instantaneamente a janela do editor em Avalonia.
2. É possível desenhar elementos e, ao fechar ou confirmar, o upload é realizado com as anotações visíveis.

---

## 📌 Milestone 6: Daemon de Atalhos Globais & Empacotamento

**Objetivo:** Fornecer execução em segundo plano para capturas instantâneas por teclas de atalho e disponibilizar pacotes prontos para instalação nas principais distribuições Linux.

### Tarefas
- [ ] **[M6-T1]** Integrar [`HotkeyHostFactory.cs`](file:///c:/ShareX/ShareX.HelpersLib/Input/HotkeyHostFactory.cs) para rodar como daemon (`sharex --daemon` / `-d`).
- [ ] **[M6-T2]** Criar serviço do systemd do usuário (`~/.config/systemd/user/sharex.service`).
- [ ] **[M6-T3]** Criar arquivo `.desktop` com ações rápidas para o menu de aplicativos (Capturar Região, Tela Cheia, Histórico).
- [ ] **[M6-T4]** Criar manifesto de empacotamento para **Arch Linux** (`PKGBUILD` para o AUR).
- [ ] **[M6-T5]** Criar manifesto de empacotamento **Flatpak** ou **AppImage**.
- [ ] **[M6-T6]** Configurar workflow do GitHub Actions para compilação e release automático de binários Linux em cada tag (`vX.Y.Z`).

### Critérios de Aceite (Definition of Done)
1. O serviço roda em background e dispara capturas através dos atalhos configurados no sistema.
2. O pacote é instalável em distribuições baseadas em Debian/Ubuntu, Arch Linux e Fedora com um único comando.
