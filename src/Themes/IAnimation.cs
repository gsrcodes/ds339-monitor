using System.Drawing;

namespace DS339Monitor.Themes;

/// <summary>
/// Fundo animado de um tema: quadros numerados, cada um com sua duração. O GIF (<see cref="FrameAnimation"/>) guarda
/// todos os quadros prontos; a apresentação do One Piece (<see cref="PanelSlideshow"/>) monta cada quadro na hora.
/// </summary>
internal interface IAnimation : IDisposable
{
    int Count { get; }
    int Width { get; }
    int Height { get; }

    /// <summary>Poucos quadros repetidos em loop: vale guardar cada um já codificado para a tela.</summary>
    bool CacheFrames { get; }

    /// <summary>Duração do quadro, em ms.</summary>
    int DelayMs(int index);

    /// <summary>Quadros com a mesma chave são idênticos: não precisam ser redesenhados nem reenviados.</summary>
    long FrameKey(int index);

    /// <summary>Desenha o quadro ocupando o canvas inteiro (o quadro é opaco).</summary>
    void DrawFrame(Bitmap canvas, Graphics g, int index);
}
