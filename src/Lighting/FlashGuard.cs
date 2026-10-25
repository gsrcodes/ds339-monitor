using System.Drawing;

namespace DS339Monitor.Lighting;

/// <summary>
/// Trava dura contra pisca-pisca, na saída do LED (vale para todos os efeitos e para qualquer soma deles). Segue a
/// definição da WCAG 2.3.1: um clarão é um par de variações opostas de pelo menos 10% da luminância máxima em que o lado
/// mais escuro fica abaixo de 80%; no máximo 3 clarões (6 variações) em qualquer janela de 1 s. Se a próxima variação
/// passaria do limite, a luz fica presa a menos de 10% do último extremo até a janela liberar — o efeito continua,
/// só sem o pico. A luminância é a do LED (o valor de cada canal já é proporcional à luz).
/// </summary>
internal sealed class FlashGuard
{
    private const double Step = 0.1;
    private const double DarkLimit = 0.8;
    private const int MaxTransitions = 6;
    private const long WindowMs = 1000;

    private readonly Queue<long> _transitions = new();
    private double _run = -1; // o ponto mais longe no sentido da última variação (o último pico ou vale)
    private int _trend;       // sentido da última variação contada: +1 subiu, -1 desceu, 0 nenhuma ainda

    /// <summary>Quantas vezes segurou a luz desde a última consulta (para o log).</summary>
    public int Held { get; private set; }

    public void Reset()
    {
        _transitions.Clear();
        _run = -1;
        _trend = 0;
    }

    public int TakeHeld()
    {
        int held = Held;
        Held = 0;
        return held;
    }

    public Color Apply(Color color, long nowMs)
    {
        while (_transitions.Count > 0 && nowMs - _transitions.Peek() > WindowMs) _transitions.Dequeue();
        double y = Luminance(color);
        if (_run < 0)
        {
            _run = y;
            return color;
        }

        // Seguindo no mesmo sentido: o extremo só anda.
        if ((_trend >= 0 && y > _run && _trend == 1) || (_trend == -1 && y < _run))
        {
            _run = y;
            return color;
        }

        // Uma variação que conta: ≥ 10% a partir do extremo, no sentido oposto ao da anterior, com o lado escuro < 80%.
        int direction = y >= _run + Step ? 1 : y <= _run - Step ? -1 : 0;
        if (direction == 0 || direction == _trend || Math.Min(y, _run) >= DarkLimit) return color;
        if (_transitions.Count >= MaxTransitions)
        {
            // Limite: segura a luz perto do extremo (no sentido em que ela queria ir) até a janela liberar.
            Held++;
            double held = Math.Max(0, _run + direction * Step * 0.95);
            return Scale(color, y <= 0 ? 0 : held / y);
        }
        _transitions.Enqueue(nowMs);
        _trend = direction;
        _run = y;
        return color;
    }

    private static double Luminance(Color c) => (0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B) / 255.0;

    private static Color Scale(Color c, double k)
    {
        k = Math.Max(0, k);
        return Color.FromArgb((int)Math.Clamp(Math.Round(c.R * k), 0, 255), (int)Math.Clamp(Math.Round(c.G * k), 0, 255), (int)Math.Clamp(Math.Round(c.B * k), 0, 255));
    }
}
