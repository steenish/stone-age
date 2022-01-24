using UnityEngine;

public class DoubleColor {
    public double a { get; set; }
    public double b { get; set; }
    public double g { get; set; }
    public double r { get; set; }

    public DoubleColor(double r, double g, double b, double a) {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
	}

    public DoubleColor(double r, double g, double b) : this(r, g, b, 1.0) {}

    public DoubleColor(Color color) {
        r = color.r;
        g = color.g;
        b = color.b;
        a = color.a;
	}
}
