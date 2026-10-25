# AsiaHorse UMBRA (Sync_light 3.5.5): o que o código mostra

Tirado do código descompilado em `descompilado/` (nada foi executado). "Certo" = está no código; "inferido" = dedução
pelo nome ou pelo uso.

## Efeitos dos fans (desenhados pelo chip do hub)

O software não tem a aparência dos efeitos: só nomes, números e quais controles cada um mostra
(`ARGB_effect_check_list = [id, velocidade, direção, brilho, nº de cores]`).

### Os três modos (abas do "ARGB Controller")

Abas: 同步灯效 Sincronizado, 异步灯效 Independente, 合并灯效 Combinado, 风扇调速 Fans, 控制器设置 Configurações.

- **Sincronizado** (certo): um efeito para todas as portas e todos os LEDs, sem zonas; cada aparelho mostra a mesma cópia.
- **Combinado** (pacote certo, sentido inferido): todas as portas como uma corrente contínua. Só os 10 efeitos
  "espaciais" (sem Estático, Respiração e Discoteca).
- **Independente** (certo): efeito por porta. 10 botões de porta ("1".."10"); portas com 0 LEDs somem.
  - Aparelhos de várias zonas ("jimu/联排") ganham a coluna de zonas: 中心 Centro, 并联 Paralelo, 串联 Série, 左侧 Esquerda,
    右侧 Direita, 全部 Tudo, e um interruptor 同步 ("juntos").
  - **Cada zona é um slot inteiro** (`ARGB_TEN_MODEL1[porta][slot] = [efeito, velocidade, direção, brilho, 12 bytes de
    cor]`): o centro e as laterais podem ter **efeitos diferentes**, não só cores.
  - Escolher a zona escolhe o arranjo (`yibu_jimu_id_changer`): Paralelo → modo 1 (Centro, Paralelo, Tudo); Série →
    modo 2 (Centro, Série, Tudo); Esquerda/Direita → modo 3 (Centro, Esquerda, Direita); Tudo → modo 4 (o slot Tudo
    nas 3 posições). Centro mantém o modo e só escolhe o slot 0 para editar.
  - Ícones das zonas: Centro = anel aceso; Paralelo = as duas laterais na mesma cor (espelhadas); Série = lateral
    esquerda vermelha e direita amarela (inferido: as duas como uma fita só); Tudo = anel + laterais.
  - "Juntos" copia os slots da porta escolhida para as outras de várias zonas (só aparece com 2 ou mais).
  - Por tipo de aparelho: tipo 9 esconde Centro e Paralelo; tipo 21 esconde todas as zonas.

### Os 15 efeitos (grade 5×3; o número é a posição na grade)

| Nº | Nome | Velocidade | Direção | Brilho | Cores |
|---|---|---|---|---|---|
| 0 | 静态色 Static | – | – | ✓ | 1 |
| 1 | 彩虹 Rainbow | ✓ | ✓ | ✓ | 0 |
| 2 | 渐变彩虹 Gradient Rainbow | ✓ | – | ✓ | 0 |
| 3 | 呼吸 Breath | ✓ | – | ✓ | 4 |
| 4 | 呼吸彩虹 Breath Rainbow | ✓ | – | ✓ | 0 |
| 5 | 跑道 Raceway | ✓ | ✓ | ✓ | 1 |
| 6 | 平铺 Tiling | ✓ | ✓ | ✓ | 4 |
| 7 | 渐灭 Fade Out | ✓ | ✓ | ✓ | 1 |
| 8 | 拖尾 Trail | ✓ | ✓ | ✓ | 4 |
| 9 | 扫描 Scan | ✓ | – | ✓ | 2 |
| 10 | 积木 Building Blocks | ✓ | ✓ | ✓ | 1 |
| 11 | 迪斯科 Disco | ✓ | ✓ | ✓ | 4 |
| 12 | 收张 Expand and Contract | ✓ | – | ✓ | 4 |
| 13 | 闪电 Lightning | ✓ | – | ✓ | 2 |
| 14 | 寻址 Addressing | ✓ | ✓ | ✓ | 1 |

- Sincronizado + Estático: a cor 1 vai nos 4 slots de cor.
- Independente + Estático: uma cor por fan encadeado (LEDs ÷ 26 no tipo 1, ÷ 18 no 9, ÷ 34 no 16, ÷ 11 no 21).
  Tipos 11 e 18: Centro com 3 cores, Tudo com 4, o resto com 1.
- Na tabela também: 20 (tela, enviado pelo software), 21 (som) e 22 (as regras do modo fixo de boot).
- **Combinado**: grade 4×3; o byte 0 é o índice 0–9: 0 Rainbow, 1 Raceway, 2 Tiling, 3 Fade Out, 4 Trail, 5 Scan,
  6 Building Blocks, 7 Expand and Contract, 8 Lightning, 9 Addressing (regras dos números 1, 5, 6, 7, 8, 9, 10, 12, 13, 14).

### Valores

- **Velocidade**: deslizante 0–50 de 10 em 10; `byte = (60 - v) // 10` → 6 níveis (1 = mais rápido, "100%"; 6 = mais
  lento, mostrado como "1%"); o rótulo é `v * 2 %`.
- **Brilho**: deslizante 0–50 de 10 em 10; `byte = v // 10 * 51` → 0/51/102/153/204/255; 0 vai como 50 ("1%").
- **Direção**: duas setas; byte = índice do botão (1 = seta direita, 正序 "normal"; 0 = seta esquerda, 倒序 "inverso").
- **Padrões** (`json_init.ARGB`): todo slot começa `[velocidade 3, direção 1, brilho 153, vermelho, verde, azul,
  amarelo]` (60% e 60%); o Combinado começa na velocidade 2; Sincronizado e os slots do Independente começam no
  Rainbow; a zona escolhida começa em Tudo.
- **Paleta**: 4 quadrados de 40×40 + o botão 调色板 (abre o seletor e define a "cor atual", padrão `[17, 78, 255]`;
  clicar num quadrado põe a cor atual nele e já envia).

### Prévias e geometria

- Sem prévias animadas: cada efeito tem um ícone SVG fixo (`img/ARGB/effect_svg/NN.svg`, NN = número + 1).
- Ícones das portas com 1, 2 ou 3 fans conforme os LEDs; LEDs por fan por tipo: 1 = 26, 9 = 18, 16 = 34, 21 = 11.
- Não encontrado: como os LEDs se dividem entre centro e laterais, e qual tipo é o Matrix Pro (o hub informa o tipo).

### Protocolo (`agreement.py`)

Quadro `[0x00, 'R' 0x52, 'B' 0x42, TAM, 0x00, CMD, dados…, SOMA]` (TAM conta os bytes antes da soma; soma & 0xFF).

- Efeito: `CMD 0x03, ORDEM, NUM` + 52 bytes.
  - Sincronizado: ORDEM 255, NUM 255; `d[0]` efeito, `d[1]` velocidade, `d[2]` direção, `d[3]` brilho, `d[4..15]` 4 cores.
  - Combinado: ORDEM 254, NUM 254; igual, com `d[0]` = índice do combinado.
  - Independente: ORDEM 253, NUM = porta 0–9; `d[0]` arranjo (1–4), `d[1..6]` efeito dos 6 slots, `d[7..9]`
    velocidades, `d[10..12]` direções, `d[13..15]` brilhos dos 3 slots ativos, `d[16..51]` 4 cores de cada um.
- Outros: `0x06` apagar; `0x0B v` modo de boot; `0x0C v` autoteste; `0x05 id,1` escolher porta; `0x02 id,func,param`
  ajuste da porta (func 2 = sentido); `0xFD v` luz pela placa-mãe × pelo software; `0xF9` salvar; `0xFA` restaurar;
  `0xFC` reiniciar; `[0,52,42,07,00,01,FF]` ler as 10 portas.

### Boot (aba Configurações, 开机灯效设置)

- 记忆模式 Memória (`0x0B 255`, padrão): o hub liga no último efeito.
- 固定模式 Fixo (`0x0B 1` + `0x03 254/254`): efeito fixo (padrão 10), só velocidade, direção, brilho e a cor 1
  editáveis (padrão: velocidade 1, direção 1, brilho 153, branco).
- 开机自检 Autoteste: `0x0C 255` ligado (padrão), `0x0C 1` desligado (a animação de "curto-circuito" que o usuário gostava
  é, ao que tudo indica, este autoteste).

## Modo tela (幕布模式, desenhado pelo software)

- `LightEffectManager.color_buffer`: 600×600×3, 0–255, `[y, x, RGB]`; grade de 20 px onde os ícones encaixam. Muitos
  efeitos desenham em 100×100 (aurora e correnteza 160×100, chamas 150×150) e ampliam por vizinho mais próximo.
- Um `QTimer` de 16 ms chama `update_effect()`, amostra os LEDs e envia a cada tique; os efeitos andam "velocidade" por
  quadro (não usam o tempo decorrido; só o relógio lê a hora).
- Cada LED pega `color_buffer[y, x]` (preto fora da tela); limite de corrente opcional do hub (`r+g+b` acima do limite
  escala os três); brilho `int(v * b / 10)` com b 1–10; **sem correção de gama**. Pacote: `[0, 0x88, total, n,
  60 bytes = 20 LEDs, soma % 256]`.
- Ícones: 50 px de largura × 150/250/300/450 px; horizontal ou vertical; "power direction" inverte a ordem. Fan de
  26 LEDs (tipo 1): por fan, 4 LEDs no topo do miolo, 4 embaixo (invertidos), 9 na borda de baixo e 9 na de cima.
- Configuração guardada em `%APPDATA%\robobloq\hid_ble\fan_mubu_effect_config.json`; padrões em
  `json_init.MUBU_EFFECT_INIT` (versão 42, efeito inicial `rainbow`, brilho 10).

Os 74 efeitos, na ordem da tela (chinês / inglês / chave): Rainbow Flow `rainbow`, Wave `wave`, Blocks `block`,
4-Way Tile `four_directions`, 4-Corner Fill `corner_fill`, Random Rings `random_circle`, 4th Dimension
`fourth_dimension`, Bubbles `bubble`, DNA Helix `dna_helix`, Radar Scan `radar`, Impact `impact`, Gradient Push
`gradient_push`, Music 1–4 (`music_visualizer`, `diagonal_visualizer`, `music_center_side`, `sound_bar_visualizer`; som),
Dual Screen `screen_effects` (captura de tela), Snake `snake`, Clock `clock`, Fireworks `fireworks`, Matrix Flow
`matrix_flow`, The Matrix `matrix_rain`, Fluid Fill `fluid_fill`, Box Stack `box_pushing`, Black Hole `black_hole`,
Custom Wave `custom_wave`, Flame `fire`, Aurora `aurora`, Rapid Flow `riptide_wave`, Linear Gradient `gradient`,
Interlace `interleave`, Spiral Rainbow `spiral_rainbow`, Meteor `meteor`, Blue Sky Clouds `sky_cloud`, Camo Fade
`camo_fade`, Laser `lasers_lasers`, Storm `storm`, Explosion `explode`, Rainbow Tunnel `rainbow_tunnel`, Super Saiyan
`saiyan`, Custom Spiral `spiral`, Poison `poison`, Windmill `windmill`, Maple Leaf `maple_leaf`, Lightsaber Duel
`lightsaber_battle`, Trail Grow `trace_growth`, Police Light `police_light`, 3D Sphere Aura `sphere_3d`, Psychedelic
Dream `dream_dream`, Neon Sunset Wave `neon_wave`, Fireflies `fireflies`, Hellfire `hellfire`, Dragon Flame
`dragon_flame`, Hydrogen Orbit `hydrogen`, Hyperspace Starfield `hyperspace`, Jellyfish `jellyfish`, Koi Pond
`koi_pond`, Laser Stage `lasers`, Lava Lamp `lava_lamp`, Super Mario `mario`, Multiverse `multiverse_simple`, Nebula
`cosmic_nebula`, Neon Grid `neon_grid`, Neural Link `neurolink`, New Year Fireworks `new_years`, Paint Splash
`paint_splatters`, Party Parrot `party_parrot`, Pixel Wave `pixel_wave`, Planet Orbit `planetary_orbit`, Plasma Flow
`plasma_flow`, Toxic Spread `poison_max`, Rainbow Ripple `rainbow_tap`, Resident Evil `resident_evil`, Rick and Morty
`rick_morty`. (Duas chaves do despacho, `lightsaber_duel` e `math_3d_rose`, não têm método.)

Portados para o programa (`src/Argb/Umbra/CanvasEffects.cs`, mesma matemática): Rainbow Flow, 4th Dimension, Radar
Scan, The Matrix, Flame, Aurora, Rapid Flow, Meteor, Storm, Hyperspace Starfield, Jellyfish, Lava Lamp. Diferenças:
`System.Random` no lugar do sorteio do numpy (mesmas chamadas, na mesma ordem); passo fixo de 16 ms; valores fora de
0–255 limitados em vez de "dar a volta"; estado separado por efeito (no original alguns campos eram compartilhados).
