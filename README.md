# DS339 Monitor

Substituto leve dos apps de duas telas de hardware, num programa só:

- **Thermalright 9,16"** (1920×462, USB 0416:5408), no lugar do TRCC;
- **Jonsbo DS339** (960×376), no lugar do JONSBO-AIO;
- **Jungle Leopard Pro Flow 360** (tela de 5,5", 960×480, na bomba do watercooler), no lugar do Jungle Leopard Display —
  com **GIFs animados**.

Os sensores são lidos uma vez por segundo e servem as duas telas.

| | JONSBO-AIO | Jungle Leopard Display | DS339 Monitor (as duas telas) |
|---|---|---|---|
| Tecnologia | WPF + OpenCV + FFmpeg + SkiaSharp (~420 MB) | Electron/Chromium + FFmpeg (~900 MB) | .NET 8 + GDI+ do Windows (~35 MB) |
| Como desenha | Janela WPF "fotografada" a cada ciclo | Janela oculta do Chromium capturada a 30 fps, sempre | Fundo pré-desenhado; por quadro só os números |
| Processos | 1 | 5 + 1 de sensores (HWiNFO), em prioridade **alta** | 1, em prioridade **baixa** |
| CPU (Ryzen 7 7800X3D) | 3,8% | 9,4% | **~0,5%** com GIF + dados; ~0,12% só GIF; ~0,04% sem GIF |
| Memória (Gerenciador de Tarefas) | 265 MB | ~740 MB | **~42 MB** com GIF; ~31 MB sem GIF |

## Como usar

1. Feche o TRCC, o JONSBO-AIO e o Jungle Leopard Display (só um programa por vez consegue falar com cada tela; o Jungle
   Leopard ainda derruba quem usar a porta da tela dele). Nesta máquina a inicialização automática do Jungle Leopard
   já foi desativada.
2. Rode `DS339Monitor.exe`. Aparece um ícone na bandeja (perto do relógio do Windows).
3. Clique no ícone:
   - **Thermalright 9,16" ›** **Tema** ("Maré" no layout largo, "Vídeo", "One Piece" ou "GIF"), **Escolher vídeo…**,
     **Escolher GIF…**, **Dados por cima**, **Brilho** e **Girar 180°**. Ver "Tela Thermalright" abaixo.
   - **DS339 ›** **Tema**, **Brilho** e **Girar 180°**. Temas:
     - "Maré" — feito para ler de longe: cada cartão de CPU/GPU enche de líquido conforme o **uso**, e a **cor** do
       líquido acompanha a **temperatura** (céu < 50° · menta ~60° · âmbar ~70° · pêssego ~78° · coral/vermelho 85°+).
       Temperatura em letra gigante.
     - "Aurora" — claro, com anéis de temperatura/uso e gráfico de uso dos últimos 60 s.
     - "Padrão" (escuro), "Padrão Claro" e os temas da Jonsbo.
   - **Pro Flow 360 ›**
     - **Escolher GIF…** — qualquer GIF animado; ele é copiado para a pasta `media\` do programa e preenche a tela
       (cortando as bordas se a proporção for diferente).
     - **Dados por cima** — *Nenhum* (só a animação), *Compacto* (uma faixa embaixo com CPU, GPU e relógio) ou
       *Completo* (painéis com as temperaturas grandes, uso e relógio). Vale para os temas GIF e One Piece.
     - **Tema** — "GIF", "One Piece" ou "Maré" (o mesmo da DS339, na altura da tela do Pro Flow).
     - **Baixar painéis do One Piece…** — abre a página que baixa os painéis do tema One Piece (ver abaixo).
     - **Brilho** e **Girar 180°**.
   - **LED da placa de vídeo** (ASRock RX 6900 XT) — **Efeito**, **Escolher cor…**, **Tom do branco**, **Brilho**,
     **Velocidade**. O título
     mostra a situação (ex.: "pausado (Polychrome aberto)"). Ver "LED da placa de vídeo" abaixo.
   - **Iniciar com o Windows** — cria uma tarefa agendada que abre o programa no logon, já como administrador.
   - **Reiniciar como administrador** — necessário para temperatura, clock e potência da **CPU**.
   - **Salvar imagem das telas** — grava o que está em cada tela (`tela-ds339.png`, `tela-proflow.png`) e abre.
   - **Abrir log** / **Sair**.
   - Passar o mouse sobre o ícone mostra um resumo (CPU °C · GPU °C · RAM %).

Na primeira vez, brilho e rotação do Pro Flow são herdados do Jungle Leopard Display (aqui: 33% e 180°).

> **Por que administrador?** A temperatura da CPU (Ryzen/Intel) só é lida com acesso ao hardware, pelo driver
> PawnIO (já instalado nesta máquina). Sem admin, a CPU aparece como `--`; GPU, uso de CPU,
> RAM, disco e relógio funcionam normalmente.

Se mover a pasta do programa depois de ligar "Iniciar com o Windows", desligue e ligue a opção de novo.

## Painel (a janela do programa)

Clique no ícone da bandeja (botão esquerdo), no atalho **DS339 Monitor** da área de trabalho ou do menu Iniciar, ou abra
o programa de novo: abre o painel. O botão direito no ícone continua com o menu rápido.

- **Visão geral**: o gabinete desenhado como está montado — a Thermalright no topo, a Pro Flow no meio e a luz da placa
  embaixo, **ao vivo** (miniaturas das duas telas e a cor do LED) —, CPU, GPU, memória e fan, e os modos.
- **Thermalright / Pro Flow 360**: tema, biblioteca de vídeos e GIFs (pasta `media`; clicar troca na hora), importar
  vídeo ou GIF, brilho, girar 180°, dados por cima e quadros por segundo.
- **LED da placa**: todos os efeitos com uma **amostra animada** de cada um (calculada pelo mesmo motor do LED), brilho,
  tom do branco, velocidade e cor.
- **Modos**: Modo Mangá (com as opções e um botão para testar cada efeito), Modo Grand Line e Desligar tudo.
- **Sistema**: iniciar com o Windows, administrador, salvar imagem das telas, log, pasta do programa e Sair.

Fechar a janela não fecha o programa. A janela é HTML desenhado pelo WebView2 (o motor do Edge que vem com o Windows
11), sem servidor nem porta aberta; as miniaturas e os sensores extras só existem com ela aberta.
`DS339Monitor.exe --criar-atalhos` recria os atalhos.

## Requisitos

- Windows 10/11 64 bits com o [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0).
- DS339: driver da tela ("MS USB Display", libusb0), instalado pelo JONSBO-AIO. Se você desinstalar o JONSBO-AIO e a
  tela parar de responder, reinstale o driver com `lib\driver\DPInst64.exe` (cópia do instalador original).
- Thermalright: nenhum driver extra — a tela se anuncia como WinUSB e o Windows carrega o driver da Microsoft sozinho.
  Desinstalar o TRCC não afeta a tela.
- Pro Flow 360: nenhum driver extra (é uma porta serial USB padrão do Windows). Desinstalar o Jungle Leopard Display
  não afeta nada — ele não controla bomba, fans nem luzes deste PC.
- Para temperatura da CPU: driver PawnIO + executar como administrador.

## Desligar tudo

**Ícone › Desligar tudo (telas, LED e SignalRGB)** apaga o gabinete inteiro de uma vez, e **Religar tudo** devolve:

- **Pro Flow 360**: um último quadro preto e a tela sai do modo ao vivo — a mesma sequência que o app original manda
  quando o PC bloqueia (FF D9 FF D9 e o comando 20 = [0, 0]).
- **DS339 (Jonsbo)**: preto ainda deixa a luz de fundo acesa (fica cinza), cada quadro novo a acende de novo, e sem
  quadros ela apaga sozinha em alguns segundos — então ela recebe um único quadro preto e mais nada até religar.
  Desativar a USB (como na Thermalright) não serve: sem o computador ela mostra a tela de espera, cinza com o logo da
  Jonsbo.
- **Thermalright**: ela não tem comando de luz de fundo (o TRCC também só escurece a imagem), então depois de um
  quadro preto o programa solta a tela e desativa o dispositivo no Windows: a porta USB entra em suspensão e a tela
  apaga de verdade. A desativação não é permanente (reiniciar o PC reativa), e religar, fechar o programa ou abri-lo
  de novo também reativam.
- **LED da placa**: preto pelo mesmo modo direto dos efeitos, qualquer que seja o efeito escolhido.
- **Fans e fitas**: com o programa no controle (ver abaixo), uma transição de meio segundo até o preto, mandada pela
  própria controladora; ao religar, voltam ao efeito.
- **SignalRGB** (se estiver aberto): fechado pelo Gerenciador de Reinicialização do Windows (o caminho que instaladores
  usam): ele recebe o aviso de "sessão encerrando" e faz a saída normal. Nunca é morto à força. Ao religar, ele é
  reaberto como no início do Windows (`SignalRgbLauncher.exe --silent`, sem administrador) — só se foi este programa
  que o fechou.

Também por linha de comando (para um atalho na área de trabalho ou uma tecla de atalho): `DS339Monitor.exe
--desligar-tudo`, `--ligar-tudo` e `--alternar-tudo`. Não fica salvo: abrir o programa de novo começa tudo aceso
(e reabre o SignalRGB, se ele ficou fechado por um "desligar tudo" de antes).

## Fans e fitas (controladora Nollie 16CH)

O que o SignalRGB fazia com os fans e as fitas ARGB, dentro do programa — página **Fans e fitas** do painel.

- **As partes vêm do SignalRGB.** Na primeira vez o programa lê (só lê) como a Nollie está montada lá: em cada canal,
  os componentes na ordem dos LEDs. Cada componente vira uma parte com o mesmo trecho de LEDs — por isso o centro e as
  duas laterais de cada fan continuam independentes, como no SignalRGB:

  | Peça | Canal | LEDs | Onde fica (na foto) |
  |---|---|---|---|
  | Fans de baixo — centro, lateral 1, lateral 2 | 1 | 25 + 31 + 31 | os três fans do piso |
  | Fans do radiador — centro, lateral 1, lateral 2 | 2 | 25 + 31 + 31 | a parede da direita, com o radiador do water cooler |
  | Memória da direita / da esquerda (a mais perto da CPU) | 3 / 7 | 9 cada | à direita da Pro Flow |
  | Cabo da placa de vídeo | 4 | 110 | o bloco branco em cima da placa |
  | Fan traseiro — centro, lateral 1, lateral 2 | 5 | 8 + 9 + 9 | o fan de 120 da parede de trás |
  | Bomba do water cooler | 6 | 5 (brilho de partida 200%: a luz dela é fraca) | atrás da Pro Flow |
  | Placa sob a placa-mãe | 8 | 50 | embaixo da placa de vídeo |
  | Cabo da placa-mãe | 9 | 110 | o tubo branco entre os canos e o radiador |

  Os nomes de cada peça foram ditos pelo usuário; **Identificar** acende só aquela parte, pulsando em branco por 8 s (no
  gabinete e no desenho do painel), e o nome se troca clicando nele. "Importar de novo" relê o SignalRGB.
- **O gabinete ao vivo.** O painel (na Visão geral e no alto da página Fans e fitas) mostra a foto do gabinete
  (`ui\gabinete.jpg`) escurecida, com as duas telas ao vivo no lugar delas, a luz da placa de vídeo em volta dela e
  cada um dos 493 LEDs como um ponto de luz na posição dele, 10 vezes por segundo.
- **Cenas do gabinete** — usam a posição de cada LED na foto (campo `Forma` de cada parte no config.json: `linha`,
  `anel`, `aneis` ou `matriz`, em pixels da foto; ver `CaseLayout.cs`), então atravessam o gabinete inteiro:

  | Cena | O que acontece |
  |---|---|
  | Mangá no gabinete | Acompanha a tinta da página da Pro Flow; com o Modo Mangá ligado, cada momento vira cena: o DON! sai da Pro Flow como onda de choque, o Haki escurece e dispara anéis, os tambores sobem do piso, a página vira da direita para a esquerda (como no mangá), o "copiar" acende as memórias, o pendrive brilha dourado, o mudo apaga, e o Gear 5 bate duas vezes na bomba e explode em branco |
  | Knock Up Stream | A corrente sobe dos fans do piso até o topo, fica um instante e cai de volta em gotas (7 s) |
  | Haki do Rei | A bomba junta força e solta ondas de choque com eco pelo gabinete (4,5 s) |
  | Aquário | O gabinete enche e esvazia devagar: luz de piscina debaixo d'água, superfície com ondinhas, bolhas subindo |
  | Fluxo de ar | O caminho de verdade do ar: entra pelo piso e pelo radiador, sai pelo fan de trás (mais morno); os centros dos fans giram; mais uso da placa = mais ar |
  | Tambores da Libertação | DON do-tto-to: cada batida é uma onda que sobe do piso |
  | Chuva | Gotas escorrendo pelas laterais e pelas memórias e respingando nos fans do piso |
  | Farol | Um facho girando em volta da bomba |
  | Mapa de calor | CPU e placa de vídeo esquentam o ar em volta (temperatura de verdade) e o calor sobe; os fans que puxam ar de fora ficam frios |

  O LED da placa de vídeo pode entrar nas cenas: efeito **Igual aos fans** (página LED da placa) — a placa mostra a
  cena no lugar dela (a onda de choque passa por ela, a água sobe por ela). `--fitas-previa "Cena" [saida.png]
  [quadros] [segundos] [momento]` desenha a cena sobre a foto em vários instantes, sem tocar na controladora (o
  momento é um do Modo Mangá: Impact, Haki, Gear5…).
- **Efeitos por parte** (cada parte anima sozinha, ao longo dos próprios LEDs): Cor fixa, Onda, Respiração, Cometa,
  Cintilar, Igual à placa de vídeo (os fans acompanham o LED da placa; o branco 255, 210, 255 dela vira branco neutro),
  Aurora, Arco-íris e Apagado. Cada parte fica em "Igual ao geral" ou escolhe o seu efeito, a sua cor, o seu brilho
  (0–200% do geral) e o sentido ("Inverter" também vira a parte nas cenas, se uma onda correr ao contrário nela).
- **Capas de cabo (estilo Strimer da Lian Li)**: efeitos que usam a grade da capa — fileiras ao longo do cabo
  (`matriz … FxL` na forma da parte) — nas duas direções. Numa parte que não é grade, viram uma fileira só.

  | Efeito | O que faz |
  |---|---|
  | Pista | Luzes de aproximação correndo das duas pontas para o meio, a fileira do meio na frente (seta); o meio acende quando elas se encontram |
  | Meteoros | Cada fileira solta meteoros em horas e velocidades sorteadas: cabeça branca, rastro na cor |
  | Corrente elétrica | Pacotes de energia disparando pelo cabo, tremendo entre as fileiras, com faíscas; de vez em quando um arco atravessa todas |
  | Empilhar | Blocos caem da ponta de lá e empilham na de cá (cada fileira no seu tempo); cheio, brilha e escoa |
  | Pingue-pongue | Uma barra atravessando as fileiras vai e volta; a cada batida, um respingo na ponta e a troca entre a cor e o branco |
  | Contramão | Fileiras alternadas correndo em sentidos opostos, listras na cor e brancas se cruzando |
  | Serpente | Duas cobras (a cor e o branco) percorrendo a grade em zigue-zague |
  | Cobrir | A cor cobre o cabo fileira atrás de fileira, com a borda clara; depois o escuro cobre de volta |
  | Caleidoscópio | Arco-íris espelhado a partir do meio do cabo e da fileira do meio, pulsando em anéis |
  | Arco-íris diagonal | O arco-íris correndo em diagonal pela grade |

  A grade tem que bater com a ordem dos LEDs no fio (o SignalRGB trata as capas como uma fita de 110 LEDs, sem grade).
  **Identificar** numa capa mostra essa ordem por 14 s: um degradê do vermelho (1º LED) ao violeta (último) e um
  ponto branco correndo do primeiro ao último, 10 por segundo. `--fitas-previa "Meteoros@cabos" saida.png 8
  "0.1,0.3,…"` desenha um efeito de parte de perto nas duas capas.
- **Quem manda.** Por padrão o programa não toca na controladora. **Controlar pelo programa** fecha o SignalRGB pelo
  caminho normal e assume; **Devolver ao SignalRGB** solta a controladora e reabre o SignalRGB. Com o SignalRGB, o app
  da Nollie ou o OpenRGB abertos, o programa não escreve nada (conferido a cada 1 s) e espera — só um programa pode
  mandar na controladora.
- **Protocolo.** O mesmo do plugin do SignalRGB para esta controladora (`Plugins\Nollie\Nollie16v3_Nos2.0.js`, USB
  16D5:2A16): um relatório HID de 1025 bytes por canal, `[0, canal físico, fim de quadro, 0, 0, G, R, B…]`; os canais 1–16
  são os físicos 3, 2, 1, 0, 8, 9, 10, 11, 4, 5, 6, 7, 15, 14, 13, 12, e o pacote de maior canal físico leva o "fim de
  quadro". Só cores: nenhum comando que grave configuração na controladora. ~30 quadros por segundo nas animações;
  parado, o quadro é reenviado a cada 1 s.
- **Limite de energia** (4–12 A, padrão 8 A): a corrente de todos os LEDs é estimada (~50 mA por LED no branco total;
  os 493 LEDs no branco cheio dariam ~25 A) e, acima do teto, o brilho de tudo baixa por igual, como no WLED. A Nollie
  tem fusível de 15 A e divide a linha de 5 V da fonte com SSDs e USB.
- **Pisca-pisca**: a luz média de todos os LEDs passa pela mesma trava do LED da placa (no máximo 3 clarões por segundo);
  os efeitos já são lentos o bastante para não chegar perto dela.
- Por linha de comando: `DS339Monitor.exe --fitas assumir`, `--fitas devolver` e `--fitas identificar N` (N = número da
  parte, a partir de 0, na ordem do config.json).

### Guardado na controladora (efeito de boot e efeito sem programa)

A Nollie toca efeitos sozinha, guardados nela: ao ligar o PC, os de **boot** tocam uma vez (antes do Windows); depois,
os **normais** ficam em loop até um programa mandar cores — e voltam quando ele para (no "tempo de volta"). O card
**Guardado na controladora** da página Fans e fitas lê o que está gravado nela e edita os dois, como o app oficial
NollieRGB faz:

- **Ao ligar o PC**: Acender devagar, Varredura de cor, Varredura lenta, Laço, Empilhar 1/2/3 ou Balas; cor principal e
  segunda, brilho, velocidade (1–10) e tamanho.
- **Sem o programa**: um dos 24 efeitos da controladora (Cor sólida, Respiração, Varredura, Arco-íris, Meteoro, Onda,
  Empilhar, Fusão, Balas…), com os mesmos ajustes.
- **Voltar ao efeito dela**: quanto tempo depois de o programa parar de mandar cores (padrão dela, 5 s … 5 min, nunca).
- Os efeitos dela passam pelas peças **do piso para o topo** (fans de baixo → placa sob a placa-mãe → radiador → cabo da
  placa de vídeo → bomba → memórias → fan traseiro → cabo da placa-mãe), e só pelos canais com LEDs.
- **Nada vai para ela até "Gravar na controladora"**. O card mostra antes o que vai mudar; só o que mudou é gravado (cada
  ajuste vira uma gravação na flash dela), e depois tudo é lido de volta para conferir. A primeira gravação também
  corrige a quantidade de LEDs de cada canal (vinha 256 em todos, o padrão de fábrica — os efeitos dela andavam por LEDs
  que não existem).
- **Ver o boot agora**: o programa para de mandar cores por 12 s e pede à controladora que toque o efeito de boot; depois
  ela volta sozinha a receber as cores do programa.

#### Animação de boot do programa (gravada na memória da controladora)

Em vez de um dos 8 efeitos de boot dela, a Nollie pode tocar ao ligar o PC uma **animação desenhada pelo programa**,
LED por LED, pela posição de cada LED no gabinete — escolha "Animação do programa" no card:

| Animação | O que acontece |
|---|---|
| Curto-circuito | Faíscas branco-azuladas de 1–2 quadros nos cabos de energia (as capas), na placa de baixo e na bomba, cada vez mais seguidas, com brasa laranja; arcos em zigue-zague entre peças próximas; um estouro do cabo da placa de vídeo até a bomba, queda de energia e a luz firmando do piso ao topo |
| Knock Up Stream | Uma volta da cena: a corrente sobe dos fans de baixo e cai em gotas |
| Haki do Rei | Uma volta da cena: ondas de choque saindo da bomba |
| Despertar | O Gear 5: dois batimentos na bomba e o branco tomando o gabinete |

- **Depois: o loop dos fans por 1–5 min** (padrão 3): a controladora guarda **um** arquivo de animação só, então o boot
  e o loop vão no mesmo arquivo — a animação até o ponto de entrega (no curto-circuito, quando a luz firma), 1 s de
  transição e o efeito configurado na página (geral + cada parte), calculado pelo mesmo motor de quando o programa está
  aberto, por alguns minutos, a 25 quadros por segundo. Fluxo ao ligar: curto-circuito → o loop dos fans tocado pela
  própria controladora → o programa abre e assume (sem escurecer: ele entra direto no mesmo efeito). Se o programa
  demorar mais que isso, vem o efeito sem programa. Medido em 2026-10-21: o programa assume de 30 s a 2,5 min depois de
  ligar o PC. 3 min = 4652 quadros, 6,9 MB, 64 s para gravar e conferir.
- **Ver** toca a animação nos fans pelo próprio programa, sem gravar nada. **Gravar na controladora** grava o arquivo na
  memória dela (o curto-circuito tem 218 quadros de 33 ms, ~315 KB, ~3 s para gravar), confere pelo CRC calculado por ela e
  só então muda o boot para "a animação, uma vez, e depois o efeito sem programa" (`FA 13 02 00`). **Ver a animação
  tocada pela controladora** pede a ela que toque o que tem gravado (`F3 02`).
- Os quadros já saem com o brilho geral, o limitador de energia e a trava de pisca-pisca (na luz média; as faíscas
  são só pontos) — a controladora toca como está. Mudou o brilho ou o limite: grave de novo.
- **Cores trocadas**: o código do NollieRGB não diz se a controladora lê o arquivo em R, G, B ou G, R, B. O programa grava
  em G, R, B; se as brasas laranja saírem verdes, ligue "Cores trocadas" e grave de novo.
- Formato (do NollieRGB, igual ao que o firmware confere): FSEQ v2 sem compressão, cabeçalho de 32 bytes, uma faixa
  esparsa por canal físico com LEDs (em ordem crescente de canal físico), 3 bytes por LED; `FA 10` + 16 × u16 = LEDs de
  cada canal (tem que bater com as faixas). Gravação na flash externa a partir de 0x10000 (nunca abaixo): `F4 05` apaga
  blocos de 64 KiB, `F4 06` grava pedaços de 1016 bytes — o pedaço com o cabeçalho por último (interrompida no meio, ela
  vê "sem animação"), `F4 07` = CRC dela (CRC-32/MPEG-2 em palavras de 4 bytes), `F9 11` = arquivo presente.

Protocolo (o mesmo do NollieRGB, lido do código dele): relatório de 1025 bytes; `FC 01` = versão (tem que ser 19),
`F9 xx` = ler, `FA xx` = gravar, `F5/F6 16` = tempo de volta. Canvases 0–13 = efeitos normais, 14–15 = boot (com outra
tabela de efeitos). `FA 02 slot` + 16 bytes = efeito, brilho, velocidade, tamanho, nº de canais, vmap, atraso, 3 cores RGB;
`FA 03 00`/`FA 0B 00` + 16 = canais (físicos) dos normais/boot; `FA 04`/`FA 05` = quantos canvases; `FA 08` + 16×u16 = LEDs
por canal; `FA 13 intro depois`; `FA 20` = tocar o boot. A controladora não tem "salvar": guarda cada ajuste sozinha.
O programa só manda uma **lista fechada** desses comandos (`NollieDevice.Allowed`); nunca os de atualização de firmware
(`FD`, `80`–`84`), de flash externa (`F4`), de restaurar/reiniciar (`FA 21/22/23`) nem de detecção de LEDs (`FA 14`).


## Modo jogo (painel de jogo na Thermalright)

Ligado (página **Modo jogo**), abrir um jogo da lista (executáveis, padrão `cs2`) em primeiro plano troca o tema da
Thermalright pelo **painel de jogo** até o processo do jogo fechar (alt-tab não tira); depois o tema volta. Três quadros
no visual de mangá do programa:

| Quadro | O que mostra | De onde vem |
|---|---|---|
| FPS | FPS do último segundo, 1% low (~10 s), frametime médio, modo de apresentação; CPU e placa (temperatura, uso, W) | PresentMon + sensores |
| Frametime | Gráfico dos últimos quadros com as linhas de 240/144/60 FPS e os picos marcados; o que limita (placa de vídeo, sincronia com o monitor, teto de FPS ou processador) e a latência do clique até a tela | PresentMon (MsBetweenPresents, MsGPUBusy, MsCPUWait, MsAllInputToPhotonLatency) |
| Ping / CS2 | Ping, jitter e perda até o endereço com que o jogo mais troca pacotes (no CS2, o relé da Valve que leva ao servidor), tráfego ↓/↑; no CS2: placar, mapa, rodada, vida, colete, dinheiro, abates/assist./mortes, arma e munição | rastreamento de rede do Windows + ICMP; Game State Integration do CS2 |

- **PresentMon** (Intel, código aberto, assinado; `tools\PresentMon.exe` v2.6.0): lê os eventos de "apresentar quadro"
  do Windows (ETW) do processo do jogo, uma linha por quadro. Sessão própria (`DS339MonitorJogo`), encerrada ao fechar
  o jogo ou o programa.
- **Rede**: rastreamento do Windows (provedor Microsoft-Windows-Kernel-Network, pelo pacote TraceEvent da Microsoft) —
  só endereços e tamanhos dos pacotes do processo do jogo, nunca o conteúdo; o endereço com mais tráfego recebe um ping
  por segundo. O ping é até o primeiro servidor do caminho (no CS2, o relé da Valve), então pode ficar uns ms abaixo do
  que o jogo mostra; servidor que não responde a ping aparece como "não responde".
- **CS2**: "Conectar ao CS2" grava `gamestate_integration_ds339monitor.cfg` na pasta de configurações do jogo (achada
  pelas bibliotecas da Steam) — o recurso oficial da Valve para apps externos; o próprio jogo manda o estado da partida
  para `http://127.0.0.1:3339`, com um código combinado (mensagens sem ele são ignoradas). Não mexe no jogo nem na
  memória dele. Vale a partir da próxima vez que o CS2 abrir.
- **O gabinete reage à partida** (com a integração do CS2), cada acontecimento como uma pequena cena pela posição
  das peças:

  | Acontecimento | No gabinete |
  |---|---|
  | Bomba plantada | Tudo baixa e um "sonar" vermelho sai do piso 1 vez por segundo — sempre no mesmo ritmo, sem acelerar (não é cronômetro). A capa de cabo da placa-mãe vira o teclado da C4 (7355608 sendo digitado) e a da placa de vídeo, o visor |
  | Explodiu | Núcleo branco-quente no piso, bola de fogo enchendo o gabinete e esfriando de amarelo a vermelho, anéis dos fans pegando fogo, fumaça e brasas caindo (com o Modo Mangá, o DON! nas telas) |
  | Desarmada | Estalo branco no piso, faíscas azuis correndo pelas fitas e uma geada ciano subindo |
  | Vitória | **Verde**: maré subindo do piso, volta olímpica nos anéis e confete verde/menta/branco |
  | Derrota | **Vermelho**: cortina caindo do topo e escorrendo para o piso; os fans param de girar |
  | Abate | Clarão no canto do piso e um traçante pelas fitas do piso e radiador acima, com respingo — cada abate da rodada acerta mais alto. Capacete: vai até o topo e vira uma estrela branca. Ace: a coroa dourada |
  | Abates na rodada | As memórias acendem um quinto por abate (cinco = dourado) |
  | Pouca vida (≤ 30) | As bordas batem em vermelho, tum-tum, mais forte quanto menos vida |
  | Morto | A linha do monitor cardíaco corre pelo piso e fica reta; uma névoa azul sobe; penumbra até a próxima rodada |
  | Flash / smoke / fogo | Branco que volta de fora para dentro (com a sombra roxa) / névoa cinza rolando / chamas lambendo de baixo |

  Quando a explosão (ou o desarme) e o fim da rodada chegam juntos, a cena da bomba vem primeiro. Vale nos fans, nas
  fitas e no LED da placa; a Thermalright mostra a faixa (BOMBA PLANTADA, EXPLODIU!, DESARMADA!, ACE!!; VITÓRIA! em
  verde, DERROTA em vermelho). "Testar as reações" encena duas rodadas de mentira; `--fitas-previa cs2:Exploded` (ou
  RoundWon, RoundLost, Planted, Kill3, Headshot1, Defused, Died, Ace+Kill5, Flash, Smoke, Fire, LowHp, somados com
  `+`) desenha a cena sobre a foto, e `--jogo-previa saida.png vitoria|derrota|plantada` o painel com a faixa.
- **Por que é seguro (VAC)**: só a Game State Integration — o recurso oficial da Valve para placares e luzes (o mesmo das
  integrações de Logitech, SteelSeries e Philips Hue): o próprio jogo manda o estado; nada lê a memória do jogo nem se
  injeta nele (o que o Modo Confiável do CS2 bloqueia). Só blocos do próprio jogador (`player_*`, `round`, `map`) — nada
  dos blocos de espectador (`allplayers_*`, `bomb`, `phase_countdowns`, com posições e cronômetro), e nenhum cronômetro
  da bomba: a Valve atrasa de propósito o aviso de "plantada" para impedir isso. O PresentMon e a rede leem eventos do
  próprio Windows, não o jogo.
- **Ver o painel de jogo**: 20 s com uma partida de mentira. `--jogo-previa saida.png` desenha o painel sem tocar em nada;
  `--fitas jogo-testar processo` trata um processo qualquer como o jogo (teste), `--fitas jogo-testar` encerra.

## Modo Umbra (o software do hub Asiahorse UMBRA, na Nollie)

Página **Modo Umbra**: o software do hub Asiahorse UMBRA (AsiaHorse UMBRA 3.5.5, "Sync_light", feito pela Robobloq)
refeito para a Nollie, para uso pessoal. Fica à parte: **Usar** liga e ele toma o lugar dos efeitos da página Fans e
fitas (que ficam guardados e voltam ao desligar); **Luzes** é o "开灯/关灯" do software.

As regras vieram do código do software (lido sem executar nada): os modos, os efeitos e os números deles, que controle
cada efeito mostra, os valores (velocidade e brilho em 6 níveis — o deslizante anda de 10 em 10 e mostra 1–100%;
direção normal/inversa; até 4 cores; padrões vermelho, verde, azul e amarelo, velocidade e brilho em 60%), as zonas e
os arranjos de cada fan e as predefinições. No hub, os 15 efeitos de fan são desenhados pelo chip dele (o software só
manda o número e os parâmetros), então a aparência foi refeita pelo nome de cada um.

| Aba | O que faz |
|---|---|
| Sincronizado | O mesmo efeito em todas as peças, cada uma com a sua cópia (num conjunto de fans, cada anel a sua) |
| Independente | Cada porta (peça, na ordem dos canais) com o seu efeito. Num fan, as zonas: **Centro**, **Paralelo** (as duas laterais iguais), **Série** (as laterais como uma fita só, subindo por uma e descendo pela outra), **Esquerda**, **Direita** e **Tudo** — escolher a zona escolhe também o arranjo (centro + paralelo, centro + série, centro + esquerda + direita, ou tudo junto), e cada zona tem efeito, velocidade, brilho, direção e cores próprios. A cor estática do centro é uma por fan. **Fans juntos** faz a mudança num fan valer para todos. **Predefinições**: até 9, com nome (Enter salva) |
| Combinado | Todas as portas em fila, como uma fita só, com os 10 efeitos que correm pela fila |
| Tela | O "modo tela" (幕布模式): 12 dos efeitos de tela portados do código original (Arco-íris em fluxo, Quarta dimensão, Radar, Matrix, Chamas, Aurora, Correnteza, Chuva de meteoros, Tempestade, Hiperespaço, Águas-vivas, Lâmpada de lava). No software as peças são ícones numa tela de 600×600; aqui a tela é a foto do gabinete e cada LED pega o pixel da sua posição. Velocidade 1–10 e cores por efeito; brilho 1–10 |

Efeitos de fan (grade do software, mesma ordem): Cor estática, Arco-íris, Arco-íris gradiente, Respiração, Respiração
arco-íris, Pista, Ladrilho, Esvanecer, Rastro, Varredura, Blocos, Discoteca, Expandir e contrair, Relâmpago (no máximo 2
clarões a cada 0,9 s) e Endereçamento. O brilho geral da página Fans e fitas continua valendo por cima, e o limitador
de energia, o balanço de branco e a trava de pisca-pisca também. `--fitas-previa "umbra:Sincronizado:5"`
(ou `umbra:Combinado:0`, `umbra:Tela:storm`, `umbra:Independente:Laterais:0:8:5` = arranjo, centro, esquerda, direita)
desenha o modo sobre a foto, sem tocar na controladora.

## Balanço de branco

Cada tipo de LED puxa o branco para um lado (o da placa de vídeo, as capas de cabo, os fans). Página **Balanço de
branco**: para a placa de vídeo e para cada peça dos fans e fitas, quanto de vermelho, verde e azul sai (0–100%).

- Aplicado só na saída: os efeitos continuam pensando em branco puro, e a correção vale para todos eles — inclusive os
  fans acompanhando a placa e a animação gravada na controladora (quando gravada de novo; o card avisa). O painel
  mostra as cores pretendidas (sem a correção).
- **Comparar brancos**: a placa e todas as partes no mesmo branco, com o balanço de cada uma, enquanto você mexe (os
  deslizantes aplicam ao arrastar). Desliga sozinho em 10 min ou ao fechar o painel.
- Placa de vídeo: até o primeiro ajuste ela segue o tom escolhido na página do LED (o branco guardado nela, Neutro,
  Quente ou Frio); no primeiro ajuste esse tom vira o ponto de partida do balanço e o tom passa a Neutro (para não
  corrigir duas vezes). "Voltar" devolve o tom de antes.
- Não vale para os efeitos guardados na própria controladora (boot e sem programa): as cores deles vão como escolhidas.
- Teste por linha de comando: `--fitas comparar 50` / `--fitas comparar off`.

## Ventoinhas (curva dos fans pela placa-mãe)

Os fans do gabinete (2×360 + 120) estão num hub PWM ligado no SYS_FAN da JGINYUE B650M Snow Dream. O chip de sensores
dela é um **ITE IT8613E**, com 4 saídas de PWM, que o programa lê e escreve pelo LibreHardwareMonitor (precisa de
administrador e do driver PawnIO). Página **Ventoinhas** do painel:

- **Saída do hub**: as 4 saídas e o estado de cada uma (automático = curva da BIOS; fixo = valor). **Testar** põe a
  saída em 100% por 6 s e devolve à BIOS: a que fizer os fans do gabinete acelerarem é a do hub (**É o hub**). O hub não
  informa rotação; a entrada 2 (~2720 rpm fixos) não muda com a PWM 2.
- **Curva**: CPU (linha cheia) e placa de vídeo (tracejada); vale a maior das duas, com um mínimo (15–60%). Prontas:
  Silenciosa, Equilibrada e Desempenho; arrastar um ponto do gráfico cria a Personalizada. As linhas pontilhadas são as
  temperaturas de agora e a verde, o que está no hub.
- **Segurança**: desligada (padrão) o programa não toca nos fans. Ligada: acima de 88 °C na CPU ou 85 °C na placa de vídeo
  vai a 100%; sobe depressa (10%/s) e desce devagar (2%/s); sem leitura de temperatura por 15 s, ao desligar a curva e
  ao fechar o programa, a **BIOS volta a mandar** (o LibreHardwareMonitor também devolve ao fechar). Só um programa
  travado sem fechar deixaria o hub na última velocidade.
- A leitura da placa usa o mesmo leitor dos sensores das telas (um segundo leitor do LibreHardwareMonitor no mesmo
  processo, ao fechar, derrubava o driver do primeiro); a curva roda numa thread própria e escreve sob a mesma trava.

## Modo Mangá (efeitos por cima do vídeo, do GIF e do mangá)

**Ícone › Modo Mangá** mantém a arte que já está nas telas — o vídeo da Thermalright, o slideshow de One Piece da
Pro Flow — e encena por cima dela, nas duas telas e no LED ao mesmo tempo, o que acontece no mangá e no PC. A
Thermalright é a "trilha sonora" (onomatopeias de mangá e manchetes); a Pro Flow é o painel que sente o golpe e onde
os personagens "falam" (balões colocados no pedaço mais vazio do painel); o LED fica no "Mangá narrado".

| Quando | Thermalright | Pro Flow | LED |
|---|---|---|---|
| Painel de **impacto** | quadro de impacto (negativo), linhas de foco, soco de câmera, ドン!! | o mesmo golpe no painel | DON |
| Painel do **Haki do Rei** | raios negros pelas bordas, 覇 | raios no painel | cai e estala |
| **Tambores da Libertação** | ドンドットット, a imagem pulsa no ritmo | linhas de foco a cada batida | tambores |
| **Gura Gura** (tremor) | o ar racha e a tela treme, グラグラ | rachadura no painel | chacoalha |
| Maré / despedida / amanhecer | a imagem balança (ザザーン) / desbota para o branco / fachos de luz | fachos de luz | ondas / baixa / nasce |
| Virada de painel | a divisa branca (ou uma onda de retícula) atravessa o vídeo | — | DON / pontilhado |
| **Jogo abriu** | **Gear 5**: coração (ドクン ×2), impacto, e o GIF do Nika numa página branca com "GEAR 5" e ドンドットット | pula para o painel do Gear 5 | coração + tambores |
| Print | flash de câmera, カシャ!, o print vira um quadro de mangá em retícula | flash | flash |
| Download terminou | "EDIÇÃO EXTRA!" com o nome do arquivo, ドン! | balão "DOWNLOAD CONCLUÍDO!" | DON |
| Copiou | — | balão "COPIADO!" | toque |
| Pendrive | "PENDRIVE CONECTADO!", ガチャ | balão "TESOURO ENCONTRADO!" | DON |
| Microfone/câmera | Den Den Mushi: プルプルプル… ガチャ | balão "ALÔ, ALÔ?" e a etiqueta "● NO AR" | toca |
| Música nova | "TOCANDO AGORA" e notas ♪ | — | respira |
| Mudo | シーン… e a imagem desbota | balão de pensamento "…" | quase apaga |
| GPU ≥ 80 °C por 10 s | ゴゴゴゴ subindo pelos lados e a temperatura | ゴ | pulso lento |
| Hora cheia (8h–23h) | página de título: 第15話 · CAPÍTULO 15 · um título · a hora | — | DON |
| PC parado / voltou | — / おかえり! e a divisa | balão "Zzz…" / "BEM-VINDO DE VOLTA!" | respira / varre |

- **Testar Modo Mangá ›** dispara cada efeito na hora (também `DS339Monitor.exe --historia gear5`, `print`,
  `download`, `impacto`…). `--manga-previa efeito` desenha os instantes de um efeito sem tocar no hardware.
- Fontes de mangá na pasta `fonts\` (licença OFL, do Google Fonts): Dela Gothic One (onomatopeias) e Bangers (balões).
- Config em `"Manga"`: `Ligado`, `GifGear5` (padrão `media\nika-luffy.gif`), `CapitulosNaHora`, `AcoesDoPc`.
- O efeito só existe enquanto acontece: no resto do tempo o vídeo sai do cache como antes (~0,7% da CPU).
- Os clarões (quadro de impacto, flash do print) respeitam o limite de 3 por segundo, nas telas e no LED.

## Modo Grand Line — "Coluna do Mar" (as duas telas e o LED, um mundo só)

**Ícone › Modo Grand Line** transforma o gabinete num mar visto de lado, de cima a baixo — na ordem em que as peças
estão montadas (a Thermalright no topo, a bomba no meio, a placa de vídeo de pé embaixo):

- **Thermalright — "Grand Line"**: o céu e a superfície, com o Thousand Sunny navegando (sol e lua de verdade,
  tempestades que chegam devagar, gaivotas, CPU/GPU e a hora nas caixas de narração).
- **Pro Flow 360 — "Fundo do Mar"**: debaixo d'água, logo abaixo do Sunny — o casco visto de baixo cortando a
  superfície, os fachos de sol (inclinados pela posição real do sol), um cardume (mais rápido com som) e bolhas que
  sobem do fundo conforme o calor da placa.
- **LED da placa — "Grand Line"**: a luz das profundezas. Branco frio da Ilha dos Tritões acompanhando o sol de
  verdade; esquenta com o calor da placa (o vulcão submarino). A borda de baixo da Pro Flow mostra a mesma luz — ela
  "continua" na placa.

A regra é a **gravidade**: o que é pesado cai do céu, atravessa a Pro Flow e chega à luz do fundo (a placa reage na
chegada); o que é leve nasce na luz da placa e sobe até estourar na superfície (a placa reage na partida). Tudo é uma
conta só, em milímetros, em função do tempo: as duas telas e o LED mostram o mesmo instante.

| Ação do PC | História |
|---|---|
| Jogo em tela cheia abriu | **Knock Up Stream**: o fundo esquenta e borbulha, uma coluna d'água sobe pela Pro Flow (a placa explode em luz morna) e lança o Sunny para o céu ("ドドドド"). Ao fechar o jogo, ele volta pendurado no **balão-polvo** ("ザブン") |
| PC parado 5 min, tela bloqueada ou monitor apagado | **A âncora** cai do Sunny ("ガラガラ"), atravessa a Pro Flow e toca a luz do fundo; a placa passa a respirar como LED de notebook dormindo. Voltou: a âncora sobe e uma faixa de luz varre o gabinete de baixo para cima — "Bem-vindo de volta, Capitão!" |
| Download grande ou do navegador terminou | **News Coo**: a gaivota-carteiro entrega a "EDIÇÃO EXTRA!" com o nome do arquivo; a moeda cai, afunda girando e tilinta na luz do fundo ("チャリン") |
| Print de tela | **Cartaz de procurado**: "カシャ!", o cartaz (com o próprio print) cai no mar, afunda e pousa na luz do fundo |
| Copiou algo | **Bolha**: nasce na luz da placa, sobe pela Pro Flow com "Aa" dentro e estoura na superfície ("パチン") |
| Música nova no Spotify | **Brook**: "♪ música" no céu e as notas afundam até o fundo, cada uma tocando a luz |
| Pendrive conectado | **Baú do tesouro** sobe das profundezas e fica no convés |
| Microfone ou webcam em uso | O **Den Den Mushi** toca (プルプル… ガチャ) e fica grudado no vidro da Pro Flow, "NO AR" |
| GPU ≥ 86 °C por 10 s | **Erupção submarina**: a placa arde em brasa, bolhas de lava sobem pela Pro Flow e o mar ferve em volta do Sunny |
| Tempestade (em ~30% das horas) | O raio cai na Thermalright, a luz dele entra pela superfície da Pro Flow e chega à placa |
| Mudo / volume | "シーン" (o silêncio do mangá) e o mar fica liso / um medidor rápido no céu |
| Vida própria | Gaivota que mergulha e pega um peixe; estrela cadente que afunda brilhando; sino do Sunny na hora cheia; e, à noite com o PC dormindo, o **Laboon** sobe para respirar (o olho dele enche a Pro Flow, a luz da placa some atrás do corpo, e ele esguicha atrás do Sunny) |

- Uma história grande por vez (as outras esperam numa fila); jogando, só a erupção interrompe. Das 23h às 7h a placa
  quase não reage e não há onomatopeias.
- **Testar cena ›** dispara cada história na hora (também `DS339Monitor.exe --historia nome`).
- A posição das telas fica em `"Cena"` no config.json: `VaoTelasMm` (da borda de baixo da Thermalright à de cima da
  Pro Flow), `VaoPlacaMm` (da Pro Flow à placa) e `DeslocamentoProFlowMm`.
- Custo: ~0,9% da CPU com tudo ligado (as duas telas a 20–30 quadros/s e o LED a 25 Hz).
- Arte redesenhada: o Thousand Sunny de verdade (cabeça de leão, Soldier Dock, vela com a caveira de chapéu de
  palha), mar em três camadas com cristas de espuma e períodos diferentes (a mesma onda não se repete lado a lado),
  seis nuvens, sol girando devagar, lua na fase real. Na Pro Flow, o casco do Sunny visto de baixo (tábuas e quilha)
  segue o barco da Thermalright milímetro a milímetro.
- A versão anterior do modo — com os momentos do mangá encenados a partir dos painéis da Pro Flow — está guardada em
  `backups\2026-10-20_1808 - Modo Grand Line (momentos do manga)`; o efeito "Mangá narrado" e o tema One Piece
  continuam no menu.

## Tela Thermalright (9,16", 1920×462)

Substitui o TRCC (Thermalright Control Center 2: TRCC.exe + USBLCD.exe + USBLCDNEW.exe + ffmpeg + HWiNFO + um .NET
próprio, ~140 MB). Feche o TRCC antes (ícone dele › sair) e desligue a inicialização automática dele: a tela só aceita
um programa por vez, e quem abrir primeiro fica com ela — o DS339 Monitor não disputa, só tenta de novo a cada 2 s.

- **Maré** (padrão): o mesmo tema da DS339 num layout largo — CPU e GPU como reservatórios (nível = uso, cor =
  temperatura), relógio com dia da semana e data, e RAM/VRAM com barras.
- **Vídeo**: um clipe em loop. **Escolher vídeo…** aceita MP4, MOV, MKV, AVI, WMV e WebM — convertidos em segundo
  plano pelo Media Foundation do próprio Windows (sem ffmpeg; até 24 quadros/s e 60 s, ~1 s para cada 4 s de vídeo) — e
  os `.zt` do TRCC (os vídeos que ele já converteu ficam em `C:\TRCCCAP\Data\USBLCD\Theme1920462\Theme*\Theme.zt`). O
  clipe fica em `media\` e é lido do disco quadro a quadro; um clipe curto (até 400 quadros) é codificado para a tela só
  na primeira volta — depois cada quadro já sai pronto.
- **One Piece** e **GIF**: os mesmos do Pro Flow, cobrindo a tela inteira.
- Não há comando de brilho nem de rotação neste modelo: como no TRCC, o brilho escurece a imagem e a rotação gira a
  imagem antes de mandar. O normal desta tela é a imagem virada 180° (o TRCC faz o mesmo no submodelo dela); "Girar
  180°" marcado manda sem virar.

## GIFs no Pro Flow 360

- Ao escolher um GIF, cada quadro é redimensionado para 960×480 **uma única vez** e guardado como JPEG em memória
  (um GIF de ~100 quadros ocupa ~8 MB, contra ~180 MB em pixels crus). A preparação leva menos de um segundo.
- A animação respeita o tempo de cada quadro do GIF, com teto de `FpsMax` (padrão 24).
- Com **Dados: Nenhum**, cada quadro já sai codificado do cache: a animação custa praticamente só a USB.
- Com dados por cima, painéis e rótulos são pré-desenhados numa camada transparente; por quadro entram só os números
  e o JPEG é gerado (~1 ms). Valores novos aparecem no quadro seguinte à leitura — sem quadros extras.

## Tema One Piece (Pro Flow 360)

24 painéis marcantes do mangá, em preto e branco, **cobrindo a tela inteira** — do capítulo 1 (o braço do Shanks, o
chapéu de palha) ao 1164 (God Valley), com 14 deles antes do capítulo 600: Nami saltando para o Going Merry, a
despedida da Vivi, Jaya, Water 7, Enies Lobby e Marineford. Painéis verticais ganharam um recorte em paisagem do
trecho que importa (`recorte` no quadros.json).

Os painéis antigos em mangá quase não existem na One Piece Wiki em inglês — para esses arcos ela usa capturas do anime.
Vários vieram da wiki alemã, que usa o mangá (com o texto dos balões apagado); são menores (~700 px de largura) e
ficam um pouco mais macios ampliados.

- **Câmera lenta:** como nenhum painel tem exatamente a proporção da tela (2:1), a imagem cobre a tela e a câmera a
  percorre devagar — acelera suave, anda em velocidade constante e freia suave —, na ordem de leitura do mangá: de cima
  para baixo, ou da direita para a esquerda nos painéis mais largos que a tela. Nada do painel fica de fora.
  - A posição da câmera é fracionária: cada quadro é reamostrado (Catmull-Rom, só no eixo do movimento) na posição
    exata, a no máximo ~1 px por quadro, 30 quadros por segundo num timer de alta resolução. A tela troca a imagem no
    meio da varredura (não há sincronismo), então com passos de 2–3 px parte dela mostrava o quadro novo e parte o
    anterior e a imagem "se desencaixava"; com ≤ 1 px a emenda some.
- **Transições de mangá**, alternadas: um *corte diagonal* (a divisa branca entre dois quadros da página atravessa a
  tela, como ao virar a página) e a *retícula* (pontos de retícula crescem até revelar o próximo painel).
- **Caixa de narração** com capítulo e legenda: entra deslizando logo depois da transição e sai antes da próxima.
- Cada quadro é montado na hora: os painéis ficam na memória em tons de cinza, já no tamanho final, e montar um quadro
  custa ~2 ms. Os painéis
  passam por um ajuste de preto e branco (scans lavados ficam iguais aos outros) e por um desfoque quase invisível: a
  tela só aceita JPEG de até 80 KB, e o hachurado do mangá custa caro — sem isso um painel cheio só cabe com qualidade
  ~35 (linhas borradas em blocos); com isso cabe com ~45–60 e as linhas ficam limpas.
- Os painéis são da One Piece Wiki (© Eiichiro Oda / Shueisha) — uso pessoal. Se faltar algum (pasta apagada, por
  exemplo), **Pro Flow 360 › Baixar painéis do One Piece…** abre uma página que baixa todos pelo navegador; os
  arquivos (`op-painel-01.png`…) vão para Downloads e são importados sozinhos para `themes\One Piece\`.
- `themes\One Piece\quadros.json` lista os painéis e o tempo de cada um (`segundosPorQuadro`; painéis muito largos
  ganham mais tempo para a câmera não correr). `"legendas": false` tira as caixas de narração. Por painel, além de
  `arquivo`, `capitulo`, `legenda`, `imagem` e `wiki`, dá para ajustar o enquadramento:
  - `movimento`: `auto` (padrão), `descer`, `subir`, `esquerda`, `direita` ou `parado`;
  - `de` / `ate`: o trecho do caminho que a câmera faz, de 0 a 1 (no `parado`, `de` é onde o enquadramento fica);
  - `recorte`: `[esquerda, cima, direita, baixo]`, frações a cortar de cada lado (margens lisas já são cortadas
    sozinhas);
  - `luz`: o momento que o LED da placa encena no efeito "Mangá narrado" — `impacto`, `tambores`, `tremor`, `mare`,
    `haki`, `despedida` ou `amanhecer` (ver "LED da placa de vídeo").
  Imagens PNG/JPG extras postas na pasta entram no fim, com o nome do arquivo como legenda.

## LED da placa de vídeo (ASRock RX 6900 XT Phantom Gaming)

Substitui o ASRock Polychrome RGB para o LED da placa, com efeitos calculados pelo programa e enviados pelo modo direto
do controlador (ver "Por que o modo direto" abaixo):

| Efeito | O que faz |
|---|---|
| Desligado | O programa não mexe no LED (padrão) |
| Cor fixa | A cor escolhida |
| Respiração | A cor escolhida acendendo e apagando (curva com correção de gama, para a respiração parecer uniforme) |
| Arco-íris | Volta completa nas cores (15 s no Normal) |
| Aurora | Desliza entre verde-água, azul, violeta e rosa |
| Temperatura da GPU | Azul ≤ 35 °C → verde-água → verde → âmbar → vermelho ≥ 82 °C, deslizando entre as leituras |
| Uso da GPU | Azul em repouso → violeta → rosa → vermelho a 100% |
| Luz da página | Branco que acompanha a tinta do painel na Pro Flow: painel claro, luz cheia; painel carregado de preto, luz baixa — deslizando junto com a câmera e as viradas |
| Mangá narrado | A luz da página + a virada de painel (um "DON!" no corte diagonal, um pontilhado na retícula) + o momento de cada painel marcado no quadros.json |
| Tambores da Libertação | Batida do Gear 5 em branco, "DON do-tto-to": uma forte e três leves, sobre um fundo baixo (a Velocidade muda o ritmo) |
| Grand Line | A luz do Modo Grand Line (ver acima): sol, som, painéis e acontecimentos do PC |
| Vela | Chama viva, quente, que nunca se repete, com um sopro de vez em quando |
| Maré viva | Parece luz fixa, mas respira em três ondas lentas que nunca se alinham |
| Relâmpago distante | Penumbra fria com um raio (ou um clarão dentro da nuvem) a cada ~24 s |
| Ciclo do sol | Acompanha o dia de verdade: claro e neutro ao meio-dia, dourado no pôr do sol, baixo e morno à noite |
| Farol das horas | Luz firme; na virada da hora, um lampejo por hora, como um farol; nos quartos de hora, um lampejo longo |
| Pulso do som | Sobe com o volume do PC e bate junto com a música; em silêncio, respira calma |
| Vaga-lumes | Quase escuro e morno, com piscadas suaves soltas (às vezes em dupla, como um vaga-lume respondendo a outro) |
| Céu estrelado | Branco frio com sete "estrelas" cintilando cada uma no seu ritmo (o Twinklefox do WLED numa luz só) |
| Sombra de nuvens | Dia claro; de vez em quando uma nuvem passa devagar e a luz cai pela metade, mais fria na sombra |
| Garoa | Penumbra fria com gotas de tamanhos diferentes (acendem rápido, apagam devagar) e pancadas mais fortes às vezes |
| Fogos de artifício | Noite; a cada 7–16 s um foguete sobe, estoura (dourado ou prateado) e cai devagar com faíscas |
| Lâmpada fluorescente | Branco frio de escritório; mais ou menos a cada minuto, o reator falha: piscadas espaçadas ou apaga e reacende |
| Coração da máquina | "Tum-tum" sobre um fundo morno: 54 batidas por minuto em repouso, até 72 com a GPU trabalhando |
| Respiro (sono) | A luz de repouso dos MacBooks: 12 respirações por minuto, quase apagando embaixo |
| Respiração guiada 4-7-8 | Inspira 4 s (sobe e esfria), segura 7 s, expira 8 s (desce e esquenta) — dá para respirar junto |
| Pomodoro (foco e pausa) | 25 min de foco (branco firme e frio, pulsando no último minuto), 5 min de pausa morna; a cada 4 focos, 15 min |
| Brasa (aviva ao digitar) | Um carvão quase apagado que aviva enquanto você digita ou mexe no mouse e cochila devagar (só percebe que houve uma tecla, nunca qual) |
| Turbina (uso da GPU) | A GPU em branco: parada, respira baixo; trabalhando, sobe com o uso e ganha um zumbido; o tom vai de frio (40 °C) a morno (80 °C) |

Do "Luz da página" para baixo, todos são em branco (no máximo um tom mais quente ou mais frio), para setups brancos —
e todos respeitam o limite de 3 clarões por segundo (WCAG 2.3.1). **Tom do branco**: *Igual ao da placa* (padrão — o branco que já
estava guardado nela, acertado no Polychrome para combinar com o resto do PC), *Neutro*, *Quente* ou *Frio*. Os níveis
são na escala do olho (passam por uma curva de gama), então "metade" parece metade.

Momentos do "Mangá narrado" (campo `luz` de cada painel; entram e saem suaves com o painel):

| `luz` | Painéis | Na placa |
|---|---|---|
| `impacto` | o braço do Shanks, "os sonhos nunca acabam", Enies Lobby, o soco no Dragão Celestial, a careta do Usopp, Katakuri, King, Kuma | DON! quando o painel entra, com um eco |
| `tambores` | Gear 5 contra o Kaido | os Tambores da Libertação enquanto o painel está na tela |
| `tremor` | o Barba Branca inclina a ilha | a luz treme por uns 4 s (Gura Gura), com um estalo antes |
| `mare` | Nami no Going Merry, Aqua Laguna | ondas lentas |
| `haki` | Barba Branca, Shanks em Marineford, Roger x Barba Branca, God Valley | a luz cai e estala numa rajada; depois, um estalo raro |
| `despedida` | Vivi, Ace | a luz baixa devagar e fica fraca até a virada |
| `amanhecer` | o Novo Mundo, o sonho do Luffy | a luz cai e nasce de novo ao longo do painel |

Sem a Pro Flow (desconectada) ou com um tema parado, os dois efeitos de página ficam num branco firme. Com outro tema
animado, a "Luz da página" acompanha o brilho da imagem dele.

A placa tem dois canais: a barra de LEDs da própria placa (15 LEDs) e o conector ARGB para uma fita externa (até 80);
os dois recebem a mesma cor. Este firmware só aceita uma cor por canal nos efeitos do programa (não LED a LED).

**Segurança** (é a placa de vídeo — tudo foi feito para não ter como dar errado):

- O caminho é o próprio driver de vídeo da AMD (I2C pelo ADL, o mesmo que o Polychrome usa) — sem driver extra.
- Só controla se tudo bater com o que foi analisado e testado: placa 1002:73BF com subsistema 1849:5212, Overdrive 8,
  o chip do LED se identificando e **firmware exatamente 0x01000101**. Se o Polychrome um dia atualizar esse firmware, o
  programa para de controlar e diz o motivo no log, em vez de arriscar.
- O código só tem como enviar consultas e o comando de cor (que fica na RAM do controlador — conferido no firmware: cor
  nova não grava na flash, então animar não desgasta nada). Comandos que regravam configuração, firmware ou o modo de
  boot do chip não existem no programa.
- Toda resposta é conferida; a primeira cor de cada sessão é lida de volta em cada canal e, se não bater, o efeito é
  desligado.
- Com **Polychrome ou OpenRGB abertos, pausa** sem escrever nada — só um programa pode controlar o LED (e o Polychrome
  pode regravar o firmware do controlador sozinho: nenhuma escrita pode cruzar com isso).
- O **SignalRGB pode ficar aberto** para os outros dispositivos, com a placa de vídeo **desativada nele**. Se ele (ou
  qualquer outro) mexer no LED, a conferência periódica percebe — a cada 2 s o estado do controlador é lido de volta e
  tem que ser o que o programa mandou — e o efeito pausa por 1 min sem brigar (o log e o menu dizem o que fazer).
- 3 erros seguidos do driver → solta a placa e tenta de novo depois de 30 s, identificando tudo outra vez.
- Ao escolher "Desligado" ou fechar o programa, devolve os 16 registros de canal ao que a placa tinha e confere um por um
  (no reinício do PC a placa volta sozinha à cor salva nela pelo Polychrome).
- Se o programa for fechado à força no meio de um efeito, a placa fica parada na última cor. Na próxima abertura o
  programa percebe (canais no modo direto) e usa as cores guardadas em `led-gpu-original.txt` na última abertura normal.
- Roda numa thread própria e só escreve quando a cor muda (no máximo 25 vezes por segundo, ~2 ms cada; um comando vale
  para a placa inteira). O log registra, ao devolver as cores, quantas escritas a sessão fez e quantas deram erro.

**Por que o modo direto.** No modo normal (o que o Polychrome e o SignalRGB usam para cor fixa) o firmware redesenha os
LEDs sem parar, bit a bit, e cada comando que chega pelo I2C interrompe esse desenho no meio: às vezes os LEDs pegam um
quadro desalinhado e **piscam numa cor errada** por um instante. Quanto mais escritas, mais piscadas. No modo direto
(`0x11`) o firmware para de redesenhar sozinho e desenha uma vez a cada comando, de dentro da própria interrupção — nada o
interrompe, e a piscada some.

Detalhes do protocolo em "Como funciona".

## Temas da DS339

Os temas ficam em `themes\<nome>\`. O formato é o mesmo do JONSBO-AIO, então temas da Jonsbo funcionam copiando a
pasta para cá (temas baixados pelo JONSBO-AIO também aparecem sozinhos, se ele continuar instalado).
Só temas 960×376 com fundo em imagem são aceitos — fundo em vídeo (`back.mp4`) não é suportado de propósito: tocar
vídeo é justamente o que deixa o app original pesado.

```
themes\Meu Tema\
  Setting.txt      definição (abaixo)
  back.png         fundo 960x376
  source\*.png     imagens extras (camadas)
  font\*.ttf       fontes usadas pelo tema
```

`Setting.txt` — uma linha por elemento, propriedades `chave@valor` separadas por vírgula; `z` maior fica por cima:

```
name:MeuTema
width:960
height:376
moldura.png:x@0,y@0,z@20,width@960,height@376
Text:x@40,y@30,z@5,maxwidth@400,maxheight@80,FontSize@48,FontFamily@#Segoe UI,Foreground@#FFFFFFFF,Title@CPU,data@CPUTemp,unit@°C
Text:x@40,y@100,z@5,FontSize@20,FontFamily@#Segoe UI,Foreground@#FFAAAAAA,IsDefaultText@true,data@Texto fixo
BorderLine:x@40,y@150,z@6,maxwidth@300,maxheight@12,MaxNum@100,CornerRadius@6 6 6 6,Fill@#FF38BDF8,BorderThicknes@0,BorderFill@#00000000,BackColor@#FF263042,data@CpuUsage
```

- `Text` — mostra `Title valor unit`. Com `IsDefaultText@true`, `data` é o próprio texto.
- `BorderLine` — barra de progresso (`MaxNum` = valor que enche a barra). Mais alta que larga = vertical.
- Cores em `#AARRGGBB`.

Dados disponíveis em `data@`:

| Grupo | Chaves |
|---|---|
| Relógio | `CurrentTime` (HH:mm:ss), `CurrentTimeShut` (HH:mm), `CurrentH`, `CurrentM`, `CurrentDate` (dd/MM/aaaa), `CurrentDates` (aaaa.MM.dd), `CurrentDateLong` (Terça-feira, 13 de outubro), `CurrentDateShort` (Ter, 13 out), `CurrentDatesMD`, `CurrentDatesY/M/D`, `WeekDays`, `WeekDaysL3` |
| CPU | `CpuUsage`, `CPUTemp`, `CpuFrequency` (MHz), `CpuFrequencyGhz`, `CpuTEC` (W), `CpuVoltage`, `CPUName` |
| GPU | `GpuUsage`, `GPUTemp`, `GpuHotSpot`, `GpuFrequency`, `GpuFrequencyGhz`, `GPUMemoryFrequency`, `GpuTEC` (W), `GpuVoltage`, `GPUMemoryLoad` (% VRAM), `GPUMemoryUse` (x/y GB), `GpuFan` (RPM), `GPUName` |
| Memória | `MemoryUsage` (%), `MemoryUse` (x/y GB), `MemoryUsageSize`, `MemoryCanUsageSize`, `MemorySize` (total) |
| Disco | `HardDiskCapacity`, `HardDiskUsed` (%), `HardDiskUsedSize`, `HardDiskFree` (unidade do Windows); `DiskCUsed`…`DiskFUsed` (%) |
| Rede | `UpNetSpeed`, `DownNetSpeed` |

Para ver um tema sem mexer na tela: `DS339Monitor.exe --renderizar "Meu Tema" previa.png`.

## config.json

Criado ao lado do executável. Pode editar com o programa fechado (ou use `--sair` antes).

```json
{
  "IntervaloMs": 1000,      // de quanto em quanto tempo os sensores são lidos
  "Idioma": "pt-BR",        // dia da semana
  "Gpu": null,              // trecho do nome da GPU, se quiser forçar uma (vazio = a dedicada)
  "DS339": {
    "Tema": "Maré",
    "Rotacao": 90,          // 90 = normal, 270 = de cabeça para baixo
    "Brilho": 100           // 5–100
  },
  "ProFlow": {
    "Tema": "GIF",          // "GIF" ou "Maré"
    "Gif": "C:\\...\\media\\nika-luffy.gif",
    "Dados": "Completo",    // Nenhum | Compacto | Completo
    "Brilho": 33,           // 5–100
    "Rotacao": 180,         // 0 ou 180
    "FpsMax": 30            // teto de quadros por segundo das animações
  },
  "LedGpu": {
    "Efeito": "Desligado",  // Desligado | Cor fixa | Respiração | Arco-íris | Aurora | Temperatura da GPU | Uso da GPU
    "Cor": "#00A2FF",       // cor de "Cor fixa" e "Respiração"
    "Brilho": 100,          // 5–100
    "Velocidade": "Normal"  // Lenta | Normal | Rápida
  }
}
```

O formato antigo (só DS339, com `Tema`/`Rotacao` na raiz) é convertido sozinho.

## Linha de comando

| Comando | O que faz |
|---|---|
| `--teste [segundos] [rotação]` | Padrão de teste na DS339 (confere orientação e conexão) |
| `--renderizar "Tema" arquivo.png [ds339\|proflow] [arquivo.gif] [estilo] [quadro]` | Salva o tema com valores reais num PNG. `quadro`: número, ou no One Piece `p3:0.2` (painel 3, a 20% do movimento da câmera) / `t3:0.5` (metade da transição que sai do painel 3) |
| `--capturar` | Pede à instância aberta (mesmo como admin) que salve as telas em `tela-ds339.png` / `tela-proflow.png` |
| `--sair` | Pede à instância aberta (mesmo como admin) que feche — útil para atualizar sem UAC |
| `--baixar-one-piece` | Gera e abre no navegador a página de download dos painéis do tema One Piece |
| `--diagnostico` | Lista temas, fontes e todos os sensores no `ds339.log` |
| `--led-gpu` | Identifica o LED da placa de vídeo e registra o estado de cada canal no log (só leitura) |
| `--led-gpu-teste "Efeito" [segundos] [#cor] [início]` | Roda um efeito do LED por alguns segundos e devolve as cores originais (com o programa fechado). Nos efeitos de página, com uma Pro Flow virtual a partir de `início` s do tema One Piece |
| `--thermalright` | Procura a tela Thermalright, se apresenta a ela e registra no log o número de série (o TRCC tem que estar fechado) |
| `--abrir` | Abre o painel; se o programa não estiver aberto, abre pela tarefa agendada (como administrador, sem o UAC) |
| `--criar-atalhos` | Cria os atalhos "DS339 Monitor" na área de trabalho e no menu Iniciar |
| `--painel-imagem página` | Abre o painel na página (overview, thermalright, proflow, led, argb, modes, system; `argb@900` = rolada 900 px) e salva uma imagem em painel.png |
| `--desligar-tudo` / `--ligar-tudo` / `--alternar-tudo` | Pede ao programa aberto que apague (ou acenda) as telas, o LED e os fans e fitas e feche (ou reabra) o SignalRGB |
| `--fitas assumir\|devolver\|identificar N` | Passa os fans e fitas para o programa (fechando o SignalRGB), devolve ao SignalRGB, ou acende só a parte N por 8 s (numa capa de cabo, 14 s com a ordem dos LEDs) |
| `--rgb-info` | Registra no log o que o Windows sabe da controladora Nollie (só leitura) |
| `--nollie-ler` | Lê o que está gravado na controladora (efeitos, canais, LEDs) e guarda uma cópia em nollie-config-*.json (só leitura) |
| `--fitas boot` | Pede ao programa aberto que deixe a controladora tocar o efeito de boot por 12 s |
| `--fitas animacao Nome` / `--fitas gravar-animacao Nome [minutos]` / `--fitas tocar-animacao` | Toca uma animação de boot nos fans pelo programa / grava-a na memória da controladora, emendada em N min do loop dos fans (sem mudar o boot) / pede à controladora que toque o que tem gravado |
| `--nollie-fseq-teste [Nome] [minutos]` | Sem tocar na controladora: confere o CRC e monta o arquivo FSEQ da animação (nollie-boot.fseq) |
| `--fitas ventoinha N` | Pede ao programa aberto que ponha a saída de PWM N da placa-mãe em 100% por 6 s (para achar a do hub) |
| `--placa-mae` | Registra no log os fans, os controles de PWM e as temperaturas da placa-mãe (chip ITE IT8613E; só leitura, precisa de administrador — pede à instância aberta) |
| `--fitas-previa "Cena" [saida.png] [quadros] [segundos] [momento]` | Desenha uma cena dos fans (ou uma animação de boot: `--fitas-previa "Curto-circuito" saida.png 8 "1.4,2.9,3.3"`) sobre a foto do gabinete em vários instantes, sem tocar na controladora |
| `--manga-previa efeito [saida.png] [quadros] [segundos] [início] [config.json]` | Encena um efeito do Modo Mangá sem tocar no hardware, por cima do vídeo e dos painéis de verdade, e desenha vários instantes das duas telas e da luz da placa |
| `--mundo-previa história [saida.png] [quadros] [segundos] [noite]` | Roda uma história do Modo Grand Line sem tocar no hardware e desenha vários instantes: a Thermalright, a Pro Flow e a luz da placa, como se vê pelo vidro |
| `--historia nome` | Pede ao programa aberto que encene uma história (knockup, return, anchor, weigh, news, poster, bubble, brook, chest, denden, eruption, lightning, gull, star, bell, laboon, mute) |
| `--sinais [segundos]` | Registra no log as ações do sistema que o programa percebe (copiar, print, pendrive, volume, microfone, app em foco, downloads, Spotify) |
| `--grand-line-previa [saida.png] [cena]` | Desenha o tema Grand Line em 15 situações (dia, pôr do sol, noite, tempestade e cada momento) numa folha, ou só a cena pedida em tamanho real, e mede o custo por quadro |
| `--converter-video entrada.mp4 [saida.zt]` | Converte um vídeo para o tema Vídeo da Thermalright (Media Foundation do Windows) |
| `--thermalright-teste [segundos] [tema]` | Liga só a Thermalright pelo mesmo caminho do programa (tema, sensores, JPEG, envio) e registra quadros e tempo de envio (com o programa fechado) |
| `--led-gpu-simular "Efeito" [saida.png] [segundos] [início]` | Sem tocar na placa: roda o efeito sobre uma Pro Flow virtual (One Piece) e desenha a luz ao longo do tempo — cor, nível, brilho da tela e os painéis com o momento de cada um |
| `--autostart on\|off` | Liga/desliga a tarefa agendada (precisa de admin) |

## Compilar

```powershell
dotnet publish -c Release -o publish
```

## Como funciona

- A DS339 é um chip MacroSilicon **MS9132** (USB `345F:9132`), um "monitor USB" com driver libusb0. A tela é retrato
  (376×960); os temas são desenhados deitados (960×376) e girados 90°.
- `lib\MSDISPLAYSDKWRRAPER.dll` é o SDK do fabricante, extraído do JONSBO-AIO. O protocolo foi reconstruído a partir do
  app original: `Start` com EDID customizado (VIC 171 = 376×960) → callback de conexão → `SetVideoParam` →
  `SendPicture` com pixels BGRA 32 bits.
- Brilho, nas duas telas, é aplicado escurecendo a imagem (SIMD, ~0,1 ms por quadro). A DS339 não tem controle de luz
  de fundo conhecido (o JONSBO-AIO não mexe nisso), e a tela do Pro Flow ignora o comando de brilho no modo ao vivo.
- O Pro Flow 360 é um chip **TXW818** com painel ST7701S, ligado como porta serial USB (`33C3:7788`, 2 Mbaud, com DTR
  ligado — sem DTR a tela não responde). Protocolo reconstruído do Jungle Leopard Display (app Electron; o código
  JavaScript estava ofuscado, mas legível):
  - comando = `55 AA` · tamanho total (2 bytes LE) · código · dados · soma de todos os bytes (2 bytes LE);
    `6` = informações (resposta em JSON: modelo, firmware, resolução), `3` = brilho, `17` = modo ao vivo, `1` = reiniciar;
  - o brilho de hardware (comando 3) é ignorado no modo ao vivo. Como o app original, o programa manda 100 uma vez
    antes de entrar nele e aplica o brilho escolhido **escurecendo a imagem** (SIMD, ~0,1 ms por quadro; no modo
    "só GIF" o brilho já fica embutido no cache);
  - no modo ao vivo cada quadro vai cru: `[tamanho do JPEG (4 bytes LE)][JPEG ≤ 80 KB][soma (2 bytes LE)]`, e o
    comando 17 é repetido a cada 1,5 s; `FF D9 FF D9` encerra a transmissão;
  - a tela espera um **fluxo contínuo**: o app original manda 30 quadros por segundo mesmo com a imagem parada. Com
    pausas longas entre quadros ela pisca e às vezes volta a mostrar por instantes um quadro anterior. Por isso o último
    quadro é reenviado sozinho a cada 100 ms (sem recodificar: é só a USB) e logo depois de cada comando 17;
  - mandar um quadro leva ~18 ms de USB; quem escreve na porta é uma thread própria, para isso não atrasar o quadro
    seguinte de uma animação;
  - JPEG acima de 80 KB a tela não mostra direito: a qualidade desce até caber. Ela fica estável de um quadro para o
    outro (oscilando, os artefatos "fervem" numa imagem em movimento): desce na hora, sobe devagar quando sobra espaço;
  - como o app original, a tela é reiniciada a cada 36 h (provável contorno de algum problema do firmware).
- No Zen 4/5 a tensão da CPU vem do VID dos núcleos convertido pela fórmula SVI3 (o LibreHardwareMonitor usa a
  fórmula antiga SVI2 e mostra ~0,45 V); é a tensão *pedida* ao regulador, não uma medição do regulador.
- GPU AMD é lida direto do driver (ADL, ~0,5 ms por leitura). CPU (e GPUs NVIDIA/Intel) via
  [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor). Uso de CPU, RAM, disco e rede
  vêm de APIs do próprio Windows.
- Desenho e sensores rodam numa única thread de prioridade baixa, que dorme até o próximo evento (virada do segundo ou
  próximo quadro da animação, acordando na hora certa com um timer de alta resolução — o relógio comum do Windows só
  anda de 15,6 em 15,6 ms). Quadros de animação que venceram saem antes da leitura dos sensores. Sem nenhuma tela
  conectada, só verifica a cada meio segundo se alguma apareceu. O tempo de cada etapa e a memória vão para o
  `ds339.log` (1 min depois de abrir, depois a cada 30).
- **Thermalright** — levantado do TRCC 2 (USBLCDNEW.dll, `ThreadSendDeviceDataLY`; e FormCZTV.ImageToJpg no TRCC.exe):
  USB 0416:5408, bulk 0x09 (saída) / 0x81 (entrada), via `winusb.dll` (interface {3876E417-E089-4BCC-BBF8-47ABA55E46FB}).
  Apresentação: 2048 bytes `[02 FF 00 00 00 00 00 00 01 …]` → resposta `[03 FF … 01 …]`, série nos bytes 16–19, modelo
  no 20 (0–3 = 1920×462). Quadro: JPEG (até 450 KB) em blocos de 512 bytes — 16 de cabeçalho
  `[01 FF | tamanho (4) | bytes no bloco (2) | 01 | nº de blocos (2) | índice (2) | 0 0 0]` + 496 de JPEG —, completados
  com blocos zerados até múltiplo de 4, em escritas de 4096 bytes; a tela confirma com 512 bytes. O TRCC conversa com o
  USBLCDNEW por memória compartilhada ("shareMemory_ImageRGB"), o que permitiu ver, só lendo, o modelo e o quadro que
  ele mandava: JPEG 1920×462 qualidade 95, ~190 KB.
- **LED da placa de vídeo** — um microcontrolador Nuvoton NUC121 no I2C da placa (endereço 0x36, linha 1 do ADL nas
  placas Overdrive 8, 100 kHz). Levantado do ASRock Polychrome RGB (C++Builder; a classe escolhe a linha pela versão de
  `ADL2_Overdrive_Caps`) e conferido no firmware do controlador (`ASRISP\APROM_VGA_201201_7EB9.bin`, Cortex-M0):
  - escrita: registrador = comando, 12 bytes `[0x00, canal/sub, 8 bytes, 0x00, 0x00]`; o controlador executa ao receber o
    12º byte (registrador + 11). Resposta do comando anterior em 12 bytes no registrador 0x9F:
    `[eco do canal/sub, 0, contador, 0, dados]`;
  - `0x10` cor do canal `[modo, R, G, B, velocidade, brilho, sync, extra]` (só RAM), `0x11` lê o canal, `0x14` lê a
    configuração (1 = canais ativos, 2 = LEDs por canal), `0xB1` identifica o chip, `0xA6` versão do firmware;
  - `0x12` grava a configuração na flash, e `0x15`, `0x20`, `0xA1`, `0xAB`–`0xAD` e `0xD0` mexem na configuração, no
    firmware ou no boot do chip — por isso o programa não tem como enviá-los;
  - o firmware desenha os LEDs (WS2812) por bit-banging com `nop`s e nunca desliga interrupções; a única interrupção
    ligada é a do I2C. No modo `1` o laço principal redesenha sem parar e o tratamento de cada comando (com uma espera de
    ~130 µs dentro da interrupção) estica bits no meio do quadro — a piscada. Os modos `0x11`/`0xE1`/`0xE2` ligam uma
    trava que para esse laço; o `0x10` com eles grava os 16 registros de canal e desenha tudo de dentro da interrupção
    (~6 ms). O primeiro comando em outro modo destrava o laço;
  - nesse desenho o controlador segura o SCL: com os 12 bytes de dados do formato do Polychrome, o 13º byte fica preso e
    o driver devolve erro em toda escrita (medido: −1 após ~6 ms, embora a cor seja aplicada). Com 11 bytes o pacote
    acaba no byte que dispara o comando e as escritas voltam a dar certo (~1,9 ms, 0 erros em centenas). Depois de uma
    escrita direta o programa espera 20 ms antes de qualquer outra transação.
