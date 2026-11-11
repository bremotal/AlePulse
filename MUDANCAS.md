# AlePulse — Lista de Mudanças (v1.5)

Trabalho feito em cima do commit `a2afba4 (v1.4 - Drag and drop para reordenar exercicios)`.
Nada do que já existia foi removido: beep, vibração, drag-and-drop, busca, perfil, histórico — tudo preservado.

## Correções de bugs

### 1. App fechava sozinho (com treino iniciado ou na tela individual)
- **Causa:** o cronômetro de descanso usava variáveis estáticas dentro da tela (`ExerciseExecutionPage`) e um timer que mexia diretamente nos elementos visuais da página. Ao sair da tela (SAIR / FINALIZAR / troca de tela), a página antiga era descartada, mas o timer continuava disparando nela a cada segundo → exceção não tratada → o Android fechava o app.
- **Solução:** o cronômetro agora mora num serviço global novo, `Services/RestTimerService.cs`. Ele guarda apenas a hora final (em UTC) e emite eventos (`Tick`/`Finished`). As telas assinam os eventos só enquanto estão visíveis (`OnAppearing`/`OnDisappearing`) e nunca mais há código tocando em tela morta. O beep + vibração agora disparados pelo serviço, uma única vez, mesmo se você trocar de tela durante o descanso.

### 2. Cronômetro regressivo congelava faltando 1 segundo
- **Causa:** o contador dependia do tick de 1 segundo do sistema, que o Android suspende quando a tela apaga ou o app vai para segundo plano (comum entre séries). Ao voltar, nada recalculava — o número ficava parado e o beep não tocava.
- **Solução:** o `RestTimerService` recalcula o tempo restante a cada disparo a partir da hora final absoluta. Se o sistema atrasar (tela apagada), no primeiro tick após voltar o número se corrige sozinho e o beep/vibração disparam na hora certa. O timer também nunca "morre": ele fica permanentemente ativo e barato (checa apenas se existe descanso rodando).

## Melhorias

### 3. SAIR da tela de treino NÃO encerra mais o treino
- Novo serviço global `Services/ActiveWorkoutSession.cs` guarda o treino ativo: ID, exercícios, posição atual e a hora de início.
- Ao apertar SAIR na execução, você volta para a tela do treino e a sessão **continua ativa** (o cronômetro de descanso também continua rodando e toca na hora certa).
- Na tela do treino, o botão laranja muda de **INICIAR** para **CONTINUAR** enquanto houver treino em andamento. Ao continuar, você volta exatamente no exercício onde parou, com o tempo de treino preservado (já lendo pesos/reps/descanso atualizados no servidor).
- Uso: SAIR → ajustar pesos, repetições, descanso (✏️), adicionar/reordenar exercícios → **CONTINUAR** → segue o treino do ponto onde parou.
- O treino só é encerrado de verdade ao tocar **FINALIZAR TREINO** no último exercício.

### 4. Tempo total do treino agora é o tempo REAL (início → fim)
- **Causa do erro:** o tempo era medido a partir da abertura da tela de execução (`DateTime.Now - _workoutStartTime`), ou seja, zerava a cada vez que você reabria o treino. Pior: o resumo formatava o resultado com o formato `mm:ss`, que **descartava as horas** — um treino de 1h05 aparecia como "05:xx". Por isso parecia que só os descansos eram contados.
- **Solução:** a duração agora vem de `ActiveWorkoutSession.Elapsed` (do INICIAR/CONTINUAR até o FINALIZAR TREINO, inclusive SAIRs no meio). E o resumo exibe `hh:mm:ss` quando o treino passa de 1 hora.

### 5. Nome do exercício: só no seletor de cima (o circulado)
- Removido o nome duplicado que ficava **abaixo** do seletor (o título grande sublinhado) — sobra mais área visível.
- O nome no seletor (entre as setas ‹ ›) aumentou de ~14 para **26, negrito** (pouco menor que o título antigo de 28).
- Como o título antigo era o toque para ver a imagem/GIF do exercício, criei o botão **🖼️** ao lado das setas: abre o GIF em tela cheia como antes. O toque no seletor continua abrindo a lista para pular direto a qualquer exercício.

### 6. Setas ‹ › funcionam sempre dentro do mesmo treino
- Antes as setas só existiam com o treino iniciado; ao tocar num exercício na lista, a tela abria "presa" naquele exercício.
- Agora, ao tocar em qualquer exercício na tela do treino, a tela abre no exercício tocado **com as setas ‹ › e a lista de troca ativas para navegar por todos os exercícios do treino** — sem precisar iniciar o treino.

## Extras

- **Resumo do treino:** texto do parabéns corrigido ("Você está um passo mais perto…") e cálculo de volume protegido contra exercícios sem ID (não quebra mais se faltar dado).
- **Arquivos lixo removidos da raiz do repositório** (commits acidentais com nomes como `x.SetNumber)`, `new`, `0`…): nenhum era usado pelo app.
- **Preparado para Google Play:** nome do app na launcher agora é "AlePulse"; ID do pacote `com.alepulse.app`; versão `1.0.0 (1)`; configuração de assinatura Release pronta (via variáveis de ambiente). Veja `COMO_PUBLICAR.md`.
- **Código sem comentários**, como pedido.
- **beep.mp3** e todo o mecanismo de drag-and-drop permanecem intactos no projeto.

## Arquivos alterados
| Arquivo | Mudança |
| --- | --- |
| `src/AlePulse.Mobile/Services/RestTimerService.cs` | NOVO — cronômetro global robusto (beep/vibração incluídos) |
| `src/AlePulse.Mobile/Services/ActiveWorkoutSession.cs` | NOVO — sessão do treino em andamento |
| `src/AlePulse.Mobile/Views/ExerciseExecutionPage.xaml.cs` | Reescrito: usa os serviços, setas em todos os modos, sessão ativa, sem estáticos de timer |
| `src/AlePulse.Mobile/Views/ExerciseExecutionPage.xaml` | Nome duplicado removido, seletor com fonte 26, botão 🖼️, linhas do grid renumeradas |
| `src/AlePulse.Mobile/Views/WorkoutDetailPage.xaml.cs` | Botão INICIAR/CONTINUAR, abrir exercício com lista completa + índice |
| `src/AlePulse.Mobile/Views/WorkoutDetailPage.xaml` | Botão do rodapé com `x:Name="StartWorkoutBtn"` |
| `src/AlePulse.Mobile/Views/WorkoutSummaryPage.xaml.cs` | Duração real (sessão) + formato hh:mm:ss + volume protegido |
| `src/AlePulse.Mobile/Views/WorkoutSummaryPage.xaml` | Texto do parabéns corrigido |
| `src/AlePulse.Mobile/AlePulse.Mobile.csproj` | Nome, ID do pacote, versão e assinatura Release |
| Raiz do repositório | 9 arquivos lixo removidos |
| Todos os `.cs`/`.xaml`/`.csproj` | Comentários removidos |

## Como testar (roteiro rápido)
1. INICIAR um treino → registrar uma série → descanso dispara → apertar SAIR no meio do descanso → o beep/vibração chega na hora certa na tela do treino.
2. Na tela do treino o botão mostra CONTINUAR → ajustar peso/descanso de um exercício → CONTINUAR → você volta no exercício onde parou, com valores novos.
3. Tocar num exercício da lista (sem iniciar treino) → usar as setas ‹ › para navegar → FINALIZAR volta para a lista.
4. FINALIZAR TREINO no último exercício → o resumo mostra o tempo real do treino (com horas quando houver).
5. Deixar a tela apagar durante o descanso → ao acender, o contador se corrige e o beep toca (não congela mais).
