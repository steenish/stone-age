import matplotlib.pyplot as plt

data = {
    "S1": 2116.858667,
    "S2": 258.0524444,
    "S3": 26.233,
    "C1": 1303.443444,
    "C2": 447.7557778,
    "C3": 649.9448889,
    "N1": 173.462,
    "N2": 768.0142222,
    "N3": 1459.667889
}
labels = list(data.keys())
values = list(data.values())

fig = plt.figure(figsize = (5, 5))

barlist = plt.bar(labels, values, width = 0.4)

barlist[0].set_color("#0B84A5")
barlist[1].set_color("#0B84A5")
barlist[2].set_color("#0B84A5")

barlist[3].set_color("#F6C85F")
barlist[4].set_color("#F6C85F")
barlist[5].set_color("#F6C85F")

barlist[6].set_color("#6F4E7C")
barlist[7].set_color("#6F4E7C")
barlist[8].set_color("#6F4E7C")

plt.grid(True, linestyle="dotted", axis="y")

ax = plt.gca()
ax.set_yticks(range(0, 2201, 100))
ax.set_ylim(0, 2200)
plt.xlabel("Parameter setting")
plt.ylabel("Execution time (s)")
plt.title("Execution time by parameter setting")

plt.gcf().subplots_adjust(left=0.15)

plt.savefig("Plots/performance.png")
plt.savefig("Plots/performance.pdf")
plt.show()