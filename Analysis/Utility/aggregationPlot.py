import matplotlib.pyplot as plt
import numpy as np
import math

def evaluate(xValues, alpha, sigma, tau):
    result = []
    for x in xValues:
        result.append(alpha + (1 - alpha) * math.exp(-sigma * (x - tau)**2))
    return result

def main():
    x = np.arange(0, 12, 0.01)
    plt.plot(x, evaluate(x, 0.1, 0.1, 1), label = "C1")
    plt.plot(x, evaluate(x, 0.01, 0.3, 1.5), label = "C2")
    plt.plot(x, evaluate(x, 0.00001, 10, 10), label = "C3")
    plt.xlim(0, 12)
    plt.ylim(0, 1.01)
    plt.title("Theoretical aggregation by number of neighbors")
    plt.ylabel("Theoretical aggregation A(n)")
    plt.xlabel("Number of neighbors n")
    plt.xticks(range(13))
    plt.yticks(np.arange(0, 1.1, 0.1))
    plt.grid(True, linestyle="dotted")
    plt.legend()

    plt.savefig("aggregation.png")
    plt.savefig("aggregation.pdf")
    plt.show()

if __name__ == "__main__":
    main()