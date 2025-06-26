using System;
using System.Globalization;

public class Program {
    public static void Main(string[] args) {
        complex z = new complex(1, 2);
        Console.WriteLine($"z = {z}");
    }
}


public struct complex {
    public double Re { get; }
    public double Im { get; }

    public complex(double re, double im = 0) { Re = re; Im = im; }
    public static implicit operator complex(double x) => new complex(x);
    public static implicit operator complex(int i) => new complex(i);

    public static double real(complex z) => z.Re;
    public static double imag(complex z) => z.Im;

    public static readonly complex Zero = new(0, 0);
    public static readonly complex One  = new(1, 0);
    public static readonly complex I    = new(0, 1);
    public static readonly complex NaN  = new(double.NaN, double.NaN);

    public complex Conjugate() => new(Re, -Im);
    public static complex operator~(complex a) => a.Conjugate();

    public static complex operator +(complex a, complex b) => new(a.Re + b.Re, a.Im + b.Im);
    public static complex operator +(complex a, double b) => new(a.Re + b, a.Im);
    public static complex operator +(double a, complex b) => new(a + b.Re, b.Im);

    public static complex operator -(complex a) => new(-a.Re, -a.Im);
    public static complex operator -(complex a, complex b) => new(a.Re - b.Re, a.Im - b.Im);
    public static complex operator -(complex a, double b) => new(a.Re - b, a.Im);
    public static complex operator -(double a, complex b) => new(a - b.Re, -b.Im);

    public static complex operator *(complex a, complex b) =>
        new(a.Re * b.Re - a.Im * b.Im, a.Re * b.Im + a.Im * b.Re);
    public static complex operator *(complex a, double b) => new(a.Re * b, a.Im * b);
    public static complex operator *(double a, complex b) => new(a * b.Re, a * b.Im);

    public static complex operator /(complex a, complex b) {
        double s = 1.0 / magnitude(b);
        double sbr = s * b.Re, sbi = s * b.Im;
        double zr = (a.Re * sbr + a.Im * sbi) * s;
        double zi = (a.Im * sbr - a.Re * sbi) * s;
        return new(zr, zi);
    }
    public static complex operator /(complex a, double x) => new(a.Re / x, a.Im / x);

    public static double argument(complex z) => Math.Atan2(z.Im, z.Re);
    public static double magnitude(complex z) {
        double zr = Math.Abs(z.Re), zi = Math.Abs(z.Im), r, t;
        if (zr > zi) { t = zi / zr; r = zr * Math.Sqrt(1 + t * t); }
        else         { t = zr / zi; r = zi * Math.Sqrt(1 + t * t); }
        return r;
    }

    static readonly string cformat = "{0:g3}+{1:g3}i";
    public override string ToString() =>
        String.Format(CultureInfo.CurrentCulture, cformat, Re, Im);
    public string ToString(string format) =>
        String.Format(CultureInfo.CurrentCulture, cformat, Re.ToString(format), Im.ToString(format));
    public string ToString(IFormatProvider provider) =>
        String.Format(provider, cformat, Re, Im);
    public string ToString(string format, IFormatProvider provider) =>
        String.Format(provider, cformat, Re.ToString(format, provider), Im.ToString(format, provider));

    public static bool approx(double a, double b, double abserr = 1e-9, double relerr = 1e-9) {
        double d = Math.Abs(a - b), s = Math.Abs(a) + Math.Abs(b);
        return d < abserr || d / s < relerr / 2;
    }
    public static bool approx(double a, complex b, double abserr = 1e-9, double relerr = 1e-9) =>
        approx(a, b.Re, abserr, relerr) && approx(0, b.Im, abserr, relerr);
    public bool approx(complex b, double abserr = 1e-9, double relerr = 1e-9) =>
        approx(Re, b.Re, abserr, relerr) && approx(Im, b.Im, abserr, relerr);

    public override bool Equals(object obj) => obj is complex b && Re.Equals(b.Re) && Im.Equals(b.Im);
    public override int GetHashCode() => HashCode.Combine(Re, Im);
}
