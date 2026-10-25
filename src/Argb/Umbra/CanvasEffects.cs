// CanvasEffects.cs - 1:1 C# ports of 12 AsiaHorse UMBRA "幕布模式 / Screen Mode" canvas effects
// (decompiled from Sync_light.pyc, class LightEffectManager; Python 3.8 + numpy 1.24.4).
//
// ---------------------------------------------------------------------------------------------
// CONVENTIONS OF THE ORIGINAL (read before using):
//
//  * Canvas: LightEffectManager.color_buffer = np.zeros((600, 600, 3), np.uint8)  -> [y, x, RGB],
//    integer 0..255 per channel (CanvasScene.setSceneRect(0, 0, 600, 600)). Many effects hard-code
//    600-based constants (radar max_radius = 600, 60-px matrix columns, randint(100, 500) ...).
//    The ports read W/H from the canvas you pass but keep those constants untouched, so pass a
//    600x600 canvas for a true 1:1 look (other sizes work, constants stay absolute pixels).
//
//  * Output range: every effect produces uint8 values; the port writes them into your
//    float[h, w, 3] canvas as value / 255f. The original applies NO gamma anywhere: the LED gets the
//    raw pixel, then optionally a hub current limit and int(v * brightness / 10) (brightness 1..10).
//    See UmbraLedPipeline at the end of the file for those two steps + LED coordinate helpers.
//
//  * Timing: CanvasScene runs QTimer(16 ms) -> update_effect() once per tick. Effects advance a fixed
//    amount per tick ("speed" units per frame) and never use dt (only Clock/Matrix read time.time()).
//    Render() therefore runs a fixed-step accumulator (TickSeconds = 0.016) and executes exactly one
//    original frame per elapsed tick (capped by MaxTicksPerRender), then presents the last frame.
//    Note: on a real PC the Python app often ran slower than 62.5 fps; tune TickSeconds to taste.
//
//  * Low-res effects draw into a uint8 buffer (100x100, 160x100, 150x150) and upscale with
//    np.linspace(0, n - 1, W).astype(int32) (nearest neighbour) - reproduced by Np.NearestMap.
//
//  * Random: numpy's RNG is replaced by System.Random, but with the same calls in the same order
//    (random/rand -> NextDouble, uniform(a,b) = a + (b-a)*NextDouble, randint(a,b) -> Next(a,b),
//    randn -> legacy polar Box-Muller with cached second value, choice -> index draw).
//
//  * Speed defaults = what the app actually starts with (json_init.MUBU_EFFECT_INIT overrides
//    default_effects: rainbow 4, fourth_dimension 1). UI slider range is 1..10 (riptide starts at 30).
// ---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DS339Monitor.Argb.Umbra;

/// <summary>RGB triple with 0..255 integer channels (the original stores colors as [r, g, b] int lists).</summary>
internal readonly record struct Rgb(int R, int G, int B)
{
    public static readonly Rgb Black = new(0, 0, 0);
    public static readonly Rgb White = new(255, 255, 255);
}

/// <summary>numpy-compatible helpers (linspace, floor-div, Python modulo, uint8 casts).</summary>
internal static class Np
{
    /// <summary>np.linspace(start, stop, num): y[i] = i * step + start, last element forced to stop.</summary>
    public static double[] Linspace(double start, double stop, int num)
    {
        var y = new double[Math.Max(0, num)];
        if (num <= 0) return y;
        int div = num - 1;
        if (div == 0) { y[0] = start; return y; }
        double step = (stop - start) / div;
        for (int i = 0; i < num; i++) y[i] = i * step + start;
        y[num - 1] = stop;
        return y;
    }

    /// <summary>np.linspace(start, stop, num, dtype=int32): float linspace truncated toward zero.</summary>
    public static int[] LinspaceInt(double start, double stop, int num)
    {
        var f = Linspace(start, stop, num);
        var r = new int[f.Length];
        for (int i = 0; i < f.Length; i++) r[i] = (int)f[i];
        return r;
    }

    /// <summary>Index map of buf[np.linspace(0, src-1, dst).astype(int)] (nearest-neighbour upscale).</summary>
    public static int[] NearestMap(int src, int dst) => LinspaceInt(0, src - 1, dst);

    /// <summary>Python a // b for ints.</summary>
    public static int FloorDiv(int a, int b) => (int)Math.Floor((double)a / b);

    /// <summary>Python float a % b (result has the sign of b).</summary>
    public static double PyMod(double a, double b)
    {
        double r = a % b;
        if (r != 0 && (r < 0) != (b < 0)) r += b;
        return r;
    }

    /// <summary>np.radians.</summary>
    public static double Radians(double deg) => deg * (Math.PI / 180.0);

    /// <summary>.astype(np.uint8) of a value the original keeps in 0..255 (clamped instead of wrapping).</summary>
    public static byte U8(double v) => v <= 0 ? (byte)0 : v >= 255 ? (byte)255 : (byte)(int)v;

    /// <summary>Python int assigned into a uint8 array (clamped instead of numpy's wrap-around).</summary>
    public static byte B(int v) => v <= 0 ? (byte)0 : v >= 255 ? (byte)255 : (byte)v;
}

/// <summary>numpy.random replacement with the same call pattern.</summary>
internal sealed class UmbraRandom
{
    private readonly Random _r;
    private bool _hasGauss;
    private double _gauss;

    public UmbraRandom(int? seed) => _r = seed is int s ? new Random(s) : new Random();

    /// <summary>np.random.random() / np.random.rand().</summary>
    public double Random() => _r.NextDouble();

    /// <summary>np.random.uniform(lo, hi).</summary>
    public double Uniform(double lo, double hi) => lo + (hi - lo) * _r.NextDouble();

    /// <summary>np.random.randint(lo, hi) (hi exclusive).</summary>
    public int RandInt(int lo, int hi) => _r.Next(lo, hi);

    /// <summary>np.random.randint(hi).</summary>
    public int RandInt(int hi) => _r.Next(0, hi);

    /// <summary>np.random.randn() - legacy polar method, second value cached like numpy's RandomState.</summary>
    public double Randn()
    {
        if (_hasGauss) { _hasGauss = false; return _gauss; }
        double x1, x2, r2;
        do
        {
            x1 = 2.0 * _r.NextDouble() - 1.0;
            x2 = 2.0 * _r.NextDouble() - 1.0;
            r2 = x1 * x1 + x2 * x2;
        } while (r2 >= 1.0 || r2 == 0.0);
        double f = Math.Sqrt(-2.0 * Math.Log(r2) / r2);
        _gauss = f * x1;
        _hasGauss = true;
        return f * x2;
    }
}

/// <summary>Base: fixed-step frame runner. One Step() == one call of the original _update_xxx_effect().</summary>
internal abstract class UmbraCanvasEffect
{
    public const int OriginalCanvasSize = 600;
    public const double OriginalTickSeconds = 0.016;

    protected UmbraCanvasEffect(int defaultSpeed, int? seed)
    {
        Speed = defaultSpeed;
        Rng = new UmbraRandom(seed);
    }

    /// <summary>Internal key used by the original (effects dict / update_effect dispatch).</summary>
    public abstract string Key { get; }
    public abstract string NameZh { get; }
    public abstract string NameEn { get; }

    /// <summary>effect["speed"] (UI slider 1..10).</summary>
    public int Speed { get; set; }

    /// <summary>Seconds per original frame (QTimer interval 16 ms).</summary>
    public double TickSeconds { get; set; } = OriginalTickSeconds;

    /// <summary>Catch-up cap so a hitch does not fast-forward the animation.</summary>
    public int MaxTicksPerRender { get; set; } = 8;

    protected UmbraRandom Rng { get; }
    protected int W { get; private set; }
    protected int H { get; private set; }

    private double _acc;
    private bool _ready;

    /// <summary>Advance by dtSeconds (in whole 16 ms original frames) and write the frame into canvas[h, w, 3] (0..1).</summary>
    public void Render(float[,,] canvas, double tSeconds, double dtSeconds)
    {
        int h = canvas.GetLength(0), w = canvas.GetLength(1);
        if (!_ready || w != W || h != H)
        {
            W = w;
            H = h;
            OnCanvasSize();
            _ready = true;
            _acc = 0;
            Step();
        }
        else
        {
            if (dtSeconds > 0) _acc += dtSeconds;
            double tick = TickSeconds > 1e-4 ? TickSeconds : OriginalTickSeconds;
            int n = (int)(_acc / tick);
            if (n > 0)
            {
                _acc -= n * tick;
                if (n > MaxTicksPerRender) { n = MaxTicksPerRender; _acc = 0; }
                for (int i = 0; i < n; i++) Step();
            }
        }
        Present(canvas);
    }

    /// <summary>Called on first render and whenever the canvas size changes.</summary>
    protected virtual void OnCanvasSize() { }

    /// <summary>One original frame.</summary>
    protected abstract void Step();

    protected abstract void Present(float[,,] canvas);

    // Shared colour helpers -------------------------------------------------------------------

    /// <summary>The 6-segment rainbow used by _update_rainbow_effect / _get_rainbow_color (pos 0..1).</summary>
    protected static void RainbowPiece(double pos, out int r, out int g, out int b)
    {
        if (pos < 0.166) { r = 255; g = (int)(255 * pos * 6); b = 0; }
        else if (pos < 0.333) { r = (int)(255 * (2 - pos * 6)); g = 255; b = 0; }
        else if (pos < 0.5) { r = 0; g = 255; b = (int)(255 * (pos * 6 - 2)); }
        else if (pos < 0.666) { r = 0; g = (int)(255 * (4 - pos * 6)); b = 255; }
        else if (pos < 0.833) { r = (int)(255 * (pos * 6 - 4)); g = 0; b = 255; }
        else { r = 255; g = 0; b = (int)(255 * (6 - pos * 6)); }
    }

    /// <summary>LightEffectManager.hsl_to_rgb(h 0..360, s 0..100, l 0..100).</summary>
    protected static Rgb HslToRgb(double h, double s, double l)
    {
        h /= 360.0;
        s /= 100.0;
        l /= 100.0;
        double r, g, b;
        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            r = HueToRgb(p, q, h + 0.3333333333333333);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 0.3333333333333333);
        }
        return new Rgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 0.16666666666666666) return p + (q - p) * 6 * t;
        if (t < 0.5) return q;
        if (t < 0.6666666666666666) return p + (q - p) * (0.6666666666666666 - t) * 6;
        return p;
    }
}

/// <summary>Effects that draw straight into the 600x600 color_buffer.</summary>
internal abstract class FullResEffect : UmbraCanvasEffect
{
    protected byte[] Buf = Array.Empty<byte>();
    private readonly List<double> _isect = new();

    protected FullResEffect(int defaultSpeed, int? seed) : base(defaultSpeed, seed) { }

    protected override void OnCanvasSize()
    {
        Buf = new byte[W * H * 3];
        ResetState();
    }

    /// <summary>Re-initialise size-dependent state.</summary>
    protected virtual void ResetState() { }

    protected override void Present(float[,,] canvas)
    {
        const float k = 1f / 255f;
        int i = 0;
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++, i += 3)
            {
                canvas[y, x, 0] = Buf[i] * k;
                canvas[y, x, 1] = Buf[i + 1] * k;
                canvas[y, x, 2] = Buf[i + 2] * k;
            }
    }

    protected void Clear() => Array.Clear(Buf);

    protected void Fill(Rgb c)
    {
        byte r = Np.B(c.R), g = Np.B(c.G), b = Np.B(c.B);
        for (int i = 0; i < Buf.Length; i += 3) { Buf[i] = r; Buf[i + 1] = g; Buf[i + 2] = b; }
    }

    protected void Set(int x, int y, byte r, byte g, byte b)
    {
        int i = (y * W + x) * 3;
        Buf[i] = r; Buf[i + 1] = g; Buf[i + 2] = b;
    }

    protected void Set(int x, int y, Rgb c) => Set(x, y, Np.B(c.R), Np.B(c.G), Np.B(c.B));

    /// <summary>color_buffer[y0:y1, x0:x1] = c (bounds already >= 0 in all callers; clipped to the canvas).</summary>
    protected void FillRect(int x0, int y0, int x1, int y1, Rgb c)
    {
        x0 = Math.Max(0, x0); y0 = Math.Max(0, y0);
        x1 = Math.Min(W, x1); y1 = Math.Min(H, y1);
        byte r = Np.B(c.R), g = Np.B(c.G), b = Np.B(c.B);
        for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++) Set(x, y, r, g, b);
    }

    /// <summary>Box + "dist &lt;= radius" disc used by radar centre dot / blips.</summary>
    protected void DrawDisc(int cx, int cy, int radius, Rgb c)
    {
        int sx = Math.Max(0, cx - radius), ex = Math.Min(W, cx + radius + 1);
        int sy = Math.Max(0, cy - radius), ey = Math.Min(H, cy + radius + 1);
        if (sx >= ex || sy >= ey) return;
        byte r = Np.B(c.R), g = Np.B(c.G), b = Np.B(c.B);
        long rr = (long)radius * radius;
        for (int y = sy; y < ey; y++)
            for (int x = sx; x < ex; x++)
            {
                long dx = x - cx, dy = y - cy;
                if (dx * dx + dy * dy <= rr) Set(x, y, r, g, b);
            }
    }

    /// <summary>LightEffectManager._draw_thick_line.</summary>
    protected void DrawThickLine(double x0, double y0, double x1, double y1, Rgb c, int thickness)
    {
        double dx = x1 - x0, dy = y1 - y0;
        double length = Math.Max(1, Math.Sqrt(dx * dx + dy * dy));
        double nx = dx / length, ny = dy / length;
        double px = -ny, py = nx;
        int half = Math.Max(1, Np.FloorDiv(thickness, 2));
        int minX = Math.Max(0, (int)(Math.Min(x0, x1) - half));
        int maxX = Math.Min(W - 1, (int)(Math.Max(x0, x1) + half));
        int minY = Math.Max(0, (int)(Math.Min(y0, y1) - half));
        int maxY = Math.Min(H - 1, (int)(Math.Max(y0, y1) + half));
        if (minX >= maxX || minY >= maxY) return;
        byte r = Np.B(c.R), g = Np.B(c.G), b = Np.B(c.B);
        for (int y = minY; y <= maxY; y++)
        {
            double vy = y - y0;
            for (int x = minX; x <= maxX; x++)
            {
                double vx = x - x0;
                double proj = vx * nx + vy * ny;
                double dist = Math.Abs(vx * px + vy * py);
                if (proj >= 0 && proj <= length && dist <= half) Set(x, y, r, g, b);
            }
        }
    }

    /// <summary>LightEffectManager.fill_polygon (scan-line, edge rule y1 &lt;= y &lt; y2, inclusive spans).</summary>
    protected void FillPolygon(double[] vx, double[] vy, int n, Rgb c)
    {
        double minY = vy[0], maxY = vy[0];
        for (int i = 1; i < n; i++) { minY = Math.Min(minY, vy[i]); maxY = Math.Max(maxY, vy[i]); }
        minY = Math.Max(0, minY);
        maxY = Math.Min(H - 1, maxY);
        byte r = Np.B(c.R), g = Np.B(c.G), b = Np.B(c.B);
        for (int y = (int)minY; y <= (int)maxY; y++)
        {
            _isect.Clear();
            for (int i = 0; i < n; i++)
            {
                double x1 = vx[i], y1 = vy[i];
                double x2 = vx[(i + 1) % n], y2 = vy[(i + 1) % n];
                if ((y1 <= y && y < y2) || (y2 <= y && y < y1))
                {
                    if (y2 != y1) _isect.Add(x1 + (x2 - x1) * (y - y1) / (y2 - y1));
                }
            }
            _isect.Sort();
            for (int k = 0; k + 1 < _isect.Count; k += 2)
            {
                int sx = (int)Math.Max(0, Math.Min(_isect[k], _isect[k + 1]));
                int ex = (int)Math.Min(W - 1, Math.Max(_isect[k], _isect[k + 1]));
                sx = Math.Max(0, sx);
                ex = Math.Min(W - 1, ex);
                for (int x = sx; x <= ex; x++) Set(x, y, r, g, b);
            }
        }
    }
}

/// <summary>Effects that render a small uint8 buffer and nearest-upscale it to the canvas.</summary>
internal abstract class LowResEffect : UmbraCanvasEffect
{
    protected readonly int LW, LH;
    protected readonly byte[] Low;
    private int[] _xm = Array.Empty<int>(), _ym = Array.Empty<int>();
    private int _mapW = -1, _mapH = -1;

    protected LowResEffect(int lw, int lh, int defaultSpeed, int? seed) : base(defaultSpeed, seed)
    {
        LW = lw;
        LH = lh;
        Low = new byte[lw * lh * 3];
    }

    protected void LowSet(int x, int y, byte r, byte g, byte b)
    {
        int i = (y * LW + x) * 3;
        Low[i] = r; Low[i + 1] = g; Low[i + 2] = b;
    }

    protected void LowSet(int x, int y, Rgb c) => LowSet(x, y, Np.B(c.R), Np.B(c.G), Np.B(c.B));

    protected void LowFill(Rgb c)
    {
        byte r = Np.B(c.R), g = Np.B(c.G), b = Np.B(c.B);
        for (int i = 0; i < Low.Length; i += 3) { Low[i] = r; Low[i + 1] = g; Low[i + 2] = b; }
    }

    protected override void Present(float[,,] canvas)
    {
        if (_mapW != W || _mapH != H)
        {
            _xm = Np.NearestMap(LW, W);
            _ym = Np.NearestMap(LH, H);
            _mapW = W;
            _mapH = H;
        }
        const float k = 1f / 255f;
        for (int y = 0; y < H; y++)
        {
            int row = _ym[y] * LW;
            for (int x = 0; x < W; x++)
            {
                int i = (row + _xm[x]) * 3;
                canvas[y, x, 0] = Low[i] * k;
                canvas[y, x, 1] = Low[i + 1] * k;
                canvas[y, x, 2] = Low[i + 2] * k;
            }
        }
    }
}

// =============================================================================================
// 1. 彩虹流动 Rainbow Flow  ("rainbow", _update_rainbow_effect)
// =============================================================================================
internal sealed class RainbowFlowEffect : FullResEffect
{
    private int _position;

    public RainbowFlowEffect(int? seed = null) : base(4, seed) { }

    public override string Key => "rainbow";
    public override string NameZh => "彩虹流动";
    public override string NameEn => "Rainbow Flow";

    protected override void Step()
    {
        _position += Speed;
        if (_position >= W) _position = 0;
        for (int x = 0; x < W; x++)
        {
            double pos = (double)((x + _position) % W) / W;
            RainbowPiece(pos, out int r, out int g, out int b);
            byte rb = Np.B(r), gb = Np.B(g), bb = Np.B(b);
            for (int y = 0; y < H; y++) Set(x, y, rb, gb, bb);
        }
    }
}

// =============================================================================================
// 2. 四次元 4th Dimension  ("fourth_dimension", _update_fourth_dimension_effect)
// =============================================================================================
internal sealed class FourthDimensionEffect : FullResEffect
{
    private double _timeCounter;
    private readonly double[] _hx = new double[6], _hy = new double[6];

    public FourthDimensionEffect(int? seed = null) : base(1, seed) { }

    public override string Key => "fourth_dimension";
    public override string NameZh => "四次元";
    public override string NameEn => "4th Dimension";

    /// <summary>effect["grid_size"].</summary>
    public int GridSize { get; set; } = 100;

    protected override void Step()
    {
        _timeCounter += Speed / 125.0;
        int grid = GridSize;
        int radius = Np.FloorDiv(grid, 2);
        Clear();
        int gapY = (int)(Math.Cos(Np.Radians(30)) * grid);
        int gridW = Np.FloorDiv(W, grid) + 1;
        int gridH = Np.FloorDiv(H, gapY) + 3;
        double centerX = W / 2.0, centerY = H / 2.0;
        for (int gy = 0; gy < gridH; gy++)
        {
            for (int gx = 0; gx < gridW; gx++)
            {
                double cx = gy % 2 == 0 ? gx * grid : gx * grid + grid / 2.0;
                double cy = gy * gapY;
                double dx = cx - centerX, dy = cy - centerY;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                double amp = Math.Sin(dist / 100 + _timeCounter) * 50;
                double ny = cy + amp;
                double hue = Np.PyMod(360 * ((double)gy / gridH + (double)gx / gridW + _timeCounter), 360);
                Rgb color = HslToRgb(hue, 100, 50);
                DrawHexagon((int)cx, (int)ny, radius, color);
            }
        }
    }

    private void DrawHexagon(int cx, int cy, int radius, Rgb color)
    {
        for (int i = 0; i < 6; i++)
        {
            double a = Np.Radians(60 * i + 30);
            _hx[i] = (int)(cx + radius * Math.Cos(a));
            _hy[i] = (int)(cy + radius * Math.Sin(a));
        }
        FillPolygon(_hx, _hy, 6, color);
    }
}

// =============================================================================================
// 3. 雷达扫描 Radar Scan  ("radar", _update_radar_effect)
// =============================================================================================
internal sealed class RadarScanEffect : FullResEffect
{
    private sealed class Blip
    {
        public double Angle, Distance;
        public int Lifetime, InitialLifetime, MoveDir;
    }

    private double _angle;
    private double _rippleTiming = 1;
    private readonly List<Blip> _blips = new();

    public RadarScanEffect(int? seed = null) : base(5, seed) { }

    public override string Key => "radar";
    public override string NameZh => "雷达扫描";
    public override string NameEn => "Radar Scan";

    /// <summary>[0] beam/centre, [1] blips, [2] ripple ring, [3] background.</summary>
    public Rgb[] Colors { get; set; } = [new(0, 255, 42), new(255, 255, 255), new(255, 255, 255), new(0, 23, 4)];
    public bool EnableBlips { get; set; } = true;
    public bool EnableRipples { get; set; } = true;
    public bool MoveBlips { get; set; } = true;
    public int BlipSize { get; set; } = 10;

    protected override void Step()
    {
        int speed = Math.Max(1, Math.Min(10, Speed));
        Rgb fg = Colors[0], blipColor = Colors[1], rippleColor = Colors[2], bg = Colors[3];
        _angle += speed * 0.005;
        if (_angle >= 2 * Math.PI) _angle -= 2 * Math.PI;
        Fill(bg);
        int cx = Np.FloorDiv(W, 2), cy = Np.FloorDiv(H, 2);
        const int maxRadius = 600;
        int endX = (int)(cx + Math.Cos(_angle) * maxRadius);
        int endY = (int)(cy + Math.Sin(_angle) * maxRadius);
        DrawThickLine(cx, cy, endX, endY, fg, 30);
        DrawDisc(cx, cy, 15, fg);

        if (EnableBlips && Rng.Random() < 0.025)
        {
            double angle = Rng.Random() * 2 * Math.PI;
            double distance = Rng.Uniform(0.2, 0.9) * maxRadius;
            int lifetime = Rng.RandInt(20, 50);
            int dir = Rng.Random() > 0.5 ? 1 : -1;
            _blips.Add(new Blip { Angle = angle, Distance = distance, Lifetime = lifetime, InitialLifetime = lifetime, MoveDir = dir });
        }
        // (the original iterates the blips even when enable_blips is off)
        foreach (var b in _blips)
        {
            b.Lifetime -= 1;
            if (MoveBlips)
            {
                double moveStep = maxRadius * 0.005;
                b.Distance += moveStep * b.MoveDir;
                if (b.Distance < 0.1 * maxRadius || b.Distance > maxRadius)
                {
                    b.MoveDir *= -1;
                    b.Distance = Math.Max(0.1 * maxRadius, Math.Min(b.Distance, maxRadius));
                }
            }
            int x = (int)(cx + Math.Cos(b.Angle) * b.Distance);
            int y = (int)(cy + Math.Sin(b.Angle) * b.Distance);
            int size = Math.Max(1, (int)(BlipSize * ((double)b.Lifetime / b.InitialLifetime)));
            if (size > 0 && 0 <= x && x < W && 0 <= y && y < H) DrawDisc(x, y, size, blipColor);
        }
        _blips.RemoveAll(b => b.Lifetime <= 0);

        if (EnableRipples)
        {
            _rippleTiming += speed * 2.0;
            double maxRippleSize = maxRadius * 1.5;
            if (_rippleTiming < maxRippleSize)
            {
                double rr = _rippleTiming;
                const int thick = 30;
                int sx = Math.Max(0, (int)(cx - rr - thick));
                int ex = Math.Min(W, (int)(cx + rr + thick) + 1);
                int sy = Math.Max(0, (int)(cy - rr - thick));
                int ey = Math.Min(H, (int)(cy + rr + thick) + 1);
                if (sx < ex && sy < ey)
                {
                    byte r = Np.B(rippleColor.R), g = Np.B(rippleColor.G), bl = Np.B(rippleColor.B);
                    for (int y = sy; y < ey; y++)
                        for (int x = sx; x < ex; x++)
                        {
                            double dx = x - cx, dy = y - cy;
                            double dist = Math.Sqrt(dx * dx + dy * dy);
                            if (dist >= rr && dist <= rr + thick) Set(x, y, r, g, bl);
                        }
                }
            }
            else
            {
                _rippleTiming = 0;
            }
        }
    }
}

// =============================================================================================
// 4. 黑客帝国 The Matrix  ("matrix_rain", _update_matrix_rain_effect)
// =============================================================================================
internal sealed class MatrixRainEffect : FullResEffect
{
    private sealed class Drop
    {
        public int X;
        public double Y, Speed;
        public int Length;
        public readonly List<char> Chars = new();
    }

    private readonly List<Drop> _drops = new();

    public MatrixRainEffect(int? seed = null) : base(5, seed) { }

    public override string Key => "matrix_rain";
    public override string NameZh => "黑客帝国";
    public override string NameEn => "The Matrix";

    public Rgb Color { get; set; } = new(0, 255, 34);
    /// <summary>effect["font_size"]: column width and block height in canvas px.</summary>
    public int FontSize { get; set; } = 60;
    /// <summary>Only consumed for RNG parity - the original never draws glyphs, just solid blocks.</summary>
    public string Chars { get; set; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    protected override void ResetState() => _drops.Clear();

    protected override void Step()
    {
        int speed = Speed;
        Clear();
        int cw = FontSize, ch = FontSize;
        int numColumns = Np.FloorDiv(W, cw);
        if (_drops.Count == 0)
        {
            for (int i = 0; i < numColumns; i++)
            {
                var d = new Drop { X = i * cw };
                d.Y = Rng.RandInt(-100, 0);
                d.Speed = Rng.Uniform(1, speed);
                d.Length = Rng.RandInt(300, 600);
                _drops.Add(d);
            }
        }
        foreach (var d in _drops)
        {
            d.Y += Rng.Uniform(speed, 10);
            if (d.Y > H + d.Length)
            {
                d.Y = Rng.RandInt(-100, 0);
                d.Speed = Rng.Uniform(30, 100);
                d.Length = Rng.RandInt(300, 600);
                d.Chars.Clear();
            }
            int startY = Math.Max(0, (int)d.Y);
            int endY = Math.Min(H, (int)(d.Y + d.Length));
            if (startY >= endY) continue;
            double relStart = (startY - d.Y) / d.Length;
            int numChars = Np.FloorDiv(endY - startY, ch);
            if (numChars <= 0) continue;
            if (d.Chars.Count < numChars)
            {
                d.Chars.Clear();
                for (int k = 0; k < numChars; k++) d.Chars.Add(Chars[Rng.RandInt(0, Chars.Length)]);
            }
            for (int i = 0; i < numChars; i++)
            {
                int charY = startY + i * ch;
                double relPos = relStart + (double)(i * ch) / d.Length;
                double brightness = Math.Max(0, Math.Min(1, 1.0 - relPos));
                var cc = new Rgb((int)(Color.R * brightness), (int)(Color.G * brightness), (int)(Color.B * brightness));
                int charX = d.X;
                if (charX < W && charY < H)
                {
                    int endX = Math.Min(W, charX + cw);
                    int endYc = Math.Min(H, charY + ch);
                    if (charX < endX && charY < endYc) FillRect(charX, charY, endX, endYc, cc);
                }
            }
        }
    }
}

// =============================================================================================
// 5. 火焰 Flame  ("fire", _update_fire_effect) - classic "doom fire" on a W/4 x H/4 grid, 4x4 blocks
// =============================================================================================
internal sealed class FlameEffect : UmbraCanvasEffect
{
    private int _fw, _fh, _cnt;
    private byte[] _heat = Array.Empty<byte>();
    private byte[] _render = Array.Empty<byte>();
    private int[] _rnd = Array.Empty<int>(), _r2 = Array.Empty<int>(), _nx = Array.Empty<int>();

    public FlameEffect(int? seed = null) : base(5, seed) { }

    public override string Key => "fire";
    public override string NameZh => "火焰";
    public override string NameEn => "Flame";

    /// <summary>Palette for heat &lt;64, &lt;128, &lt;192, &gt;=192.</summary>
    public Rgb[] Colors { get; set; } = [new(20, 10, 5), new(60, 20, 5), new(180, 80, 0), new(240, 160, 40)];

    protected override void OnCanvasSize()
    {
        _fw = Np.FloorDiv(W, 4);
        _fh = Np.FloorDiv(H, 4);
        _heat = new byte[_fw * _fh];
        if (_fh > 0)
            for (int x = 0; x < _fw; x++) _heat[(_fh - 1) * _fw + x] = 255;
        _render = new byte[_fw * _fh * 3];
        _rnd = new int[_fw];
        _r2 = new int[_fw];
        _nx = new int[_fw];
        _cnt = 0;
    }

    protected override void Step()
    {
        int speed = Math.Max(1, Speed);
        _cnt += 1;
        if (_cnt < 11 - speed) return; // the original keeps the previous frame on skipped ticks
        _cnt = 0;
        int fw = _fw, fh = _fh;
        if (fw == 0 || fh == 0) return;
        for (int y = fh - 2; y >= 0; y--)
        {
            for (int i = 0; i < fw; i++) _rnd[i] = Rng.RandInt(0, 4);
            for (int i = 0; i < fw; i++) _nx[i] = Math.Clamp(i - (_rnd[i] & 1) + (_rnd[i] >> 1), 0, fw - 1);
            for (int i = 0; i < fw; i++) _r2[i] = Rng.RandInt(0, 4);
            int below = (y + 1) * fw, row = y * fw;
            // buf[y, nx] = max(0, buf[y+1, nx] - rand): fancy assignment, duplicates -> last one wins,
            // columns not hit keep their old value (faithful quirk).
            for (int i = 0; i < fw; i++) _heat[row + _nx[i]] = (byte)Math.Max(0, _heat[below + _nx[i]] - _r2[i]);
        }
        for (int x = 0; x < fw; x++) _heat[(fh - 1) * fw + x] = 255;
        for (int i = 0, j = 0; i < _heat.Length; i++, j += 3)
        {
            int v = _heat[i];
            Rgb c = v < 64 ? Colors[0] : v < 128 ? Colors[1] : v < 192 ? Colors[2] : Colors[3];
            _render[j] = Np.B(c.R); _render[j + 1] = Np.B(c.G); _render[j + 2] = Np.B(c.B);
        }
    }

    protected override void Present(float[,,] canvas)
    {
        const float k = 1f / 255f;
        for (int y = 0; y < H; y++)
        {
            int fy = Math.Min(y >> 2, _fh - 1);
            for (int x = 0; x < W; x++)
            {
                if (_fw == 0 || fy < 0) { canvas[y, x, 0] = canvas[y, x, 1] = canvas[y, x, 2] = 0; continue; }
                int i = (fy * _fw + Math.Min(x >> 2, _fw - 1)) * 3;
                canvas[y, x, 0] = _render[i] * k;
                canvas[y, x, 1] = _render[i + 1] * k;
                canvas[y, x, 2] = _render[i + 2] * k;
            }
        }
    }
}

// =============================================================================================
// 6. 极光 Aurora  ("aurora", _update_aurora_effect) - fixed palette, 160x100 buffer
// =============================================================================================
internal sealed class AuroraEffect : LowResEffect
{
    private double _time;
    private readonly double[] _xs = Np.Linspace(0, 1, 160), _ys = Np.Linspace(0, 1, 100);

    public AuroraEffect(int? seed = null) : base(160, 100, 5, seed) { }

    public override string Key => "aurora";
    public override string NameZh => "极光";
    public override string NameEn => "Aurora";

    protected override void Step()
    {
        double speed = Speed / 10.0;
        _time += speed * 0.1;
        for (int j = 0; j < LH; j++)
        {
            double y = _ys[j];
            double mask = Math.Clamp(1.0 - Math.Pow(y, 1.6), 0, 1);
            for (int i = 0; i < LW; i++)
            {
                double x = _xs[i];
                double n1 = Math.Sin(x * 8.0 + _time * 1.2 + y * 2.0) * 0.5 + 0.5;
                double n2 = Math.Sin(x * 4.0 + _time * 0.7 + y * 1.5) * 0.5 + 0.5;
                double n3 = Math.Sin(x * 2.0 + _time * 0.3) * 0.5 + 0.5;
                double noise = n1 * 0.5 + n2 * 0.3 + n3 * 0.2;
                double alpha = noise * mask;
                LowSet(i, j, Np.U8(alpha * (180 - 100 * x)), Np.U8(alpha * (80 + 100 * x)), Np.U8(alpha * 220));
            }
        }
    }
}

// =============================================================================================
// 7. 激流 Rapid Flow  ("riptide_wave", _update_riptide_wave_effect) - 160x100 buffer
// =============================================================================================
internal sealed class RapidFlowEffect : LowResEffect
{
    private const int BubbleCount = 80;
    private const int BubbleRadius = 2;
    private int _time;
    private double _phase;
    private bool _bubblesInit;
    private readonly double[] _bx = new double[BubbleCount], _by = new double[BubbleCount];
    private readonly double[] _xs = Np.Linspace(0, 1, 160), _ys = Np.Linspace(0, 1, 100);
    private readonly double[] _wave = new double[160];
    private readonly List<int> _reset = new();

    public RapidFlowEffect(int? seed = null) : base(160, 100, 30, seed) { }

    public override string Key => "riptide_wave";
    public override string NameZh => "激流";
    public override string NameEn => "Rapid Flow";

    /// <summary>[0] background, [1] wave band, [2] bubbles.</summary>
    public Rgb[] Colors { get; set; } = [new(0, 26, 51), new(6, 66, 115), new(255, 255, 255)];

    protected override void Step()
    {
        double speed = Speed / 12.0;
        Rgb bg = Colors[0], bs = Colors[1], bub = Colors[2];
        int t = _time;
        double phase = _phase;
        _time += 1;
        _phase += speed * 0.08;
        bool reverse = t > 1350;
        if (reverse)
        {
            phase *= -2.5;
            if (t > 1600) _time = 0;
        }
        for (int i = 0; i < LW; i++)
        {
            double x = _xs[i];
            double wave1 = Math.Sin(x * 7.0 + phase * 1.3) * 0.45 + 0.45;
            double wave2 = Math.Sin(x * 14.0 + phase * 2.0) * 0.2 + 0.2;
            double wave3 = Math.Sin(x * 3.0 + phase * 0.7) * 0.15 + 0.15;
            _wave[i] = (wave1 + wave2 + wave3) * 0.8;
        }
        double vRange = reverse ? 0.4 : 0.22;
        for (int j = 0; j < LH; j++)
        {
            double y = _ys[j];
            for (int i = 0; i < LW; i++)
            {
                double yWave = 0.5 + (y - 0.5) * 0.15 - _wave[i] * vRange;
                double alpha = Math.Clamp(1.0 - Math.Abs(y - yWave) * 8.0, 0.0, 0.6);
                LowSet(i, j,
                    Np.U8(bg.R * (1 - alpha) + bs.R * alpha),
                    Np.U8(bg.G * (1 - alpha) + bs.G * alpha),
                    Np.U8(bg.B * (1 - alpha) + bs.B * alpha));
            }
        }

        if (!_bubblesInit)
        {
            for (int i = 0; i < BubbleCount; i++)
            {
                _bx[i] = Rng.Random() * LW;
                _by[i] = Rng.Random() * LH;
            }
            _bubblesInit = true;
        }
        double rise = speed * (reverse ? 3.0 : 1.0);
        for (int i = 0; i < BubbleCount; i++) _by[i] -= rise;
        for (int i = 0; i < BubbleCount; i++) _bx[i] += Rng.Randn() * 0.3;
        _reset.Clear();
        for (int i = 0; i < BubbleCount; i++)
            if (_by[i] < -BubbleRadius * 3) _reset.Add(i);
        foreach (int i in _reset) _bx[i] = Rng.Random() * LW;
        foreach (int i in _reset) _by[i] = LH + Rng.Random() * 20;

        byte br = Np.B(bub.R), bgc = Np.B(bub.G), bb = Np.B(bub.B);
        for (int i = 0; i < BubbleCount; i++)
        {
            int cx = (int)Math.Clamp(_bx[i], 0, LW - 1);
            int cy = (int)Math.Clamp(_by[i], 0, LH - 1);
            for (int y = Math.Max(0, cy - BubbleRadius); y <= Math.Min(LH - 1, cy + BubbleRadius); y++)
                for (int x = Math.Max(0, cx - BubbleRadius); x <= Math.Min(LW - 1, cx + BubbleRadius); x++)
                {
                    int dx = x - cx, dy = y - cy;
                    if (dx * dx + dy * dy <= BubbleRadius * BubbleRadius) LowSet(x, y, br, bgc, bb);
                }
        }
    }
}

// =============================================================================================
// 8. 流星 Meteor  ("meteor", _update_meteor_effect) - 100x100 buffer
// =============================================================================================
internal sealed class MeteorEffect : LowResEffect
{
    private sealed class Meteor
    {
        public double X, Y, Vx;
    }

    private readonly List<Meteor> _meteors = new();
    private readonly double[] _ys = Np.Linspace(0, 1, 100);

    public MeteorEffect(int? seed = null) : base(100, 100, 5, seed) { }

    public override string Key => "meteor";
    public override string NameZh => "流星";
    public override string NameEn => "Meteor";

    /// <summary>[0] sky top, [1] sky bottom, [2] meteor ([3] unused by the original).</summary>
    public Rgb[] Colors { get; set; } = [new(231, 67, 204), new(153, 67, 231), new(255, 246, 101), new(0, 0, 0)];
    /// <summary>effect["size"].</summary>
    public int Size { get; set; } = 10;
    /// <summary>effect["path"]: non-zero = random diagonal drift.</summary>
    public int Path { get; set; } = 1;

    protected override void Step()
    {
        int size = Math.Max(1, Size);
        Rgb sky1 = Colors[0], sky2 = Colors[1], star = Colors[2];
        // buf[..., i] = (sky1*(1-y) + sky2*y).astype(uint8); buf = clip(buf * 0.85).astype(uint8)
        for (int j = 0; j < LH; j++)
        {
            double y = _ys[j];
            byte r = Np.U8(Np.U8(sky1.R * (1 - y) + sky2.R * y) * 0.85);
            byte g = Np.U8(Np.U8(sky1.G * (1 - y) + sky2.G * y) * 0.85);
            byte b = Np.U8(Np.U8(sky1.B * (1 - y) + sky2.B * y) * 0.85);
            for (int i = 0; i < LW; i++) LowSet(i, j, r, g, b);
        }
        int maxCount = Math.Max(1, (int)(60.0 / size));
        if (_meteors.Count < maxCount && Rng.Random() < 0.25)
            _meteors.Add(new Meteor { X = Rng.Random() * LW, Y = -5, Vx = 0.0 });

        double moveSpeed = size * (Speed / 50.0);
        int tail = Math.Max(3, (int)(size * 0.7));
        byte sr = Np.B(star.R), sg = Np.B(star.G), sb = Np.B(star.B);
        byte tr = Np.U8(sr * 0.7), tg = Np.U8(sg * 0.7), tb = Np.U8(sb * 0.7);
        int rad = Math.Max(1, Np.FloorDiv(size, 2));

        var alive = new List<Meteor>(_meteors.Count);
        foreach (var m in _meteors)
        {
            double x = m.X, y = m.Y, vx = m.Vx;
            if (Path != 0)
            {
                if (Rng.RandInt(40) == 0) vx = moveSpeed * (Rng.Random() > 0.5 ? 1 : -1);
            }
            else
            {
                vx = 0;
            }
            x += vx;
            y += moveSpeed;
            m.X = x; m.Y = y; m.Vx = vx;
            int cx = (int)Math.Round(x, MidpointRounding.ToEven); // Python round(): half-to-even
            int cy = (int)Math.Round(y, MidpointRounding.ToEven);
            for (int dy = -rad; dy <= rad; dy++)
                for (int dx = -rad; dx <= rad; dx++)
                {
                    int d = Math.Abs(dx) + Math.Abs(dy);
                    if (d <= rad && (Math.Abs(dx) == Math.Abs(dy) || dx == 0 || dy == 0))
                    {
                        int px = cx + dx, py = cy + dy;
                        if (0 <= px && px < LW && 0 <= py && py < LH) LowSet(px, py, sr, sg, sb);
                    }
                }
            for (int i = 1; i < tail; i++)
            {
                double ty = y - i * 2;
                double tx = x - vx * i * 0.4;
                int px = (int)Math.Round(tx, MidpointRounding.ToEven);
                int py = (int)Math.Round(ty, MidpointRounding.ToEven);
                if (0 <= px && px < LW && 0 <= py && py < LH) LowSet(px, py, tr, tg, tb);
            }
            if (y < LH + 50) alive.Add(m);
        }
        _meteors.Clear();
        _meteors.AddRange(alive);
    }
}

// =============================================================================================
// 9. 暴风雨 Storm  ("storm", _update_storm_effect) - 100x100 buffer, rain + flash + bolts
// =============================================================================================
internal sealed class StormEffect : LowResEffect
{
    private sealed class RainDrop
    {
        public int X;
        public double Y, Speed;
        public Rgb Color;
    }

    private sealed class Bolt
    {
        public int X, Life;
    }

    private readonly List<RainDrop> _drops = new();
    private readonly List<Bolt> _bolts = new();
    private int _flash, _thunderTimer;
    private readonly int[] _px = new int[5], _py = new int[5];

    public StormEffect(int? seed = null) : base(100, 100, 5, seed) { }

    public override string Key => "storm";
    public override string NameZh => "暴风雨";
    public override string NameEn => "Storm";

    /// <summary>[0] sky, [1] rain colour A, [2] rain colour B (lightning is hard-coded white).</summary>
    public Rgb[] Colors { get; set; } = [new(0, 20, 40), new(0, 250, 255), new(0, 140, 255)];
    public Rgb Lightning { get; set; } = Rgb.White;
    public bool EnableThunder { get; set; } = true;
    public bool EnableLightning { get; set; } = true;
    public bool RandomBolts { get; set; } = true;

    protected override void Step()
    {
        double speed = Speed / 10.0;
        Rgb bg = Colors[0], rain1 = Colors[1], rain2 = Colors[2];
        const int rainSize = 2, lightningWidth = 2;
        LowFill(bg);

        if (_drops.Count < 60 && Rng.RandInt(0, 4) == 0)
        {
            int x = Rng.RandInt(0, LW);
            double sp = 0.5 + Rng.Random() * 1.5;
            Rgb c = Rng.Random() > 0.5 ? rain1 : rain2;
            _drops.Add(new RainDrop { X = x, Y = -5, Speed = sp, Color = c });
        }
        var alive = new List<RainDrop>(_drops.Count);
        foreach (var d in _drops)
        {
            d.Y += d.Speed * speed;
            int x = d.X;
            int y = (int)d.Y;
            if (0 <= x && x < LW && 0 <= y && y < LH)
            {
                for (int yy = y; yy < Math.Min(LH, y + rainSize); yy++)
                    for (int xx = x; xx < Math.Min(LW, x + rainSize); xx++) LowSet(xx, yy, d.Color);
            }
            if (d.Y < LH + 10) alive.Add(d);
        }
        _drops.Clear();
        _drops.AddRange(alive);

        _thunderTimer += 1;
        if (EnableThunder && _thunderTimer > 240)
        {
            _thunderTimer = 0;
            _flash = 20;
        }
        if (_flash > 0)
        {
            LowFill(Lightning);
            _flash -= 1;
        }

        if (EnableLightning && _bolts.Count < 1 && Rng.RandInt(0, 80) == 0)
            _bolts.Add(new Bolt { X = Rng.RandInt(20, LW - 20), Life = 12 });

        var boltAlive = new List<Bolt>(_bolts.Count);
        foreach (var b in _bolts)
        {
            int x = b.X;
            if (RandomBolts)
            {
                _px[0] = x; _py[0] = 0;
                _px[1] = x + Rng.RandInt(-4, 4); _py[1] = 25;
                _px[2] = x + Rng.RandInt(-4, 4); _py[2] = 50;
                _px[3] = x + Rng.RandInt(-4, 4); _py[3] = 75;
                _px[4] = x; _py[4] = LH;
                DrawBolt(lightningWidth, Lightning);
            }
            else
            {
                for (int yy = 0; yy < LH; yy++)
                    for (int xx = x; xx < Math.Min(LW, x + lightningWidth); xx++) LowSet(xx, yy, Lightning);
            }
            b.Life -= 1;
            if (b.Life > 0) boltAlive.Add(b);
        }
        _bolts.Clear();
        _bolts.AddRange(boltAlive);
    }

    /// <summary>LightEffectManager._draw_bolt: linspace-sampled segments, horizontally widened.</summary>
    private void DrawBolt(int width, Rgb color)
    {
        for (int i = 0; i < 4; i++)
        {
            int x1 = _px[i], y1 = _py[i], x2 = _px[i + 1], y2 = _py[i + 1];
            int n = Math.Max(5, (int)Math.Sqrt((double)(x2 - x1) * (x2 - x1) + (double)(y2 - y1) * (y2 - y1)));
            int[] xs = Np.LinspaceInt(x1, x2, n), ys = Np.LinspaceInt(y1, y2, n);
            int hw = Np.FloorDiv(width, 2);
            for (int k = 0; k < n; k++)
            {
                int y = ys[k];
                for (int dx = -hw; dx <= hw; dx++)
                {
                    int xx = xs[k] + dx;
                    if (0 <= xx && xx < LW && 0 <= y && y < LH) LowSet(xx, y, color);
                }
            }
        }
    }
}

// =============================================================================================
// 10. 超空间星场 Hyperspace Starfield  ("hyperspace", _update_hyperspace_effect) - 100x100, float32
// =============================================================================================
internal sealed class HyperspaceEffect : LowResEffect
{
    private const int N = 100, Stars = 50, RingCount = 40;
    private readonly float[] _x = new float[Stars], _y = new float[Stars], _z = new float[Stars];
    private readonly float[] _x2d = new float[Stars], _y2d = new float[Stars], _size = new float[Stars], _alpha = new float[Stars];
    private readonly float[] _rings;
    private readonly float[] _distCenter = new float[N * N];
    private bool _init;
    private double _ringT; // advanced but unused by the original (rings are static)

    public HyperspaceEffect(int? seed = null) : base(N, N, 5, seed)
    {
        var r = Np.Linspace(2, N / 2, RingCount);
        _rings = new float[RingCount];
        for (int i = 0; i < RingCount; i++) _rings[i] = (float)r[i];
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float dx = x - 50f, dy = y - 50f;
                _distCenter[y * N + x] = MathF.Sqrt(dx * dx + dy * dy);
            }
    }

    public override string Key => "hyperspace";
    public override string NameZh => "超空间星场";
    public override string NameEn => "Hyperspace Starfield";

    /// <summary>[0] background, [1] stars, [2] ring tint ([3] unused by the original).</summary>
    public Rgb[] Colors { get; set; } = [new(0, 0, 0), new(255, 255, 255), new(0, 201, 252), new(255, 255, 0)];

    protected override void Step()
    {
        int speed = Math.Clamp(Speed, 1, 10);
        if (!_init)
        {
            for (int i = 0; i < Stars; i++) _x[i] = (float)Rng.Uniform(0, N);
            for (int i = 0; i < Stars; i++) _y[i] = (float)Rng.Uniform(0, N);
            for (int i = 0; i < Stars; i++) _z[i] = (float)Rng.Uniform(0.1, N);
            _ringT = 0.0;
            _init = true;
        }
        const float cx = 50f, cy = 50f;
        float moveStep = (float)(speed * 0.9 * 1);
        for (int i = 0; i < Stars; i++) _z[i] -= moveStep;
        // reset: z = 100, then x for every reset star, then y (numpy order)
        for (int i = 0; i < Stars; i++) if (_z[i] <= 0) _z[i] = float.NaN; // mark
        for (int i = 0; i < Stars; i++) if (float.IsNaN(_z[i])) _x[i] = (float)Rng.Uniform(0, N);
        for (int i = 0; i < Stars; i++) if (float.IsNaN(_z[i])) { _y[i] = (float)Rng.Uniform(0, N); _z[i] = N; }

        for (int i = 0; i < Stars; i++)
        {
            float zSafe = MathF.Max(_z[i], 1.0f);
            float scale = N / zSafe;
            _x2d[i] = (_x[i] - cx) * scale + cx;
            _y2d[i] = (_y[i] - cy) * scale + cy;
            _size[i] = scale * 1.2f;
            _alpha[i] = Math.Clamp(2.2f - scale / 4f, 0.2f, 1.0f);
        }
        _ringT += speed * 0.08;

        Rgb bg = Colors[0], star = Colors[1], ring = Colors[2];
        float dr = (float)(N / (double)RingCount * 0.5);
        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                float sum = 0f;
                for (int i = 0; i < Stars; i++)
                {
                    float dx = x - _x2d[i], dy = y - _y2d[i];
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    if (dist < _size[i]) sum += _alpha[i];
                }
                float r = bg.R * (1 - sum) + star.R * sum;
                float g = bg.G * (1 - sum) + star.G * sum;
                float b = bg.B * (1 - sum) + star.B * sum;
                float dc = _distCenter[y * N + x];
                for (int k = 0; k < RingCount; k++)
                {
                    if (MathF.Abs(dc - _rings[k]) < dr)
                    {
                        r = r * 0.75f + ring.R * 0.25f;
                        g = g * 0.75f + ring.G * 0.25f;
                        b = b * 0.75f + ring.B * 0.25f;
                    }
                }
                LowSet(x, y, Np.U8(r), Np.U8(g), Np.U8(b));
            }
        }
    }
}

// =============================================================================================
// 11. 飘逸水母 Jellyfish  ("jellyfish", _update_jellyfish_effect) - 100x100 float32 trail buffer
// =============================================================================================
internal sealed class JellyfishEffect : LowResEffect
{
    private const int N = 100, Arms = 9, Points = 5;
    private const float Trace = 0.95f;
    private readonly float[] _buf = new float[N * N * 3];
    private readonly float[] _glow = new float[N * N];
    private readonly double[,] _px = new double[Arms, Points], _py = new double[Arms, Points];
    private readonly double[] _hx = new double[Arms], _hy = new double[Arms];
    private readonly double[] _angles = Np.Linspace(0, 2 * Math.PI, Arms);
    private double _t;
    private bool _init;

    public JellyfishEffect(int? seed = null) : base(N, N, 5, seed) { }

    public override string Key => "jellyfish";
    public override string NameZh => "飘逸水母";
    public override string NameEn => "Jellyfish";

    /// <summary>[0] water, [1] glow ([2],[3] unused by the original).</summary>
    public Rgb[] Colors { get; set; } = [new(4, 0, 20), new(30, 180, 220), new(80, 220, 255), new(140, 240, 255)];

    protected override void Step()
    {
        int speed = Math.Clamp(Speed, 1, 10);
        double moveSpeed = speed * 0.08;
        Rgb bg = Colors[0];
        Rgb c1 = Colors.Length >= 2 ? Colors[1] : new Rgb(30, 180, 220);
        if (!_init)
        {
            for (int i = 0; i < _buf.Length; i += 3) { _buf[i] = bg.R; _buf[i + 1] = bg.G; _buf[i + 2] = bg.B; }
            _t = 0.0;
            for (int a = 0; a < Arms; a++) for (int p = 0; p < Points; p++) _px[a, p] = Rng.Uniform(0, N);
            for (int a = 0; a < Arms; a++) for (int p = 0; p < Points; p++) _py[a, p] = Rng.Uniform(0, N);
            _init = true;
        }
        _t += moveSpeed;
        double t = _t;
        double tx = N / 2.0 + 28 * Math.Cos(t * 0.5);
        double ty = N / 2.0 + 28 * Math.Sin(t * 0.4);
        double r = 18 + 6 * Math.Sin(t * 0.8);
        for (int a = 0; a < Arms; a++)
        {
            _hx[a] = tx + r * Math.Cos(_angles[a]);
            _hy[a] = ty + r * Math.Sin(_angles[a]);
        }
        // chain constraint: each point is pulled to exactly 2.5 px from its predecessor
        for (int i = 0; i < Points; i++)
            for (int a = 0; a < Arms; a++)
            {
                double lx = i == 0 ? _hx[a] : _px[a, i - 1];
                double ly = i == 0 ? _hy[a] : _py[a, i - 1];
                double dx = _px[a, i] - lx, dy = _py[a, i] - ly;
                double dist = Math.Sqrt(dx * dx + dy * dy + 1e-08);
                _px[a, i] = lx + dx * 2.5 / dist;
                _py[a, i] = ly + dy * 2.5 / dist;
            }

        float bR = bg.R * (float)(1 - 0.95), bG = bg.G * (float)(1 - 0.95), bB = bg.B * (float)(1 - 0.95);
        for (int i = 0; i < _buf.Length; i += 3)
        {
            _buf[i] = _buf[i] * Trace + bR;
            _buf[i + 1] = _buf[i + 1] * Trace + bG;
            _buf[i + 2] = _buf[i + 2] * Trace + bB;
        }

        Array.Clear(_glow);
        for (int a = 0; a < Arms; a++)
        {
            for (int i = 0; i < Points; i++)
            {
                double x0 = i == 0 ? _hx[a] : _px[a, i - 1];
                double y0 = i == 0 ? _hy[a] : _py[a, i - 1];
                double x1 = _px[a, i], y1 = _py[a, i];
                double dxs = x1 - x0, dys = y1 - y0;
                double len2 = dxs * dxs + dys * dys + 1e-08;
                for (int y = 0; y < N; y++)
                    for (int x = 0; x < N; x++)
                    {
                        double tt = ((x - x0) * dxs + (y - y0) * dys) / len2;
                        tt = Math.Clamp(tt, 0, 1);
                        double pxn = x0 + tt * dxs, pyn = y0 + tt * dys;
                        double ex = x - pxn, ey = y - pyn;
                        double d = Math.Sqrt(ex * ex + ey * ey);
                        int gi = y * N + x;
                        _glow[gi] = (float)(_glow[gi] + Math.Exp(-d * 0.75) * 0.18);
                    }
            }
        }
        for (int gi = 0, i = 0; gi < _glow.Length; gi++, i += 3)
        {
            float gv = _glow[gi];
            _buf[i] += gv * c1.R;
            _buf[i + 1] += gv * c1.G;
            _buf[i + 2] += gv * c1.B;
            Low[i] = Np.U8(_buf[i]);
            Low[i + 1] = Np.U8(_buf[i + 1]);
            Low[i + 2] = Np.U8(_buf[i + 2]);
        }
    }
}

// =============================================================================================
// 12. 熔岩灯 Lava Lamp  ("lava_lamp", _update_lava_lamp_effect) - metaballs, 100x100 float32 buffer
// =============================================================================================
internal sealed class LavaLampEffect : LowResEffect
{
    private const int N = 100, BlobCount = 7;
    private const float Fade = 0.92f, Smooth = 0.15f;
    private readonly float[] _buf = new float[N * N * 3];
    private readonly float[,] _blobs = new float[BlobCount, 5]; // x, y, vx, vy, r
    private double _t;
    private bool _init;

    public LavaLampEffect(int? seed = null) : base(N, N, 5, seed) { }

    public override string Key => "lava_lamp";
    public override string NameZh => "熔岩灯";
    public override string NameEn => "Lava Lamp";

    /// <summary>[0] background, [1] blob rim glow, [2] blob core glow ([3] unused by the original).</summary>
    public Rgb[] Colors { get; set; } = [new(11, 3, 18), new(0, 13, 237), new(228, 0, 32), new(0, 0, 0)];

    protected override void Step()
    {
        int speed = Math.Clamp(Speed, 1, 10);
        float moveSpeed = (float)(speed * 0.2);
        Rgb bg = Colors[0];
        Rgb c1 = Colors.Length >= 2 ? Colors[1] : new Rgb(0, 13, 237);
        Rgb c2 = Colors.Length >= 3 ? Colors[2] : new Rgb(228, 0, 32);
        if (!_init)
        {
            for (int i = 0; i < _buf.Length; i += 3) { _buf[i] = bg.R; _buf[i + 1] = bg.G; _buf[i + 2] = bg.B; }
            _t = 0.0;
            for (int i = 0; i < BlobCount; i++) _blobs[i, 0] = (float)Rng.Uniform(20, N - 20);
            for (int i = 0; i < BlobCount; i++) _blobs[i, 1] = (float)Rng.Uniform(20, N - 20);
            for (int i = 0; i < BlobCount; i++) _blobs[i, 2] = (float)Rng.Uniform(-0.8, 0.8);
            for (int i = 0; i < BlobCount; i++) _blobs[i, 3] = (float)Rng.Uniform(-0.8, 0.8);
            for (int i = 0; i < BlobCount; i++) _blobs[i, 4] = (float)Rng.Uniform(8, 18);
            _init = true;
        }
        _t += speed * 0.2;
        for (int i = 0; i < BlobCount; i++)
        {
            _blobs[i, 0] += _blobs[i, 2] * moveSpeed;
            _blobs[i, 1] += _blobs[i, 3] * moveSpeed;
        }
        for (int i = 0; i < BlobCount; i++)
        {
            float x = _blobs[i, 0], y = _blobs[i, 1], r = _blobs[i, 4];
            if (x < r || x > N - r) _blobs[i, 2] *= -1;
            if (y < r || y > N - r) _blobs[i, 3] *= -1;
        }

        float bR = bg.R * (float)(1 - 0.92), bG = bg.G * (float)(1 - 0.92), bB = bg.B * (float)(1 - 0.92);
        for (int py = 0, i = 0; py < N; py++)
        {
            for (int px = 0; px < N; px++, i += 3)
            {
                float field = 0f;
                for (int k = 0; k < BlobCount; k++)
                {
                    float dx = px - _blobs[k, 0], dy = py - _blobs[k, 1];
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    float r = _blobs[k, 4];
                    field += r * r / (dist * dist + 1e-06f);
                }
                float glow1 = Math.Clamp(field - 1.0f, 0f, 1f);
                float glow2 = Math.Clamp(field - 1.1f, 0f, 1f);
                float vr = _buf[i] * Fade + bR, vg = _buf[i + 1] * Fade + bG, vb = _buf[i + 2] * Fade + bB;
                vr += glow1 * c1.R * Smooth; vg += glow1 * c1.G * Smooth; vb += glow1 * c1.B * Smooth;
                vr += glow2 * c2.R * Smooth * 0.7f; vg += glow2 * c2.G * Smooth * 0.7f; vb += glow2 * c2.B * Smooth * 0.7f;
                _buf[i] = vr; _buf[i + 1] = vg; _buf[i + 2] = vb;
                Low[i] = Np.U8(vr); Low[i + 1] = Np.U8(vg); Low[i + 2] = Np.U8(vb);
            }
        }
    }
}

// =============================================================================================
// Catalog / factory
// =============================================================================================
internal static class UmbraEffectCatalog
{
    /// <summary>Keys of the effects ported in this file, in the app's display order.</summary>
    public static readonly string[] PortedKeys =
    [
        "rainbow", "fourth_dimension", "radar", "matrix_rain", "fire", "aurora",
        "riptide_wave", "meteor", "storm", "hyperspace", "jellyfish", "lava_lamp",
    ];

    public static UmbraCanvasEffect? Create(string key, int? seed = null) => key switch
    {
        "rainbow" => new RainbowFlowEffect(seed),
        "fourth_dimension" => new FourthDimensionEffect(seed),
        "radar" => new RadarScanEffect(seed),
        "matrix_rain" => new MatrixRainEffect(seed),
        "fire" => new FlameEffect(seed),
        "aurora" => new AuroraEffect(seed),
        "riptide_wave" => new RapidFlowEffect(seed),
        "meteor" => new MeteorEffect(seed),
        "storm" => new StormEffect(seed),
        "hyperspace" => new HyperspaceEffect(seed),
        "jellyfish" => new JellyfishEffect(seed),
        "lava_lamp" => new LavaLampEffect(seed),
        _ => null,
    };
}

// =============================================================================================
// LED side of the pipeline (CanvasScene.update_buffer_and_redraw / save_icons_state)
// =============================================================================================
internal static class UmbraLedPipeline
{
    /// <summary>get_rgb_from_coordinates: the pixel under the LED, black when outside the canvas.</summary>
    public static (int R, int G, int B) Sample(float[,,] canvas, int x, int y)
    {
        if (0 <= x && x < canvas.GetLength(1) && 0 <= y && y < canvas.GetLength(0))
            return ((int)MathF.Round(canvas[y, x, 0] * 255f), (int)MathF.Round(canvas[y, x, 1] * 255f), (int)MathF.Round(canvas[y, x, 2] * 255f));
        return (0, 0, 0);
    }

    /// <summary>yaguang (only on PID_COMPAY == 10 hubs): if r+g+b &gt; limit scale all by limit/sum.
    /// limit = (hub_cfg[50] &lt;&lt; 8) | hub_cfg[51].</summary>
    public static void CurrentLimit(ref int r, ref int g, ref int b, int limit)
    {
        int sum = r + g + b;
        if (sum > limit)
        {
            double k = (double)limit / sum;
            r = (int)(r * k);
            g = (int)(g * k);
            b = (int)(b * k);
        }
    }

    /// <summary>liangdu_changer: brightness slider 1..10, int(v * brightness / 10).</summary>
    public static int Brightness(int v, int brightness) => (int)(v * (brightness / 10.0));

    /// <summary>Generic strip icon (PID_COMPAY 1/4 path): n LEDs evenly along the icon length, 25 px in.</summary>
    public static List<(int X, int Y)> StripLeds(double iconX, double iconY, int iconLength, int ledCount, bool horizontal, int powerDirection)
    {
        var list = new List<(int X, int Y)>(ledCount);
        double ju = (double)iconLength / ledCount;
        for (int i = 0; i < ledCount; i++)
        {
            if (horizontal) list.Add(((int)iconX + (int)(ju * (i + 1)), (int)iconY + 25));
            else list.Add(((int)iconX + 25, (int)iconY + (int)(ju * (i + 1))));
        }
        if (powerDirection != 1) list.Reverse();
        return list;
    }

    /// <summary>Device type 1 (26-LED fan, 150 px per fan, 50 px wide icon): per fan
    /// 4 centre-top + 4 centre-bottom (reversed, closing the hub loop) + 9 bottom edge + 9 top edge, 16 px pitch.</summary>
    public static List<(int X, int Y)> Fan26Leds(double iconX, double iconY, int fanCount, bool horizontal, int powerDirection)
    {
        int xb = (int)iconX, yb = (int)iconY;
        var fans = new List<List<(int X, int Y)>[]>();
        for (int t = 0; t < fanCount; t++)
        {
            int index = t * 150;
            var g0 = new List<(int, int)>(); var g1 = new List<(int, int)>();
            var g2 = new List<(int, int)>(); var g3 = new List<(int, int)>();
            for (int i = 0; i < 4; i++)
                g0.Add(horizontal ? (xb + 34 + index + 16 * (i + 1), yb + 16) : (xb + 32, yb + 34 + index + 16 * (i + 1)));
            for (int i = 0; i < 4; i++)
                g1.Add(horizontal ? (xb + 34 + index + 16 * (i + 1), yb + 32) : (xb + 16, yb + 34 + index + 16 * (i + 1)));
            g1.Reverse();
            for (int i = 0; i < 9; i++)
                g2.Add(horizontal ? (xb + index + 16 * (i + 1), yb + 49) : (xb + 0, yb + index + 16 * (i + 1)));
            for (int i = 0; i < 9; i++)
                g3.Add(horizontal ? (xb + index + 16 * (i + 1), yb) : (xb + 49, yb + index + 16 * (i + 1)));
            fans.Add([g0, g1, g2, g3]);
        }
        if (powerDirection != 1)
        {
            fans.Reverse();
            foreach (var f in fans)
            {
                var mp = new[] { new List<(int, int)>(f[0]), new List<(int, int)>(f[1]), new List<(int, int)>(f[2]), new List<(int, int)>(f[3]) };
                f[0] = Reversed(mp[1]);
                f[1] = Reversed(mp[0]);
                f[2] = mp[3];
                f[3] = mp[2];
                for (int k = 0; k < 4; k++) f[k] = Reversed(f[k]);
            }
        }
        var flat = new List<(int X, int Y)>(fanCount * 26);
        foreach (var f in fans) foreach (var g in f) flat.AddRange(g);
        return flat;
    }

    private static List<(int X, int Y)> Reversed(List<(int X, int Y)> l)
    {
        var r = new List<(int X, int Y)>(l);
        r.Reverse();
        return r;
    }
}
