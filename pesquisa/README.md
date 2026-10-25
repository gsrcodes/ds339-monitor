# Pesquisa: engenharia reversa da Nollie e do Asiahorse UMBRA

Material de estudo que deu origem ao controle da Nollie 16CH e ao Modo Umbra do programa. **Não faz parte do
programa** (o `.csproj` ignora esta pasta). É código de terceiros descompilado ou desmontado, guardado aqui para uso
pessoal e para refazer/conferir o trabalho. Nada disto foi executado: os programas foram só extraídos e lidos.

## O que tem aqui

| Pasta | O que é |
|---|---|
| `nollie/desmontagem/` | Desmontagem (xdis) dos módulos do **NollieRGB** (app oficial da Nollie, Python 3.11 empacotado com PyInstaller): o transporte HID, a lista de efeitos, a configuração, o formato FSEQ, a gravação na flash |
| `nollie/desmontagem-telas/` | Desmontagem das telas do NollieRGB (início, ferramentas internas, atualização) |
| `umbra/descompilado/` | O software **AsiaHorse UMBRA 3.5.5** ("Sync_light", da Robobloq; Python 3.8 + PySide2), descompilado com o decompyle3: `effects.py` (os efeitos do modo tela, classe `LightEffectManager`), `fan.py` (a janela principal: modos, zonas, efeitos dos fans), `strings.txt` (todos os textos de cada função), `yuyan.txt` (as traduções chinês → inglês → alemão da interface), `cjk.txt` |
| `umbra/descompilado/ui/` | Os widgets próprios da interface (`pyside2_kittle_ui`) |
| `umbra/descompilado/modulos/` | `port.py` (portas e aparelhos), `json_init.py` (os padrões), `agreement_funcs.py` (o protocolo do hub), `initui.py` (a montagem da tela) |
| `umbra/descompilado/analise/` | Trechos refeitos à mão a partir do bytecode onde o decompilador errou (radar, matrix, correnteza, meteoros, tempestade) |
| `umbra/teste-modo-tela/` | O teste usado para conferir a tradução do modo tela (`sheet.png` = os 12 efeitos lado a lado) |
| `ferramentas/` | Os scripts usados (extração do PyInstaller, descompilação e desmontagem de Python 3.8/3.11, listagem de textos) |
| `umbra/especificacao.md` | O que se descobriu do UMBRA: modos, zonas, efeitos, parâmetros, protocolo, modo tela |

O que ficou de fora, por tamanho ou por ser binário de terceiros (dá para baixar de novo): os instaladores (o do
UMBRA tem 109 MB, acima do limite do GitHub), as pastas extraídas (DLLs, executáveis, o bytecode `.marshal`/`.pyc`) e o
ambiente Python das ferramentas.

## Como refazer

Use uma pasta só para isso, fora do projeto, e rode o Python sempre com `-I` (isolado), para nada da pasta extraída
ser carregado por engano.

**NollieRGB** (o protocolo resumido está no README principal, seções "Guardado na controladora" e "Animação de boot"):
1. Baixe o NollieRGB em <https://nolliergb.com> (ou <https://nollie.cn>).
2. `python -I ferramentas/pyi_extract.py NollieRGB.exe saida/` — tira o arquivo do PyInstaller sem executar.
3. `python -I ferramentas/dis311.py saida/pyz/<modulo>.marshal desmontagem.txt` — precisa do `xdis` (`pip install xdis`).

**AsiaHorse UMBRA**:
1. Baixe "AsiaHorse UMBRA 3.5.5" na central de download: <https://asiahorse.store/pages/software-download-center>.
2. Extraia o instalador (Inno Setup) com o [innoextract](https://constexpr.org/innoextract/): `innoextract -s -d x UMBRA.exe`.
3. `python -I ferramentas/pyi_extract.py x/app/Sync_light.exe saida/` — o executável principal é o `Sync_light.exe`.
4. Num ambiente com `decompyle3` e `xdis`:
   - `python -I ferramentas/decomp38.py saida/pyz/<pasta ou arquivo> destino/` — descompila módulos inteiros;
   - `python -I ferramentas/decomp_funcs38.py Sync_light.pyc saida.py "<regex>"` — função por função (o arquivo principal
     inteiro trava o decompyle3; por função, as que falham saem como desmontagem);
   - `python -I ferramentas/strings38.py Sync_light.pyc strings.txt` — funções e textos.

## O que se descobriu (resumo)

- **Nollie**: comandos de configuração `FC`/`F9`/`FA`, efeitos guardados por "canvas" (0–13 normais, 14–15 de boot),
  a animação de boot em FSEQ v2 gravada na flash externa a partir de 0x10000 (`F4 05/06/07`), CRC-32/MPEG-2, a lista
  do que nunca mandar (atualização de firmware `FD`/`80–84`, restaurar/zerar `FA 21–23`). Detalhes no README principal.
- **UMBRA**: o hub é da Robobloq; os 15 efeitos de fan são desenhados pelo **chip do hub** (o software só manda número,
  velocidade, direção, brilho e 4 cores), o modo tela é desenhado pelo **software** numa tela de 600×600. Ver
  `umbra/especificacao.md`.
